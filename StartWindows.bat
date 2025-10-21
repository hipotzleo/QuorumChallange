@echo off
setlocal

set "PUB=%~dp0LegalQuorum\bin\Release\net8.0\win-x64\publish"
set "APP=%PUB%\LegalQuorum.exe"

if not exist "%APP%" (
  echo [ERRO] Nao encontrei o executavel: %APP%
  pause
  exit /b 1
)

set "ASPNETCORE_CONTENTROOT=%PUB%"

set "PORT=5000"

start "" /D "%PUB%" "%APP%" --urls=http://localhost:%PORT%

for /l %%i in (1,1,15) do (
  powershell -NoProfile -Command "try{(New-Object Net.Sockets.TcpClient 'localhost',%PORT%).Dispose();exit 0}catch{exit 1}" >nul
  if not errorlevel 1 goto :open
  timeout /t 1 >nul
)

:open
start "" "http://localhost:%PORT%"
endlocal
