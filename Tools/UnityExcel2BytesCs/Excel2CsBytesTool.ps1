# 基于原 Excel2CsBytesTool 的字段定义 -> C# / bytes 流程改造.
# CSV 由 schema 提供原 XLSX 的字段类型和说明, 不再依赖生成类型的编译结果.
param([string]$SchemaPath = (Join-Path $PSScriptRoot 'Localization.schema.json'))
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$utf8 = [Text.UTF8Encoding]::new($false, $true)
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$schemaFile = (Resolve-Path -LiteralPath $SchemaPath).Path
$schema = [IO.File]::ReadAllText($schemaFile, $utf8) | ConvertFrom-Json

function Resolve-ProjectPath([string]$RelativePath) {
    $resolved = [IO.Path]::GetFullPath((Join-Path $projectRoot $RelativePath))
    if (-not $resolved.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "路径超出项目目录: $RelativePath"
    }
    return $resolved
}

$source = Resolve-ProjectPath $schema.source
$bytesPath = Resolve-ProjectPath $schema.bytesOutput
$csPath = Resolve-ProjectPath $schema.csOutput
$className = [string]$schema.className
$namespace = [string]$schema.namespace
if ($className -cnotmatch '^[A-Z][A-Za-z0-9_]*$' -or $namespace -cnotmatch '^[A-Z][A-Za-z0-9_]*(\.[A-Z][A-Za-z0-9_]*)*$') {
    throw '类型名和命名空间必须是大写字母开头的 C# 标识符.'
}
$columns = @($schema.columns)
$supported = @('string', 'int', 'bool', 'string[]', 'int[]', 'bool[]')
$fieldNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($column in $columns) {
    if ($column.name -cnotmatch '^[A-Z][A-Za-z0-9_]*$' -or -not $fieldNames.Add($column.name)) { throw "字段名无效或重复: $($column.name)" }
    if ($column.type -cnotin $supported) { throw "暂不支持字段类型: $($column.type)" }
}
if (-not $fieldNames.Contains($schema.uniqueKey)) { throw 'uniqueKey 未对应到字段.' }

# 加载系统自带的 CSV 读取器, 支持带引号的逗号、双引号和多行内容.
Add-Type -AssemblyName Microsoft.VisualBasic
$parser = [Microsoft.VisualBasic.FileIO.TextFieldParser]::new($source, $utf8, $true)
$parser.TextFieldType = [Microsoft.VisualBasic.FileIO.FieldType]::Delimited
$parser.SetDelimiters(',')
$parser.HasFieldsEnclosedInQuotes = $true
$parser.TrimWhiteSpace = $false
$rows = [Collections.Generic.List[object]]::new()
$keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
try {
    $headers = $parser.ReadFields()
    if ($null -eq $headers -or $headers.Length -ne $columns.Count) { throw 'CSV 表头列数与 schema 不符.' }
    for ($i = 0; $i -lt $columns.Count; $i++) {
        if ($headers[$i] -cne $columns[$i].header) { throw "CSV 表头不符: 预期 $($columns[$i].header), 实际 $($headers[$i])" }
    }
    while (-not $parser.EndOfData) {
        $line = $parser.LineNumber
        $cells = $parser.ReadFields()
        if ($cells.Length -ne $columns.Count) { throw "CSV 第 $line 行列数错误." }
        $record = @{}
        for ($i = 0; $i -lt $columns.Count; $i++) {
            $record[$columns[$i].name] = $cells[$i].Replace("`r`n", "`n").Replace('<br>', "`n")
        }
        $key = [string]$record[$schema.uniqueKey]
        if ([string]::IsNullOrWhiteSpace($key) -or -not $keys.Add($key)) { throw "CSV 第 $line 行 key 为空或重复: $key" }
        $expected = $null
        foreach ($field in $schema.matchingPlaceholders) {
            $value = [string]$record[$field]
            if ([string]::IsNullOrEmpty($value)) { continue }
            $parameters = (@([regex]::Matches($value, '\{([A-Za-z][A-Za-z0-9_]*)\}') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique) -join ',')
            if ($null -ne $expected -and $parameters -cne $expected) { throw "占位符不匹配: key=$key, field=$field" }
            $expected = $parameters
        }
        $rows.Add($record)
    }
} finally { $parser.Dispose() }

$sha = [Security.Cryptography.SHA256]::Create()
try { $fingerprint = $sha.ComputeHash([IO.File]::ReadAllBytes($schemaFile)) } finally { $sha.Dispose() }
$fingerprintHex = ([BitConverter]::ToString($fingerprint)).Replace('-', '').ToLowerInvariant()
$buffer = [IO.MemoryStream]::new()
$writer = [IO.BinaryWriter]::new($buffer, $utf8, $true)
function Write-Value([string]$Type, [string]$Value) {
    switch -CaseSensitive ($Type) {
        'string' { $writer.Write([string]$Value) }
        'int' { $writer.Write([int]::Parse($Value, [Globalization.CultureInfo]::InvariantCulture)) }
        'bool' { $writer.Write([bool]::Parse($Value)) }
        default { throw "无法写入类型: $Type" }
    }
}
try {
    $writer.Write([int]0x31425445) # ETB1, 显式二进制格式版本.
    $writer.Write([byte[]]$fingerprint)
    $writer.Write([int]$rows.Count)
    foreach ($record in $rows) {
        foreach ($column in $columns) {
            $value = [string]$record[$column.name]
            if ($column.type.EndsWith('[]')) {
                $items = @()
                if ($value.Length -gt 0) { $items = $value.Split('#') }
                $writer.Write([int]$items.Count)
                foreach ($item in $items) { Write-Value $column.type.Replace('[]', '') $item }
            } else { Write-Value $column.type $value }
        }
    }
    $writer.Flush()
    $bytes = $buffer.ToArray()
} finally { $writer.Dispose(); $buffer.Dispose() }

$code = [Text.StringBuilder]::new()
[void]$code.AppendLine('// <auto-generated> 由 Tools/UnityExcel2BytesCs/Excel2CsBytesTool.ps1 生成, 请勿手改. </auto-generated>')
[void]$code.AppendLine('using System;')
[void]$code.AppendLine('using System.Collections.Generic;')
[void]$code.AppendLine('using System.IO;')
[void]$code.AppendLine('using System.Text;')
[void]$code.AppendLine("namespace $namespace")
[void]$code.AppendLine('{')
[void]$code.AppendLine("    public sealed class $className")
[void]$code.AppendLine('    {')
foreach ($column in $columns) {
    $description = ([string]$column.description).Replace("`r", ' ').Replace("`n", ' ')
    [void]$code.AppendLine("        // $description")
    [void]$code.AppendLine("        public $($column.type) $($column.name) { get; private set; }")
}
[void]$code.AppendLine("        public static List<$className> LoadBytes(byte[] data)")
[void]$code.AppendLine('        {')
[void]$code.AppendLine('            using (var stream = new MemoryStream(data, false))')
[void]$code.AppendLine('            using (var reader = new BinaryReader(stream, new UTF8Encoding(false, true)))')
[void]$code.AppendLine('            {')
[void]$code.AppendLine('                if (reader.ReadInt32() != 0x31425445) throw new InvalidDataException("Invalid table format.");')
[void]$code.AppendLine("                if (BitConverter.ToString(reader.ReadBytes(32)).Replace(`"-`", `"`").ToLowerInvariant() != `"$fingerprintHex`")")
[void]$code.AppendLine('                    throw new InvalidDataException("Table schema mismatch. Export bytes and C# together.");')
[void]$code.AppendLine('                int count = reader.ReadInt32();')
[void]$code.AppendLine('                if (count < 0 || count > stream.Length - stream.Position) throw new InvalidDataException("Invalid row count.");')
[void]$code.AppendLine("                var rows = new List<$className>(count);")
[void]$code.AppendLine('                for (int i = 0; i < count; i++)')
[void]$code.AppendLine('                {')
[void]$code.AppendLine("                    var row = new $className();")
$readers = @{ 'string'='ReadString'; 'int'='ReadInt32'; 'bool'='ReadBoolean' }
foreach ($column in $columns) {
    $field = $column.name
    if ($column.type.EndsWith('[]')) {
        $elementType = $column.type.Replace('[]', '')
        $method = $readers[$elementType]
        [void]$code.AppendLine("                    int count$field = reader.ReadInt32();")
        [void]$code.AppendLine("                    if (count$field < 0 || count$field > stream.Length - stream.Position) throw new InvalidDataException(`"Invalid array length.`");")
        [void]$code.AppendLine("                    row.$field = new $elementType[count$field];")
        [void]$code.AppendLine("                    for (int j = 0; j < count$field; j++) row.$field[j] = reader.$method();")
    } else {
        $method = $readers[$column.type]
        [void]$code.AppendLine("                    row.$field = reader.$method();")
    }
}
[void]$code.AppendLine('                    rows.Add(row);')
[void]$code.AppendLine('                }')
[void]$code.AppendLine('                if (stream.Position != stream.Length) throw new InvalidDataException("Unexpected trailing table data.");')
[void]$code.AppendLine('                return rows;')
[void]$code.AppendLine('            }')
[void]$code.AppendLine('        }')
[void]$code.AppendLine('    }')
[void]$code.AppendLine('}')

# 所有读取、验证与序列化成功后才替换产物, 数据不合法时保留上次有效导出.
foreach ($path in @($bytesPath, $csPath)) { [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) }
$codeText = $code.ToString().Replace("`r`n", "`n")
[IO.File]::WriteAllBytes($bytesPath + '.tmp', $bytes)
[IO.File]::WriteAllText($csPath + '.tmp', $codeText, $utf8)
Move-Item -LiteralPath ($bytesPath + '.tmp') -Destination $bytesPath -Force
if (-not [IO.File]::Exists($csPath) -or [IO.File]::ReadAllText($csPath) -cne $codeText) {
    Move-Item -LiteralPath ($csPath + '.tmp') -Destination $csPath -Force
} else { Remove-Item -LiteralPath ($csPath + '.tmp') }
Write-Output "导表完成. Table=$className; Rows=$($rows.Count); Bytes=$($bytes.Length); Output=$($schema.bytesOutput)"
