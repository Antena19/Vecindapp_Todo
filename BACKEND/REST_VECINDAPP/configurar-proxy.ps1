# Script para configurar y verificar proxy del sistema
Write-Host "=== Configuración de Proxy del Sistema ===" -ForegroundColor Green
Write-Host "Fecha: $(Get-Date)" -ForegroundColor Cyan

# Función para mostrar configuración actual de proxy
function Show-ProxyConfiguration {
    Write-Host "`n=== Configuración Actual de Proxy ===" -ForegroundColor Yellow
    
    try {
        # Verificar configuración de proxy del sistema
        $proxy = [System.Net.WebRequest]::GetSystemWebProxy()
        $proxyUri = $proxy.GetProxy("https://webpay3gint.transbank.cl")
        
        Write-Host "Proxy del sistema configurado: $($proxyUri)" -ForegroundColor White
        
        # Verificar configuración de Internet Explorer (Windows)
        $ieSettings = Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings" -ErrorAction SilentlyContinue
        
        if ($ieSettings) {
            Write-Host "Proxy habilitado en IE: $($ieSettings.ProxyEnable)" -ForegroundColor White
            Write-Host "Servidor proxy: $($ieSettings.ProxyServer)" -ForegroundColor White
            Write-Host "Bypass para local: $($ieSettings.ProxyOverride)" -ForegroundColor White
        }
        
        # Verificar variables de entorno
        $httpProxy = $env:HTTP_PROXY
        $httpsProxy = $env:HTTPS_PROXY
        $noProxy = $env:NO_PROXY
        
        Write-Host "HTTP_PROXY: $httpProxy" -ForegroundColor White
        Write-Host "HTTPS_PROXY: $httpsProxy" -ForegroundColor White
        Write-Host "NO_PROXY: $noProxy" -ForegroundColor White
        
    } catch {
        Write-Host "Error al obtener configuración de proxy: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Función para configurar proxy manualmente
function Set-ProxyConfiguration {
    param(
        [string]$ProxyServer,
        [string]$BypassList = ""
    )
    
    Write-Host "`n=== Configurando Proxy ===" -ForegroundColor Yellow
    
    try {
        # Configurar proxy en registro de Windows
        Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings" -Name "ProxyEnable" -Value 1
        Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings" -Name "ProxyServer" -Value $ProxyServer
        
        if ($BypassList) {
            Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings" -Name "ProxyOverride" -Value $BypassList
        }
        
        Write-Host "Proxy configurado exitosamente: $ProxyServer" -ForegroundColor Green
        
        # Configurar variables de entorno
        $env:HTTP_PROXY = "http://$ProxyServer"
        $env:HTTPS_PROXY = "http://$ProxyServer"
        
        Write-Host "Variables de entorno configuradas" -ForegroundColor Green
        
    } catch {
        Write-Host "Error al configurar proxy: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Función para deshabilitar proxy
function Disable-ProxyConfiguration {
    Write-Host "`n=== Deshabilitando Proxy ===" -ForegroundColor Yellow
    
    try {
        # Deshabilitar proxy en registro de Windows
        Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings" -Name "ProxyEnable" -Value 0
        
        # Limpiar variables de entorno
        $env:HTTP_PROXY = $null
        $env:HTTPS_PROXY = $null
        
        Write-Host "Proxy deshabilitado exitosamente" -ForegroundColor Green
        
    } catch {
        Write-Host "Error al deshabilitar proxy: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Función para probar conectividad con proxy
function Test-ProxyConnectivity {
    Write-Host "`n=== Probando Conectividad con Proxy ===" -ForegroundColor Yellow
    
    try {
        $proxy = [System.Net.WebRequest]::GetSystemWebProxy()
        $proxyUri = $proxy.GetProxy("https://webpay3gint.transbank.cl")
        
        Write-Host "Proxy detectado: $proxyUri" -ForegroundColor White
        
        # Probar conectividad básica
        $response = Invoke-WebRequest -Uri "https://webpay3gint.transbank.cl" -TimeoutSec 30 -UseBasicParsing
        Write-Host "Conectividad exitosa con proxy - Status: $($response.StatusCode)" -ForegroundColor Green
        return $true
        
    } catch {
        Write-Host "Error de conectividad con proxy: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Mostrar configuración actual
Show-ProxyConfiguration

# Menú de opciones
Write-Host "`n=== Opciones ===" -ForegroundColor Cyan
Write-Host "1. Mostrar configuración actual" -ForegroundColor White
Write-Host "2. Configurar proxy manualmente" -ForegroundColor White
Write-Host "3. Deshabilitar proxy" -ForegroundColor White
Write-Host "4. Probar conectividad con proxy" -ForegroundColor White
Write-Host "5. Salir" -ForegroundColor White

$opcion = Read-Host "`nSeleccione una opción (1-5)"

switch ($opcion) {
    "1" {
        Show-ProxyConfiguration
    }
    "2" {
        $proxyServer = Read-Host "Ingrese el servidor proxy (ej: proxy.corporativo.com:8080)"
        $bypassList = Read-Host "Ingrese lista de bypass (opcional, ej: localhost;127.0.0.1)"
        Set-ProxyConfiguration -ProxyServer $proxyServer -BypassList $bypassList
    }
    "3" {
        Disable-ProxyConfiguration
    }
    "4" {
        Test-ProxyConnectivity
    }
    "5" {
        Write-Host "Saliendo..." -ForegroundColor Green
        exit
    }
    default {
        Write-Host "Opción inválida" -ForegroundColor Red
    }
}

Write-Host "`n=== Fin del Script ===" -ForegroundColor Green 