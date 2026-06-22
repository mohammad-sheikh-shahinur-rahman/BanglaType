# BanglaType Installer Build Script
# This script ensures the application is built, downloads VC++ redistributables if missing, and compiles the Inno Setup installer.

$ErrorActionPreference = "Stop"

# 1. Locate MSBuild and build the project in Release mode
Write-Host "Locating MSBuild..." -ForegroundColor Cyan
$msBuildPaths = @(
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
)

$msBuildPath = $null
foreach ($path in $msBuildPaths) {
    if (Test-Path $path) {
        $msBuildPath = $path
        break
    }
}

if (-not $msBuildPath) {
    # Try using Get-Command as a fallback
    $cmd = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($cmd) {
        $msBuildPath = $cmd.Source
    }
}

if (-not $msBuildPath) {
    Write-Error "MSBuild.exe was not found. Please ensure Visual Studio or Visual Studio Build Tools are installed."
}

Write-Host "Found MSBuild at: $msBuildPath" -ForegroundColor Green
Write-Host "Building project in Release mode..." -ForegroundColor Cyan
& $msBuildPath /property:Configuration=Release BanglaType.sln

# 2. Check and download Visual C++ Redistributables if missing
Write-Host "Checking for VC++ Redistributables..." -ForegroundColor Cyan
$redistDir = Join-Path $PSScriptRoot "bin\Redist"
if (-not (Test-Path $redistDir)) {
    New-Item -ItemType Directory -Force -Path $redistDir | Out-Null
}

$x86Redist = Join-Path $redistDir "vc_redist.x86.exe"
$x64Redist = Join-Path $redistDir "vc_redist.x64.exe"

if (-not (Test-Path $x86Redist)) {
    Write-Host "Downloading VC++ Redistributable (x86)..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri "https://aka.ms/vs/17/release/vc_redist.x86.exe" -OutFile $x86Redist -UseBasicParsing
    Write-Host "x86 Redist downloaded successfully." -ForegroundColor Green
} else {
    Write-Host "x86 Redist already exists." -ForegroundColor Green
}

if (-not (Test-Path $x64Redist)) {
    Write-Host "Downloading VC++ Redistributable (x64)..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri "https://aka.ms/vs/17/release/vc_redist.x64.exe" -OutFile $x64Redist -UseBasicParsing
    Write-Host "x64 Redist downloaded successfully." -ForegroundColor Green
} else {
    Write-Host "x64 Redist already exists." -ForegroundColor Green
}

# 3. Locate Inno Setup Compiler (ISCC.exe)
Write-Host "Locating Inno Setup Compiler..." -ForegroundColor Cyan
$isccPaths = @(
    "$env:LocalAppData\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
)

$isccPath = $null
foreach ($path in $isccPaths) {
    if (Test-Path $path) {
        $isccPath = $path
        break
    }
}

if (-not $isccPath) {
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) {
        $isccPath = $cmd.Source
    }
}

if (-not $isccPath) {
    Write-Error "Inno Setup Compiler (ISCC.exe) was not found. Please install Inno Setup 6."
}

Write-Host "Found Inno Setup Compiler at: $isccPath" -ForegroundColor Green

# 4. Compile Installer
Write-Host "Compiling installer using Inno Setup..." -ForegroundColor Cyan
$issPath = Join-Path $PSScriptRoot "BanglaType.iss"
& $isccPath $issPath

# 5. Copy Production Build to Releases folder
Write-Host "Copying production builds to Releases folder..." -ForegroundColor Cyan
$releasesDir = Join-Path $PSScriptRoot "Releases"
if (-not (Test-Path $releasesDir)) {
    New-Item -ItemType Directory -Force -Path $releasesDir | Out-Null
}
Copy-Item (Join-Path $PSScriptRoot "bin\Release\BanglaType.exe") (Join-Path $releasesDir "BanglaType.exe") -Force
Copy-Item (Join-Path $PSScriptRoot "Output\BanglaTypeInstaller.exe") (Join-Path $releasesDir "BanglaTypeInstaller.exe") -Force

Write-Host "Production build completed successfully! Check the 'Releases' directory." -ForegroundColor Green
