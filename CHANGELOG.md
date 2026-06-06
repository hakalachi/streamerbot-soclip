# Changelog

All notable changes to this project. Versioning follows [SemVer](https://semver.org/).

## v1.2.0 — 2026-06-06

Brand the card to match your stream.

- **New `accent` setting** — any CSS color (`"#ff4757"`, `"gold"`,
  `"rgb(80,200,120)"`) recolors the frame, banner line, avatar ring, `@`, and
  game-name highlight (a pale tint is derived automatically). Invalid colors
  fall back to the default Twitch purple with a console warning.
- **New `font` setting** — any [Google Font](https://fonts.google.com) name
  loads automatically at runtime (nothing to install), or use any font already
  installed on the machine. Falls back to the system stack.
- Both work in `config.js` and as URL options (`?accent=%23ff4757&font=Bangers`).
- Test mode now shows Twitch's stock avatar instead of a broken-image glyph.
- `install.sb` / the Streamer.bot action are unchanged — just replace
  `overlay.html` and `config.js` keeps working without the new keys.

## v1.1.1 — 2026-06-05

Two overlay fixes that together caused "chat works, but no clip ever plays"
inside OBS — found and verified live against a real Streamer.bot 1.0.4 + OBS:

- **Auth now works inside OBS browser sources.** OBS serves Local-file
  sources from an insecure origin where the browser's `crypto.subtle` API
  doesn't exist, so the overlay silently failed the authentication handshake
  and Streamer.bot dropped it 30s later (close 4006) — over and over. The
  SHA-256 needed for the handshake is now computed in plain JS when
  `crypto.subtle` is unavailable.
- **Subscribe to broadcasts.** Streamer.bot 1.x only delivers
  `WebsocketBroadcastJson` payloads to clients *subscribed* to the
  `General → Custom` event, wrapped in an event envelope — a plain connected
  (even authenticated) client receives nothing. The overlay now subscribes
  after connecting and unwraps the envelope, while still accepting the raw
  payloads older Streamer.bot versions send to everyone.
- The status badge now shows Streamer.bot's own close reasons verbatim
  (e.g. "Did not receive Authenticate request within 30s") and auth errors
  can no longer fail without surfacing on the badge.
- `install.sb` / the Streamer.bot action are unchanged — just replace
  `overlay.html` and refresh the browser source.

## v1.1.0 — 2026-06-05

Overlay settings now live in a friendly `config.js` instead of URL query strings.

- **New `config.js`** — sits next to `overlay.html`; every setting (port,
  password, host, position, width, volume, test mode) is listed with a
  plain-English comment. Edit in Notepad, save, refresh the browser source.
  No more unticking *Local file* to sneak options into a `file:///` URL.
- URL query options still work and **override** `config.js` (handy for a quick
  `?test=1`). `config.js` is optional — without it the old defaults apply, so
  existing setups keep working unchanged.
- If an edit breaks `config.js` (lost quote/comma), the overlay's status badge
  says so instead of silently falling back to defaults.
- Overlay hint messages (password missing/wrong, test clip) now point at
  `config.js` instead of the source URL.
- `install.sb` / the Streamer.bot action are unchanged — no re-import needed;
  just replace `overlay.html` and add `config.js` next to it.

## v1.0.0 — 2026-06-05

Initial release.

- `!so <channel>` / `!soclip <channel>` — posts a shoutout in chat and plays one
  of the channel's top Twitch clips on stream via an OBS browser-source overlay.
- Same action handles **raids**: add the Twitch → Raid trigger and raiders get
  auto-shoutouts with a clip.
- Clip selection: prefers *featured* clips (falls back to all), skips clips
  longer than 61s (configurable), avoids repeating the previous clip per
  channel, picks randomly from the channel's top 50.
- Overlay: queued playback, avatar + name + last-played-game banner, slide/fade
  animation, position/width/volume URL options, `?test=1` positioning mode,
  WebSocket auto-reconnect, optional Streamer.bot WebSocket authentication, and
  an on-screen badge that says exactly what's wrong when it can't connect.
- Graceful degradation: when a channel has no usable clips (or the video URL
  can't be resolved), the shoutout still happens in chat and the reason is
  logged with a `[SoClip]` prefix.
- Optional native Twitch `/shoutout` (off by default — Twitch rate-limits it).
- Tooling: `install.sb` is generated from source by `tools/build-install-sb.ps1`;
  `tools/compile-check.ps1` compile-verifies the C# without a Streamer.bot install.
