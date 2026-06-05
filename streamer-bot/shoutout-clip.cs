using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// SoClip — shoutout with a clip.
// Triggered by the !so command (or a Twitch Raid trigger). Looks up the target
// channel, posts a shoutout in chat, picks one of their top clips, resolves a
// playable video URL, and broadcasts it to the overlay over Streamer.bot's
// WebSocket server. The overlay (overlay.html) does the on-screen part.
//
// NOTE: written deliberately without newer C# syntax (no $"...", no ?.) so it
// compiles everywhere, including older toolchains. Keep it that way.
public class CPHInline
{
    // ===================== CONFIG =====================
    // Edit these if you want, then click Compile. Everything has a sane default.

    private const int    MaxClipSeconds     = 61;    // skip clips longer than this; 0 = allow any length
    private const bool   PreferFeatured     = true;  // try the channel's "featured" clips first, fall back to all
    private const int    ClipFetchCount     = 50;    // how many of their top clips to choose from
    private const bool   SendChatMessage    = true;  // post the shoutout line in chat
    private const bool   SendNativeShoutout = false; // also fire Twitch's /shoutout (you must be live; 1 per 2 min)
    private const string ChatTemplate =
        "Go show {name} some love at https://twitch.tv/{login} — last seen playing {game}!";

    // ==================================================

    private const string GqlEndpoint = "https://gql.twitch.tv/gql";
    // Twitch's own public web-player client id — used only to resolve clip video
    // URLs. Twitch rotates it once in a blue moon; when that happens the script
    // discovers the new one from twitch.tv automatically and remembers it
    // (global var soclip.gqlClientId), so this constant is just the first guess.
    // Verified current as of 2026-06-05.
    private const string GqlClientId = "kimne78kx3ncx6brgo4mv6wki5h1ko";

    public bool Execute()
    {
        try
        {
            string target = ResolveTarget();
            if (target == null) return false; // ResolveTarget already told chat what's wrong

            using (HttpClient http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(10);

                JObject user = HelixGetFirst(http,
                    "https://api.twitch.tv/helix/users?login=" + Uri.EscapeDataString(target));
                if (user == null)
                {
                    CPH.SendMessage("Couldn't find a Twitch channel called \"" + target + "\".");
                    return false;
                }

                string userId      = (string)user["id"];
                string login       = (string)user["login"];
                string displayName = (string)user["display_name"];
                string avatarUrl   = (string)user["profile_image_url"];
                if (string.IsNullOrEmpty(displayName)) displayName = login;

                JObject channel = HelixGetFirst(http,
                    "https://api.twitch.tv/helix/channels?broadcaster_id=" + userId);
                string game = channel == null ? null : (string)channel["game_name"];
                if (string.IsNullOrEmpty(game)) game = "something cool";

                if (SendChatMessage)
                {
                    CPH.SendMessage(ChatTemplate
                        .Replace("{name}", displayName)
                        .Replace("{login}", login)
                        .Replace("{game}", game));
                }

                if (SendNativeShoutout) TryNativeShoutout(http, userId);

                JObject clip = PickClip(http, userId);
                if (clip == null)
                {
                    CPH.LogInfo("[SoClip] No eligible clips for " + login + " — shoutout was chat-only.");
                    return true;
                }

                string videoUrl = ResolveVideoUrl(http, (string)clip["id"], (string)clip["thumbnail_url"]);
                if (videoUrl == null)
                {
                    CPH.LogInfo("[SoClip] Couldn't resolve a playable URL for clip "
                        + (string)clip["id"] + " — shoutout was chat-only.");
                    return true;
                }

                JObject payload = new JObject();
                payload["type"] = "soclip.play";
                JObject c = new JObject();
                c["videoUrl"]    = videoUrl;
                c["title"]       = (string)clip["title"];
                c["duration"]    = clip["duration"] == null ? 0.0 : (double)clip["duration"];
                c["displayName"] = displayName;
                c["login"]       = login;
                c["avatarUrl"]   = avatarUrl;
                c["game"]        = game;
                payload["clip"] = c;
                CPH.WebsocketBroadcastJson(payload.ToString(Formatting.None));

                CPH.LogInfo("[SoClip] Sent clip " + (string)clip["id"] + " for " + login + " to the overlay.");
                return true;
            }
        }
        catch (Exception ex)
        {
            CPH.LogError("[SoClip] " + ex.ToString());
            CPH.SendMessage("Shoutout hit an error — check the Streamer.bot logs.");
            return false;
        }
    }

    // Figure out who we're shouting out: "!so name" for commands, the raider for raids.
    private string ResolveTarget()
    {
        string rawInput;
        CPH.TryGetArg("rawInput", out rawInput);
        rawInput = (rawInput ?? string.Empty).Trim();

        if (rawInput.Length == 0)
        {
            string source;
            CPH.TryGetArg("__source", out source);
            if (source != null && source.IndexOf("Raid", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CPH.TryGetArg("userName", out rawInput);
                if (string.IsNullOrWhiteSpace(rawInput)) CPH.TryGetArg("user", out rawInput);
                rawInput = (rawInput ?? string.Empty).Trim();
            }
        }

        if (rawInput.Length == 0)
        {
            CPH.SendMessage("Usage: !so <channel>");
            return null;
        }

        // Accept "@Name", "Name", or a pasted "twitch.tv/Name" link.
        string target = rawInput.Split(' ')[0].TrimStart('@');
        int slash = target.LastIndexOf('/');
        if (slash >= 0) target = target.Substring(slash + 1);
        target = target.TrimEnd('.', ',', '!', '?').ToLowerInvariant();

        if (target.Length == 0)
        {
            CPH.SendMessage("Usage: !so <channel>");
            return null;
        }
        return target;
    }

    private JObject HelixGet(HttpClient http, string url)
    {
        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Authorization", "Bearer " + CPH.TwitchOAuthToken);
        req.Headers.Add("Client-Id", CPH.TwitchClientId);
        HttpResponseMessage resp = http.SendAsync(req).GetAwaiter().GetResult();
        string text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!resp.IsSuccessStatusCode)
        {
            CPH.LogError("[SoClip] Helix " + url + " failed (" + (int)resp.StatusCode + "): " + text);
            return null;
        }
        return JObject.Parse(text);
    }

    private JObject HelixGetFirst(HttpClient http, string url)
    {
        JObject body = HelixGet(http, url);
        if (body == null) return null;
        JArray data = body["data"] as JArray;
        if (data == null || data.Count == 0) return null;
        return data[0] as JObject;
    }

    private JObject PickClip(HttpClient http, string userId)
    {
        List<JObject> pool = FetchClips(http, userId, PreferFeatured);
        if (pool.Count == 0 && PreferFeatured) pool = FetchClips(http, userId, false);
        if (pool.Count == 0) return null;

        // Don't replay the clip we used for this channel last time (if they have others).
        string lastKey = "soclip.lastClip." + userId;
        string lastId = CPH.GetGlobalVar<string>(lastKey, false);
        List<JObject> fresh = pool.Where(p => (string)p["id"] != lastId).ToList();
        if (fresh.Count == 0) fresh = pool;

        JObject pick = fresh[new Random().Next(fresh.Count)];
        CPH.SetGlobalVar(lastKey, (string)pick["id"], false);
        return pick;
    }

    private List<JObject> FetchClips(HttpClient http, string userId, bool featuredOnly)
    {
        string url = "https://api.twitch.tv/helix/clips?broadcaster_id=" + userId
            + "&first=" + ClipFetchCount;
        if (featuredOnly) url += "&is_featured=true";

        List<JObject> eligible = new List<JObject>();
        JObject body = HelixGet(http, url);
        if (body == null) return eligible;
        JArray data = body["data"] as JArray;
        if (data == null) return eligible;

        foreach (JToken t in data)
        {
            JObject clip = t as JObject;
            if (clip == null) continue;
            double duration = clip["duration"] == null ? 0.0 : (double)clip["duration"];
            if (MaxClipSeconds > 0 && duration > MaxClipSeconds) continue;
            eligible.Add(clip);
        }
        return eligible;
    }

    // Turn a clip id into a direct video URL. Twitch doesn't expose this via the
    // official API, so we ask the same GQL endpoint the Twitch web player uses,
    // and fall back to the legacy thumbnail-to-mp4 mapping for older clips.
    private string ResolveVideoUrl(HttpClient http, string clipId, string thumbnailUrl)
    {
        string fromGql = null;
        try { fromGql = ResolveViaGql(http, clipId); }
        catch (Exception ex) { CPH.LogDebug("[SoClip] GQL resolve failed: " + ex.Message); }
        if (!string.IsNullOrEmpty(fromGql)) return fromGql;

        if (!string.IsNullOrEmpty(thumbnailUrl))
        {
            int idx = thumbnailUrl.IndexOf("-preview-", StringComparison.Ordinal);
            if (idx > 0) return thumbnailUrl.Substring(0, idx) + ".mp4";
        }
        return null;
    }

    private string ResolveViaGql(HttpClient http, string clipId)
    {
        string clientId = CPH.GetGlobalVar<string>("soclip.gqlClientId", true);
        if (string.IsNullOrEmpty(clientId)) clientId = GqlClientId;

        bool rejected;
        string url = GqlClipQuery(http, clientId, clipId, out rejected);
        if (url != null || !rejected) return url;

        // Twitch rotated its web client id. Discover the current one, remember
        // it, and retry once.
        string discovered = DiscoverGqlClientId(http);
        if (string.IsNullOrEmpty(discovered) || discovered == clientId) return null;
        CPH.LogInfo("[SoClip] Twitch web client id changed — now using " + discovered);
        CPH.SetGlobalVar("soclip.gqlClientId", discovered, true);
        return GqlClipQuery(http, discovered, clipId, out rejected);
    }

    private string GqlClipQuery(HttpClient http, string clientId, string clipId, out bool clientIdRejected)
    {
        clientIdRejected = false;

        // A raw GQL query (not a persisted-query hash — those rotate too often).
        JObject query = new JObject();
        query["query"] = "query($slug: ID!) { clip(slug: $slug) { "
            + "playbackAccessToken(params: {platform: \"web\", playerType: \"embed\"}) { signature value } "
            + "videoQualities { quality sourceURL } } }";
        JObject variables = new JObject();
        variables["slug"] = clipId;
        query["variables"] = variables;

        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, GqlEndpoint);
        req.Headers.Add("Client-ID", clientId);
        req.Content = new StringContent(query.ToString(Formatting.None), Encoding.UTF8, "application/json");
        HttpResponseMessage resp = http.SendAsync(req).GetAwaiter().GetResult();
        string text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!resp.IsSuccessStatusCode)
        {
            clientIdRejected = (int)resp.StatusCode == 400
                && text.IndexOf("Client-ID", StringComparison.OrdinalIgnoreCase) >= 0;
            CPH.LogDebug("[SoClip] GQL returned " + (int)resp.StatusCode + ": " + text);
            return null;
        }

        JObject parsed = JObject.Parse(text);
        JToken clip = parsed["data"] == null ? null : parsed["data"]["clip"];
        if (clip == null || clip.Type == JTokenType.Null) return null;

        JArray qualities = clip["videoQualities"] as JArray;
        JToken token = clip["playbackAccessToken"];
        if (qualities == null || qualities.Count == 0 || token == null || token.Type == JTokenType.Null)
            return null;

        // Highest resolution available.
        string sourceUrl = null;
        int best = -1;
        foreach (JToken q in qualities)
        {
            int height;
            if (!int.TryParse((string)q["quality"], out height)) height = 0;
            if (height > best)
            {
                best = height;
                sourceUrl = (string)q["sourceURL"];
            }
        }

        string sig = (string)token["signature"];
        string val = (string)token["value"];
        if (string.IsNullOrEmpty(sourceUrl) || string.IsNullOrEmpty(sig) || string.IsNullOrEmpty(val))
            return null;
        return sourceUrl + "?sig=" + sig + "&token=" + Uri.EscapeDataString(val);
    }

    // The web client id is embedded in twitch.tv's own page source.
    private string DiscoverGqlClientId(HttpClient http)
    {
        try
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://www.twitch.tv/");
            req.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");
            HttpResponseMessage resp = http.SendAsync(req).GetAwaiter().GetResult();
            string html = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Match m = Regex.Match(html, "clientId\\s*[=:]\\s*\"([A-Za-z0-9]{20,40})\"");
            if (m.Success) return m.Groups[1].Value;
        }
        catch (Exception ex)
        {
            CPH.LogDebug("[SoClip] client id discovery failed: " + ex.Message);
        }
        return null;
    }

    private void TryNativeShoutout(HttpClient http, string targetUserId)
    {
        try
        {
            // No params = "who owns this token", i.e. the broadcaster account.
            JObject me = HelixGetFirst(http, "https://api.twitch.tv/helix/users");
            if (me == null) return;
            string meId = (string)me["id"];
            if (meId == targetUserId) return; // Twitch rejects self-shoutouts

            string url = "https://api.twitch.tv/helix/chat/shoutouts"
                + "?from_broadcaster_id=" + meId
                + "&to_broadcaster_id=" + targetUserId
                + "&moderator_id=" + meId;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("Authorization", "Bearer " + CPH.TwitchOAuthToken);
            req.Headers.Add("Client-Id", CPH.TwitchClientId);
            HttpResponseMessage resp = http.SendAsync(req).GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
            {
                string text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                CPH.LogInfo("[SoClip] Native /shoutout didn't go through (" + (int)resp.StatusCode + "): "
                    + text + " — you must be live, and Twitch allows one every 2 minutes.");
            }
        }
        catch (Exception ex)
        {
            CPH.LogDebug("[SoClip] Native shoutout failed: " + ex.Message);
        }
    }
}
