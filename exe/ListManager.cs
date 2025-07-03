using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevStackManager
{
    public static class ListManager
    {
        /// <summary>
        /// Lista todas as ferramentas instaladas em formato de tabela
        /// </summary>
        public static void ListInstalledVersions()
        {
            var data = DataManager.GetInstalledVersions();
            
            if (data.Status == "warning")
            {
                DevStackConfig.WriteColoredLine(data.Message, ConsoleColor.Yellow);
                return;
            }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Ferramentas instaladas:");
            Console.ResetColor();
            
            // Tabela de Ferramentas Instaladas
            int col1 = 15, col2 = 40;
            string header = new string('_', col1 + col2 + 3);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(header);
            
            string headerRow = $"|{CenterText("Ferramenta", col1)}|{CenterText("Versões Instaladas", col2)}|";
            Console.WriteLine(headerRow);
            
            string separator = $"|{new string('-', col1)}+{new string('-', col2)}|";
            Console.WriteLine(separator);
            Console.ResetColor();
            
            foreach (var comp in data.Components)
            {
                string ferramenta = CenterText(comp.Name, col1);
                
                if (comp.Installed && comp.Versions.Count > 0)
                {
                    string status = CenterText(string.Join(", ", comp.Versions), col2);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(ferramenta);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(status);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("|");
                    Console.ResetColor();
                }
                else
                {
                    string status = CenterText("NÃO INSTALADO", col2);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(ferramenta);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(status);
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("|");
                    Console.ResetColor();
                }
            }
            
            // Garantir que a cor está resetada antes do rodapé
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            string footer = new string('¯', col1 + col2 + 3);
            Console.WriteLine(footer);
            Console.ResetColor();
        }

        /// <summary>
        /// Imprime uma tabela horizontal com versões disponíveis e instaladas
        /// </summary>
        public static void PrintHorizontalTable(VersionData data)
        {
            if (data.Status == "error")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(data.Message);
                Console.ResetColor();
                return;
            }
            
            var items = data.Versions;
            string header = data.Header;
            var installed = data.Installed;
            bool orderDescending = data.OrderDescending ?? true;
            int cols = 6;
            
            if (items == null || items.Count == 0)
            {
                Console.WriteLine("Nenhuma versão encontrada.");
                return;
            }
            
            // Ordenar da maior para menor (descendente)
            if (orderDescending)
            {
                items = items.OrderByDescending(v => v).ToList();
            }
            
            int total = items.Count;
            int rows = (int)Math.Ceiling((double)total / cols);
            int width = 16;
            int tableWidth = (width * cols) + cols + 1;
            
            Console.WriteLine(header);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('-', tableWidth));
            Console.ResetColor();
            
            // Preencher linhas de cima para baixo (coluna 1: maior, coluna 2: próxima maior, etc)
            for (int r = 0; r < rows; r++)
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("|");
                for (int c = 0; c < cols; c++)
                {
                    int idx = c * rows + r;
                    if (idx < total)
                    {
                        string val = Regex.Replace(items[idx], @"[^\d\.]", "");
                        string cellText = CenterText(val, width);
                        
                        if (installed.Contains(val))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(cellText);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(cellText);
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.Write(CenterText("", width));
                    }
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("|");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
            
            // Garantir que a cor está resetada antes do rodapé
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(new string('¯', tableWidth));
            Console.ResetColor();
        }

        /// <summary>
        /// Centraliza um texto em uma largura específica
        /// </summary>
        private static string CenterText(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', width);
                
            if (text.Length >= width)
                return text.Substring(0, width);
                
            int padding = width - text.Length;
            int leftPadding = padding / 2;
            int rightPadding = padding - leftPadding;
            
            return new string(' ', leftPadding) + text + new string(' ', rightPadding);
        }

        // Métodos específicos para cada ferramenta
        public static void ListNginxVersions()
        {
            var data = DataManager.GetNginxVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPHPVersions()
        {
            var data = DataManager.GetPHPVersions();
            PrintHorizontalTable(data);
        }

        public static void ListNodeVersions()
        {
            var data = DataManager.GetNodeVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPythonVersions()
        {
            var data = DataManager.GetPythonVersions();
            PrintHorizontalTable(data);
        }

        public static void ListMySQLVersions()
        {
            var data = DataManager.GetMySQLVersions();
            PrintHorizontalTable(data);
        }

        public static void ListComposerVersions()
        {
            var data = DataManager.GetComposerVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPhpMyAdminVersions()
        {
            var data = DataManager.GetPhpMyAdminVersions();
            PrintHorizontalTable(data);
        }

        public static void ListMongoDBVersions()
        {
            var data = DataManager.GetMongoDBVersions();
            PrintHorizontalTable(data);
        }

        public static void ListRedisVersions()
        {
            var data = DataManager.GetRedisVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPgSQLVersions()
        {
            var data = DataManager.GetPgSQLVersions();
            PrintHorizontalTable(data);
        }

        public static void ListMailHogVersions()
        {
            var data = DataManager.GetMailHogVersions();
            PrintHorizontalTable(data);
        }

        public static void ListElasticsearchVersions()
        {
            var data = DataManager.GetElasticsearchVersions();
            PrintHorizontalTable(data);
        }

        public static void ListMemcachedVersions()
        {
            var data = DataManager.GetMemcachedVersions();
            PrintHorizontalTable(data);
        }

        public static void ListDockerVersions()
        {
            var data = DataManager.GetDockerVersions();
            PrintHorizontalTable(data);
        }

        public static void ListYarnVersions()
        {
            var data = DataManager.GetYarnVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPnpmVersions()
        {
            var data = DataManager.GetPnpmVersions();
            PrintHorizontalTable(data);
        }

        public static void ListWPCLIVersions()
        {
            var data = DataManager.GetWPCLIVersions();
            PrintHorizontalTable(data);
        }

        public static void ListAdminerVersions()
        {
            var data = DataManager.GetAdminerVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPoetryVersions()
        {
            var data = DataManager.GetPoetryVersions();
            PrintHorizontalTable(data);
        }

        public static void ListRubyVersions()
        {
            var data = DataManager.GetRubyVersions();
            PrintHorizontalTable(data);
        }

        public static void ListGoVersions()
        {
            var data = DataManager.GetGoVersions();
            PrintHorizontalTable(data);
        }

        public static void ListCertbotVersions()
        {
            var data = DataManager.GetCertbotVersions();
            PrintHorizontalTable(data);
        }

        public static void ListOpenSSLVersions()
        {
            var data = DataManager.GetOpenSSLVersions();
            PrintHorizontalTable(data);
        }

        public static void ListPHPCsFixerVersions()
        {
            var data = DataManager.GetPHPCsFixerVersions();
            PrintHorizontalTable(data);
        }

        public static void ListGitVersions()
        {
            var data = DataManager.GetGitVersions();
            PrintHorizontalTable(data);
        }

        /// <summary>
        /// Lista versões baseado no componente especificado
        /// </summary>
        public static void ListVersions(string component)
        {
            switch (component.ToLowerInvariant())
            {
                case "nginx":
                    ListNginxVersions();
                    break;
                case "php":
                    ListPHPVersions();
                    break;
                case "node":
                case "nodejs":
                    ListNodeVersions();
                    break;
                case "python":
                    ListPythonVersions();
                    break;
                case "mysql":
                    ListMySQLVersions();
                    break;
                case "composer":
                    ListComposerVersions();
                    break;
                case "phpmyadmin":
                case "pma":
                    ListPhpMyAdminVersions();
                    break;
                case "mongodb":
                case "mongo":
                    ListMongoDBVersions();
                    break;
                case "redis":
                    ListRedisVersions();
                    break;
                case "postgresql":
                case "pgsql":
                    ListPgSQLVersions();
                    break;
                case "mailhog":
                    ListMailHogVersions();
                    break;
                case "elasticsearch":
                case "elastic":
                    ListElasticsearchVersions();
                    break;
                case "memcached":
                    ListMemcachedVersions();
                    break;
                case "docker":
                    ListDockerVersions();
                    break;
                case "yarn":
                    ListYarnVersions();
                    break;
                case "pnpm":
                    ListPnpmVersions();
                    break;
                case "wp-cli":
                case "wpcli":
                    ListWPCLIVersions();
                    break;
                case "adminer":
                    ListAdminerVersions();
                    break;
                case "poetry":
                    ListPoetryVersions();
                    break;
                case "ruby":
                    ListRubyVersions();
                    break;
                case "go":
                case "golang":
                    ListGoVersions();
                    break;
                case "certbot":
                    ListCertbotVersions();
                    break;
                case "openssl":
                    ListOpenSSLVersions();
                    break;
                case "php-cs-fixer":
                case "phpcsfixer":
                    ListPHPCsFixerVersions();
                    break;
                case "git":
                    ListGitVersions();
                    break;
                default:
                    DevStackConfig.WriteColoredLine($"Componente não reconhecido: {component}", ConsoleColor.Red);
                    DevStackConfig.WriteColoredLine("Use 'DevStackManager help' para ver os componentes disponíveis.", ConsoleColor.Yellow);
                    break;
            }
        }
    }

    /// <summary>
    /// Classe para representar dados de versões
    /// </summary>
    public class VersionData
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = "";
        public string Header { get; set; } = "";
        public List<string> Versions { get; set; } = new List<string>();
        public List<string> Installed { get; set; } = new List<string>();
        public bool? OrderDescending { get; set; } = true;
    }

    /// <summary>
    /// Classe para representar dados de instalação
    /// </summary>
    public class InstallationData
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = "";
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();
    }

    /// <summary>
    /// Informações sobre um componente
    /// </summary>
    public class ComponentInfo
    {
        public string Name { get; set; } = "";
        public bool Installed { get; set; } = false;
        public List<string> Versions { get; set; } = new List<string>();
    }
}
