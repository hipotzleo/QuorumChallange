#!/usr/bin/env bash
set -euo pipefail

APP_NAME="LegalQuorum"
ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJ="$ROOT/LegalQuorum/${APP_NAME}.csproj"
PORT="${PORT:-5000}"

UNAME="$(uname -s)"
case "$UNAME" in
  Linux*) RID="linux-x64" ;;
  Darwin*) RID="osx-arm64" ;;
  *) echo "[ERROR] Unsupported OS: $UNAME"; exit 1 ;;
esac

PUB="$ROOT/LegalQuorum/bin/Release/net8.0/$RID/publish"
APP="$PUB/$APP_NAME"

function have_dotnet8() {
  dotnet --list-sdks 2>/dev/null | grep -qE '^8\.' || return 1
}

function wait_port() {
  local host=$1 port=$2 tries=${3:-20}
  for _ in $(seq 1 "$tries"); do
    (echo >"/dev/tcp/$host/$port") >/dev/null 2>&1 && return 0
    command -v nc >/dev/null && nc -z "$host" "$port" >/dev/null 2>&1 && return 0
    sleep 1
  done
  return 1
}

function open_browser() {
  local url=$1
  if command -v xdg-open >/dev/null; then xdg-open "$url" >/dev/null 2>&1 || true
  elif command -v open >/dev/null; then open "$url" >/dev/null 2>&1 || true
  else echo "Open manually: $url"
  fi
}

# ==== Checks ====
if ! command -v dotnet >/dev/null; then
  echo "[ERROR] dotnet not found. Install .NET 8 SDK."
  exit 1
fi
if ! have_dotnet8; then
  echo "[ERROR] .NET 8 SDK not detected. Please install .NET 8 SDK."
  exit 1
fi

if [[ ! -f "$APP" ]]; then
  echo "[INFO] Executable not found. Publishing for RID=$RID ..."
  dotnet restore "$PROJ" --nologo --verbosity minimal
  dotnet publish "$PROJ" -c Release -r "$RID" \
    -p:PublishSingleFile=true -p:UseAppHost=true --self-contained true --no-restore
fi

if [[ ! -f "$APP" ]]; then
  echo "[ERROR] Publish completed but executable not found at: $APP"
  exit 1
fi

export ASPNETCORE_CONTENTROOT="$PUB"
echo "[RUN] Starting at http://localhost:$PORT ..."
"$APP" --urls="http://localhost:$PORT" >/dev/null 2>&1 &

if wait_port localhost "$PORT" 20; then
  open_browser "http://localhost:$PORT"
else
  echo "[WARN] Server did not open port $PORT in time; try opening the URL manually."
fi
