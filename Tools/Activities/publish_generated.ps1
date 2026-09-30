param(
    [string]$ApiBase = 'http://127.0.0.1:5080',
    [string]$PublishKey = $env:ACHEN_CONTENT_PUBLISH_KEY
)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
if ([string]::IsNullOrWhiteSpace($PublishKey)) {
    $localKey = Join-Path $projectRoot 'Library/Development/publish.key'
    if (Test-Path -LiteralPath $localKey) { $PublishKey = (Get-Content -LiteralPath $localKey -Raw).Trim() }
}
if ([string]::IsNullOrWhiteSpace($PublishKey)) { throw '请设置 ACHEN_CONTENT_PUBLISH_KEY 或提供 PublishKey' }
function Get-FileSha([byte[]]$Bytes) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return [Convert]::ToHexString($sha.ComputeHash($Bytes)).ToLowerInvariant() }
    finally { $sha.Dispose() }
}
$sourceRoot = Join-Path $projectRoot 'ActivityTableData'
$generatedRoot = Join-Path $projectRoot 'Assets/ActivityConfiguration'
$manifest = Get-Content -LiteralPath (Join-Path $generatedRoot 'generated.json') -Raw -Encoding utf8 | ConvertFrom-Json
[string[]]$paths = [IO.Directory]::GetFiles($sourceRoot, '*.csv', [IO.SearchOption]::AllDirectories)
[Array]::Sort($paths, [StringComparer]::Ordinal)
$sourceLines = foreach ($path in $paths) {
    $relative = [IO.Path]::GetRelativePath($projectRoot, $path).Replace('\', '/')
    $relative + ':' + (Get-FileSha ([Text.Encoding]::UTF8.GetBytes([IO.File]::ReadAllText($path).Replace("`r`n", "`n"))))
}
$sourceHash = Get-FileSha ([Text.Encoding]::UTF8.GetBytes(($sourceLines -join "`n")))
if ($sourceHash -ne $manifest.SourceHash) { throw '活动 CSV 自生成后已变化，请先执行 Tools/AddToActBytes' }
$headers = @{ 'X-Content-Publish-Key' = $PublishKey }
$endpoint = $ApiBase.TrimEnd('/') + '/api/admin/activities'
$current = Invoke-RestMethod -Uri $endpoint -Headers $headers
$manifest.ExpectedRevision = if ($null -eq $current.current) { 0 } else { $current.current.revision }
$manifest.ReleaseId = [Guid]::NewGuid().ToString('D')
$memory = [IO.MemoryStream]::new()
try {
    $zip = [IO.Compression.ZipArchive]::new($memory, [IO.Compression.ZipArchiveMode]::Create, $true)
    try {
        foreach ($file in $manifest.Files) {
            if ($file.Table -notmatch '^[a-zA-Z0-9_-]{1,96}$') { throw '生成清单表名无效' }
            $bytes = [IO.File]::ReadAllBytes((Join-Path $generatedRoot ($file.Table + '.bytes')))
            if ($bytes.LongLength -ne $file.Size -or (Get-FileSha $bytes) -ne $file.Sha256) { throw ($file.Table + '.bytes 已变化，请重新生成') }
            $stream = $zip.CreateEntry($file.Table + '.bytes').Open()
            try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
        }
        $stream = $zip.CreateEntry('manifest.json').Open()
        try {
            $bytes = [Text.Encoding]::UTF8.GetBytes(($manifest | ConvertTo-Json -Depth 20))
            $stream.Write($bytes, 0, $bytes.Length)
        } finally { $stream.Dispose() }
    } finally { $zip.Dispose() }
    $result = Invoke-RestMethod -Uri ($endpoint + '/config') -Method Put -Headers $headers -ContentType 'application/zip' -Body $memory.ToArray()
    Write-Output ('已发布全平台活动版本 ' + $result.revision + ' / ' + $result.releaseId)
} finally { $memory.Dispose() }
