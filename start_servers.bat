@echo off
:: Vérifie si on est admin
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo 🔐 Relance du script en mode administrateur...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    pause
    exit /b
)

echo ============================================
echo 🚀 Démarrage des serveurs WCF en administrateur
echo ============================================

:: %~dp0 = dossier où se trouve ce fichier .bat
set "BASE_DIR=%~dp0"

:: Définit les chemins relatifs à partir du dossier du script
set "ROUTING_PATH=%BASE_DIR%LetsGoBiking\RoutingServer\bin\Debug\RoutingServer.exe"
set "PROXY_PATH=%BASE_DIR%LetsGoBiking\ProxyService\bin\Debug\ProxyService.exe"

:: Lance le ProxyService
echo ▶️ Lancement du ProxyService...
start "Proxy Service" cmd /k "%PROXY_PATH%"

:: Petite pause entre les deux pour éviter les conflits de port
timeout /t 2 >nul


:: Lance le serveur RoutingWithBikes
echo ▶️ Lancement du serveur RoutingWithBikes...
start "RoutingWithBikes Server" cmd /k "%ROUTING_PATH%"

