@echo off
setlocal
title Akhil Face ID Setup
echo ========================================================
echo   Akhil Face ID - Windows 11 Biometric Setup Downloader
echo ========================================================
echo.
echo Fetching latest release from GitHub...
powershell -NoProfile -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; (New-Object System.Net.WebClient).DownloadFile('https://github.com/Akhil4826/akhil-face-id/releases/download/v1.0.0/AkhilFaceID-Setup-x64.exe', '$env:TEMP\AkhilFaceID-Setup-x64.exe')"
if not exist "%TEMP%\AkhilFaceID-Setup-x64.exe" (
    echo [ERROR] Download failed. You can manually download from:
    echo https://github.com/Akhil4826/akhil-face-id/releases/tag/v1.0.0
    pause
    exit /b 1
)
echo [OK] Download complete. Starting installation...
start "" "%TEMP%\AkhilFaceID-Setup-x64.exe"
