using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace DevStackManager
{
    public class Program
    {
        // Win32 API declarations for console window management
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

        public static string baseDir => DevStackConfig.baseDir;
        public static string phpDir => DevStackConfig.phpDir;
        public static string nginxDir => DevStackConfig.nginxDir;
        public static string mysqlDir => DevStackConfig.mysqlDir;
        public static string nodeDir => DevStackConfig.nodeDir;
        public static string pythonDir => DevStackConfig.pythonDir;
        public static string composerDir => DevStackConfig.composerDir;
        public static string pmaDir => DevStackConfig.pmaDir;
        public static string mongoDir => DevStackConfig.mongoDir;
        public static string redisDir => DevStackConfig.redisDir;
        public static string pgsqlDir => DevStackConfig.pgsqlDir;
        public static string mailhogDir => DevStackConfig.mailhogDir;
        public static string elasticDir => DevStackConfig.elasticDir;
        public static string memcachedDir => DevStackConfig.memcachedDir;
        public static string dockerDir => DevStackConfig.dockerDir;
        public static string yarnDir => DevStackConfig.yarnDir;
        public static string pnpmDir => DevStackConfig.pnpmDir;
        public static string wpcliDir => DevStackConfig.wpcliDir;
        public static string adminerDir => DevStackConfig.adminerDir;
        public static string poetryDir => DevStackConfig.poetryDir;
        public static string rubyDir => DevStackConfig.rubyDir;
        public static string goDir => DevStackConfig.goDir;
        public static string certbotDir => DevStackConfig.certbotDir;
        public static string openSSLDir => DevStackConfig.openSSLDir;
        public static string phpcsfixerDir => DevStackConfig.phpcsfixerDir;
        public static string nginxSitesDir => DevStackConfig.nginxSitesDir;
        public static string tmpDir => DevStackConfig.tmpDir;
        
        public static PathManager? pathManager => DevStackConfig.pathManager;

        /// <summary>
        /// Determina se o aplicativo foi iniciado por duplo clique (Explorer) ou por linha de comando
        /// </summary>
        private static bool IsStartedFromExplorer()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var parentProcessId = GetParentProcessId(currentProcess.Id);
                
                if (parentProcessId > 0)
                {
                    var parentProcess = Process.GetProcessById(parentProcessId);
                    return parentProcess.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                // Se não conseguirmos determinar, assumimos que foi CLI
            }
            return false;
        }

        /// <summary>
        /// Obtém o ID do processo pai
        /// </summary>
        private static int GetParentProcessId(int processId)
        {
            try
            {
                using (var query = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    using (var results = query.Get())
                    {
                        foreach (ManagementObject result in results)
                        {
                            return Convert.ToInt32(result["ParentProcessId"]);
                        }
                    }
                }
            }
            catch
            {
                // Se falhar, retorna 0
            }
            return 0;
        }

        public static int Main(string[] args)
        {
            // Configurar console para CLI
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Se não há argumentos, mostrar help
            if (args.Length == 0)
            {
                ShowUsage();
                return 0;
            }

            string command = args[0].ToLowerInvariant();
            string[] commandArgs = args.Skip(1).ToArray();

            try
            {
                LoadConfiguration();
                return ExecuteCommand(command, commandArgs);
            }
            catch (Exception ex)
            {
                WriteErrorMsg($"Erro inesperado: {ex.Message}");
                WriteLog($"Erro inesperado: {ex}");
                return 1;
            }
        }

        private static void LoadConfiguration()
        {
            DevStackConfig.Initialize();
        }

        private static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteWarningMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteErrorMsg(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteColoredLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteLog(string message)
        {
            try
            {
                string logFile = Path.Combine(baseDir, "devstack.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private static void StatusComponent(string component)
        {
            var status = GetComponentStatus(component);
            
            if (!status.installed)
            {
                WriteWarningMsg(status.message);
                return;
            }
            
            WriteInfo($"{component} instalado(s):");
            foreach (var version in status.versions)
            {
                Console.WriteLine($"  {version}");
            }
        }

        private static void StatusAll()
        {
            Console.WriteLine("Status do DevStack:");
            
            var allStatus = GetAllComponentsStatus();
            
            foreach (var comp in allStatus.Keys)
            {
                if (allStatus[comp].installed)
                {
                    WriteInfo($"{comp} instalado(s):");
                    foreach (var version in allStatus[comp].versions)
                    {
                        Console.WriteLine($"  {version}");
                    }
                }
                else
                {
                    WriteWarningMsg($"{comp} não está instalado.");
                }
            }
        }

        private static void TestAll()
        {
            Console.WriteLine("Testando ferramentas instaladas:");
            
            var tools = new[]
            {
                new { name = "php", exe = "php-*.exe", dir = phpDir, args = "-v" },
                new { name = "nginx", exe = "nginx-*.exe", dir = nginxDir, args = "-v" },
                new { name = "mysql", exe = "mysqld-*.exe", dir = mysqlDir, args = "--version" },
                new { name = "nodejs", exe = "node-*.exe", dir = nodeDir, args = "-v" },
                new { name = "python", exe = "python-*.exe", dir = pythonDir, args = "--version" },
                new { name = "git", exe = "git.exe", dir = baseDir, args = "--version" },
                new { name = "composer", exe = "composer-*.exe", dir = composerDir, args = "--version" },
                new { name = "phpmyadmin", exe = "index.php", dir = pmaDir, args = "" },
                new { name = "mongodb", exe = "mongo.exe", dir = mongoDir, args = "--version" },
                new { name = "redis", exe = "redis-server.exe", dir = redisDir, args = "--version" },
                new { name = "pgsql", exe = "psql.exe", dir = pgsqlDir, args = "--version" },
                new { name = "mailhog", exe = "mailhog.exe", dir = mailhogDir, args = "--version" },
                new { name = "elasticsearch", exe = "elasticsearch.exe", dir = elasticDir, args = "--version" },
                new { name = "memcached", exe = "memcached.exe", dir = memcachedDir, args = "-h" },
                new { name = "docker", exe = "docker.exe", dir = dockerDir, args = "--version" },
                new { name = "yarn", exe = "yarn.cmd", dir = yarnDir, args = "--version" },
                new { name = "pnpm", exe = "pnpm.exe", dir = pnpmDir, args = "--version" },
                new { name = "wpcli", exe = "wp-cli.phar", dir = wpcliDir, args = "--version" },
                new { name = "adminer", exe = "adminer-*.php", dir = adminerDir, args = "" },
                new { name = "poetry", exe = "poetry.exe", dir = poetryDir, args = "--version" },
                new { name = "ruby", exe = "ruby.exe", dir = rubyDir, args = "--version" },
                new { name = "go", exe = "go.exe", dir = goDir, args = "version" },
                new { name = "certbot", exe = "certbot.exe", dir = certbotDir, args = "--version" },
                new { name = "openssl", exe = "openssl.exe", dir = openSSLDir, args = "version" },
                new { name = "phpcsfixer", exe = "php-cs-fixer-*.exe", dir = phpcsfixerDir, args = "--version" }
            };

            foreach (var tool in tools)
            {
                string? found = null;
                
                if (tool.name == "git")
                {
                    found = FindFile(tool.dir, tool.exe, true, "*\\cmd\\git.exe");
                }
                else
                {
                    found = FindFile(tool.dir, tool.exe, true);
                }

                if (!string.IsNullOrEmpty(found))
                {
                    try
                    {
                        var output = ExecuteProcess(found, tool.args);
                        WriteInfo($"{tool.name}: {output}");
                    }
                    catch
                    {
                        WriteErrorMsg($"{tool.name}: erro ao executar {found}");
                    }
                }
                else
                {
                    WriteWarningMsg($"{tool.name}: não encontrado.");
                }
            }
        }

        private static void DepsCheck()
        {
            Console.WriteLine("Verificando dependências do sistema...");
            var missing = new List<string>();

            if (!IsAdministrator())
            {
                missing.Add("Permissão de administrador");
            }

            if (missing.Count == 0)
            {
                WriteInfo("Todas as dependências estão presentes.");
            }
            else
            {
                WriteErrorMsg($"Dependências ausentes: {string.Join(", ", missing)}");
            }
        }

        private static string CenterText(string text, int width)
        {
            int pad = Math.Max(0, width - text.Length);
            int padLeft = (int)Math.Floor(pad / 2.0);
            int padRight = pad - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        private static void UpdateComponent(string component)
        {
            _ = HandleInstallCommand([component]);
        }

        private static void AliasComponent(string component, string version)
        {
            string aliasDir = Path.Combine(baseDir, "aliases");
            if (!Directory.Exists(aliasDir))
            {
                Directory.CreateDirectory(aliasDir);
            }

            string? exe = component.ToLowerInvariant() switch
            {
                "php" => Path.Combine(phpDir, $"php-{version}", $"php-{version}.exe"),
                "nginx" => Path.Combine(nginxDir, $"nginx-{version}", $"nginx-{version}.exe"),
                "nodejs" => Path.Combine(nodeDir, $"node-v{version}-win-x64", $"node-{version}.exe"),
                "python" => Path.Combine(pythonDir, $"python-{version}", $"python-{version}.exe"),
                "git" => Path.Combine(baseDir, $"git-{version}", "cmd", "git.exe"),
                "mysql" => Path.Combine(mysqlDir, $"mysql-{version}", "bin", "mysql.exe"),
                "phpmyadmin" => Path.Combine(pmaDir, $"phpmyadmin-{version}", "index.php"),
                "mongodb" => Path.Combine(mongoDir, $"mongodb-{version}", "bin", "mongo.exe"),
                "redis" => Path.Combine(redisDir, $"redis-{version}", "redis-server.exe"),
                "pgsql" => Path.Combine(pgsqlDir, $"pgsql-{version}", "bin", "psql.exe"),
                "mailhog" => Path.Combine(mailhogDir, $"mailhog-{version}", "mailhog.exe"),
                "elasticsearch" => Path.Combine(elasticDir, $"elasticsearch-{version}", "bin", "elasticsearch.exe"),
                "memcached" => Path.Combine(memcachedDir, $"memcached-{version}", "memcached.exe"),
                "docker" => Path.Combine(dockerDir, $"docker-{version}", "docker.exe"),
                "yarn" => Path.Combine(yarnDir, $"yarn-v{version}", "bin", "yarn.cmd"),
                "pnpm" => Path.Combine(pnpmDir, $"pnpm-v{version}", "pnpm.exe"),
                "wpcli" => Path.Combine(wpcliDir, $"wp-cli-{version}", "wp-cli.phar"),
                "adminer" => Path.Combine(adminerDir, $"adminer-{version}.php"),
                "poetry" => Path.Combine(poetryDir, $"poetry-{version}", "bin", "poetry.exe"),
                "ruby" => Path.Combine(rubyDir, $"ruby-{version}", "bin", "ruby.exe"),
                "go" => Path.Combine(goDir, $"go{version}", "bin", "go.exe"),
                "certbot" => Path.Combine(certbotDir, $"certbot-{version}", "certbot.exe"),
                "openssl" => Path.Combine(openSSLDir, $"openssl-{version}", "bin", "openssl.exe"),
                "phpcsfixer" => Path.Combine(phpcsfixerDir, $"php-cs-fixer-{version}", $"php-cs-fixer-{version}.exe"),
                _ => null
            };

            if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
            {
                string bat = Path.Combine(aliasDir, $"{component}{version}.bat");
                File.WriteAllText(bat, $"@echo off\r\n\"{exe}\" %*", Encoding.UTF8);
                WriteInfo($"Alias criado: {bat}");
            }
            else
            {
                WriteErrorMsg($"Executável não encontrado para {component} {version}");
            }
        }

        private static void ShowUsage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DevStack CLI - Comandos disponíveis:");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Para interface gráfica, use: DevStackGUI.exe");
            Console.WriteLine();

            var cmds = new[]
            {
                new { cmd = "install <componente> [versão]", desc = "Instala uma ferramenta ou versão específica." },
                new { cmd = "uninstall <componente> [versão]", desc = "Remove uma ferramenta ou versão específica." },
                new { cmd = "list <componente|--installed>", desc = "Lista versões disponíveis ou instaladas." },
                new { cmd = "path [add|remove|list|help]", desc = "Gerencia PATH das ferramentas instaladas." },
                new { cmd = "status", desc = "Mostra status de todas as ferramentas." },
                new { cmd = "test", desc = "Testa todas as ferramentas instaladas." },
                new { cmd = "update <componente>", desc = "Atualiza uma ferramenta para a última versão." },
                new { cmd = "deps", desc = "Verifica dependências do sistema." },
                new { cmd = "alias <componente> <versão>", desc = "Cria um alias .bat para a versão da ferramenta." },
                new { cmd = "global", desc = "Adiciona DevStack ao PATH e cria alias global." },
                new { cmd = "self-update", desc = "Atualiza o DevStackManager." },
                new { cmd = "clean", desc = "Remove logs e arquivos temporários." },
                new { cmd = "backup", desc = "Cria backup das configs e logs." },
                new { cmd = "logs", desc = "Exibe as últimas linhas do log." },
                new { cmd = "enable <serviço>", desc = "Ativa um serviço do Windows." },
                new { cmd = "disable <serviço>", desc = "Desativa um serviço do Windows." },
                new { cmd = "config", desc = "Abre o diretório de configuração." },
                new { cmd = "reset <componente>", desc = "Remove e reinstala uma ferramenta." },
                new { cmd = "proxy [set <url>|unset|show]", desc = "Gerencia variáveis de proxy." },
                new { cmd = "ssl <domínio> [-openssl <versão>]", desc = "Gera certificado SSL autoassinado." },
                new { cmd = "db <mysql|pgsql|mongo> <comando> [args...]", desc = "Gerencia bancos de dados básicos." },
                new { cmd = "service", desc = "Lista serviços DevStack (Windows)." },
                new { cmd = "doctor", desc = "Diagnóstico do ambiente DevStack." },
                new { cmd = "site <domínio> [opções]", desc = "Cria configuração de site nginx." },
                new { cmd = "help", desc = "Exibe esta ajuda." }
            };

            int col1 = 50, col2 = 60;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('_', col1 + col2 + 3));
            Console.WriteLine($"|{CenterText("Comando", col1)}|{CenterText("Descrição", col2)}|");
            Console.WriteLine($"|{new string('-', col1)}+{new string('-', col2)}|");

            foreach (var c in cmds)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("|");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(CenterText(c.cmd, col1));
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("|");
                Console.ResetColor();
                Console.Write(CenterText(c.desc, col2));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("|");
            }
            Console.WriteLine($"|{new string('_', col1)}+{new string('_', col2)}|");
            Console.ResetColor();
        }

        private static int ExecuteCommand(string command, string[] args)
        {
            WriteLog($"Comando executado: {command} {string.Join(" ", args)}");

            switch (command)
            {
                case "help":
                    ShowUsage();
                    break;

                case "list":
                    return HandleListCommand(args);

                case "site":
                    return HandleSiteCommand(args);

                case "install":
                    return HandleInstallCommand(args).Result;

                case "path":
                    return HandlePathCommand(args);

                case "uninstall":
                    return HandleUninstallCommand(args);

                case "start":
                    return HandleStartCommand(args);

                case "stop":
                    return HandleStopCommand(args);

                case "restart":
                    return HandleRestartCommand(args);

                case "status":
                    StatusAll();
                    break;

                case "test":
                    TestAll();
                    break;

                case "deps":
                    DepsCheck();
                    break;

                case "update":
                    foreach (var component in args)
                    {
                        UpdateComponent(component);
                    }
                    break;

                case "alias":
                    return HandleAliasCommand(args);

                case "self-update":
                    return HandleSelfUpdateCommand();

                case "clean":
                    return HandleCleanCommand();

                case "backup":
                    return HandleBackupCommand();

                case "logs":
                    return HandleLogsCommand();

                case "enable":
                    return HandleEnableCommand(args);

                case "disable":
                    return HandleDisableCommand(args);

                case "config":
                    return HandleConfigCommand();

                case "reset":
                    return HandleResetCommand(args);

                case "proxy":
                    return HandleProxyCommand(args);

                case "ssl":
                    return HandleSslCommand(args).GetAwaiter().GetResult();

                case "db":
                    return HandleDbCommand(args);

                case "service":
                    return HandleServiceCommand();

                case "doctor":
                    return HandleDoctorCommand();

                case "global":
                    return HandleGlobalCommand();

                default:
                    Console.WriteLine($"Comando desconhecido: {command}");
                    return 1;
            }

            return 0;
        }

        private static int HandleListCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso: DevStackManager list <php|nodejs|python|composer|mysql|nginx|phpmyadmin|git|mongodb|redis|pgsql|mailhog|elasticsearch|memcached|docker|yarn|pnpm|wpcli|adminer|poetry|ruby|go|certbot|openssl|phpcsfixer|--installed>");
                return 1;
            }

            string firstArg = args[0].Trim();
            if (firstArg == "--installed")
            {
                ListManager.ListInstalledVersions();
                return 0;
            }

            ListManager.ListVersions(firstArg);
            return 0;
        }

        private static int HandleSiteCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager site <dominio> [-root <diretorio>] [-php <php-upstream>] [-nginx <nginx-version>]");
                return 1;
            }

            string domain = args[0];
            string? root = null;
            string? phpUpstream = null;
            string? nginxVersion = null;
            string? indexLocation = null;

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-root":
                        if (++i < args.Length) root = args[i];
                        break;
                    case "-php":
                        if (++i < args.Length) phpUpstream = args[i];
                        break;
                    case "-nginx":
                        if (++i < args.Length) nginxVersion = args[i];
                        break;
                    case "-index":
                        if (++i < args.Length) indexLocation = args[i];
                        break;
                }
            }

            CreateNginxSiteConfig(domain, root, phpUpstream, nginxVersion, indexLocation);
            return 0;
        }

        private static async Task<int> HandleInstallCommand(string[] args)
        {
            await InstallCommands(args);
            return 0;
        }

        private static int HandleUninstallCommand(string[] args)
        {
            UninstallCommands(args);
            return 0;
        }

        private static int HandlePathCommand(string[] args)
        {
            if (pathManager == null)
            {
                WriteErrorMsg("PathManager não foi inicializado.");
                return 1;
            }

            if (args.Length == 0)
            {
                pathManager.AddBinDirsToPath();
                return 0;
            }

            string subCommand = args[0].ToLowerInvariant();
            switch (subCommand)
            {
                case "add":
                    pathManager.AddBinDirsToPath();
                    break;

                case "remove":
                    if (args.Length > 1)
                    {
                        var dirsToRemove = args.Skip(1).ToArray();
                        pathManager.RemoveFromPath(dirsToRemove);
                    }
                    else
                    {
                        pathManager.RemoveAllDevStackFromPath();
                    }
                    break;

                case "list":
                    pathManager.ListCurrentPath();
                    break;

                case "help":
                    WriteInfo("Uso do comando path:");
                    WriteInfo("  path         - Adiciona diretórios das ferramentas ao PATH");
                    WriteInfo("  path add     - Adiciona diretórios das ferramentas ao PATH");
                    WriteInfo("  path remove  - Remove todos os diretórios DevStack do PATH");
                    WriteInfo("  path remove <dir1> <dir2> ... - Remove diretórios específicos do PATH");
                    WriteInfo("  path list    - Lista todos os diretórios no PATH do usuário");
                    WriteInfo("  path help    - Mostra esta ajuda");
                    break;

                default:
                    WriteErrorMsg($"Subcomando desconhecido: {subCommand}");
                    WriteInfo("Use 'path help' para ver os comandos disponíveis.");
                    return 1;
            }

            return 0;
        }

        private static int HandleStartCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager start <nginx|php|--all> [<x.x.x>]");
                return 1;
            }

            string target = args[0].ToLowerInvariant();
            if (target == "--all")
            {
                if (Directory.Exists(nginxDir))
                {
                    ForEachVersion("nginx", v => StartComponent("nginx", v));
                }
                else
                {
                    WriteWarningMsg("Diretório do nginx não encontrado. Ignorando.");
                }

                if (Directory.Exists(phpDir))
                {
                    ForEachVersion("php", v => StartComponent("php", v));
                }
                else
                {
                    WriteWarningMsg("Diretório do PHP não encontrado. Ignorando.");
                }
                return 0;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Uso: DevStackManager start <nginx|php> <x.x.x>");
                return 1;
            }

            string version = args[1];
            StartComponent(target, version);
            return 0;
        }

        private static int HandleStopCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager stop <nginx|php|--all> [<x.x.x>]");
                return 1;
            }

            string target = args[0].ToLowerInvariant();
            if (target == "--all")
            {
                if (Directory.Exists(nginxDir))
                {
                    ForEachVersion("nginx", v => StopComponent("nginx", v));
                }
                else
                {
                    WriteWarningMsg("Diretório do nginx não encontrado. Ignorando.");
                }

                if (Directory.Exists(phpDir))
                {
                    ForEachVersion("php", v => StopComponent("php", v));
                }
                else
                {
                    WriteWarningMsg("Diretório do PHP não encontrado. Ignorando.");
                }
                return 0;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Uso: DevStackManager stop <nginx|php> <x.x.x>");
                return 1;
            }

            string version = args[1];
            StopComponent(target, version);
            return 0;
        }

        private static int HandleRestartCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager restart <nginx|php|--all> [<x.x.x>]");
                return 1;
            }

            string target = args[0].ToLowerInvariant();
            if (target == "--all")
            {
                if (Directory.Exists(nginxDir))
                {
                    ForEachVersion("nginx", v => 
                    {
                        StopComponent("nginx", v);
                        Thread.Sleep(1000);
                        StartComponent("nginx", v);
                    });
                }
                else
                {
                    WriteWarningMsg("Diretório do nginx não encontrado. Ignorando.");
                }

                if (Directory.Exists(phpDir))
                {
                    ForEachVersion("php", v => 
                    {
                        StopComponent("php", v);
                        Thread.Sleep(1000);
                        StartComponent("php", v);
                    });
                }
                else
                {
                    WriteWarningMsg("Diretório do PHP não encontrado. Ignorando.");
                }
                return 0;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Uso: DevStackManager restart <nginx|php> <x.x.x>");
                return 1;
            }

            string version = args[1];
            StopComponent(target, version);
            Thread.Sleep(1000);
            StartComponent(target, version);
            return 0;
        }

        private static int HandleAliasCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Uso: DevStackManager alias <componente> <versão>");
                return 1;
            }

            AliasComponent(args[0], args[1]);
            return 0;
        }

        private static int HandleSelfUpdateCommand()
        {
            string repoDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            if (Directory.Exists(Path.Combine(repoDir, ".git")))
            {
                WriteInfo("Atualizando via git pull...");
                try
                {
                    var result = ExecuteProcess("git", "pull", repoDir);
                    WriteInfo("DevStackManager atualizado com sucesso.");
                }
                catch (Exception ex)
                {
                    WriteErrorMsg($"Erro ao atualizar via git: {ex.Message}");
                }
            }
            else
            {
                WriteWarningMsg("Não é um repositório git. Atualize manualmente copiando os arquivos do repositório.");
            }
            return 0;
        }

        private static int HandleCleanCommand()
        {
            string logDir = Path.Combine(baseDir, "logs");
            string tmpDir = Path.Combine(baseDir, "tmp");
            string logFile = Path.Combine(baseDir, "devstack.log");
            int count = 0;

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
                count++;
            }

            if (Directory.Exists(logDir))
            {
                Directory.Delete(logDir, true);
                count++;
            }

            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, true);
                count++;
            }

            WriteInfo($"Limpeza concluída. ({count} itens removidos)");
            return 0;
        }

        private static int HandleBackupCommand()
        {
            string backupDir = Path.Combine(baseDir, $"backup-{DateTime.Now:yyyyMMdd-HHmmss}");
            string[] toBackup = { "configs", "devstack.log" };

            Directory.CreateDirectory(backupDir);

            foreach (string item in toBackup)
            {
                string src = Path.Combine(baseDir, item);
                if (File.Exists(src))
                {
                    File.Copy(src, Path.Combine(backupDir, item));
                }
                else if (Directory.Exists(src))
                {
                    CopyDirectory(src, Path.Combine(backupDir, item));
                }
            }

            WriteInfo($"Backup criado em {backupDir}");
            return 0;
        }

        private static int HandleLogsCommand()
        {
            string logFile = Path.Combine(baseDir, "devstack.log");
            if (File.Exists(logFile))
            {
                Console.WriteLine($"Últimas 50 linhas de {logFile}:");
                var lines = File.ReadAllLines(logFile, Encoding.UTF8);
                var lastLines = lines.Skip(Math.Max(0, lines.Length - 50));
                foreach (var line in lastLines)
                {
                    Console.WriteLine(line);
                }
            }
            else
            {
                WriteWarningMsg("Arquivo de log não encontrado.");
            }
            return 0;
        }

        private static int HandleEnableCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager enable <serviço>");
                return 1;
            }

            string svc = args[0];
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                var service = new ServiceController(svc);
                service.Start();
                WriteInfo($"Serviço {svc} ativado.");
            }
            catch (Exception ex)
            {
                WriteErrorMsg($"Erro ao ativar serviço {svc}: {ex.Message}");
            }
#pragma warning restore CA1416
            return 0;
        }

        private static int HandleDisableCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager disable <serviço>");
                return 1;
            }

            string svc = args[0];
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                var service = new ServiceController(svc);
                service.Stop();
                WriteInfo($"Serviço {svc} desativado.");
            }
            catch (Exception ex)
            {
                WriteErrorMsg($"Erro ao desativar serviço {svc}: {ex.Message}");
            }
#pragma warning restore CA1416
            return 0;
        }

        private static int HandleConfigCommand()
        {
            string configDir = Path.Combine(baseDir, "configs");
            if (Directory.Exists(configDir))
            {
                Process.Start("explorer.exe", configDir);
                WriteInfo("Diretório de configuração aberto.");
            }
            else
            {
                WriteWarningMsg("Diretório de configuração não encontrado.");
            }
            return 0;
        }

        private static int HandleResetCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager reset <componente>");
                return 1;
            }

            string comp = args[0];
            WriteInfo($"Resetando {comp}...");
            UninstallCommands(new[] { comp });
            _ = InstallCommands(new[] { comp });
            WriteInfo($"{comp} resetado.");
            return 0;
        }

        private static int HandleProxyCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"Proxy atual: {Environment.GetEnvironmentVariable("HTTP_PROXY")}");
                return 0;
            }

            switch (args[0])
            {
                case "set":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Uso: DevStackManager proxy set <url>");
                        return 1;
                    }
                    Environment.SetEnvironmentVariable("HTTP_PROXY", args[1]);
                    Environment.SetEnvironmentVariable("HTTPS_PROXY", args[1]);
                    WriteInfo($"Proxy definido para {args[1]}");
                    break;

                case "unset":
                    Environment.SetEnvironmentVariable("HTTP_PROXY", null);
                    Environment.SetEnvironmentVariable("HTTPS_PROXY", null);
                    WriteInfo("Proxy removido.");
                    break;

                default:
                    Console.WriteLine("Uso: DevStackManager proxy [set <url>|unset|show]");
                    break;
            }
            return 0;
        }

        private static async Task<int> HandleSslCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: DevStackManager ssl <dominio> [-openssl <versao>]");
                return 1;
            }

            string domain = args[0];
            string sslDir = Path.Combine(baseDir, "configs", "nginx", "ssl");
            if (!Directory.Exists(sslDir))
            {
                Directory.CreateDirectory(sslDir);
            }

            string crt = Path.Combine(sslDir, $"{domain}.crt");
            string key = Path.Combine(sslDir, $"{domain}.key");
            string? opensslVersion = null;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-openssl" && (i + 1) < args.Length)
                {
                    opensslVersion = args[i + 1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(opensslVersion))
            {
                opensslVersion = await InstallManager.GetLatestOpenSSLVersion();
            }

            string dir = Path.Combine(openSSLDir, $"openssl-{opensslVersion}", "bin");
            string opensslExe = Path.Combine(dir, "openssl.exe");

            if (!File.Exists(opensslExe))
            {
                Console.WriteLine($"OpenSSL {opensslVersion} não encontrado. Instalando...");
                await InstallManager.InstallOpenSSL(opensslVersion);
            }

            if (!File.Exists(opensslExe))
            {
                WriteErrorMsg($"OpenSSL {opensslVersion} não encontrado no PATH nem em {opensslExe}. Instale para usar este comando.");
                return 1;
            }

            ExecuteProcess(opensslExe, $"req -x509 -nodes -days 365 -newkey rsa:2048 -keyout \"{key}\" -out \"{crt}\" -subj \"/CN={domain}\"");
            WriteInfo($"Certificado gerado: {crt}, {key}");
            return 0;
        }

        private static int HandleDbCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Uso: DevStackManager db <mysql|pgsql|mongo> <comando> [args...]");
                return 1;
            }

            string db = args[0].ToLowerInvariant();
            string cmd = args[1].ToLowerInvariant();

            switch (db)
            {
                case "mysql":
                    {
                        string? mysqlExe = FindFile(mysqlDir, "mysql.exe", true);
                        if (string.IsNullOrEmpty(mysqlExe))
                        {
                            WriteErrorMsg("mysql.exe não encontrado.");
                            return 1;
                        }

                        switch (cmd)
                        {
                            case "list":
                                ExecuteProcess(mysqlExe, "-e \"SHOW DATABASES;\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ExecuteProcess(mysqlExe, $"-e \"CREATE DATABASE {args[2]};\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ExecuteProcess(mysqlExe, $"-e \"DROP DATABASE {args[2]};\"");
                                break;
                            default:
                                Console.WriteLine("Comando db mysql desconhecido.");
                                break;
                        }
                        break;
                    }

                case "pgsql":
                    {
                        string? psqlExe = FindFile(pgsqlDir, "psql.exe", true);
                        if (string.IsNullOrEmpty(psqlExe))
                        {
                            WriteErrorMsg("psql.exe não encontrado.");
                            return 1;
                        }

                        switch (cmd)
                        {
                            case "list":
                                ExecuteProcess(psqlExe, "-c \"\\l\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ExecuteProcess(psqlExe, $"-c \"CREATE DATABASE {args[2]};\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ExecuteProcess(psqlExe, $"-c \"DROP DATABASE {args[2]};\"");
                                break;
                            default:
                                Console.WriteLine("Comando db pgsql desconhecido.");
                                break;
                        }
                        break;
                    }

                case "mongo":
                    {
                        string? mongoExe = FindFile(mongoDir, "mongo.exe", true);
                        if (string.IsNullOrEmpty(mongoExe))
                        {
                            WriteErrorMsg("mongo.exe não encontrado.");
                            return 1;
                        }

                        switch (cmd)
                        {
                            case "list":
                                ExecuteProcess(mongoExe, "--eval \"db.adminCommand('listDatabases')\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ExecuteProcess(mongoExe, $"--eval \"db.getSiblingDB('{args[2]}')\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ExecuteProcess(mongoExe, $"--eval \"db.getSiblingDB('{args[2]}').dropDatabase()\"");
                                break;
                            default:
                                Console.WriteLine("Comando db mongo desconhecido.");
                                break;
                        }
                        break;
                    }

                default:
                    Console.WriteLine($"Banco de dados não suportado: {db}");
                    break;
            }
            return 0;
        }

        private static int HandleServiceCommand()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            try
            {
                var services = ServiceController.GetServices()
                    .Where(s => s.DisplayName.Contains("devstack", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (services.Any())
                {
                    Console.WriteLine($"{"Name".PadRight(20)} {"Status".PadRight(15)} {"DisplayName".PadRight(40)}");
                    Console.WriteLine(new string('-', 75));
                    foreach (var service in services)
                    {
                        Console.WriteLine($"{service.ServiceName.PadRight(20)} {service.Status.ToString().PadRight(15)} {service.DisplayName.PadRight(40)}");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhum serviço DevStack encontrado.");
                }
            }
            catch (Exception ex)
            {
                WriteErrorMsg($"Erro ao listar serviços: {ex.Message}");
            }
#pragma warning restore CA1416
            return 0;
        }

        private static int HandleDoctorCommand()
        {
            Console.WriteLine("Diagnóstico do ambiente DevStack:");
            ListManager.ListInstalledVersions();

            // Forçar sincronização do PATH com as configurações do usuário
            RefreshProcessPathFromUser();
            WriteInfo("PATH sincronizado com configurações do usuário.");

            // Tabela PATH (agora mostra o PATH atual do processo + do usuário)
            var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            
            // Combinar e remover duplicatas para mostrar o PATH efetivo
            var processPathList = processPath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var userPathList = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var combinedPaths = processPathList.Concat(userPathList).Distinct().ToArray();
            
            int maxPathLen = combinedPaths.Length > 0 ? combinedPaths.Max(p => p.Length) : 20;
            string headerPath = string.Concat(Enumerable.Repeat('_', maxPathLen + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerPath);
            Console.Write("| ");
            Console.ForegroundColor = ConsoleColor.Gray;
            string prefix = "PATH (Processo + Usuário + ";
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("DevStack");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(")");
            string fullText = prefix + "DevStack)";
            Console.WriteLine(new string(' ', maxPathLen - fullText.Length) + " |");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"|{string.Concat(Enumerable.Repeat('-', maxPathLen + 2))}|");
            
            foreach (string p in combinedPaths)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("| ");
                    Console.ResetColor();
                    
                    // Destacar paths do DevStack
                    if (p.Contains("devstack", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    Console.Write(p.PadRight(maxPathLen));
                    
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(" |");
                    Console.ResetColor();
                }
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(string.Concat(Enumerable.Repeat('¯', maxPathLen + 4)));
            Console.ResetColor();

            // Tabela Usuário
            string user = Environment.UserName;
            int colUser = Math.Max(8, user.Length);
            string headerUser = string.Concat(Enumerable.Repeat('_', colUser + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerUser);
            Console.WriteLine($"| {"Usuário".PadRight(colUser)} |");
            Console.WriteLine($"|{string.Concat(Enumerable.Repeat('-', colUser + 2))}|");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("| ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{user.PadRight(colUser)}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" |");
            Console.WriteLine(string.Concat(Enumerable.Repeat('¯', colUser + 4)));
            Console.ResetColor();

            // Tabela Sistema
            string os = Environment.OSVersion.ToString();
            int colOS = Math.Max(8, os.Length);
            string headerOS = string.Concat(Enumerable.Repeat('_', colOS + 4));
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(headerOS);
            Console.WriteLine($"| {"Sistema".PadRight(colOS)} |");
            Console.WriteLine($"|{string.Concat(Enumerable.Repeat('-', colOS + 2))}|");
            Console.Write("| ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{os.PadRight(colOS)}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" |");
            Console.WriteLine(string.Concat(Enumerable.Repeat('¯', colOS + 4)));
            Console.ResetColor();

            return 0;
        }

        private static int HandleGlobalCommand()
        {
            string devstackDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            string currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            
            if (!currentPath.Contains(devstackDir))
            {
                Environment.SetEnvironmentVariable("Path", $"{currentPath};{devstackDir}", EnvironmentVariableTarget.User);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Diretório {devstackDir} adicionado ao PATH do usuário.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Diretório {devstackDir} já está no PATH do usuário.");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Agora você pode rodar 'DevStackManager' de qualquer lugar no terminal.");
            Console.ResetColor();
            
            return 0;
        }

        /// <summary>
        /// Sincroniza o PATH do processo atual com o PATH do usuário
        /// Isso garante que mudanças no PATH do usuário sejam refletidas imediatamente
        /// </summary>
        private static void RefreshProcessPathFromUser()
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
                var machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) ?? "";
                var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                
                // Combinar Machine PATH + User PATH (ordem padrão do Windows)
                var combinedPath = string.IsNullOrEmpty(machinePath) ? userPath : 
                                  string.IsNullOrEmpty(userPath) ? machinePath : 
                                  $"{machinePath};{userPath}";
                
                // Atualizar PATH do processo atual
                Environment.SetEnvironmentVariable("PATH", combinedPath);
                
                Program.WriteLog($"PATH do processo sincronizado com PATH do usuário. Total de entradas: {combinedPath.Split(';').Length}");
            }
            catch (Exception ex)
            {
                Program.WriteLog($"Erro ao sincronizar PATH: {ex.Message}");
            }
        }

        // Helper methods that need to be implemented
        private static (bool installed, string message, string[] versions) GetComponentStatus(string component)
        {
            // Implementation needed - placeholder
            return (false, $"{component} não está instalado.", Array.Empty<string>());
        }

        private static Dictionary<string, (bool installed, string message, string[] versions)> GetAllComponentsStatus()
        {
            // Implementation needed - placeholder
            return new Dictionary<string, (bool installed, string message, string[] versions)>();
        }

        private static string? FindFile(string directory, string pattern, bool recursive, string? pathFilter = null)
        {
            try
            {
                if (!Directory.Exists(directory)) return null;

                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(directory, pattern, searchOption);
                
                if (!string.IsNullOrEmpty(pathFilter))
                {
                    files = files.Where(f => f.Contains(pathFilter)).ToArray();
                }

                return files.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string ExecuteProcess(string fileName, string arguments, string? workingDirectory = null)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? ""
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Trim();
        }

#pragma warning disable CA1416 // Validate platform compatibility
        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore CA1416

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        private static void CreateNginxSiteConfig(string domain, string? root, string? phpUpstream, string? nginxVersion, string? indexLocation) 
        { 
            InstallManager.CreateNginxSiteConfig(domain, root, phpUpstream, nginxVersion, indexLocation);
        }
        
        private static async Task InstallCommands(string[] args) 
        {
            await InstallManager.InstallCommands(args);
            
            // Atualizar PATH após instalação bem-sucedida
            if (pathManager != null)
            {
                pathManager.AddBinDirsToPath();
            }
        }
        
        private static void UninstallCommands(string[] args) 
        { 
            // Extrair os componentes e versões específicas dos argumentos
            var specificVersions = new List<string>();
            
            foreach (string arg in args)
            {
                // Tentar extrair componente e versão
                if (Regex.IsMatch(arg, @"^php-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^php-", "");
                    specificVersions.Add($"php:{version}");
                }
                else if (Regex.IsMatch(arg, @"^nginx-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^nginx-", "");
                    specificVersions.Add($"nginx:{version}");
                }
                else if (Regex.IsMatch(arg, @"^mysql-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^mysql-", "");
                    specificVersions.Add($"mysql:{version}");
                }
                else if (Regex.IsMatch(arg, @"^nodejs-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^nodejs-", "");
                    specificVersions.Add($"nodejs:{version}");
                }
                else if (Regex.IsMatch(arg, @"^python-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^python-", "");
                    specificVersions.Add($"python:{version}");
                }
                else if (Regex.IsMatch(arg, @"^git-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^git-", "");
                    specificVersions.Add($"git:{version}");
                }
                else if (Regex.IsMatch(arg, @"^composer-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^composer-", "");
                    specificVersions.Add($"composer:{version}");
                }
                // Para comandos sem versão específica, detectar se há argumentos adicionais
                else
                {
                    // Para comandos como "uninstall php 8.4.10", o próximo argumento seria a versão
                    int currentIndex = Array.IndexOf(args, arg);
                    if (currentIndex >= 0 && currentIndex + 1 < args.Length)
                    {
                        string nextArg = args[currentIndex + 1];
                        // Verificar se o próximo argumento parece uma versão
                        if (Regex.IsMatch(nextArg, @"^\d+\.\d+"))
                        {
                            string component = arg.ToLowerInvariant();
                            if (component == "node") component = "nodejs";
                            specificVersions.Add($"{component}:{nextArg}");
                        }
                    }
                }
            }
            
            UninstallManager.UninstallCommands(args);
        }
        
        private static void AddBinDirsToPath() 
        { 
            if (pathManager != null)
            {
                pathManager.AddBinDirsToPath();
            }
            else
            {
                WriteWarningMsg("PathManager não foi inicializado");
            }
        }
        
        private static void ForEachVersion(string component, Action<string> action)
        {
            ProcessManager.ForEachVersion(component, action);
        }
        
        private static void StartComponent(string component, string version) 
        { 
            ProcessManager.StartComponent(component, version);
        }
        
        private static void StopComponent(string component, string version) 
        { 
            ProcessManager.StopComponent(component, version);
        }

    }
}
