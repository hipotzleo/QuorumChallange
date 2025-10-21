@echo off
setlocal
set "PROJ=%~dp0LegalQuorum\LegalQuorum.csproj"
set "PUB=%~dp0LegalQuorum\bin\Release\net8.0\win-x64\publish"
set "APP=%PUB%\LegalQuorum.exe"

if not exist "%APP%" (
  echo [INFO] Not Founded. Publishing...
  dotnet publish "%PROJ%" -c Release -r win-x64 -p:PublishSingleFile=true -p:UseAppHost=true --self-contained true || (
    echo [ERRO] Fail at publish.
    pause
    exit /b 1
  )
)

set "ASPNETCORE_CONTENTROOT=%PUB%"
start "" /D "%PUB%" "%APP%" --urls=http://localhost:5000"

for /l %%i in (1,1,15) do (
  powershell -NoProfile -Command "try{(New-Object Net.Sockets.TcpClient 'localhost',5000).Dispose();exit 0}catch{exit 1}" >nul
  if not errorlevel 1 goto :open
  timeout /t 1 >nul
)
:open
start "" "http://localhost:5000"
endlocal