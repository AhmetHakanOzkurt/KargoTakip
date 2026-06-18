$vpsUrl = "http://89.168.66.116/api"
$secret = "KargoProjesiGizliAnahtar2026"
$currentUrl = ""
$logFile = "/app/tunnel_log.txt"
$deadUrls = @()
$lastRestartTime = [DateTime]::MinValue
$failureCount = 0

function Write-Log {
    param([string]$Message, [string]$Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $logMessage -ForegroundColor $Color
    try {
        Add-Content -Path $logFile -Value $logMessage -ErrorAction Stop
    } catch {
        # Ignore if file is locked
    }
}

Write-Log "KargoTakip Self-Healing Tünel Yöneticisi Başlatıldı..." "Cyan"

function Restart-QuickTunnel {
    if ((Get-Date) - $lastRestartTime -lt [TimeSpan]::FromSeconds(30)) {
        Write-Log "Tünel yeni başlatıldı, logların oluşması bekleniyor..." "Yellow"
        return
    }
    Write-Log "Cloudflare Quick Tunnel (TryCloudflare) yeniden başlatılıyor..." "Red"
    docker restart kargotakip-cloudflared-quick-1 | Out-Null
    $global:lastRestartTime = Get-Date
    $global:failureCount = 0
    Start-Sleep -Seconds 10
}

while ($true) {
    # 1. Aşama: Loglardan aktif linki bul
    $logs = docker logs --tail 100 kargotakip-cloudflared-quick-1 2>&1
    $matches = $logs | Select-String -Pattern "https://[a-zA-Z0-9-]+\.trycloudflare\.com" -AllMatches | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value
    
    $validMatches = @()
    if ($matches) {
        $validMatches = @($matches | Where-Object { $deadUrls -notcontains $_ })
    }
    
    $newUrl = $validMatches | Select-Object -Last 1
    
    if ($newUrl) {
        $currentUrl = $newUrl
        
        # Sürekli Gönderim (Heartbeat)
        try {
            $body = @{ url = $currentUrl; secret = $secret } | ConvertTo-Json
            $response = Invoke-RestMethod -Uri $vpsUrl -Method Post -Body $body -ContentType "application/json" -TimeoutSec 5
            Write-Log "VPS API Güncellendi (Heartbeat): $currentUrl" "Green"
        } catch {
            Write-Log "VPS'e ulaşılamadı (API kapalı veya Firewall engeli): $($_.Exception.Message)" "DarkYellow"
        }
        
        # 2. Aşama: Healthcheck
        try {
            $check = Invoke-WebRequest -Uri $currentUrl -Method Get -TimeoutSec 5 -UseBasicParsing
            Write-Log "Healthcheck OK: $currentUrl" "DarkGray"
            $failureCount = 0
        } catch {
            $failureCount++
            Write-Log "Healthcheck BAŞARISIZ ($failureCount/3): $($_.Exception.Message)" "Red"
            if ($failureCount -ge 3) {
                $deadUrls += $currentUrl
                Restart-QuickTunnel
            }
        }
    } else {
        Write-Log "Loglarda geçerli TryCloudflare linki bulunamadı. Bekleniyor..." "DarkYellow"
        Restart-QuickTunnel
    }
    
    Start-Sleep -Seconds 15
}
