using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    public static class ProcessManager
    {
        private static readonly string baseDir = "C:\\devstack";
        private static readonly string nginxDir = Path.Combine(baseDir, "nginx");
        private static readonly string phpDir = Path.Combine(baseDir, "php");

        /// <summary>
        /// Inicia um componente específico com uma versão
        /// </summary>
        public static void StartComponent(string component, string version)
        {
            switch (component.ToLowerInvariant())
            {
                case "nginx":
                    StartNginx(version);
                    break;
                case "php":
                    StartPhp(version);
                    break;
                default:
                    DevStackConfig.WriteColoredLine($"Componente desconhecido: {component}", ConsoleColor.Red);
                    break;
            }
        }

        /// <summary>
        /// Para um componente específico com uma versão
        /// </summary>
        public static void StopComponent(string component, string version)
        {
            switch (component.ToLowerInvariant())
            {
                case "nginx":
                    StopNginx(version);
                    break;
                case "php":
                    StopPhp(version);
                    break;
                default:
                    DevStackConfig.WriteColoredLine($"Componente desconhecido: {component}", ConsoleColor.Red);
                    break;
            }
        }

        /// <summary>
        /// Inicia o Nginx de uma versão específica
        /// </summary>
        private static void StartNginx(string version)
        {
            var nginxExe = Path.Combine(nginxDir, $"nginx-{version}", $"nginx-{version}.exe");
            var nginxWorkDir = Path.Combine(nginxDir, $"nginx-{version}");

            if (!File.Exists(nginxExe))
            {
                DevStackConfig.WriteColoredLine($"Nginx {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(nginxExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    DevStackConfig.WriteColoredLine($"Nginx {version} já está em execução.", ConsoleColor.Yellow);
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = nginxExe,
                    WorkingDirectory = nginxWorkDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(startInfo);
                DevStackConfig.WriteColoredLine($"Nginx {version} iniciado.", ConsoleColor.Green);
                WriteLog($"Nginx {version} iniciado.");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao iniciar Nginx {version}: {ex.Message}", ConsoleColor.Red);
                WriteLog($"Erro ao iniciar Nginx {version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Para o Nginx de uma versão específica
        /// </summary>
        private static void StopNginx(string version)
        {
            var nginxExe = Path.Combine(nginxDir, $"nginx-{version}", $"nginx-{version}.exe");

            if (!File.Exists(nginxExe))
            {
                DevStackConfig.WriteColoredLine($"Nginx {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(nginxExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    foreach (var process in runningProcesses)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Aguarda até 5 segundos
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteColoredLine($"Erro ao parar processo Nginx {process.Id}: {ex.Message}", ConsoleColor.Yellow);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                    DevStackConfig.WriteColoredLine($"Nginx {version} parado.", ConsoleColor.Green);
                    WriteLog($"Nginx {version} parado.");
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"Nginx {version} não está em execução.", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao verificar processos Nginx: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Inicia o PHP-CGI de uma versão específica
        /// </summary>
        private static void StartPhp(string version)
        {
            var phpExe = Path.Combine(phpDir, $"php-{version}", $"php-cgi-{version}.exe");
            var phpWorkDir = Path.Combine(phpDir, $"php-{version}");

            if (!File.Exists(phpExe))
            {
                DevStackConfig.WriteColoredLine($"php-cgi {version} não encontrado.", ConsoleColor.Red);
                return;
            }

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(phpExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    DevStackConfig.WriteColoredLine($"php-cgi {version} já está em execução.", ConsoleColor.Yellow);
                    return;
                }

                // Inicia 6 workers php-cgi para FastCGI
                for (int i = 1; i <= 6; i++)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = phpExe,
                        Arguments = $"-b 127.{version}:9000",
                        WorkingDirectory = phpWorkDir,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(startInfo);
                }

                DevStackConfig.WriteColoredLine($"php-cgi {version} iniciado com 6 workers em 127.{version}:9000.", ConsoleColor.Green);
                WriteLog($"php-cgi {version} iniciado com 6 workers em 127.{version}:9000.");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao iniciar php-cgi {version}: {ex.Message}", ConsoleColor.Red);
                WriteLog($"Erro ao iniciar php-cgi {version}: {ex.Message}");
            }
        }

        /// <summary>
        /// Para o PHP-CGI de uma versão específica
        /// </summary>
        private static void StopPhp(string version)
        {
            var phpExe = Path.Combine(phpDir, $"php-{version}", $"php-cgi-{version}.exe");

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(phpExe, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                if (runningProcesses.Any())
                {
                    foreach (var process in runningProcesses)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Aguarda até 5 segundos
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteColoredLine($"Erro ao parar processo PHP {process.Id}: {ex.Message}", ConsoleColor.Yellow);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                    DevStackConfig.WriteColoredLine($"php-cgi {version} parado.", ConsoleColor.Green);
                    WriteLog($"php-cgi {version} parado.");
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"php-cgi {version} não está em execução.", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao verificar processos PHP: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Executa uma ação para cada versão de um componente
        /// </summary>
        public static void ForEachVersion(string component, Action<string> action)
        {
            var dir = component.ToLowerInvariant() switch
            {
                "nginx" => nginxDir,
                "php" => phpDir,
                _ => null
            };

            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return;
            }

            var prefix = $"{component}-";
            
            try
            {
                var directories = Directory.GetDirectories(dir, $"{prefix}*");
                
                foreach (var directory in directories)
                {
                    var dirName = Path.GetFileName(directory);
                    if (dirName.StartsWith(prefix))
                    {
                        var version = dirName.Substring(prefix.Length);
                        action(version);
                    }
                }
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteColoredLine($"Erro ao listar versões de {component}: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Obtém todas as versões instaladas de um componente
        /// </summary>
        public static List<string> GetInstalledVersions(string component)
        {
            var versions = new List<string>();
            
            ForEachVersion(component, version => versions.Add(version));
            
            return versions.OrderBy(v => v).ToList();
        }

        /// <summary>
        /// Verifica se um componente específico está em execução
        /// </summary>
        public static bool IsComponentRunning(string component, string version)
        {
            var exePath = component.ToLowerInvariant() switch
            {
                "nginx" => Path.Combine(nginxDir, $"nginx-{version}", $"nginx-{version}.exe"),
                "php" => Path.Combine(phpDir, $"php-{version}", $"php-cgi-{version}.exe"),
                _ => null
            };

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                return false;
            }

            try
            {
                return Process.GetProcesses()
                    .Any(p => 
                    {
                        try
                        {
                            return p.MainModule?.FileName?.Equals(exePath, StringComparison.OrdinalIgnoreCase) == true;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lista o status de todos os componentes e versões
        /// </summary>
        public static void ListComponentsStatus()
        {
            var components = new[] { "nginx", "php" };
            
            DevStackConfig.WriteColoredLine("Status dos componentes:", ConsoleColor.Cyan);
            Console.WriteLine();
            
            foreach (var component in components)
            {
                DevStackConfig.WriteColoredLine($"{component.ToUpper()}:", ConsoleColor.Yellow);
                
                var versions = GetInstalledVersions(component);
                if (versions.Any())
                {
                    foreach (var version in versions)
                    {
                        var isRunning = IsComponentRunning(component, version);
                        var status = isRunning ? "EXECUTANDO" : "PARADO";
                        var color = isRunning ? ConsoleColor.Green : ConsoleColor.Red;
                        
                        Console.Write($"  {component}-{version}: ");
                        DevStackConfig.WriteColoredLine(status, color);
                    }
                }
                else
                {
                    DevStackConfig.WriteColoredLine($"  Nenhuma versão de {component} instalada.", ConsoleColor.Gray);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Para todos os componentes em execução
        /// </summary>
        public static void StopAllComponents()
        {
            var components = new[] { "nginx", "php" };
            
            foreach (var component in components)
            {
                var versions = GetInstalledVersions(component);
                foreach (var version in versions)
                {
                    if (IsComponentRunning(component, version))
                    {
                        StopComponent(component, version);
                    }
                }
            }
        }

        /// <summary>
        /// Inicia todos os componentes instalados
        /// </summary>
        public static void StartAllComponents()
        {
            var components = new[] { "nginx", "php" };
            
            foreach (var component in components)
            {
                var versions = GetInstalledVersions(component);
                foreach (var version in versions)
                {
                    if (!IsComponentRunning(component, version))
                    {
                        StartComponent(component, version);
                    }
                }
            }
        }

        /// <summary>
        /// Reinicia um componente específico
        /// </summary>
        public static void RestartComponent(string component, string version)
        {
            DevStackConfig.WriteColoredLine($"Reiniciando {component} {version}...", ConsoleColor.Cyan);
            
            if (IsComponentRunning(component, version))
            {
                StopComponent(component, version);
                
                // Aguarda um pouco para garantir que o processo foi finalizado
                System.Threading.Thread.Sleep(2000);
            }
            
            StartComponent(component, version);
        }

        /// <summary>
        /// Escreve uma mensagem no log
        /// </summary>
        private static void WriteLog(string message)
        {
            try
            {
                string logFile = Path.Combine(baseDir, "devstack.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignora erros de logging
            }
        }
    }
}
