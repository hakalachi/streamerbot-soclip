/* ===================================================================
   SoClip overlay settings
   ===================================================================
   Edit a value below, save this file, then refresh the source in OBS:
   right-click the SoClip source -> Properties -> "Refresh cache of
   current page".

   Keep this file in the SAME FOLDER as overlay.html.
   =================================================================== */

window.SOCLIP = {

  /* ---- connecting to Streamer.bot --------------------------------
     Find these in Streamer.bot under Servers/Clients -> WebSocket
     Server. Only change them if yours are different. */

  port: 8080,          // the WebSocket server's port
  password: "",        // only if "Authentication" is ticked -- put the password between the quotes
  host: "127.0.0.1",   // only if Streamer.bot runs on a different computer

  /* ---- how the clip card looks ------------------------------------ */

  position: "center",  // center, top, bottom, top-left, top-right, bottom-left, bottom-right
  width: 480,          // card width in pixels (240-1280)
  volume: 0.85,        // clip volume: 0.0 (mute) to 1.0 (full)

  /* ---- branding -----------------------------------------------------
     Match the card to your stream's look. accent takes any CSS color;
     font takes any Google Font name (it loads automatically -- browse
     fonts.google.com) or the name of a font installed on your PC. */

  accent: "#9146ff",   // frame + name color, e.g. "#ff4757", "gold", "rgb(80,200,120)"
  font: "",            // e.g. "Poppins", "Bangers", "Comic Neue" -- empty = standard font

  /* ---- positioning helper ------------------------------------------
     Set to true and a fake clip plays on a loop, so you can move and
     resize the card without spamming your chat. Set it back to false
     (and refresh) when you're done. */

  test: false

};
