param([string]$Command = 'eval', [string]$Code = '', [string]$ArgumentsJson = '{}', [string]$Endpoint = '')
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$taskDescriptor = Get-Content -LiteralPath (Join-Path $taskRoot 'Library/Pipeline/.unity-pipeline-port') -Raw | ConvertFrom-Json
$taskHeaders = @{ Authorization = ('Bearer ' + $taskDescriptor.evalToken) }
$taskUrl = 'http://127.0.0.1:' + $taskDescriptor.port
if ($Endpoint) {
    Invoke-RestMethod -Uri ($taskUrl + '/api/' + $Endpoint) -Headers $taskHeaders -TimeoutSec 55 | ConvertTo-Json -Depth 25
} else {
    $taskArguments = $ArgumentsJson | ConvertFrom-Json -AsHashtable
    if ($Code) { $taskArguments = @{code=$Code; timeout=45000} }
    $taskBody = @{command=$Command; parameters=$taskArguments} | ConvertTo-Json -Depth 20 -Compress
    Invoke-RestMethod -Uri ($taskUrl + '/api/exec') -Method Post -Headers $taskHeaders -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($taskBody)) -TimeoutSec 55 | ConvertTo-Json -Depth 30
}
