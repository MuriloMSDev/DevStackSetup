# Script para copiar os executáveis gerados para a pasta raiz
# Execute este script após o build para mover os .exe para a pasta principal

Write-Host "=== DevStack Deploy Script ===" -ForegroundColor Green
Write-Host ""

# Verificar se estamos na pasta correta
if (!(Test-Path "bin\Release\net9.0-windows\DevStack.exe") -or !(Test-Path "bin\Release\net9.0-windows\DevStackGUI.exe")) {
    Write-Host "Erro: Execute o build.ps1 primeiro para gerar os executáveis" -ForegroundColor Red
    exit 1
}

# Caminhos
$sourceDir = "bin\Release\net9.0-windows"
$targetDir = ".."

# Copiar DevStack.exe (CLI)
Write-Host "Copiando DevStack.exe (CLI)..." -ForegroundColor Cyan
Copy-Item "$sourceDir\DevStack.exe" "$targetDir\DevStack.exe" -Force
Copy-Item "$sourceDir\DevStack.dll" "$targetDir\DevStack.dll" -Force
Copy-Item "$sourceDir\DevStack.deps.json" "$targetDir\DevStack.deps.json" -Force
Copy-Item "$sourceDir\DevStack.runtimeconfig.json" "$targetDir\DevStack.runtimeconfig.json" -Force

# Copiar DevStackGUI.exe (GUI)
Write-Host "Copiando DevStackGUI.exe (GUI)..." -ForegroundColor Cyan
Copy-Item "$sourceDir\DevStackGUI.exe" "$targetDir\DevStackGUI.exe" -Force
Copy-Item "$sourceDir\DevStackGUI.dll" "$targetDir\DevStackGUI.dll" -Force
Copy-Item "$sourceDir\DevStackGUI.deps.json" "$targetDir\DevStackGUI.deps.json" -Force
Copy-Item "$sourceDir\DevStackGUI.runtimeconfig.json" "$targetDir\DevStackGUI.runtimeconfig.json" -Force

# Copiar ícone
Copy-Item "DevStack.ico" "$targetDir\DevStack.ico" -Force

# Copiar dependências DLL
Get-ChildItem "$sourceDir" -Filter "*.dll" | Where-Object { $_.Name -notmatch "^DevStack" } | ForEach-Object {
    Copy-Item $_.FullName "$targetDir" -Force
}

Write-Host ""
Write-Host "=== Deploy Concluído! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Arquivos na pasta raiz:" -ForegroundColor Yellow
Get-ChildItem "$targetDir" -Filter "DevStack*" | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "Uso dos executáveis:" -ForegroundColor Yellow
Write-Host "  DevStack.exe [comando] [argumentos]    # Interface de linha de comando" -ForegroundColor White
Write-Host "  DevStackGUI.exe                        # Interface gráfica (sem console)" -ForegroundColor White
