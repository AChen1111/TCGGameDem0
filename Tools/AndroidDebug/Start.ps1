$ErrorActionPreference = 'Stop'
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding = $OutputEncoding
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
Set-Location -LiteralPath $projectRoot

$candidates = @()
if ($env:ACHEN_PYTHON) {
    $candidates += $env:ACHEN_PYTHON
} else {
    foreach ($name in @('py.exe', 'python.exe')) {
        $application = Get-Command $name -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($application -and $application.Source -notlike '*\Microsoft\WindowsApps\*') {
            $candidates += $application.Source
        }
    }
    $candidates += Join-Path $env:SystemRoot 'py.exe'
    foreach ($installRoot in @("$env:LOCALAPPDATA\Programs\Python", "$env:LOCALAPPDATA\Python")) {
        if (Test-Path -LiteralPath $installRoot) {
            $candidates += Get-ChildItem -LiteralPath $installRoot -Directory |
                Sort-Object Name -Descending | ForEach-Object { Join-Path $_.FullName 'python.exe' }
        }
    }
}

foreach ($candidate in ($candidates | Select-Object -Unique)) {
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) { continue }
    $prefix = @()
    if ([System.IO.Path]::GetFileName($candidate) -ieq 'py.exe') { $prefix = @('-3') }
    try {
        & $candidate @prefix -c 'import sys; sys.exit(0 if sys.version_info >= (3, 10) else 1)' 2>$null
        if ($LASTEXITCODE -ne 0) { continue }
    } catch {
        continue
    }
    & $candidate @prefix -X utf8 (Join-Path $PSScriptRoot 'main.py') @args
    exit $LASTEXITCODE
}
Write-Host 'Python 3.10+ not found. Install Python or set ACHEN_PYTHON.' -ForegroundColor Red
exit 1
