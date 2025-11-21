@echo off
:: ============================================================
::   VERIFICATION ADMIN
:: ============================================================
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo 🔐 Relance du script en mode administrateur...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

cd /d "%~dp0"

echo ==========================================
echo      Lancement de Let's Go Biking
echo ==========================================

:: 1. Lancer le Proxy Service
echo [1/3] Demarrage du ProxyService...
start "ProxyService" "LetsGoBiking\ProxyService\bin\Debug\ProxyService.exe"

:: 2. Lancer le Routing Server
echo [2/3] Demarrage du RoutingServer...
start "RoutingServer" "LetsGoBiking\RoutingServer\bin\Debug\RoutingServer.exe"

:: Petit délai pour laisser les serveurs s'initialiser
echo ... Attente de l'initialisation des serveurs (2 secondes) ...
timeout /t 2 /nobreak >nul

:: 3. Lancer le Heavy Client
echo [3/3] Demarrage du HeavyClient...
start "HeavyClient" "LetsGoBiking\HeavyClient\bin\Debug\HeavyClient.exe"

echo ==========================================
echo      Tout est lance ! Bon voyage.
echo ==========================================
