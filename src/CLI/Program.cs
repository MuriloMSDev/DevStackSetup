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
        public static string pgsqlDir => DevStackConfig.pgsqlDir;
        public static string elasticDir => DevStackConfig.elasticDir;
        public static string wpcliDir => DevStackConfig.wpcliDir;
        public static string adminerDir => DevStackConfig.adminerDir;
        public static string goDir => DevStackConfig.goDir;
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

            try
            {
                LoadConfiguration();

                // Se não há argumentos, entrar em modo REPL
                if (args.Length == 0)
                {
                    Console.WriteLine("DevStack Shell Interativo. Digite 'help' para ajuda ou digite 'exit' para sair.");
                    while (true)
                    {
                        Console.Write("DevStack> ");
                        string? input = Console.ReadLine();
                        if (input == null) continue;
                        input = input.Trim();
                        if (string.IsNullOrEmpty(input)) continue;
                        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                            break;

                        // Separar comando e argumentos
                        var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string command = split[0].ToLowerInvariant();
                        string[] commandArgs = split.Skip(1).ToArray();
                        try
                        {
                            int result = ExecuteCommand(command, commandArgs);
                            // Opcional: mostrar código de saída se não for 0
                            if (result != 0)
                                Console.WriteLine($"(código de saída: {result})");
                        }
                        catch (Exception ex)
                        {
                            WriteErrorMsg($"Erro inesperado: {ex.Message}");
                            WriteLog($"Erro inesperado: {ex}");
                        }
                    }
                    return 0;
                }

                // Modo tradicional (com argumentos)
                string commandArg = args[0].ToLowerInvariant();
                string[] commandArgsArr = args.Skip(1).ToArray();
                return ExecuteCommand(commandArg, commandArgsArr);
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
            var allStatus = DevStackManager.DataManager.GetAllComponentsStatus();
            foreach (var comp in allStatus.Keys)
            {
                var status = allStatus[comp];
                if (status.Installed && status.Versions.Count > 0)
                {
                    // Detectar se é serviço monitorado
                    var isService = comp == "php" || comp == "nginx";
                    WriteInfo($"{comp} instalado(s):");
                    foreach (var version in status.Versions)
                    {
                        if (isService)
                        {
                            bool running = status.RunningList != null && status.RunningList.TryGetValue(version, out var isRunning) && isRunning;
                            string runningText = running ? "[executando]" : "[parado]";
                            Console.WriteLine($"  {version} {runningText}");
                        }
                        else
                        {
                            Console.WriteLine($"  {version}");
                        }
                    }
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
                new { name = "node", exe = "node-*.exe", dir = nodeDir, args = "-v" },
                new { name = "python", exe = "python-*.exe", dir = pythonDir, args = "--version" },
                new { name = "git", exe = "git.exe", dir = baseDir, args = "--version" },
                new { name = "composer", exe = "composer-*.phar", dir = composerDir, args = "--version" },
                new { name = "phpmyadmin", exe = "index.php", dir = pmaDir, args = "" },
                new { name = "mongodb", exe = "mongo.exe", dir = mongoDir, args = "--version" },
                new { name = "pgsql", exe = "psql.exe", dir = pgsqlDir, args = "--version" },
                new { name = "elasticsearch", exe = "elasticsearch.exe", dir = elasticDir, args = "--version" },
                new { name = "wpcli", exe = "wp-cli.phar", dir = wpcliDir, args = "--version" },
                new { name = "adminer", exe = "adminer-*.php", dir = adminerDir, args = "" },
                new { name = "go", exe = "go.exe", dir = goDir, args = "version" },
                new { name = "openssl", exe = "openssl.exe", dir = openSSLDir, args = "version" },
                new { name = "phpcsfixer", exe = "php-cs-fixer-*.phar", dir = phpcsfixerDir, args = "--version" }
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
                        var output = ProcessManager.ExecuteProcess(found, tool.args);
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
                "node" => Path.Combine(nodeDir, $"node-{version}", $"node-{version}.exe"),
                "python" => Path.Combine(pythonDir, $"python-{version}", $"python-{version}.exe"),
                "git" => Path.Combine(baseDir, $"git-{version}", "cmd", "git.exe"),
                "mysql" => Path.Combine(mysqlDir, $"mysql-{version}", "bin", "mysql.exe"),
                "phpmyadmin" => Path.Combine(pmaDir, $"phpmyadmin-{version}", "index.php"),
                "mongodb" => Path.Combine(mongoDir, $"mongodb-{version}", "bin", "mongo.exe"),
                "pgsql" => Path.Combine(pgsqlDir, $"pgsql-{version}", "bin", "psql.exe"),
                "elasticsearch" => Path.Combine(elasticDir, $"elasticsearch-{version}", "bin", "elasticsearch.exe"),
                "wpcli" => Path.Combine(wpcliDir, $"wp-cli-{version}", "wp-cli.phar"),
                "adminer" => Path.Combine(adminerDir, $"adminer-{version}.php"),
                "go" => Path.Combine(goDir, $"go{version}", "bin", "go.exe"),
                "openssl" => Path.Combine(openSSLDir, $"openssl-{version}", "bin", "openssl.exe"),
                "phpcsfixer" => Path.Combine(phpcsfixerDir, $"phpcsfixer-{version}", $"php-cs-fixer-{version}.phar"),
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

            // Obter a tabela como string e exibi-la com cores
            var helpTable = DevStackConfig.ShowHelpTable();
            var lines = helpTable.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.Contains("Comando") || line.Contains("Descrição") || line.StartsWith("╔") || line.StartsWith("╠") || line.StartsWith("╚"))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(line);
                    Console.ResetColor();
                }
                else if (line.StartsWith("║"))
                {
                    // Linha de comando - colorir diferente
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("║");
                    Console.ResetColor();
                    
                    // Extrair comando e descrição
                    var parts = line.Split('║');
                    if (parts.Length >= 3)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(parts[1]);
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("║");
                        Console.ResetColor();
                        Console.Write(parts[2]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("║");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine(line);
                }
            }
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
                    return HandleSslCommand(args).Result;

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
                Console.WriteLine("Uso: DevStackManager list <php|node|python|composer|mysql|nginx|phpmyadmin|git|mongodb|pgsql|elasticsearch|wpcli|adminer|go|openssl|phpcsfixer|--installed>");
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
            if (args.Length < 4)
            {
                Console.WriteLine("Uso: DevStackManager site <dominio> -Root <diretorio> -PHP <php-upstream> -Nginx <nginx-version>");
                return 1;
            }

            string domain = args[0];
            string root = "";
            string phpUpstream = "";
            string nginxVersion = "";

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "-root":
                        root = args[i];
                        break;
                    case "-php":
                        phpUpstream = $"127.{args[i]}:9000";
                        break;
                    case "-nginx":
                        nginxVersion = args[i];
                        break;
                }
            }
            
            if (string.IsNullOrWhiteSpace(domain))
            {
                Console.WriteLine("Erro: domínio é obrigatório.");
                return 1;
            }
            if (string.IsNullOrWhiteSpace(root))
            {
                Console.WriteLine("Erro: Root é obrigatório.");
                return 1;
            }
            if (string.IsNullOrWhiteSpace(phpUpstream))
            {
                Console.WriteLine("Erro: PHP é obrigatório.");
                return 1;
            }
            if (string.IsNullOrWhiteSpace(nginxVersion))
            {
                Console.WriteLine("Erro: Nginx é obrigatório.");
                return 1;
            }

            InstallManager.CreateNginxSiteConfig(domain, root, phpUpstream, nginxVersion);
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
                    ProcessManager.ForEachVersion("nginx", v => ProcessManager.StartComponent("nginx", v));
                }
                else
                {
                    WriteWarningMsg("Diretório do nginx não encontrado. Ignorando.");
                }

                if (Directory.Exists(phpDir))
                {
                    ProcessManager.ForEachVersion("php", v => ProcessManager.StartComponent("php", v));
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
            ProcessManager.StartComponent(target, version);
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
                    ProcessManager.ForEachVersion("nginx", v => ProcessManager.StopComponent("nginx", v));
                }
                else
                {
                    WriteWarningMsg("Diretório do nginx não encontrado. Ignorando.");
                }

                if (Directory.Exists(phpDir))
                {
                    ProcessManager.ForEachVersion("php", v => ProcessManager.StopComponent("php", v));
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
            ProcessManager.StopComponent(target, version);
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
                    ProcessManager.ForEachVersion("nginx", v => 
                    {
                        ProcessManager.StopComponent("nginx", v);
                        Thread.Sleep(1000);
                        ProcessManager.StartComponent("nginx", v);
                    });
                }
                else
                {
                    WriteWarningMsg("Diretório do nginx não encontrado. Ignorando.");
                }

                if (Directory.Exists(phpDir))
                {
                    ProcessManager.ForEachVersion("php", v => 
                    {
                        ProcessManager.StopComponent("php", v);
                        Thread.Sleep(1000);
                        ProcessManager.StartComponent("php", v);
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
            ProcessManager.StopComponent(target, version);
            Thread.Sleep(1000);
            ProcessManager.StartComponent(target, version);
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
                    var result = ProcessManager.ExecuteProcess("git", "pull", repoDir);
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
            await GenerateManager.GenerateSslCertificate(args);
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
                                ProcessManager.ExecuteProcess(mysqlExe, "-e \"SHOW DATABASES;\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mysqlExe, $"-e \"CREATE DATABASE {args[2]};\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mysqlExe, $"-e \"DROP DATABASE {args[2]};\"");
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
                                ProcessManager.ExecuteProcess(psqlExe, "-c \"\\l\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(psqlExe, $"-c \"CREATE DATABASE {args[2]};\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(psqlExe, $"-c \"DROP DATABASE {args[2]};\"");
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
                                ProcessManager.ExecuteProcess(mongoExe, "--eval \"db.adminCommand('listDatabases')\"");
                                break;
                            case "create":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mongoExe, $"--eval \"db.getSiblingDB('{args[2]}')\"");
                                break;
                            case "drop":
                                if (args.Length > 2)
                                    ProcessManager.ExecuteProcess(mongoExe, $"--eval \"db.getSiblingDB('{args[2]}').dropDatabase()\"");
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
            // Map component name to directory
            string? dir = component.ToLowerInvariant() switch
            {
                "php" => phpDir,
                "nginx" => nginxDir,
                "mysql" => mysqlDir,
                "node" or "nodejs" => nodeDir,
                "python" => pythonDir,
                "composer" => composerDir,
                "git" => baseDir,
                "phpmyadmin" or "pma" => pmaDir,
                "mongodb" or "mongo" => mongoDir,
                "pgsql" or "postgresql" => pgsqlDir,
                "elasticsearch" or "elastic" => elasticDir,
                "wpcli" or "wp-cli" => wpcliDir,
                "adminer" => adminerDir,
                "go" or "golang" => goDir,
                "openssl" => openSSLDir,
                "phpcsfixer" => phpcsfixerDir,
                _ => null
            };

            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return (false, string.Empty, Array.Empty<string>());
            }

            string[] versions;
            if (component.Equals("git", StringComparison.OrdinalIgnoreCase))
            {
                // git: procurar subdiretórios git-*
                versions = Directory.GetDirectories(dir, "git-*")
                    .Select(f => Path.GetFileName(f) ?? string.Empty)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToArray();
            }
            else
            {
                versions = Directory.GetDirectories(dir)
                    .Select(f => Path.GetFileName(f) ?? string.Empty)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToArray();
            }

            if (versions.Length == 0)
            {
                return (false, string.Empty, Array.Empty<string>());
            }

            // Serviços monitorados pela aba Serviços
            var serviceComponents = new[] { "php", "nginx" };
            if (serviceComponents.Contains(component.ToLowerInvariant()))
            {
                var processList = System.Diagnostics.Process.GetProcesses();
                var versionsWithStatus = versions.Select(version => {
                    string dirName = version;
                    string status = "parado";
                    if (component.Equals("php", StringComparison.OrdinalIgnoreCase))
                    {
                        // Exemplo: dirName = "php-8.2.12"; version = "php-8.2.12" ou "8.2.12"
                        string search = dirName;
                        var running = processList.Any(p => {
                            try
                            {
                                if (p.ProcessName.StartsWith("php", StringComparison.OrdinalIgnoreCase))
                                {
                                    var processPath = p.MainModule?.FileName;
                                    return !string.IsNullOrEmpty(processPath) && processPath.Contains(search, StringComparison.OrdinalIgnoreCase);
                                }
                            }
                            catch { }
                            return false;
                        });
                        if (running) status = "executando";
                    }
                    else if (component.Equals("nginx", StringComparison.OrdinalIgnoreCase))
                    {
                        string search = dirName;
                        var running = processList.Any(p => {
                            try
                            {
                                if (p.ProcessName.StartsWith("nginx", StringComparison.OrdinalIgnoreCase))
                                {
                                    var processPath = p.MainModule?.FileName;
                                    return !string.IsNullOrEmpty(processPath) && processPath.Contains(search, StringComparison.OrdinalIgnoreCase);
                                }
                            }
                            catch { }
                            return false;
                        });
                        if (running) status = "executando";
                    }
                    return $"{version} ({status})";
                }).ToArray();
                return (true, $"{component} instalado(s)", versionsWithStatus);
            }

            return (true, $"{component} instalado(s)", versions);
        }

        private static Dictionary<string, (bool installed, string message, string[] versions)> GetAllComponentsStatus()
        {
            string[] components = new[]
            {
                "php", "nginx", "mysql", "nodejs", "python", "composer", "git", "phpmyadmin",
                "mongodb", "pgsql", "elasticsearch", "wpcli", "adminer",
                "go", "openssl", "phpcsfixer"
            };

            var results = new Dictionary<string, (bool installed, string message, string[] versions)>();
            foreach (var comp in components)
            {
                results[comp] = GetComponentStatus(comp);
            }
            return results;
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
                else if (Regex.IsMatch(arg, @"^node-(.+)$"))
                {
                    var version = Regex.Replace(arg, @"^node-", "");
                    specificVersions.Add($"node:{version}");
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
                            if (component == "node") component = "node";
                            specificVersions.Add($"{component}:{nextArg}");
                        }
                    }
                }
            }
            
            UninstallManager.UninstallCommands(args);
        }
    }
}
