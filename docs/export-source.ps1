# Script kendi konumundan yola çıkarak proje kökünü bulur
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $scriptDir
$out = "$scriptDir\PROJE_SOURCE_DUMP.txt"

$exts = @(".cs", ".xaml", ".csproj", ".slnx", ".json", ".config", ".editorconfig")
$excludeFolders = @("bin", "obj", ".vs", ".git", "logs", "Migrations", "docs")
$excludeFiles = @("PROJE_SOURCE_DUMP.txt", "export-source.ps1")

$files = Get-ChildItem -Path $root -Recurse -File | Where-Object {
    $rel = $_.FullName.Substring($root.Length + 1)
    if ($exts -notcontains $_.Extension) { return $false }
    foreach ($f in $excludeFolders) {
        if ($rel -like "*\$f\*" -or $rel -like "$f\*") { return $false }
    }
    if ($excludeFiles -contains $_.Name) { return $false }
    return $true
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("=" * 70)
[void]$sb.AppendLine("MUHASEBE ERP - KAYNAK KOD DUMP")
[void]$sb.AppendLine("Tarih: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$sb.AppendLine("Toplam Dosya: $($files.Count)")
[void]$sb.AppendLine("=" * 70)
[void]$sb.AppendLine("")

$i = 0
foreach ($file in $files | Sort-Object FullName) {
    $i++
    $rel = $file.FullName.Substring($root.Length + 1)
    Write-Host "[$i/$($files.Count)] $rel"
    [void]$sb.AppendLine("=" * 70)
    [void]$sb.AppendLine("=== FILE: $rel ===")
    [void]$sb.AppendLine("=" * 70)
    [void]$sb.AppendLine("")
    try {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        [void]$sb.AppendLine($content)
    }
    catch {
        [void]$sb.AppendLine("[HATA: $($_.Exception.Message)]")
    }
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("")
}

$sb.ToString() | Out-File -FilePath $out -Encoding UTF8

$sizeMB = [math]::Round((Get-Item $out).Length / 1MB, 2)
Write-Host ""
Write-Host "TAMAMLANDI: $out" -ForegroundColor Green
Write-Host "Boyut: $sizeMB MB" -ForegroundColor Green
Write-Host "Dosya sayisi: $($files.Count)" -ForegroundColor Green