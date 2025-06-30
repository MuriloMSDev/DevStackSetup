param(
    [Parameter(Mandatory=$false, Position=0)]
    [ValidateSet("install", "site", "uninstall", "path", "list", "start", "stop", "restart", "status", "update", "deps", "test", "alias", "global", "self-update", "clean", "backup", "logs", "enable", "disable", "config", "reset", "proxy", "ssl", "db", "service", "doctor", "help", "gui", "Invoke-DevStackGUI")]
    [string]$Command = "gui",

    [Parameter(Position=1, ValueFromRemainingArguments=$true)]
    [string[]]$Args
)

# Configurar codificação para UTF-8 para garantir caracteres especiais
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
# Força uso de caracteres UTF-8 diretos em vez de códigos escape
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

. "$PSScriptRoot\src\load.ps1"

function Write-Info($msg) { 
    # Garantir que a mensagem seja exibida com codificação UTF-8
    $utf8Msg = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::Default.GetBytes($msg))
    Write-Host $utf8Msg -ForegroundColor Cyan 
}

function Write-WarningMsg($msg) { 
    $utf8Msg = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::Default.GetBytes($msg))
    Write-Host $utf8Msg -ForegroundColor Yellow 
}

function Write-ErrorMsg($msg) { 
    $utf8Msg = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::Default.GetBytes($msg))
    Write-Host $utf8Msg -ForegroundColor Red 
}
function Write-Log($msg) {
    $logFile = Join-Path $baseDir "devstack.log"
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] $msg"
}

function Status-Component {
    param([string]$Component)
    
    $status = Get-ComponentStatus -Component $Component
    
    if (-not $status.installed) {
        Write-WarningMsg $status.message
        return
    }
    
    Write-Info "$Component instalado(s):"
    $status.versions | ForEach-Object {
        Write-Host "  $_"
    }
}

function Status-All {
    Write-Host "Status do DevStack:"
    
    $allStatus = Get-AllComponentsStatus
    
    foreach ($comp in $allStatus.Keys) {
        if ($allStatus[$comp].installed) {
            Write-Info "$comp instalado(s):"
            $allStatus[$comp].versions | ForEach-Object {
                Write-Host "  $_"
            }
        } else {
            Write-WarningMsg "$comp não está instalado."
        }
    }
}

function Test-All {
    Write-Host "Testando ferramentas instaladas:"
    $tools = @(
        @{ name = "php"; exe = "php-*.exe"; dir = $phpDir; args = "-v" },
        @{ name = "nginx"; exe = "nginx-*.exe"; dir = $nginxDir; args = "-v" },
        @{ name = "mysql"; exe = "mysqld-*.exe"; dir = $mysqlDir; args = "--version" },
        @{ name = "nodejs"; exe = "node-*.exe"; dir = $nodeDir; args = "-v" },
        @{ name = "python"; exe = "python-*.exe"; dir = $pythonDir; args = "--version" },
        @{ name = "git"; exe = "git.exe"; dir = $baseDir; args = "--version" },
        @{ name = "composer"; exe = "composer-*.exe"; dir = $composerDir; args = "--version" },
        @{ name = "phpmyadmin"; exe = "index.php"; dir = $pmaDir; args = "" },
        @{ name = "mongodb"; exe = "mongo.exe"; dir = $mongoDir; args = "--version" },
        @{ name = "redis"; exe = "redis-server.exe"; dir = $redisDir; args = "--version" },
        @{ name = "pgsql"; exe = "psql.exe"; dir = $pgsqlDir; args = "--version" },
        @{ name = "mailhog"; exe = "mailhog.exe"; dir = $mailhogDir; args = "--version" },
        @{ name = "elasticsearch"; exe = "elasticsearch.exe"; dir = $elasticDir; args = "--version" },
        @{ name = "memcached"; exe = "memcached.exe"; dir = $memcachedDir; args = "-h" },
        @{ name = "docker"; exe = "docker.exe"; dir = $dockerDir; args = "--version" },
        @{ name = "yarn"; exe = "yarn.cmd"; dir = $yarnDir; args = "--version" },
        @{ name = "pnpm"; exe = "pnpm.exe"; dir = $pnpmDir; args = "--version" },
        @{ name = "wpcli"; exe = "wp-cli.phar"; dir = $wpcliDir; args = "--version" },
        @{ name = "adminer"; exe = "adminer-*.php"; dir = $adminerDir; args = "" },
        @{ name = "poetry"; exe = "poetry.exe"; dir = $poetryDir; args = "--version" },
        @{ name = "ruby"; exe = "ruby.exe"; dir = $rubyDir; args = "--version" },
        @{ name = "go"; exe = "go.exe"; dir = $goDir; args = "version" },
        @{ name = "certbot"; exe = "certbot.exe"; dir = $certbotDir; args = "--version" },
        @{ name = "openssl"; exe = "openssl.exe"; dir = $openSSLDir; args = "version" },
        @{ name = "phpcsfixer"; exe = "php-cs-fixer-*.exe"; dir = $phpcsfixerDir; args = "--version" }
    )
    foreach ($tool in $tools) {
        if ($tool.name -eq "git") {
            $found = Get-ChildItem $tool.dir -Recurse -Filter $tool.exe -ErrorAction SilentlyContinue | Where-Object { $_.FullName -like "*\\cmd\\git.exe" } | Select-Object -First 1
        } else {
            $found = Get-ChildItem $tool.dir -Recurse -Filter $tool.exe -ErrorAction SilentlyContinue | Select-Object -First 1
        }
        if ($found) {
            try {
                $output = & $found.FullName $tool.args
                Write-Info "$($tool.name): $output"
            } catch {
                Write-ErrorMsg "$($tool.name): erro ao executar $($found.FullName)"
            }
        } else {
            Write-WarningMsg "$($tool.name): não encontrado."
        }
    }
}

function Deps-Check {
    Write-Host "Verificando dependências do sistema..."
    $missing = @()
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        $missing += "Permissão de administrador"
    }
    if (-not (Get-Command Expand-Archive -ErrorAction SilentlyContinue)) {
        $missing += "Expand-Archive (PowerShell 5+)"
    }
    if ($missing.Count -eq 0) {
        Write-Info "Todas as dependências estão presentes."
    } else {
        Write-ErrorMsg "Dependências ausentes: $($missing -join ", ")"
    }
}

function Center-Text($text, $width) {
    $pad = [Math]::Max(0, $width - $text.Length)
    $padLeft = [Math]::Floor($pad / 2)
    $padRight = $pad - $padLeft
    return (' ' * $padLeft) + $text + (' ' * $padRight)
}

function Update-Component {
    param([string]$Component)
    switch ($Component) {
        "php" { Install-PHP }
        "nginx" { Install-Nginx }
        "mysql" { Install-MySQL }
        "nodejs" { Install-NodeJS }
        "python" { Install-Python }
        "composer" { Install-Composer }
        "phpmyadmin" { Install-PhpMyAdmin }
        "git" { Install-Git }
        "phpmyadmin" { Install-PhpMyAdmin }
        "mongodb" { Install-MongoDB }
        "redis" { Install-Redis }
        "pgsql" { Install-Pgsql }
        "mailhog" { Install-Mailhog }
        "elasticsearch" { Install-Elastic }
        "memcached" { Install-Memcached }
        "docker" { Install-Docker }
        "yarn" { Install-Yarn }
        "pnpm" { Install-Pnpm }
        "wpcli" { Install-Wpcli }
        "adminer" { Install-Adminer }
        "poetry" { Install-Poetry }
        "ruby" { Install-Ruby }
        "go" { Install-Go }
        "certbot" { Install-Certbot }
        "openssl" { Install-OpenSSL }
        "phpcsfixer" { Install-PHPCsFixer }
        default { Write-ErrorMsg "Componente desconhecido: $Component" }
    }
}

function Alias-Component {
    param([string]$Component, [string]$Version)
    $aliasDir = Join-Path $baseDir "aliases"
    if (-not (Test-Path $aliasDir)) { New-Item -ItemType Directory -Path $aliasDir | Out-Null }
    $exe = switch ($Component) {
        "php" { Join-Path $phpDir "php-$Version\php-$Version.exe" }
        "nginx" { Join-Path $nginxDir "nginx-$Version\nginx-$Version.exe" }
        "nodejs" { Join-Path $nodeDir "node-v$Version-win-x64\node-$Version.exe" }
        "python" { Join-Path $pythonDir "python-$Version\python-$Version.exe" }
        "git" { Join-Path $baseDir "git-$Version\cmd\git.exe" }
        "mysql" { Join-Path $mysqlDir "mysql-$Version\bin\mysql.exe" }
        "phpmyadmin" { Join-Path $pmaDir "phpmyadmin-$Version\index.php" }
        "mongodb" { Join-Path $mongoDir "mongodb-$Version\bin\mongo.exe" }
        "redis" { Join-Path $redisDir "redis-$Version\redis-server.exe" }
        "pgsql" { Join-Path $pgsqlDir "pgsql-$Version\bin\psql.exe" }
        "mailhog" { Join-Path $mailhogDir "mailhog-$Version\mailhog.exe" }
        "elasticsearch" { Join-Path $elasticDir "elasticsearch-$Version\bin\elasticsearch.exe" }
        "memcached" { Join-Path $memcachedDir "memcached-$Version\memcached.exe" }
        "docker" { Join-Path $dockerDir "docker-$Version\docker.exe" }
        "yarn" { Join-Path $yarnDir "yarn-v$Version\bin\yarn.cmd" }
        "pnpm" { Join-Path $pnpmDir "pnpm-v$Version\pnpm.exe" }
        "wpcli" { Join-Path $wpcliDir "wp-cli-$Version\wp-cli.phar" }
        "adminer" { Join-Path $adminerDir "adminer-$Version.php" }
        "poetry" { Join-Path $poetryDir "poetry-$Version\bin\poetry.exe" }
        "ruby" { Join-Path $rubyDir "ruby-$Version\bin\ruby.exe" }
        "go" { Join-Path $goDir "go$Version\bin\go.exe" }
        "certbot" { Join-Path $certbotDir "certbot-$Version\certbot.exe" }
        "openssl" { Join-Path $openSSLDir "openssl-$Version\bin\openssl.exe" }
        "phpcsfixer" { Join-Path $phpcsfixerDir "php-cs-fixer-$Version\php-cs-fixer-$Version.exe" }
        default { $null }
    }
    if ($exe -and (Test-Path $exe)) {
        $bat = Join-Path $aliasDir "$Component$Version.bat"
        Set-Content -Path $bat -Value "@echo off`r`n\"$exe\" %*"
        Write-Info "Alias criado: $bat"
    } else {
        Write-ErrorMsg "Executável não encontrado para $Component $Version"
    }
}

function Help-DevStack {
    Write-Host "DevStackSetup - Comandos disponíveis:" -ForegroundColor Cyan
    $cmds = @(
        @{ cmd = "install <componente> [versão]"; desc = "Instala uma ferramenta ou versão específica." },
        @{ cmd = "uninstall <componente> [versão]"; desc = "Remove uma ferramenta ou versão específica." },
        @{ cmd = "list <componente|--installed>"; desc = "Lista versões disponíveis ou instaladas." },
        @{ cmd = "status"; desc = "Mostra status de todas as ferramentas." },
        @{ cmd = "test"; desc = "Testa todas as ferramentas instaladas." },
        @{ cmd = "update <componente>"; desc = "Atualiza uma ferramenta para a última versão." },
        @{ cmd = "deps"; desc = "Verifica dependências do sistema." },
        @{ cmd = "alias <componente> <versão>"; desc = "Cria um alias .bat para a versão da ferramenta." },
        @{ cmd = "global"; desc = "Adiciona DevStack ao PATH e cria alias global." },
        @{ cmd = "self-update"; desc = "Atualiza o DevStackSetup." },
        @{ cmd = "clean"; desc = "Remove logs e arquivos temporários." },
        @{ cmd = "backup"; desc = "Cria backup das configs e logs." },
        @{ cmd = "logs"; desc = "Exibe as últimas linhas do log." },
        @{ cmd = "enable <serviço>"; desc = "Ativa um serviço do Windows." },
        @{ cmd = "disable <serviço>"; desc = "Desativa um serviço do Windows." },
        @{ cmd = "config"; desc = "Abre o diretório de configuração." },
        @{ cmd = "reset <componente>"; desc = "Remove e reinstala uma ferramenta." },
        @{ cmd = "proxy [set <url>|unset|show]"; desc = "Gerencia variáveis de proxy." },
        @{ cmd = "ssl <domínio> [-openssl <versão>]"; desc = "Gera certificado SSL autoassinado." },
        @{ cmd = "db <mysql|pgsql|mongo> <comando> [args...]"; desc = "Gerencia bancos de dados básicos." },
        @{ cmd = "service"; desc = "Lista serviços DevStack (Windows)." },
        @{ cmd = "doctor"; desc = "Diagnóstico do ambiente DevStack." },
        @{ cmd = "site <domínio> [opções]"; desc = "Cria configuração de site nginx." },
        @{ cmd = "gui"; desc = "Inicia a interface gráfica do DevStack." },
        @{ cmd = "help"; desc = "Exibe esta ajuda." }
    )
    $col1 = 46; $col2 = 60
    Write-Host ("_" * ($col1 + $col2 + 3))
    Write-Host ("|{0}|{1}|" -f (Center-Text 'Comando' $col1), (Center-Text 'Descrição' $col2))
    Write-Host ("|" + ('-' * $col1) + "+" + ('-' * $col2) + "|")
    foreach ($c in $cmds) {
        Write-Host -NoNewline "|"
        Write-Host -NoNewline (Center-Text $c.cmd $col1) -ForegroundColor Yellow
        Write-Host -NoNewline "|"
        Write-Host -NoNewline (Center-Text $c.desc $col2) -ForegroundColor Gray
        Write-Host "|"
    }
    Write-Host ("¯" * ($col1 + $col2 + 3))
}

switch ($Command) {
    "help" {
        Write-Log "Comando executado: help"
        Help-DevStack
    }    
    "list" {
        Write-Log "Comando executado: list $($Args -join ' ')"
        if ($Args.Count -eq 0) {
            Write-Host "Uso: setup.ps1 list <php|nodejs|python|composer|mysql|nginx|phpmyadmin|git|mongodb|redis|pgsql|mailhog|elasticsearch|memcached|docker|yarn|pnpm|wpcli|adminer|poetry|ruby|go|certbot|openssl|phpcsfixer|--installed>"
            exit 1
        }
        $firstArg = $Args[0].Trim()
        if ($firstArg -eq '--installed') {
            List-InstalledVersions
            return
        }
        switch ($firstArg.ToLower()) {
            "php"           { List-PHPVersions }
            "nodejs"        { List-NodeVersions }
            "node"          { List-NodeVersions }
            "python"        { List-PythonVersions }
            "composer"      { List-ComposerVersions }
            "mysql"         { List-MySQLVersions }
            "nginx"         { List-NginxVersions }
            "phpmyadmin"    { List-PhpMyAdminVersions }
            "git"           { List-GitVersions }
            "mongodb"       { List-MongoDBVersions }
            "redis"         { List-RedisVersions }
            "pgsql"         { List-PgSQLVersions }
            "mailhog"       { List-MailHogVersions }
            "elasticsearch" { List-ElasticsearchVersions }
            "memcached"     { List-MemcachedVersions }
            "docker"        { List-DockerVersions }
            "yarn"          { List-YarnVersions }
            "pnpm"          { List-PnpmVersions }
            "wpcli"         { List-WPCLIVersions }
            "adminer"       { List-AdminerVersions }
            "poetry"        { List-PoetryVersions }
            "ruby"          { List-RubyVersions }
            "go"            { List-GoVersions }
            "certbot"       { List-CertbotVersions }
            "openssl"       { List-OpenSSLVersions }
            "phpcsfixer"    { List-PHPCsFixerVersions }
            default         { Write-Host "Ferramenta desconhecida: $($firstArg)" }
        }
    }
    "site" {
        Write-Log "Comando executado: site $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 site <dominio> [-root <diretorio>] [-php <php-upstream>] [-nginx <nginx-version>]"
            exit 1
        }
        $Domain = $Args[0]

        for ($i = 1; $i -lt $Args.Count; $i++) {
            switch ($Args[$i]) {
                "-root" {
                    $i++; if ($i -lt $Args.Count) { $Root = $Args[$i] }
                }
                "-php" {
                    $i++; if ($i -lt $Args.Count) { $PhpUpstream = $Args[$i] }
                }
                "-nginx" {
                    $i++; if ($i -lt $Args.Count) { $NginxVersion = $Args[$i] }
                }
                "-index" {
                    $i++; if ($i -lt $Args.Count) { $IndexLocation = $Args[$i] }
                }
            }
        }
        Create-NginxSiteConfig -Domain $Domain -Root $Root -PhpUpstream $PhpUpstream -NginxVersion $NginxVersion -IndexLocation $IndexLocation
    }
    "install" {
        Write-Log "Comando executado: install $($Args -join ' ')"
        Install-Commands @Args
    }
    "path" {
        Write-Log "Comando executado: path $($Args -join ' ')"
        Add-BinDirsToPath
    }
    "uninstall" {
        Write-Log "Comando executado: uninstall $($Args -join ' ')"
        Uninstall-Commands @Args
    }
    "start" {
        Write-Log "Comando executado: start $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 start <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            if (Test-Path $nginxDir) {
                ForEach-Version "nginx" { param($v) & $PSCommandPath -Command start -Args @("nginx", $v) }
            } else {
                Write-WarningMsg "Diretório do nginx não encontrado. Ignorando."
            }
            if (Test-Path $phpDir) {
                ForEach-Version "php" { param($v) & $PSCommandPath -Command start -Args @("php", $v) }
            } else {
                Write-WarningMsg "Diretório do PHP não encontrado. Ignorando."
            }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 start <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Start-Component $target $version
    }
    "stop" {
        Write-Log "Comando executado: stop $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 stop <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            if (Test-Path $nginxDir) {
                ForEach-Version "nginx" { param($v) & $PSCommandPath -Command stop -Args @("nginx", $v) }
            } else {
                Write-WarningMsg "Diretório do nginx não encontrado. Ignorando."
            }
            if (Test-Path $phpDir) {
                ForEach-Version "php" { param($v) & $PSCommandPath -Command stop -Args @("php", $v) }
            } else {
                Write-WarningMsg "Diretório do PHP não encontrado. Ignorando."
            }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 stop <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Stop-Component $target $version
    }
    "restart" {
        Write-Log "Comando executado: restart $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 restart <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            if (Test-Path $nginxDir) {
                ForEach-Version "nginx" { param($v) & $PSCommandPath -Command restart -Args @("nginx", $v) }
            } else {
                Write-WarningMsg "Diretório do nginx não encontrado. Ignorando."
            }
            if (Test-Path $phpDir) {
                ForEach-Version "php" { param($v) & $PSCommandPath -Command restart -Args @("php", $v) }
            } else {
                Write-WarningMsg "Diretório do PHP não encontrado. Ignorando."
            }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 restart <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Stop-Component $target $version
        Start-Sleep -Seconds 1
        Start-Component $target $version
    }
    "status" {
        Write-Log "Comando executado: status $($Args -join ' ')"
        Status-All
    }
    "test" {
        Write-Log "Comando executado: test $($Args -join ' ')"
        Test-All
    }
    "deps" {
        Write-Log "Comando executado: deps $($Args -join ' ')"
        Deps-Check
    }
    "update" {
        Write-Log "Comando executado: update $($Args -join ' ')"
        foreach ($component in $Args) {
            Update-Component $component
        }
    }
    "alias" {
        Write-Log "Comando executado: alias $($Args -join ' ')"
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 alias <componente> <versão>"
            exit 1
        }
        Alias-Component $Args[0] $Args[1]
    }
    "self-update" {
        Write-Log "Comando executado: self-update $($Args -join ' ')"
        # Atualiza o DevStackSetup via git pull ou cópia do repositório
        $repoDir = $PSScriptRoot
        if (Test-Path (Join-Path $repoDir ".git")) {
            Write-Info "Atualizando via git pull..."
            try {
                Push-Location $repoDir
                git pull
                Pop-Location
                Write-Info "DevStackSetup atualizado com sucesso."
            } catch {
                Write-ErrorMsg "Erro ao atualizar via git: $_"
            }
        } else {
            Write-WarningMsg "Não é um repositório git. Atualize manualmente copiando os arquivos do repositório."
        }
    }
    "clean" {
        Write-Log "Comando executado: clean $($Args -join ' ')"
        # Limpa arquivos temporários e logs
        $logDir = Join-Path $baseDir "logs"
        $tmpDir = Join-Path $baseDir "tmp"
        $logFile = Join-Path $baseDir "devstack.log"
        $count = 0
        if (Test-Path $logFile) { Remove-Item $logFile -Force; $count++ }
        if (Test-Path $logDir) { Remove-Item $logDir -Recurse -Force; $count++ }
        if (Test-Path $tmpDir) { Remove-Item $tmpDir -Recurse -Force; $count++ }
        Write-Info "Limpeza concluída. ($count itens removidos)"
    }
    "backup" {
        Write-Log "Comando executado: backup $($Args -join ' ')"
        # Backup dos diretórios de configuração e logs
        $backupDir = Join-Path $baseDir ("backup-" + (Get-Date -Format "yyyyMMdd-HHmmss"))
        $toBackup = @("configs", "devstack.log")
        foreach ($item in $toBackup) {
            $src = Join-Path $baseDir $item
            if (Test-Path $src) {
                Copy-Item $src -Destination $backupDir -Recurse -Force
            }
        }
        Write-Info "Backup criado em $backupDir"
    }
    "logs" {
        Write-Log "Comando executado: logs $($Args -join ' ')"
        # Exibe as últimas 50 linhas do log principal
        $logFile = Join-Path $baseDir "devstack.log"
        if (Test-Path $logFile) {
            Write-Host "Últimas 50 linhas de $($logFile):"
            Get-Content $logFile -Tail 50
        } else {
            Write-WarningMsg "Arquivo de log não encontrado."
        }
    }
    "enable" {
        Write-Log "Comando executado: enable $($Args -join ' ')"
        # Ativa um serviço (Windows Service) relacionado ao DevStack
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 enable <serviço>"; exit 1 }
        $svc = $Args[0]
        try {
            Start-Service -Name $svc
            Write-Info "Serviço $($svc) ativado."
        } catch {
            Write-ErrorMsg "Erro ao ativar serviço $($svc): $_"
        }
    }
    "disable" {
        Write-Log "Comando executado: disable $($Args -join ' ')"
        # Desativa um serviço (Windows Service) relacionado ao DevStack
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 disable <serviço>"; exit 1 }
        $svc = $Args[0]
        try {
            Stop-Service -Name $svc
            Write-Info "Serviço $($svc) desativado."
        } catch {
            Write-ErrorMsg "Erro ao desativar serviço $($svc): $_"
        }
    }
    "config" {
        Write-Log "Comando executado: config $($Args -join ' ')"
        # Abre o diretório de configuração para edição
        $configDir = Join-Path $baseDir "configs"
        if (Test-Path $configDir) {
            Invoke-Item $configDir
            Write-Info "Diretório de configuração aberto."
        } else {
            Write-WarningMsg "Diretório de configuração não encontrado."
        }
    }
    "reset" {
        Write-Log "Comando executado: reset $($Args -join ' ')"
        # Remove e reinstala uma ferramenta
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 reset <componente>"; exit 1 }
        $comp = $Args[0]
        Write-Info "Resetando $comp..."
        & $PSCommandPath uninstall $comp
        & $PSCommandPath install $comp
        Write-Info "$comp resetado."
    }
    "proxy" {
        Write-Log "Comando executado: proxy $($Args -join ' ')"
        # Gerencia variáveis de ambiente de proxy
        if ($Args.Count -eq 0) {
            Write-Host "Proxy atual: $env:HTTP_PROXY"
            return
        }
        switch ($Args[0]) {
            "set" {
                if ($Args.Count -lt 2) { Write-Host "Uso: setup.ps1 proxy set <url>"; exit 1 }
                $env:HTTP_PROXY = $Args[1]
                $env:HTTPS_PROXY = $Args[1]
                Write-Info "Proxy definido para $($Args[1])"
            }
            "unset" {
                Remove-Item Env:HTTP_PROXY -ErrorAction SilentlyContinue
                Remove-Item Env:HTTPS_PROXY -ErrorAction SilentlyContinue
                Write-Info "Proxy removido."
            }
            default {
                Write-Host "Uso: setup.ps1 proxy [set <url>|unset|show]"
            }
        }
    }
    "ssl" {
        Write-Log "Comando executado: ssl $($Args -join ' ')"
        # Gera certificado SSL autoassinado para um domínio
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 ssl <dominio> [-openssl <versao>]"; exit 1 }
        $domain = $Args[0]
        $sslDir = Join-Path $baseDir "configs\nginx\ssl"
        if (-not (Test-Path $sslDir)) { New-Item -ItemType Directory -Path $sslDir | Out-Null }
        $crt = Join-Path $sslDir "$domain.crt"
        $key = Join-Path $sslDir "$domain.key"
        $opensslVersion = $null
        for ($i = 1; $i -lt $Args.Count; $i++) {
            if ($Args[$i] -eq "-openssl" -and ($i+1) -lt $Args.Count) {
                $opensslVersion = $Args[$i+1]
                break
            }
        }
        if (-not $opensslVersion) {
            $opensslVersion = Get-LatestOpenSSLVersion
        }
        $dir = Join-Path $openSSLDir "openssl-$opensslVersion\bin"
        $opensslExe = Join-Path $dir "openssl.exe"
        if (-not (Test-Path $opensslExe)) {
            Write-Host "OpenSSL $opensslVersion não encontrado. Instalando..."
            Install-OpenSSL $opensslVersion
        }
        if (-not (Test-Path $opensslExe)) {
            Write-ErrorMsg "OpenSSL $opensslVersion não encontrado no PATH nem em $opensslExe. Instale para usar este comando."
            exit 1
        }
        & $opensslExe req -x509 -nodes -days 365 -newkey rsa:2048 -keyout $key -out $crt -subj "/CN=$domain"
        Write-Info "Certificado gerado: $($crt), $($key)"
    }
    "db" {
        Write-Log "Comando executado: db $($Args -join ' ')"
        # Gerenciamento básico de bancos de dados (MySQL, PostgreSQL, MongoDB)
        if ($Args.Count -lt 2) { Write-Host "Uso: setup.ps1 db <mysql|pgsql|mongo> <comando> [args...]"; exit 1 }
        $db = $Args[0].ToLower()
        $cmd = $Args[1].ToLower()
        switch ($db) {
            "mysql" {
                $mysqlExe = Get-ChildItem $mysqlDir -Recurse -Filter "mysql.exe" | Select-Object -First 1
                if (-not $mysqlExe) { Write-ErrorMsg "mysql.exe não encontrado."; return }
                switch ($cmd) {
                    "list" { & $mysqlExe.FullName -e "SHOW DATABASES;" }
                    "create" { & $mysqlExe.FullName -e "CREATE DATABASE $($Args[2]);" }
                    "drop" { & $mysqlExe.FullName -e "DROP DATABASE $($Args[2]);" }
                    default { Write-Host "Comando db mysql desconhecido." }
                }
            }
            "pgsql" {
                $psqlExe = Get-ChildItem $pgsqlDir -Recurse -Filter "psql.exe" | Select-Object -First 1
                if (-not $psqlExe) { Write-ErrorMsg "psql.exe não encontrado."; return }
                switch ($cmd) {
                    "list" { & $psqlExe.FullName -c "\l" }
                    "create" { & $psqlExe.FullName -c "CREATE DATABASE $($Args[2]);" }
                    "drop" { & $psqlExe.FullName -c "DROP DATABASE $($Args[2]);" }
                    default { Write-Host "Comando db pgsql desconhecido." }
                }
            }
            "mongo" {
                $mongoExe = Get-ChildItem $mongoDir -Recurse -Filter "mongo.exe" | Select-Object -First 1
                if (-not $mongoExe) { Write-ErrorMsg "mongo.exe não encontrado."; return }
                switch ($cmd) {
                    "list" { & $mongoExe.FullName --eval "db.adminCommand('listDatabases')" }
                    "create" { & $mongoExe.FullName --eval "db.getSiblingDB('$($Args[2])')" }
                    "drop" { & $mongoExe.FullName --eval "db.getSiblingDB('$($Args[2])').dropDatabase()" }
                    default { Write-Host "Comando db mongo desconhecido." }
                }
            }
            default { Write-Host "Banco de dados não suportado: $($db)" }
        }
    }
    "service" {
        Write-Log "Comando executado: service $($Args -join ' ')"
        # Lista serviços DevStack (Windows Services)
        $services = Get-Service | Where-Object { $_.DisplayName -like '*devstack*' -or $_.ServiceType -eq 'Win32OwnProcess' }
        if ($services) {
            $services | Format-Table Name, Status, DisplayName
        } else {
            Write-Host "Nenhum serviço DevStack encontrado."
        }
    }
    "doctor" {
        Write-Log "Comando executado: doctor $($Args -join ' ')"
        # Diagnóstico do ambiente DevStack
        Write-Host "Diagnóstico do ambiente DevStack:"
        List-InstalledVersions
        # Tabela PATH
        $pathList = $env:Path -split ';'
        $maxPathLen = ($pathList | Measure-Object -Property Length -Maximum).Maximum
        $headerPath = ('_' * ($maxPathLen + 4))
        Write-Host $headerPath -ForegroundColor Gray
        Write-Host ("| {0,-$maxPathLen} |" -f 'PATH') -ForegroundColor Gray
        Write-Host ("|" + ('-' * ($maxPathLen + 2)) + "|") -ForegroundColor Gray
        foreach ($p in $pathList) {
            if (![string]::IsNullOrWhiteSpace($p)) {
                Write-Host ("| {0,-$maxPathLen} |" -f $p) -ForegroundColor DarkGray
            }
        }
        Write-Host ("¯" * ($maxPathLen + 4)) -ForegroundColor Gray
        # Tabela Usuário
        $user = $env:USERNAME
        $userLen = $user.Length
        $colUser = [Math]::Max(8, $userLen)
        $headerUser = ('_' * ($colUser + 4))
        Write-Host $headerUser -ForegroundColor Gray
        # Usar a função Write-Info que já foi melhorada para tratar UTF-8
        Write-Info ("| {0,-$colUser} |" -f 'Usuário')
        Write-Host ("|" + ('-' * ($colUser + 2)) + "|") -ForegroundColor Gray
        Write-Host -NoNewline "| " -ForegroundColor Gray
        Write-Host -NoNewline ("{0,-$colUser}" -f $user) -ForegroundColor Cyan
        Write-Host " |" -ForegroundColor Gray
        Write-Host ("¯" * ($colUser + 4)) -ForegroundColor Gray
        # Tabela Sistema
        $os = $env:OS
        $osLen = $os.Length
        $colOS = [Math]::Max(8, $osLen)
        $headerOS = ('_' * ($colOS + 4))
        Write-Host $headerOS -ForegroundColor Gray
        Write-Host ("| {0,-$colOS} |" -f 'Sistema') -ForegroundColor Gray
        Write-Host ("|" + ('-' * ($colOS + 2)) + "|") -ForegroundColor Gray
        Write-Host -NoNewline "| " -ForegroundColor Gray
        Write-Host -NoNewline ("{0,-$colOS}" -f $os) -ForegroundColor Cyan
        Write-Host " |" -ForegroundColor Gray
        Write-Host ("¯" * ($colOS + 4)) -ForegroundColor Gray
    }
    "global" {
        Write-Log "Comando executado: global $($Args -join ' ')"
        $devstackDir = $PSScriptRoot
        $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
        if ($currentPath -notlike "*$devstackDir*") {
            [Environment]::SetEnvironmentVariable("Path", "$currentPath;$devstackDir", "User")
            Write-Host "Diretório $devstackDir adicionado ao PATH do usuário." -ForegroundColor Green
        } else {
            Write-Host "Diretório $devstackDir já está no PATH do usuário." -ForegroundColor Yellow
        }
        $profilePath = $PROFILE
        $aliasLine = "Set-Alias devstack '$devstackDir\setup.ps1'"
        if (-not (Test-Path $profilePath)) { New-Item -ItemType File -Path $profilePath -Force | Out-Null }
        $profileContent = Get-Content $profilePath -Raw
        if ($profileContent -notmatch "devstack.*setup.ps1") {
            Add-Content $profilePath "`n$aliasLine"
            Write-Host "Alias 'devstack' adicionado ao seu perfil do PowerShell." -ForegroundColor Green
        } else {
            Write-Host "Alias 'devstack' já existe no seu perfil do PowerShell." -ForegroundColor Yellow
        }
        Write-Host "Agora você pode rodar 'devstack' ou 'setup.ps1' de qualquer lugar no terminal." -ForegroundColor Cyan
    }    
    "gui" {
        Write-Log "Comando executado: gui"
        # Inicia a GUI do DevStack
        try {
            if ($Args.Count -gt 0 -and $Args[0] -and $Args[0].ToLower() -eq "--isnew") {
                Invoke-DevStackGUI
            } else {
                # Determinar qual versão do PowerShell usar
                $psExecutable = $null
                $psArgs = @("-NoProfile", "-ExecutionPolicy", "Bypass")
                # Verificar se estamos em Windows PowerShell ou PowerShell Core
                if ($PSVersionTable.PSEdition -eq "Core") {
                    if (Get-Command "pwsh" -ErrorAction SilentlyContinue) {
                        Write-Log "Usando PowerShell Core (pwsh.exe)"
                        $psExecutable = "pwsh"
                    }
                }
                if (-not $psExecutable) {
                    Write-Log "Usando Windows PowerShell (powershell.exe)"
                    $psExecutable = "powershell"
                }
                $guiArgs = @(
                    "-WindowStyle", "Hidden", 
                    "-Command", "& { . '$PSScriptRoot\setup.ps1' gui --IsNew }"
                )
                $finalArgs = $psArgs + $guiArgs
                Write-Log "Iniciando GUI em novo processo: $psExecutable $finalArgs"
                $process = Start-Process -FilePath $psExecutable -ArgumentList $finalArgs -WindowStyle Hidden -PassThru
                if ($null -ne $process) {
                    Write-Host "Interface gráfica iniciada com sucesso (PID: $($process.Id))" -ForegroundColor Green
                } else {
                    Write-ErrorMsg "Falha ao iniciar a interface gráfica"
                }
            }
        } catch {
            Write-ErrorMsg "Erro ao iniciar a interface gráfica: $_"
        }
    }
    default {
        Write-Host "Comando desconhecido: $Command"
    }
}