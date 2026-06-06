# Changelog

All notable changes to this project. Versioning follows [SemVer](https://semver.org/).

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
