# streamerbot-soclip

**Current version: v1.0.0** — **[⬇ Download install.sb](https://github.com/hakalachi/streamerbot-soclip/releases/latest/download/install.sb)** · see [CHANGELOG.md](CHANGELOG.md) for what's new.

**Shoutouts that actually show the streamer.** Type `!so somestreamer` (or get raided) and one of their best Twitch clips plays right on your stream — with their name and the game they were last playing — while a shoutout message goes to chat.

Designed for non-technical streamers. No developer accounts, no API keys, no auth pages: Streamer.bot is already connected to Twitch, and that's all this needs. Setup is importing one file and adding one browser source. About 5 minutes.

---

## Install (for streamers)

### What you need first
- **Streamer.bot** running and connected to your Twitch broadcaster account.
- **OBS** (or any software that supports browser sources).

### Step 1 — Import into Streamer.bot (~1 minute)

1. Download **[install.sb](https://github.com/hakalachi/streamerbot-soclip/releases/latest/download/install.sb)** (clicking the link downloads it).
2. In Streamer.bot, click the **Import** button at the top. Drag `install.sb` into the dialog (or open it in Notepad and paste the contents). Click **Import**.
3. You now have a **SoClip - Shoutout** action with the `!so` and `!soclip` commands already bound.

### Step 2 — Make sure the WebSocket server is on (~30 seconds)

The overlay listens to Streamer.bot over a local WebSocket.

1. In Streamer.bot go to **Servers/Clients → WebSocket Server**.
2. It should say it's running on `127.0.0.1:8080` (the default). If it isn't running, click **Start Server** and tick **Auto Start**.
3. If **Authentication** is enabled, either untick it, or note the password — you'll add it to the overlay URL in the next step.

### Step 3 — Add the overlay to OBS (~2 minutes)

1. Download **[overlay.html](https://raw.githubusercontent.com/hakalachi/streamerbot-soclip/main/overlay.html)** (right-click → *Save link as…*) and put it somewhere it can stay (e.g. next to your OBS scene collection — it must not move later).
2. In OBS: **Sources → + → Browser**, name it `SoClip`.
3. Tick **Local file** and pick `overlay.html`. Set **Width** and **Height** to your canvas size (usually `1920` × `1080`).
4. *(Recommended)* In the source's properties, set **Control audio via OBS** so the clip sound shows up in your mixer.

The card sits in the **center** by default and that's all most people need — if so, you're done with this step.

Want options (position, size, volume, test mode)? OBS doesn't allow query strings on *Local file* sources, so untick **Local file** and put a `file:///` path with options in the **URL** box instead:

```
file:///C:/path/to/overlay.html?position=bottom-right&width=420&volume=0.7
```

To position the card without spamming your chat, temporarily add `&test=1` — a fake clip loops every few seconds while you drag things around. Remove it when you're happy.

| Option | Values | Default |
| --- | --- | --- |
| `position` | `center`, `top`, `bottom`, `top-left`, `top-right`, `bottom-left`, `bottom-right` | `center` |
| `width` | card width in px (240–1280) | `480` |
| `volume` | `0.0`–`1.0` | `0.85` |
| `test` | `1` = loop a fake clip for positioning | off |
| `password` | WebSocket password, only if Authentication is on | — |
| `host`, `port` | only if you changed the WebSocket defaults | `127.0.0.1`, `8080` |

### Step 4 — Try it

In your Twitch chat, type:

```
!so <some channel that has clips>
```

You should see: a shoutout message in chat, and a few seconds later their clip slides in on stream, plays once, and slides out.

### Step 5 (optional) — Auto-shoutout your raiders

1. In Streamer.bot, select the **SoClip - Shoutout** action.
2. In the **Triggers** panel: right-click → **Add** → **Twitch** → **Raid** → **Raid**.

That's it — when someone raids you, their clip plays automatically. The same action handles both; no copies needed.

---

## Tweaking it

Open the action's C# sub-action (double-click it) — the top of the file is a small config block:

| Setting | What it does | Default |
| --- | --- | --- |
| `MaxClipSeconds` | Skip clips longer than this (keeps shoutouts snappy) | `61` |
| `PreferFeatured` | Use the channel's *featured* clips first | `true` |
| `ClipFetchCount` | How many of their top clips to pick from | `50` |
| `SendChatMessage` | Post the shoutout line in chat | `true` |
| `ChatTemplate` | The chat line — `{name}`, `{login}`, `{game}` get filled in | see file |
| `SendNativeShoutout` | Also fire Twitch's own `/shoutout` (you must be live; Twitch allows one per 2 min) | `false` |

Change values, click **Compile**, close. Done.

**Who can use `!so`?** Everyone, by default. You probably want mods only: Streamer.bot **Commands** tab → `so` → set *Permitted users/groups*.

---

## If something doesn't work

| Symptom | Try this |
| --- | --- |
| Chat message appears, but no clip on stream | The overlay isn't hearing Streamer.bot. Check Step 2 (WebSocket server running?). If Authentication is on, pass `password` in the overlay URL or turn it off. |
| Tiny red badge in the overlay's top-left corner | That's the overlay telling you exactly what's wrong (can't reach Streamer.bot / wrong password). Fix what it says; it disappears when healthy. |
| "Couldn't find a Twitch channel called …" | Typo in the name, or the account is banned/deleted. |
| Clip plays but silent | In OBS, check the source's **Control audio via OBS** setting and the mixer. Also check `volume` in the overlay URL. |
| Shoutout works for some channels but is chat-only for others | That channel has no clips, or none under `MaxClipSeconds`. Check the Streamer.bot **Logs** tab for `[SoClip]` lines — it says why. |
| Nothing at all, not even chat | Is Streamer.bot connected to Twitch? Open the action's **Run History** — did the trigger fire? Logs tab → look for `[SoClip]` errors. |
| Raids don't trigger it | Did you add the Raid trigger? (Step 5 — it's manual on purpose, some people don't want it.) |

> **Heads-up on clip playback:** Twitch has no official "give me the clip video file" API. SoClip resolves clips the same way Twitch's own web player does, which the community has relied on for years. It even self-heals when Twitch rotates their web client id (it re-discovers the current one automatically — this has already happened once and SoClip handles it). But if Twitch ever changes the mechanism more deeply, clips may temporarily stop playing; shoutouts gracefully fall back to chat-only and the `[SoClip]` log line says why. If that happens, check this repo for an update.

---

<details>
<summary><strong>For maintainers / forkers</strong></summary>

### Layout

- `streamer-bot/shoutout-clip.cs` — the single source of truth for the action code.
- `overlay.html` — the OBS browser source. Talks to Streamer.bot's WebSocket server, queues clips, handles the (optional) auth handshake.
- `install.sb` — **generated, don't hand-edit.** Rebuild with `tools/build-install-sb.ps1`.
- `tools/build-install-sb.ps1` — packs the `.cs` into the Streamer.bot import format: `base64("SBAE" + gzip(exportJson))`. No manual re-export from Streamer.bot needed, ever.
- `tools/compile-check.ps1` — compiles the `.cs` against a stub of the CPH API surface using the legacy C# 5 compiler. Run it after any code change; it catches typos and accidental use of modern syntax (the script intentionally avoids `$"..."` / `?.` so this check works on any Windows box).

### Making a change

1. Edit `streamer-bot/shoutout-clip.cs`.
2. `powershell -File tools\compile-check.ps1`
3. `powershell -File tools\build-install-sb.ps1 -Version X.Y.Z`
4. Bump the version in this README + add a `CHANGELOG.md` entry (SemVer).
5. Tag and release like spotify-control: `git tag vX.Y.Z && git push --tags`, GitHub Release with the changelog section.

### Forking

Find-and-replace `hakalachi/streamerbot-soclip` in this README with your own `<user>/<repo>`. The overlay and install.sb have no repo-specific strings.

</details>
