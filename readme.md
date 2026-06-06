# streamerbot-soclip

**Current version: v1.3.0** — **[⬇ Download install.sb](https://github.com/hakalachi/streamerbot-soclip/releases/latest/download/install.sb)** · see [CHANGELOG.md](CHANGELOG.md) for what's new.

**Shoutouts that actually show the streamer.** Type `!so somestreamer` (or get raided) and one of their best Twitch clips plays right on your stream — with their name and the game they were last playing — while a shoutout message goes to chat.

![SoClip overlay sliding in and playing a clip](docs/demo.gif)

*(shown in `test` mode — a real shoutout plays the streamer's actual top clip in the video area)*

Designed for non-technical streamers. No developer accounts, no API keys, no auth pages: Streamer.bot is already connected to Twitch, and that's all this needs. Setup is importing one file and adding one browser source — and all the settings live in a point-and-click page (`config.html`) with a live preview and a connection tester, so there's nothing to hand-edit. About 5 minutes.

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
3. Using a different port (or address)? Note it — you'll enter it in the settings page in Step 4.
4. If **Authentication** is enabled, either untick it, or note the password — you'll enter it in the settings page in Step 4.

### Step 3 — Add the overlay to OBS (~2 minutes)

1. Download **[overlay.html](https://github.com/hakalachi/streamerbot-soclip/releases/latest/download/overlay.html)**, **[config.js](https://github.com/hakalachi/streamerbot-soclip/releases/latest/download/config.js)** and **[config.html](https://github.com/hakalachi/streamerbot-soclip/releases/latest/download/config.html)** (clicking the links downloads them). Put all three **together in one folder** that can stay put (e.g. next to your OBS scene collection — they must not move later).
2. In OBS: **Sources → + → Browser**, name it `SoClip`.
3. Tick **Local file** and pick `overlay.html`. Set **Width** and **Height** to your canvas size (usually `1920` × `1080`).
4. *(Recommended)* In the source's properties, set **Control audio via OBS** so the clip sound shows up in your mixer.

### Step 4 — Point it at your Streamer.bot (~1 minute)

If your WebSocket server uses the defaults (port `8080`, no Authentication) and you're happy with the card in the **center**, skip this step — you're done.

Otherwise, **double-click `config.html`** — it opens in your browser as a friendly settings form:

- it fills itself in from your current `config.js`
- **⚡ Test connection** checks your port & password against Streamer.bot on the spot — green check or a plain-English reason
- a **live preview** of the card updates as you pick position, size, accent color and font
- **💾 Save config.js** writes a perfectly-formed file — save it over the old one, nothing to type, no syntax to break

Then tell OBS to reload it: right-click the `SoClip` source → **Properties** → **Refresh cache of current page**.

Prefer a text editor? `config.js` works the same as ever — every setting has a plain-English comment:

```js
port: 8080,          // the WebSocket server's port
password: "",        // only if "Authentication" is ticked -- put the password between the quotes
host: "127.0.0.1",   // only if Streamer.bot runs on a different computer
position: "center",  // center, top, bottom, top-left, top-right, bottom-left, bottom-right
width: 480,          // card width in pixels (240-1280)
volume: 0.85,        // clip volume: 0.0 (mute) to 1.0 (full)
accent: "#9146ff",   // frame + name color -- any CSS color, e.g. "#ff4757", "gold"
font: "",            // any Google Font name ("Poppins", "Bangers") or installed font
test: false          // true = loop a fake clip so you can position the card
```

**Make it match your brand:** `accent` recolors the frame, banner line, avatar
ring and highlights — any CSS color works. `font` swaps the banner typeface:
give it any name from [fonts.google.com](https://fonts.google.com) (it loads
automatically, nothing to install) or any font already installed on your PC.
Set `test: true` while you experiment so you can see changes after each
refresh without touching chat.

Change what you need, save, then tell OBS to reload it: right-click the `SoClip` source → **Properties** → **Refresh cache of current page**.

To position and size the card without spamming your chat, set `test: true` — a fake clip loops every few seconds while you drag things around. Set it back to `false` (and refresh again) when you're happy.

Typos are okay: if a lost quote or comma breaks `config.js`, a small badge appears in the overlay's top-left corner saying so, instead of your settings being silently ignored. (Files written by `config.html` can't have this problem.)

<details>
<summary><strong>Advanced: URL options instead of config.js</strong></summary>

Every `config.js` setting also works as a query option on the source URL, and URL options win over `config.js`. OBS doesn't allow query strings on *Local file* sources, so untick **Local file** and put a `file:///` path in the **URL** box:

```
file:///C:/path/to/overlay.html?port=8990&password=hunter2&position=bottom-right&test=1
```

Handy for a quick `test=1` without editing the file, or for running two overlays with different settings from the same folder. (URL-encode `#` in colors: `accent=%23ff4757`.)

</details>

### Step 5 — Try it

In your Twitch chat, type:

```
!so <some channel that has clips>
```

You should see: a shoutout message in chat, and a few seconds later their clip slides in on stream, plays once, and slides out.

### Step 6 (optional) — Auto-shoutout your raiders

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
| Chat message appears, but no clip on stream | The overlay isn't hearing Streamer.bot. Easiest check: open `config.html` and click **⚡ Test connection** — it tells you in plain English whether it's the server (Step 2), the port, or the password. |
| I changed my settings but nothing happened | OBS caches the page: right-click the `SoClip` source → **Properties** → **Refresh cache of current page**. Also make sure `config.js` sits in the same folder as `overlay.html` (if you used `config.html`'s download fallback, move the new file there). |
| Tiny red badge in the overlay's top-left corner | That's the overlay telling you exactly what's wrong (can't reach Streamer.bot / wrong password / broken `config.js`). Fix what it says; it disappears when healthy. |
| "Couldn't find a Twitch channel called …" | Typo in the name, or the account is banned/deleted. |
| Clip plays but silent | In OBS, check the source's **Control audio via OBS** setting and the mixer. Also check the volume slider in `config.html` (or `volume` in `config.js`). |
| Custom `font` doesn't show | Type it in `config.html` — the live preview shows immediately whether the name is right (check spelling on [fonts.google.com](https://fonts.google.com), e.g. `"Comic Neue"`, not `"Comic Neu"`). Then save and refresh the source cache. Fonts installed on your PC work by their installed name. |
| Shoutout works for some channels but is chat-only for others | That channel has no clips, or none under `MaxClipSeconds`. Check the Streamer.bot **Logs** tab for `[SoClip]` lines — it says why. |
| Nothing at all, not even chat | Is Streamer.bot connected to Twitch? Open the action's **Run History** — did the trigger fire? Logs tab → look for `[SoClip]` errors. |
| Raids don't trigger it | Did you add the Raid trigger? (Step 6 — it's manual on purpose, some people don't want it.) |

> **Heads-up on clip playback:** Twitch has no official "give me the clip video file" API. SoClip resolves clips the same way Twitch's own web player does, which the community has relied on for years. It even self-heals when Twitch rotates their web client id (it re-discovers the current one automatically — this has already happened once and SoClip handles it). But if Twitch ever changes the mechanism more deeply, clips may temporarily stop playing; shoutouts gracefully fall back to chat-only and the `[SoClip]` log line says why. If that happens, check this repo for an update.

---

<details>
<summary><strong>For maintainers / forkers</strong></summary>

### Layout

- `streamer-bot/shoutout-clip.cs` — the single source of truth for the action code.
- `overlay.html` — the OBS browser source. Talks to Streamer.bot's WebSocket server, queues clips, handles the (optional) auth handshake.
- `config.js` — the streamer-edited settings file; `overlay.html` loads it from the same folder. Ships with all defaults spelled out. Optional at runtime (missing file = defaults), and URL query options override it.
- `config.html` — the settings builder UI. Self-contained; pre-fills from `config.js` in the same folder, live-previews the card, tests the Streamer.bot connection with the overlay's real handshake (same subscribe + auth code), and generates a `config.js` with values safely JSON-escaped. Keep its generated template in sync with `config.js`'s comments.
- `install.sb` — **generated, don't hand-edit.** Rebuild with `tools/build-install-sb.ps1`.
- `tools/build-install-sb.ps1` — packs the `.cs` into the Streamer.bot import format: `base64("SBAE" + gzip(exportJson))`. No manual re-export from Streamer.bot needed, ever.
- `tools/compile-check.ps1` — compiles the `.cs` against a stub of the CPH API surface using the legacy C# 5 compiler. Run it after any code change; it catches typos and accidental use of modern syntax (the script intentionally avoids `$"..."` / `?.` so this check works on any Windows box).

### Making a change

1. Edit `streamer-bot/shoutout-clip.cs`.
2. `powershell -File tools\compile-check.ps1`
3. `powershell -File tools\build-install-sb.ps1 -Version X.Y.Z`
4. Bump the version in this README + add a `CHANGELOG.md` entry (SemVer).
5. Tag and release: `git tag vX.Y.Z && git push --tags`, then a GitHub Release with the changelog section, attaching **install.sb, overlay.html, config.js and config.html** — the README's download links point at `releases/latest/download/<file>`, so every release must carry all four.

### Forking

Find-and-replace `hakalachi/streamerbot-soclip` in this README with your own `<user>/<repo>`. The overlay and install.sb have no repo-specific strings.

</details>
