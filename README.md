# Quorum Challenge - Execution Guide

This document explains how to **run the LegalQuorum application** on Windows, Linux, and macOS.

Git Link: https://github.com/hipotzleo/QuorumChallange

---

## Windows Execution

### 1. Requirements
- The `.NET 8 Runtime` **is not required** (application is self-contained).
- Ensure the folder structure matches:

```
QuorumChallenge/
├── StartLegalQuorum.bat
├── start_legalquorum.sh
└── LegalQuorum/
    └── bin/
        └── Release/
            └── net8.0/
                └── win-x64/
                    └── publish/
                        ├── LegalQuorum.App.exe
                        ├── appsettings.json
                        └── data/
```

### 2. Running the App

Just **double-click** the file:

```
StartWindows.bat
```

This script will:
- Start the server on **http://localhost:5000**
- Wait until it’s online
- Automatically open your browser

If you see `[ERROR] Not Founded`, make sure the folder paths are correct.

---

## Linux & macOS Execution

### 1. Make the script executable

Navigate to the project folder and run:

```bash
chmod +x StartLinuxAndMac.sh
```

### 2. Start the server

```bash
./StartLinuxAndMac.sh
```

This will:
- Start the LegalQuorum server on **http://localhost:5000**
- Wait up to 15 seconds for it to start
- Automatically open your default browser

### 3. Manual Launch (optional)

If you want to run manually:

```bash
cd LegalQuorum/bin/Release/net8.0/win-x64/publish
./LegalQuorum --urls=http://localhost:5000
```

Then open your browser and go to:
```
http://localhost:5000
```

---

## Notes

- Both launchers set `ASPNETCORE_CONTENTROOT` to the correct **publish folder**,
  so all relative paths (`./data/...`) work automatically.
- You can adjust the port by changing the `PORT` variable in either script.
- To publish for another OS, run:

```bash
dotnet publish .\LegalQuorum\LegalQuorum.App.csproj -c Release -r linux-x64 -p:PublishSingleFile=true -p:UseAppHost=true --self-contained true
```

---

