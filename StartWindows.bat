@echo off
setlocal
set "ROOT=%~dp0"
set "PROJ=%ROOT%LegalQuorum\LegalQuorum.csproj"
set "PUB=%ROOT%LegalQuorum\bin\Release\net8.0\win-x64\publish"
set "APP=%PUB%\LegalQuorum.exe"
set "PORT=5000"

if not exist "%APP%" (
  echo [INFO] Not Founded. Publishing...
  dotnet publish "%PROJ%" -c Release -r win-x64 -p:PublishSingleFile=true -p:UseAppHost=true --self-contained true
  if errorlevel 1 (
    echo [ERRO] Failed at Publish.
    pause
    exit /b 1
  )
)

set "ASPNETCORE_CONTENTROOT=%PUB%"
start "" /D "%PUB%" "%APP%" --urls=http://localhost:%PORT%

for /l %%i in (1,1,15) do (
  powershell -NoProfile -Command "try{(New-Object Net.Sockets.TcpClient 'localhost',%PORT%).Dispose();exit 0}catch{exit 1}" >nul
  if not errorlevel 1 goto :open
  timeout /t 1 >nul
)
:open
start "" "http://localhost:%PORT%"
endlocal
