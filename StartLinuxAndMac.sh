#!/bin/bash
DIR="$(cd "$(dirname "$0")" && pwd)"
PUB="$DIR/LegalQuorum/bin/Release/net8.0/linux-x64/publish"
APP="$PUB/LegalQuorum"

if [ ! -f "$APP" ]; then
  echo "[ERROR] Executable not found at: $APP"
  exit 1
fi

export ASPNETCORE_CONTENTROOT="$PUB"
PORT=5000

echo "Starting LegalQuorum on http://localhost:$PORT ..."
"$APP" --urls="http://localhost:$PORT" &

# Wait for the app to start
for i in {1..15}; do
  nc -z localhost $PORT && break
  sleep 1
done

# Open the browser
if command -v xdg-open >/dev/null; then
  xdg-open "http://localhost:$PORT"
elif command -v open >/dev/null; then
  open "http://localhost:$PORT"
else
  echo "Open manually: http://localhost:$PORT"
fi
