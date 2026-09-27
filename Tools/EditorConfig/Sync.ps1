param([string]$BackendUrl = 'http://127.0.0.1:5080')
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$uri = [Uri]$BackendUrl
if (-not $uri.IsLoopback -or $uri.Scheme -notin @('http','https')) {
    throw 'This tool only publishes Editor configuration to a local backend.'
}
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Get-BytesHash([byte[]]$Bytes) {
    $hash = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($hash.ComputeHash($Bytes))).Replace('-','').ToLowerInvariant() }
    finally { $hash.Dispose() }
}
$source = Join-Path $projectRoot 'Assets/GameConfiguration'
[string[]]$paths = @(Get-ChildItem -LiteralPath $source -Filter '*.bytes' -File | ForEach-Object FullName)
if ($paths.Count -eq 0) { throw 'No generated configuration. Run Unity Tools/AddToBytes first.' }
[Array]::Sort($paths, [StringComparer]::Ordinal)
$configs = @()
$files = @()
$output = Join-Path $projectRoot 'Temp/EditorConfig'
[IO.Directory]::CreateDirectory($output) | Out-Null
$archivePath = Join-Path $output (([Guid]::NewGuid().ToString('N')) + '.zip')
$zip = [IO.Compression.ZipFile]::Open($archivePath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($path in $paths) {
        $name = [IO.Path]::GetFileNameWithoutExtension($path)
        $relative = 'GameConfig/' + $name + '.bytes'
        $bytes = [IO.File]::ReadAllBytes($path)
        $sha = Get-BytesHash $bytes
        $configs += @{category=$name; address=('GameConfig/' + $name); format='bytes'; path=$relative; size=$bytes.LongLength; sha256=$sha}
        $files += @{path=$relative; size=$bytes.LongLength; sha256=$sha}
        $stream = $zip.CreateEntry($relative).Open()
        try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
    }
    $configHash = Get-BytesHash ([Text.Encoding]::UTF8.GetBytes((($configs | ForEach-Object { $_.category + ':' + $_.sha256 }) -join "`n")))
    $manifest = @{schemaVersion=5; platform='Editor'; configHash=$configHash; configs=$configs; files=$files}
    $bytes = [Text.Encoding]::UTF8.GetBytes(($manifest | ConvertTo-Json -Depth 8))
    $stream = $zip.CreateEntry('manifest.json').Open()
    try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
} finally { $zip.Dispose() }
Write-Host ('Editor tables: ' + $configs.Count)
Write-Host ('Config hash: ' + $configHash)
$headers = @{'X-Artifact-Sha256'=(Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()}
if ($env:ACHEN_CONTENT_PUBLISH_KEY) { $headers['X-Content-Publish-Key'] = $env:ACHEN_CONTENT_PUBLISH_KEY }
$result = Invoke-RestMethod -Uri ($BackendUrl.TrimEnd('/') + '/api/dev/editor-config') -Method Put -Headers $headers -ContentType 'application/zip' -InFile $archivePath
if ($result.configHash -ne $configHash) { throw 'Backend returned a different configuration hash.' }
Write-Host 'Editor configuration synchronized. Start Unity Play Mode again.' -ForegroundColor Green
