# Script para compilar DevStack CLI e GUI
# Execute este script a partir da pasta exe

Write-Host "=== DevStack Build Script ===" -ForegroundColor Green
Write-Host ""

# Verificar se estamos na pasta correta
if (!(Test-Path "DevStackCLI.csproj") -or !(Test-Path "DevStackGUI.csproj")) {
    Write-Host "Erro: Execute este script a partir da pasta 'exe' onde estão os arquivos .csproj" -ForegroundColor Red
    exit 1
}

# Limpar builds anteriores
Write-Host "Limpando builds anteriores..." -ForegroundColor Yellow
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}

Write-Host ""

# Compilar DevStack CLI (Exe)
Write-Host "Compilando DevStack CLI (DevStack.exe)..." -ForegroundColor Cyan
dotnet build DevStackCLI.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar DevStack CLI!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Compilar DevStack GUI (WinExe)
Write-Host "Compilando DevStack GUI (DevStackGUI.exe)..." -ForegroundColor Cyan
dotnet build DevStackGUI.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar DevStack GUI!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Build Concluído com Sucesso! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Arquivos gerados:" -ForegroundColor Yellow
Write-Host "  • DevStack.exe     (CLI - Console Application)" -ForegroundColor White
Write-Host "  • DevStackGUI.exe  (GUI - Windows Application)" -ForegroundColor White
Write-Host ""
Write-Host "Localizados em: bin\Release\net9.0-windows\" -ForegroundColor Gray
Write-Host ""

# Mostrar informações dos arquivos gerados
$cliPath = "bin\Release\net9.0-windows\DevStack.exe"
$guiPath = "bin\Release\net9.0-windows\DevStackGUI.exe"

if (Test-Path $cliPath) {
    $cliInfo = Get-Item $cliPath
    Write-Host "DevStack.exe:    $($cliInfo.Length) bytes - $($cliInfo.LastWriteTime)" -ForegroundColor Green
}

if (Test-Path $guiPath) {
    $guiInfo = Get-Item $guiPath
    Write-Host "DevStackGUI.exe: $($guiInfo.Length) bytes - $($guiInfo.LastWriteTime)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Uso:" -ForegroundColor Yellow
Write-Host "  DevStack.exe [comando] [argumentos]    # Interface de linha de comando" -ForegroundColor White
Write-Host "  DevStackGUI.exe                        # Interface gráfica" -ForegroundColor White
