# Compile-checks streamer-bot/shoutout-clip.cs without a Streamer.bot install.
#
# Streamer.bot compiles the script with `CPH` provided by its host (the class is
# rewritten to inherit CPHInlineBase). We mimic that here with a stub declaring
# exactly the CPH members the script is allowed to use - so this catches typos,
# C#-version slips, and accidental use of unverified CPH methods.
#
# Uses the .NET Framework 4.x csc (C# 5 only) on purpose: the script promises
# to avoid newer syntax, and this enforces it.
#
# Usage:  powershell -File tools\compile-check.ps1 [-NewtonsoftDll <path>]

param(
    [string]$NewtonsoftDll = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$csc  = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) { throw "csc.exe not found at $csc" }

if (-not $NewtonsoftDll) {
    # Any Newtonsoft.Json.dll on the machine will do for a compile check.
    $candidates = @(
        "$env:USERPROFILE\.nuget\packages\newtonsoft.json",
        "$env:LOCALAPPDATA\Microsoft\TeamsMeetingAdd-in"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) {
            $hit = Get-ChildItem $c -Recurse -Filter "Newtonsoft.Json.dll" -ErrorAction SilentlyContinue |
                Select-Object -First 1
            if ($hit) { $NewtonsoftDll = $hit.FullName; break }
        }
    }
}
if (-not $NewtonsoftDll -or -not (Test-Path $NewtonsoftDll)) {
    throw "Newtonsoft.Json.dll not found - pass -NewtonsoftDll with a path to one."
}

$work = Join-Path $env:TEMP "soclip-compile-check"
New-Item -ItemType Directory -Force $work | Out-Null

# Stub of the CPH surface the script is permitted to call. If the script grows a
# new CPH call, add it here ONLY after verifying it exists in Streamer.bot.
@'
public abstract class CPHInlineBase
{
    public IInlineInvokeProxy CPH;
}

public interface IInlineInvokeProxy
{
    string TwitchOAuthToken { get; }
    string TwitchClientId { get; }
    bool TryGetArg(string argName, out string value);
    void SendMessage(string message, bool useBot = true, bool fallback = true);
    void LogError(string message);
    void LogInfo(string message);
    void LogDebug(string message);
    T GetGlobalVar<T>(string varName, bool persisted = true);
    void SetGlobalVar(string varName, object value, bool persisted = true);
    void WebsocketBroadcastJson(string data);
}
'@ | Out-File "$work\cph-stub.cs" -Encoding utf8

# Streamer.bot injects the base class; replicate that for the check.
$src = [IO.File]::ReadAllText("$root\streamer-bot\shoutout-clip.cs")
$patched = $src -replace 'public class CPHInline\b', 'public class CPHInline : CPHInlineBase'
if ($patched -eq $src) { throw "Did not find 'public class CPHInline' to patch." }
[IO.File]::WriteAllText("$work\shoutout-clip.cs", $patched)

& $csc /nologo /t:library /out:"$work\check.dll" `
    /r:System.dll /r:System.Core.dll /r:System.Net.Http.dll /r:"$NewtonsoftDll" `
    "$work\shoutout-clip.cs" "$work\cph-stub.cs"
if ($LASTEXITCODE -ne 0) { throw "Compile check FAILED." }
Write-Host "Compile check passed (csc C#5 + CPH stub)." -ForegroundColor Green
