# BanglaType Build Automation Script
# Generates 32-bit (x86) and 64-bit (x64) Portable ZIPs, and compiles the Inno Setup installer.

$ErrorActionPreference = "Stop"

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "          BANGLATYPE BUILD AUTOMATION         " -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

# 1. Clean previous build outputs if they exist
Write-Host "Stopping any running instances..." -ForegroundColor Green
Stop-Process -Name BanglaType -Force -ErrorAction SilentlyContinue

Write-Host "Cleaning old build files..." -ForegroundColor Green
$foldersToClean = @("bin\Release_x86", "bin\Release_x64", "bin\Release", "Output")
foreach ($folder in $foldersToClean) {
    if (Test-Path $folder) {
        Remove-Item -Path $folder -Recurse -Force
    }
}
New-Item -ItemType Directory -Force -Path "Output" | Out-Null

# Download VC++ Redistributables if not already present
$redistDir = "bin\Redist"
if (-not (Test-Path $redistDir)) {
    New-Item -ItemType Directory -Force -Path $redistDir | Out-Null
}
$vcX64 = "$redistDir\vc_redist.x64.exe"
$vcX86 = "$redistDir\vc_redist.x86.exe"
if (-not (Test-Path $vcX64)) {
    Write-Host "Downloading VC++ 2015-2022 Redistributable (x64)..." -ForegroundColor Green
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri "https://aka.ms/vs/17/release/vc_redist.x64.exe" -OutFile $vcX64
}
if (-not (Test-Path $vcX86)) {
    Write-Host "Downloading VC++ 2015-2022 Redistributable (x86)..." -ForegroundColor Green
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri "https://aka.ms/vs/17/release/vc_redist.x86.exe" -OutFile $vcX86
}

# MSBuild Path
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msbuild)) {
    Write-Error "MSBuild.exe was not found at expected path: $msbuild"
}

# Inno Setup Path
$iscc = "C:\Users\shahi\AppData\Local\Programs\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $iscc)) {
    Write-Error "ISCC.exe was not found at expected path: $iscc"
}

# 2. Build 32-bit (x86) configuration
Write-Host "Building 32-bit (x86) configuration..." -ForegroundColor Green
& $msbuild BanglaType.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:PlatformTarget=x86 /p:OutputPath=bin\Release_x86\ | Out-Null

# 3. Build 64-bit (x64) configuration
Write-Host "Building 64-bit (x64) configuration..." -ForegroundColor Green
& $msbuild BanglaType.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:PlatformTarget=x64 /p:OutputPath=bin\Release_x64\ | Out-Null

# 4. Build Any CPU configuration for installer
Write-Host "Building Any CPU configuration..." -ForegroundColor Green
& $msbuild BanglaType.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=bin\Release\ | Out-Null

# 5. Create 32-bit Portable ZIP
Write-Host "Creating 32-bit Portable ZIP..." -ForegroundColor Green
$x86Source = "bin\Release_x86"
$x86Zip = "Output\BanglaType_Portable_x86.zip"
# Copy resources we want to include
$tempFolderX86 = "bin\temp_x86"
New-Item -ItemType Directory -Force -Path $tempFolderX86 | Out-Null
Copy-Item -Path "$x86Source\BanglaType.exe" -Destination $tempFolderX86
Copy-Item -Path "$x86Source\BanglaType.exe.config" -Destination $tempFolderX86
Copy-Item -Path "$x86Source\Resources" -Destination $tempFolderX86 -Recurse
Compress-Archive -Path "$tempFolderX86\*" -DestinationPath $x86Zip -Force
Remove-Item -Path $tempFolderX86 -Recurse -Force

# 6. Create 64-bit Portable ZIP
Write-Host "Creating 64-bit Portable ZIP..." -ForegroundColor Green
$x64Source = "bin\Release_x64"
$x64Zip = "Output\BanglaType_Portable_x64.zip"
# Copy resources we want to include
$tempFolderX64 = "bin\temp_x64"
New-Item -ItemType Directory -Force -Path $tempFolderX64 | Out-Null
Copy-Item -Path "$x64Source\BanglaType.exe" -Destination $tempFolderX64
Copy-Item -Path "$x64Source\BanglaType.exe.config" -Destination $tempFolderX64
Copy-Item -Path "$x64Source\Resources" -Destination $tempFolderX64 -Recurse
Compress-Archive -Path "$tempFolderX64\*" -DestinationPath $x64Zip -Force
Remove-Item -Path $tempFolderX64 -Recurse -Force

# 7. Compile Inno Setup Installer
Write-Host "Compiling Professional Installer..." -ForegroundColor Green
& $iscc BanglaType.iss | Out-Null

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "          BUILD COMPLETED SUCCESSFULLY        " -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Artifacts generated in 'Output' directory:" -ForegroundColor Yellow
Write-Host " - 32-bit Portable ZIP: Output\BanglaType_Portable_x86.zip"
Write-Host " - 64-bit Portable ZIP: Output\BanglaType_Portable_x64.zip"
Write-Host " - Professional Installer: Output\BanglaTypeInstaller.exe"
Write-Host "==============================================" -ForegroundColor Cyan
