[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [ValidateRange(10, 1800)]
    [int]$TimeoutSeconds = 300
)

$ErrorActionPreference = 'Stop'

$projectRoot = (Resolve-Path -LiteralPath $ProjectPath).Path
$unityCli = Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
if (-not (Test-Path -LiteralPath $unityCli -PathType Leaf)) {
    $unityCommand = Get-Command unity -ErrorAction SilentlyContinue
    if ($null -eq $unityCommand) {
        throw 'Unity CLI was not found in LOCALAPPDATA or PATH.'
    }
    $unityCli = $unityCommand.Source
}

$baseArguments = @(
    '--format', 'json',
    '--no-banner',
    '--non-interactive',
    '--quiet',
    '--proxy-disable',
    'command',
    '--project-path', $projectRoot
)

function Invoke-UnityCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$CommandArguments,

        [switch]$AllowFailure
    )

    $output = (& $script:unityCli @script:baseArguments @CommandArguments 2>&1 | Out-String).Trim()
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0 -and -not $AllowFailure) {
        throw "Unity CLI failed with exit code $exitCode.`n$output"
    }

    return [PSCustomObject]@{
        ExitCode = $exitCode
        Output = $output
    }
}

function ConvertTo-RecompileStatus {
    param([string]$Json)

    if ([string]::IsNullOrWhiteSpace($Json)) {
        return $null
    }

    try {
        $response = $Json | ConvertFrom-Json
        if ($response.success -ne $true -or $null -eq $response.data.result) {
            return $null
        }

        if ($response.data.result -is [string]) {
            return $response.data.result | ConvertFrom-Json
        }
        return $response.data.result
    }
    catch {
        return $null
    }
}

function Get-FreshRecompileStatus {
    param([datetime]$StartedAt)

    $statusFile = Join-Path $script:projectRoot 'Temp\pipeline_recompile_status.json'
    $response = Invoke-UnityCommand -CommandArguments @('recompile_status') -AllowFailure
    $status = ConvertTo-RecompileStatus -Json $response.Output

    if (Test-Path -LiteralPath $statusFile -PathType Leaf) {
        $statusInfo = Get-Item -LiteralPath $statusFile
        if ($statusInfo.LastWriteTime -ge $StartedAt.AddSeconds(-2)) {
            if ($null -ne $status) {
                return $status
            }

            try {
                return Get-Content -LiteralPath $statusFile -Raw | ConvertFrom-Json
            }
            catch {
                return $null
            }
        }
    }

    return $null
}

function Invoke-UnityCSharpSync {
    $startedAt = Get-Date
    Write-Host "[Unity Sync] Refreshing C# project files for $script:projectRoot"

    Invoke-UnityCommand -CommandArguments @('set_autotick', '--enable', 'true', '--interval_ms', '16') | Out-Null

    $trigger = Invoke-UnityCommand -CommandArguments @('recompile') -AllowFailure
    if ($trigger.ExitCode -ne 0) {
        Write-Warning 'Unity reloaded while handling the request; waiting for the persisted compile status.'
    }

    $deadline = $startedAt.AddSeconds($script:TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $status = Get-FreshRecompileStatus -StartedAt $startedAt
        if ($null -ne $status) {
            if ($status.status -eq 'up_to_date') {
                Write-Host '[Unity Sync] Project files are up to date.'
                return
            }

            if ($status.status -eq 'completed') {
                if ($status.failed -eq $true) {
                    $errors = @($status.errors) -join "`n"
                    throw "Unity compilation failed.`n$errors"
                }

                Write-Host '[Unity Sync] Compilation completed successfully.'
                return
            }
        }

        Start-Sleep -Seconds 1
    }

    throw "Timed out after $script:TimeoutSeconds seconds while waiting for Unity compilation."
}

function Invoke-UnityCSharpSyncLocked {
    $tempDirectory = Join-Path $script:projectRoot 'Temp'
    [System.IO.Directory]::CreateDirectory($tempDirectory) | Out-Null
    $lockPath = Join-Path $tempDirectory 'cursor_unity_csharp_sync.lock'
    $lockDeadline = (Get-Date).AddSeconds(30)
    $lockStream = $null

    while ($null -eq $lockStream -and (Get-Date) -lt $lockDeadline) {
        try {
            $lockStream = [System.IO.File]::Open(
                $lockPath,
                [System.IO.FileMode]::OpenOrCreate,
                [System.IO.FileAccess]::ReadWrite,
                [System.IO.FileShare]::None
            )
        }
        catch [System.IO.IOException] {
            Start-Sleep -Milliseconds 250
        }
    }

    if ($null -eq $lockStream) {
        throw 'Another Unity C# synchronization is still running.'
    }

    try {
        Invoke-UnityCSharpSync
    }
    finally {
        $lockStream.Dispose()
    }
}

Invoke-UnityCSharpSyncLocked
