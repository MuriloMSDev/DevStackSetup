using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevStackManager
{
    public class PathManager
    {
        private readonly string baseDir;
        private readonly string phpDir;
        private readonly string nodeDir;
        private readonly string pythonDir;
        private readonly string nginxDir;
        private readonly string mysqlDir;

        public PathManager(string baseDir, string phpDir, string nodeDir, string pythonDir, string nginxDir, string mysqlDir)
        {
            this.baseDir = baseDir;
            this.phpDir = phpDir;
            this.nodeDir = nodeDir;
            this.pythonDir = pythonDir;
            this.nginxDir = nginxDir;
            this.mysqlDir = mysqlDir;
        }

        /// <summary>
        /// Normaliza um caminho, removendo espaços em branco e barras finais
        /// </summary>
        public static string? NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                return Path.GetFullPath(path).TrimEnd('\\');
            }
            catch
            {
                return path?.TrimEnd('\\');
            }
        }

        /// <summary>
        /// Adiciona diretórios de executáveis das ferramentas instaladas ao PATH do usuário
        /// </summary>
        public void AddBinDirsToPath()
        {
            var pathsToAdd = new List<string>();

            // PHP
            if (Directory.Exists(phpDir))
            {
                foreach (var dir in Directory.GetDirectories(phpDir))
                {
                    var phpExe = Directory.GetFiles(dir, "php-*.exe").FirstOrDefault();
                    if (phpExe != null)
                    {
                        pathsToAdd.Add(dir);
                    }
                }
            }

            // Node.js
            if (Directory.Exists(nodeDir))
            {
                foreach (var dir in Directory.GetDirectories(nodeDir))
                {
                    var nodeExe = Directory.GetFiles(dir, "node-*.exe").FirstOrDefault();
                    if (nodeExe != null)
                    {
                        pathsToAdd.Add(dir);
                    }
                }
            }

            // Python
            if (Directory.Exists(pythonDir))
            {
                foreach (var dir in Directory.GetDirectories(pythonDir))
                {
                    var pythonExe = Directory.GetFiles(dir, "python-*.exe").FirstOrDefault();
                    if (pythonExe != null)
                    {
                        pathsToAdd.Add(dir);
                    }
                }
            }

            // Nginx
            if (Directory.Exists(nginxDir))
            {
                foreach (var dir in Directory.GetDirectories(nginxDir))
                {
                    var nginxExe = Directory.GetFiles(dir, "nginx-*.exe").FirstOrDefault();
                    if (nginxExe != null)
                    {
                        pathsToAdd.Add(dir);
                    }
                }
            }

            // MySQL (bin)
            if (Directory.Exists(mysqlDir))
            {
                foreach (var dir in Directory.GetDirectories(mysqlDir))
                {
                    var mysqlBin = Path.Combine(dir, "bin");
                    if (Directory.Exists(mysqlBin))
                    {
                        var mysqldExe = Directory.GetFiles(mysqlBin, "mysqld-*.exe").FirstOrDefault();
                        if (mysqldExe != null)
                        {
                            pathsToAdd.Add(mysqlBin);
                        }
                    }
                }
            }

            // Git (Portable)
            if (Directory.Exists(baseDir))
            {
                foreach (var dir in Directory.GetDirectories(baseDir, "git-*"))
                {
                    var gitBin = Path.Combine(dir, "cmd");
                    if (Directory.Exists(gitBin))
                    {
                        pathsToAdd.Add(gitBin);
                    }
                }
            }

            // Obter PATH atual do usuário
            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var currentPathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(NormalizePath)
                                            .Where(p => !string.IsNullOrEmpty(p))
                                            .ToList();

            // Filtrar apenas novos caminhos
            var newPaths = pathsToAdd.Select(NormalizePath)
                                    .Where(p => !string.IsNullOrEmpty(p) && !currentPathList.Contains(p))
                                    .ToList();

            if (newPaths.Count > 0)
            {
                var allPaths = currentPathList.Concat(newPaths);
                var newPathValue = string.Join(";", allPaths);
                
                // Atualizar PATH do usuário
                Environment.SetEnvironmentVariable("Path", newPathValue, EnvironmentVariableTarget.User);
                
                // Atualizar PATH da sessão atual (processo)
                UpdateProcessPath();

                DevStackConfig.WriteColoredLine("Os seguintes diretórios foram adicionados ao PATH do usuário:", ConsoleColor.Green);
                foreach (var path in newPaths)
                {
                    DevStackConfig.WriteColoredLine($"  {path}", ConsoleColor.Yellow);
                }
                DevStackConfig.WriteColoredLine("O PATH do terminal atual também foi atualizado.", ConsoleColor.Green);
            }
            else
            {
                // Mesmo sem novos paths, garantir que o processo está sincronizado
                UpdateProcessPath();
                DevStackConfig.WriteColoredLine("Nenhum novo diretório foi adicionado ao PATH.", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// Remove diretórios específicos do PATH do usuário
        /// </summary>
        public void RemoveFromPath(string[] dirsToRemove)
        {
            if (dirsToRemove == null || dirsToRemove.Length == 0)
                return;

            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var currentPathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(NormalizePath)
                                            .Where(p => !string.IsNullOrEmpty(p))
                                            .ToList();

            var dirsToRemoveNorm = dirsToRemove.Select(NormalizePath)
                                              .Where(p => !string.IsNullOrEmpty(p))
                                              .ToHashSet();

            var newPathList = currentPathList.Where(p => !dirsToRemoveNorm.Contains(p)).ToList();
            var newPathValue = string.Join(";", newPathList);

            // Atualizar PATH do usuário
            Environment.SetEnvironmentVariable("Path", newPathValue, EnvironmentVariableTarget.User);
            
            // Atualizar PATH da sessão atual
            UpdateProcessPath();

            var removedPaths = dirsToRemoveNorm.Where(dir => currentPathList.Contains(dir)).ToList();
            if (removedPaths.Count > 0)
            {
                DevStackConfig.WriteColoredLine("Os seguintes diretórios foram removidos do PATH do usuário:", ConsoleColor.Green);
                foreach (var path in removedPaths)
                {
                    DevStackConfig.WriteColoredLine($"  {path}", ConsoleColor.Yellow);
                }
            }
        }

        /// <summary>
        /// Remove todos os diretórios relacionados às ferramentas DevStack do PATH
        /// </summary>
        public void RemoveAllDevStackFromPath()
        {
            var dirsToRemove = new List<string>();

            // Coletar todos os diretórios que podem estar no PATH
            if (Directory.Exists(phpDir))
            {
                dirsToRemove.AddRange(Directory.GetDirectories(phpDir));
            }

            if (Directory.Exists(nodeDir))
            {
                dirsToRemove.AddRange(Directory.GetDirectories(nodeDir));
            }

            if (Directory.Exists(pythonDir))
            {
                dirsToRemove.AddRange(Directory.GetDirectories(pythonDir));
            }

            if (Directory.Exists(nginxDir))
            {
                dirsToRemove.AddRange(Directory.GetDirectories(nginxDir));
            }

            if (Directory.Exists(mysqlDir))
            {
                foreach (var dir in Directory.GetDirectories(mysqlDir))
                {
                    var mysqlBin = Path.Combine(dir, "bin");
                    if (Directory.Exists(mysqlBin))
                    {
                        dirsToRemove.Add(mysqlBin);
                    }
                }
            }

            // Git (Portable)
            if (Directory.Exists(baseDir))
            {
                foreach (var dir in Directory.GetDirectories(baseDir, "git-*"))
                {
                    var gitBin = Path.Combine(dir, "cmd");
                    if (Directory.Exists(gitBin))
                    {
                        dirsToRemove.Add(gitBin);
                    }
                }
            }

            if (dirsToRemove.Count > 0)
            {
                RemoveFromPath(dirsToRemove.ToArray());
            }
        }

        /// <summary>
        /// Lista todos os diretórios atualmente no PATH do usuário
        /// </summary>
        public void ListCurrentPath()
        {
            var currentPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
            var pathList = currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(NormalizePath)
                                     .Where(p => !string.IsNullOrEmpty(p))
                                     .ToList();

            DevStackConfig.WriteColoredLine("Diretórios atualmente no PATH do usuário:", ConsoleColor.Cyan);
            foreach (var path in pathList)
            {
                var exists = Directory.Exists(path) ? "✓" : "✗";
                var color = Directory.Exists(path) ? ConsoleColor.Green : ConsoleColor.Red;
                DevStackConfig.WriteColoredLine($"  {exists} {path}", color);
            }
        }

        /// <summary>
        /// Atualiza o PATH do processo atual combinando Machine PATH + User PATH
        /// </summary>
        private void UpdateProcessPath()
        {
            try
            {
                var userPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
                var machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine) ?? "";
                
                // Combinar Machine PATH + User PATH (ordem padrão do Windows)
                var combinedPath = string.IsNullOrEmpty(machinePath) ? userPath : 
                                  string.IsNullOrEmpty(userPath) ? machinePath : 
                                  $"{machinePath};{userPath}";
                
                // Atualizar PATH do processo atual
                Environment.SetEnvironmentVariable("PATH", combinedPath);
                
                DevStackConfig.WriteLog($"PATH do processo atualizado. Total de entradas: {combinedPath.Split(';').Length}");
            }
            catch (Exception ex)
            {
                DevStackConfig.WriteLog($"Erro ao atualizar PATH do processo: {ex.Message}");
            }
        }
    }
}
