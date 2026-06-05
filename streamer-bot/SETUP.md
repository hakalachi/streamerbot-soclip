# Manual setup (without install.sb)

Use this if you'd rather build the action yourself than import the bundled file
(e.g. to see exactly what it does first, or because an import failed).
Otherwise just follow the README — importing `install.sb` does all of this.

## 1. Create the action

1. In Streamer.bot's **Actions** tab: right-click → **Add**.
2. Name: `SoClip - Shoutout`, Group: `SoClip`. OK.

## 2. Add the C# sub-action

1. With the action selected, in the **Sub-Actions** panel: right-click → **Add**
   → **Core** → **C#** → **Execute C# Code**.
2. Delete the template code and paste the entire contents of
   [`shoutout-clip.cs`](shoutout-clip.cs).
3. Click the **References** tab of the code editor and make sure these are
   present (add the missing ones with *Add Reference* — they ship with
   Streamer.bot / Windows):
   - `mscorlib.dll`
   - `System.dll`
   - `System.Core.dll`
   - `System.Net.Http.dll`
   - `Newtonsoft.Json.dll`
4. Click **Compile** — it must say *Compiled Successfully*. Close the editor.

## 3. Add the command

1. **Commands** tab → right-click → **Add**.
2. Name: `so`, Command: `!so` (add `!soclip` on a second line if you want the
   alias), Group: `SoClip Commands`. Enabled, source Twitch.
3. Back on the action: **Triggers** panel → right-click → **Add** → **Core** →
   **Commands** → **Command Triggered** → pick `so`.

## 4. (Optional) raid trigger

**Triggers** panel → right-click → **Add** → **Twitch** → **Raid** → **Raid**.

## 5. Overlay

Continue from **Step 2** of the [README](../readme.md) (WebSocket server +
OBS browser source).
