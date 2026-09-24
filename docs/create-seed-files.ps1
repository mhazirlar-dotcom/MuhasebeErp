# =============================================================
# create-seed-files.ps1
# Amaç: Eksik 9 seed JSON dosyasını oluşturur
# Kullanım: & ".\docs\create-seed-files.ps1"
# =============================================================

$ErrorActionPreference = "Stop"

# Çalışma dizini
$projectRoot = Split-Path -Parent $PSScriptRoot
$dataFolder = Join-Path $projectRoot "src\Core\Accounting.Core.DataAccess\Seeds\Data"

# Klasör var mı kontrol
if (-not (Test-Path $dataFolder))
{
    Write-Host "HATA: Klasör bulunamadı: $dataFolder" -ForegroundColor Red
    exit 1
}

Write-Host "Seed dosyaları oluşturuluyor..." -ForegroundColor Cyan
Write-Host "Klasör: $dataFolder" -ForegroundColor Gray
Write-Host ""

# =============================================================
# 1. accounting-methods.json
# =============================================================
$accountingMethods = @'
[
  {
    "code": "ISLETME",
    "name": "İşletme Defteri"
  },
  {
    "code": "SERBEST_MESLEK",
    "name": "Serbest Meslek Defteri"
  },
  {
    "code": "BILANCO",
    "name": "Bilanço Esası"
  }
]
'@

# =============================================================
# 2. currencies.json
# =============================================================
$currencies = @'
[
  {
    "code": "TRY",
    "name": "Türk Lirası"
  },
  {
    "code": "USD",
    "name": "Amerikan Doları"
  },
  {
    "code": "EUR",
    "name": "Euro"
  },
  {
    "code": "GBP",
    "name": "İngiliz Sterlini"
  }
]
'@

# =============================================================
# 3. declaration-types.json
# =============================================================
$declarationTypes = @'
[
  {
    "code": "AYLIK",
    "name": "Aylık"
  },
  {
    "code": "UC_AYLIK",
    "name": "Üç Aylık"
  },
  {
    "code": "YILLIK",
    "name": "Yıllık"
  }
]
'@

# =============================================================
# 4. exchange-rate-modes.json
# =============================================================
$exchangeRateModes = @'
[
  {
    "code": "TCMB_ALIS",
    "name": "TCMB Alış"
  },
  {
    "code": "TCMB_SATIS",
    "name": "TCMB Satış"
  },
  {
    "code": "TCMB_EFEKTIF",
    "name": "TCMB Efektif"
  },
  {
    "code": "SERBEST",
    "name": "Serbest Piyasa"
  }
]
'@

# =============================================================
# 5. ledger-types.json
# =============================================================
$ledgerTypes = @'
[
  {
    "code": "ISLETME",
    "name": "İşletme Defteri"
  },
  {
    "code": "SERBEST_MESLEK",
    "name": "Serbest Meslek Kazanç Defteri"
  },
  {
    "code": "BASIT",
    "name": "Basit Usul"
  }
]
'@

# =============================================================
# 6. month-names.json
# =============================================================
$monthNames = @'
[
  {
    "code": "01",
    "name": "Ocak"
  },
  {
    "code": "02",
    "name": "Şubat"
  },
  {
    "code": "03",
    "name": "Mart"
  },
  {
    "code": "04",
    "name": "Nisan"
  },
  {
    "code": "05",
    "name": "Mayıs"
  },
  {
    "code": "06",
    "name": "Haziran"
  },
  {
    "code": "07",
    "name": "Temmuz"
  },
  {
    "code": "08",
    "name": "Ağustos"
  },
  {
    "code": "09",
    "name": "Eylül"
  },
  {
    "code": "10",
    "name": "Ekim"
  },
  {
    "code": "11",
    "name": "Kasım"
  },
  {
    "code": "12",
    "name": "Aralık"
  }
]
'@

# =============================================================
# 7. vat-rates.json
# =============================================================
$vatRates = @'
[
  {
    "code": "0",
    "name": "%0"
  },
  {
    "code": "1",
    "name": "%1"
  },
  {
    "code": "8",
    "name": "%8"
  },
  {
    "code": "10",
    "name": "%10"
  },
  {
    "code": "18",
    "name": "%18"
  },
  {
    "code": "20",
    "name": "%20"
  }
]
'@

# =============================================================
# 8. voucher-number-lengths.json
# =============================================================
$voucherNumberLengths = @'
[
  {
    "code": "3",
    "name": "3 Hane"
  },
  {
    "code": "4",
    "name": "4 Hane"
  },
  {
    "code": "5",
    "name": "5 Hane"
  },
  {
    "code": "6",
    "name": "6 Hane"
  },
  {
    "code": "7",
    "name": "7 Hane"
  },
  {
    "code": "8",
    "name": "8 Hane"
  }
]
'@

# =============================================================
# 9. voucher-sort-modes.json
# =============================================================
$voucherSortModes = @'
[
  {
    "code": "TARIH",
    "name": "Tarihe Göre"
  },
  {
    "code": "FIS_NO",
    "name": "Fiş Numarasına Göre"
  },
  {
    "code": "EVRAK_NO",
    "name": "Evrak Numarasına Göre"
  }
]
'@

# =============================================================
# Dosyaları yaz
# =============================================================

$files = @{
    "accounting-methods.json"       = $accountingMethods
    "currencies.json"               = $currencies
    "declaration-types.json"        = $declarationTypes
    "exchange-rate-modes.json"      = $exchangeRateModes
    "ledger-types.json"             = $ledgerTypes
    "month-names.json"              = $monthNames
    "vat-rates.json"                = $vatRates
    "voucher-number-lengths.json"   = $voucherNumberLengths
    "voucher-sort-modes.json"       = $voucherSortModes
}

$createdCount = 0
$skippedCount = 0

foreach ($kvp in $files.GetEnumerator())
{
    $filePath = Join-Path $dataFolder $kvp.Key

    if (Test-Path $filePath)
    {
        Write-Host "  ATLANDI (zaten var): $($kvp.Key)" -ForegroundColor Yellow
        $skippedCount++
        continue
    }

    $content = $kvp.Value.TrimStart("`r", "`n")
    Set-Content -Path $filePath -Value $content -Encoding UTF8 -NoNewline

    Write-Host "  OLUŞTURULDU: $($kvp.Key)" -ForegroundColor Green
    $createdCount++
}

Write-Host ""
Write-Host "Özet:" -ForegroundColor Cyan
Write-Host "  Oluşturulan: $createdCount" -ForegroundColor Green
Write-Host "  Atlanan:     $skippedCount" -ForegroundColor Yellow
Write-Host ""

if ($createdCount -gt 0)
{
    Write-Host "Sonraki adımlar:" -ForegroundColor Cyan
    Write-Host "  1. dotnet build" -ForegroundColor White
    Write-Host "  2. API'yi yeniden başlat (seed çalışacak)" -ForegroundColor White
}