param(
    [string]$ApiBase = "http://127.0.0.1:5080",
    [string]$Target = "Editor",
    [Parameter(Mandatory = $true)][string]$PublishKey
)
$ErrorActionPreference = "Stop"
$headers = @{ "X-Content-Publish-Key" = $PublishKey }
$endpoint = $ApiBase.TrimEnd('/') + "/api/admin/activities"
$catalog = Invoke-RestMethod -Uri ($endpoint + "/translations/" + $Target) -Headers $headers
$examples = Get-Content -LiteralPath (Join-Path $PSScriptRoot "examples.json") -Raw -Encoding utf8 | ConvertFrom-Json
$gifts = @(Invoke-RestMethod -Uri ($endpoint + "/gifts") -Headers $headers)
$activities = @(Invoke-RestMethod -Uri ($endpoint + "/") -Headers $headers)
foreach ($gift in $examples.gifts) {
    if ($gifts.id -contains $gift.id) { continue }
    $body = @{ definition = $gift; expectedVersion = 0 } | ConvertTo-Json -Depth 20
    Invoke-RestMethod -Uri ($endpoint + "/gifts/" + $gift.id) -Method Put -Headers $headers -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) | Out-Null
}
foreach ($definition in $examples.activities) {
    if ($activities.id -contains $definition.id) { continue }
    $body = @{ target = $Target; configHash = $catalog.configHash; expectedVersion = 0; definition = $definition } | ConvertTo-Json -Depth 20
    Invoke-RestMethod -Uri ($endpoint + "/") -Method Post -Headers $headers -ContentType "application/json; charset=utf-8" -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) | Out-Null
}
Write-Output "计划书示例草稿已创建；可在活动管理页发布，先发布国庆见面礼，再发布跳转公告。"
