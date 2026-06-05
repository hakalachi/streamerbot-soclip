# Regenerates install.sb from streamer-bot/shoutout-clip.cs.
#
# Streamer.bot's .sb import format is: base64( "SBAE" + gzip( export JSON ) ).
# The export JSON schema here was taken from a real Streamer.bot 1.0.4 export
# (sibling project spotify-control), so imports look identical to a hand export
# - but this never drifts from the canonical .cs source.
#
# Usage:  powershell -File tools\build-install-sb.ps1 [-Version 1.0.0]

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

# --- embed the C# source (CRLF, UTF-8, base64) ---
$code = [IO.File]::ReadAllText("$root\streamer-bot\shoutout-clip.cs")
$code = ($code -replace "`r`n", "`n") -replace "`n", "`r`n"
$byteCode = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($code))

# Stable ids: re-importing a newer install.sb refers to the same objects.
$actionId    = "7a3c9f41-5b22-4e08-9d35-c6f0a81d2e54"
$triggerId   = "1f6f3a88-4c91-4d2a-b7c3-92e5d10c7b6f"
$subActionId = "c4e8b7d2-91a0-4f3b-8e67-3d5a2c4b9e10"
$commandId   = "5d2e8c3a-7f41-4b96-a8d0-1e63f9b72c85"

$export = [ordered]@{
    meta = [ordered]@{
        name           = "install.sb"
        author         = "404DroidsNotFound"
        version        = $Version
        description    = "SoClip - shoutout with a clip"
        autoRunAction  = $null
        minimumVersion = $null
    }
    data = [ordered]@{
        actions = @(
            [ordered]@{
                id                 = $actionId
                queue              = "00000000-0000-0000-0000-000000000000"
                enabled            = $true
                excludeFromHistory = $false
                excludeFromPending = $false
                name               = "SoClip - Shoutout"
                group              = "SoClip"
                alwaysRun          = $false
                randomAction       = $false
                concurrent         = $false
                triggers           = @(
                    [ordered]@{
                        commandId  = $commandId
                        id         = $triggerId
                        type       = 401      # Command trigger
                        enabled    = $true
                        exclusions = @()
                    }
                )
                subActions         = @(
                    [ordered]@{
                        name                 = ""
                        description          = ""
                        references           = @(
                            "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll",
                            "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.dll",
                            "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Core.dll",
                            "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Net.Http.dll",
                            ".\Newtonsoft.Json.dll"
                        )
                        byteCode             = $byteCode
                        precompile           = $false
                        delayStart           = $false
                        saveResultToVariable = $false
                        saveToVariable       = ""
                        id                   = $subActionId
                        weight               = 0.0
                        type                 = 99999  # Execute C# Code
                        parentId             = ""
                        enabled              = $true
                        index                = 0
                    }
                )
                collapsedGroups    = @()
            }
        )
        queues           = @()
        commands         = @(
            [ordered]@{
                permittedUsers       = @()
                permittedGroups      = @()
                id                   = $commandId
                name                 = "so"
                enabled              = $true
                include              = $false
                mode                 = 0
                command              = "!so`r`n!soclip"
                regexExplicitCapture = $false
                location             = 0
                ignoreBotAccount     = $true
                ignoreInternal       = $true
                sources              = 1      # Twitch
                persistCounter       = $false
                persistUserCounter   = $false
                caseSensitive        = $false
                globalCooldown       = 0
                userCooldown         = 0
                group                = "SoClip Commands"
                grantType            = 0
            }
        )
        websocketServers = @()
        websocketClients = @()
        timers           = @()
    }
    version        = 23
    exportedFrom   = "1.0.4"
    minimumVersion = "1.0.0-alpha.1"
}

$json = $export | ConvertTo-Json -Depth 12 -Compress

# --- wrap: "SBAE" + gzip, then base64 ---
$jsonBytes = [Text.Encoding]::UTF8.GetBytes($json)
$ms = New-Object IO.MemoryStream
$gz = New-Object IO.Compression.GzipStream($ms, [IO.Compression.CompressionMode]::Compress)
$gz.Write($jsonBytes, 0, $jsonBytes.Length)
$gz.Close()
$payload = [byte[]]([Text.Encoding]::ASCII.GetBytes("SBAE") + $ms.ToArray())

$out = Join-Path $root "install.sb"
[IO.File]::WriteAllText($out, [Convert]::ToBase64String($payload))
Write-Host "Wrote $out (v$Version, $((Get-Item $out).Length) chars)" -ForegroundColor Green
