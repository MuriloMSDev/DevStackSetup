using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevStackManager
{
    public static class DataManager
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        static DataManager()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DevStackManager/1.0");
        }

        /// <summary>
        /// Obtém informações sobre versões instaladas de todas as ferramentas
        /// </summary>
        public static InstallationData GetInstalledVersions()
        {
            var baseDir = "C:\\devstack";
            
            if (!Directory.Exists(baseDir))
            {
                return new InstallationData
                {
                    Status = "warning",
                    Message = $"O diretório {baseDir} não existe. Nenhuma ferramenta instalada.",
                    Components = new List<ComponentInfo>()
                };
            }

            var components = new[]
            {
                new { name = "php", dir = Path.Combine(baseDir, "php"), pattern = (string?)null },
                new { name = "nginx", dir = Path.Combine(baseDir, "nginx"), pattern = (string?)null },
                new { name = "mysql", dir = Path.Combine(baseDir, "mysql"), pattern = (string?)null },
                new { name = "nodejs", dir = Path.Combine(baseDir, "nodejs"), pattern = (string?)null },
                new { name = "python", dir = Path.Combine(baseDir, "python"), pattern = (string?)null },
                new { name = "composer", dir = Path.Combine(baseDir, "composer"), pattern = (string?)null },
                new { name = "phpmyadmin", dir = Path.Combine(baseDir, "phpmyadmin"), pattern = (string?)null },
                new { name = "git", dir = baseDir, pattern = (string?)"git-*" },
                new { name = "mongodb", dir = Path.Combine(baseDir, "mongodb"), pattern = (string?)null },
                new { name = "redis", dir = Path.Combine(baseDir, "redis"), pattern = (string?)null },
                new { name = "pgsql", dir = Path.Combine(baseDir, "pgsql"), pattern = (string?)null },
                new { name = "mailhog", dir = Path.Combine(baseDir, "mailhog"), pattern = (string?)null },
                new { name = "elasticsearch", dir = Path.Combine(baseDir, "elasticsearch"), pattern = (string?)null },
                new { name = "memcached", dir = Path.Combine(baseDir, "memcached"), pattern = (string?)null },
                new { name = "docker", dir = Path.Combine(baseDir, "docker"), pattern = (string?)null },
                new { name = "yarn", dir = Path.Combine(baseDir, "yarn"), pattern = (string?)null },
                new { name = "pnpm", dir = Path.Combine(baseDir, "pnpm"), pattern = (string?)null },
                new { name = "wpcli", dir = Path.Combine(baseDir, "wpcli"), pattern = (string?)null },
                new { name = "adminer", dir = Path.Combine(baseDir, "adminer"), pattern = (string?)null },
                new { name = "poetry", dir = Path.Combine(baseDir, "poetry"), pattern = (string?)null },
                new { name = "ruby", dir = Path.Combine(baseDir, "ruby"), pattern = (string?)null },
                new { name = "go", dir = Path.Combine(baseDir, "go"), pattern = (string?)null },
                new { name = "certbot", dir = Path.Combine(baseDir, "certbot"), pattern = (string?)null },
                new { name = "openssl", dir = Path.Combine(baseDir, "openssl"), pattern = (string?)null },
                new { name = "php-cs-fixer", dir = Path.Combine(baseDir, "phpcsfixer"), pattern = (string?)null }
            };

            var result = new List<ComponentInfo>();

            foreach (var comp in components)
            {
                var item = new ComponentInfo
                {
                    Name = comp.name,
                    Installed = false,
                    Versions = new List<string>()
                };

                if (Directory.Exists(comp.dir))
                {
                    var versions = new List<string>();
                    
                    if (comp.name == "git" && !string.IsNullOrEmpty(comp.pattern))
                    {
                        versions = Directory.GetDirectories(comp.dir)
                            .Select(d => Path.GetFileName(d))
                            .Where(name => Regex.IsMatch(name, comp.pattern.Replace("*", ".*")))
                            .ToList();
                    }
                    else
                    {
                        try
                        {
                            versions = Directory.GetDirectories(comp.dir)
                                .Select(d => Path.GetFileName(d))
                                .ToList();
                        }
                        catch
                        {
                            // Ignorar erros de acesso
                        }
                    }

                    if (versions.Count > 0)
                    {
                        item.Installed = true;
                        item.Versions = versions;
                    }
                }

                result.Add(item);
            }

            return new InstallationData
            {
                Status = "success",
                Message = "",
                Components = result
            };
        }

        /// <summary>
        /// Obtém versões disponíveis do Nginx
        /// </summary>
        public static VersionData GetNginxVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://nginx.org/en/download.html").Result;
                var matches = Regex.Matches(response, @"nginx-([\d\.]+)\.zip");
                var versions = matches.Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .Distinct()
                    .OrderByDescending(v => v)
                    .ToList();

                var installed = GetInstalledVersionsForComponent("nginx", "nginx-");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de Nginx disponíveis para Windows:",
                    Versions = versions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do Nginx: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões disponíveis do PHP
        /// </summary>
        public static VersionData GetPHPVersions()
        {
            try
            {
                var urls = new[]
                {
                    "https://windows.php.net/downloads/releases/",
                    "https://windows.php.net/downloads/releases/archives/"
                };

                var allVersions = new List<string>();

                foreach (var url in urls)
                {
                    try
                    {
                        var response = httpClient.GetStringAsync(url).Result;
                        var matches = Regex.Matches(response, @"php-([\d\.]+)-Win32-[^-]+-x64\.zip");
                        var versions = matches.Cast<Match>()
                            .Select(m => m.Groups[1].Value)
                            .ToList();
                        allVersions.AddRange(versions);
                    }
                    catch
                    {
                        // Continuar com a próxima URL se esta falhar
                    }
                }

                var uniqueVersions = allVersions.Distinct().OrderByDescending(v => v).ToList();
                var installed = GetInstalledVersionsForComponent("php", "php-");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de PHP disponíveis para Windows x64:",
                    Versions = uniqueVersions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do PHP: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões disponíveis do Node.js
        /// </summary>
        public static VersionData GetNodeVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://nodejs.org/dist/index.json").Result;
                var nodeReleases = JsonSerializer.Deserialize<JsonElement[]>(response);
                var versions = (nodeReleases ?? Array.Empty<JsonElement>())
                    .Select(release => release.GetProperty("version").GetString() ?? "")
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                var installed = GetInstalledVersionsForComponent("nodejs", "node-v", "-win-x64");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de Node.js disponíveis:",
                    Versions = versions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do Node.js: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões disponíveis do Python
        /// </summary>
        public static VersionData GetPythonVersions()
        {
            try
            {
                var pythonIndexUrls = new[]
                {
                    "https://www.python.org/ftp/python/index-windows-recent.json",
                    "https://www.python.org/ftp/python/index-windows-legacy.json",
                    "https://www.python.org/ftp/python/index-windows.json"
                };

                var versions = new List<string>();

                foreach (var indexUrl in pythonIndexUrls)
                {
                    try
                    {
                        var response = httpClient.GetStringAsync(indexUrl).Result;
                        var pythonVersions = JsonSerializer.Deserialize<JsonElement>(response);

                        if (pythonVersions.TryGetProperty("versions", out var versionsArray))
                        {
                            foreach (var version in versionsArray.EnumerateArray())
                            {
                                if (version.TryGetProperty("url", out var urlElement))
                                {
                                    var url = urlElement.GetString();
                                    if (!string.IsNullOrEmpty(url))
                                    {
                                        var match = Regex.Match(url, @"python-(\d+\.\d+\.\d+)-amd64\.zip$");
                                        if (match.Success && !url.Contains("embeddable") && !url.Contains("test"))
                                        {
                                            var versionNumber = match.Groups[1].Value;
                                            if (!versions.Contains(versionNumber))
                                            {
                                                versions.Add(versionNumber);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continuar com o próximo índice se este falhar
                    }
                }

                var sortedVersions = versions.OrderByDescending(v => 
                {
                    if (Version.TryParse(v, out var version))
                        return version;
                    return new Version(0, 0, 0);
                }).ToList();

                var installed = GetInstalledVersionsForComponent("python", "python-");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de Python disponíveis:",
                    Versions = sortedVersions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do Python: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões de uma ferramenta através de releases do GitHub
        /// </summary>
        private static VersionData GetGitHubReleaseVersions(string repo, string header, string componentName, string prefix = "", string suffix = "")
        {
            try
            {
                var response = httpClient.GetStringAsync($"https://api.github.com/repos/{repo}/releases").Result;
                var releases = JsonSerializer.Deserialize<JsonElement[]>(response);
                var versions = (releases ?? Array.Empty<JsonElement>())
                    .Select(release => release.GetProperty("tag_name").GetString() ?? "")
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                var installed = GetInstalledVersionsForComponent(componentName, prefix, suffix);

                return new VersionData
                {
                    Status = "success",
                    Header = header,
                    Versions = versions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        // Métodos específicos para cada ferramenta usando GitHub releases
        public static VersionData GetMongoDBVersions() => GetGitHubReleaseVersions("mongodb/mongo", "Versões de MongoDB disponíveis:", "mongodb", "mongodb-");
        public static VersionData GetRedisVersions() => GetGitHubReleaseVersions("microsoftarchive/redis", "Versões de Redis para Windows (Microsoft Archive):", "redis", "redis-");
        public static VersionData GetMailHogVersions() => GetGitHubReleaseVersions("mailhog/MailHog", "Versões de MailHog disponíveis:", "mailhog", "mailhog-");
        public static VersionData GetElasticsearchVersions() => GetGitHubReleaseVersions("elastic/elasticsearch", "Versões de Elasticsearch disponíveis:", "elasticsearch", "elasticsearch-");
        public static VersionData GetMemcachedVersions() => GetGitHubReleaseVersions("memcached/memcached", "Versões de Memcached disponíveis:", "memcached", "memcached-");
        public static VersionData GetDockerVersions() => GetGitHubReleaseVersions("docker/cli", "Versões de Docker CLI disponíveis:", "docker", "docker-");
        public static VersionData GetYarnVersions() => GetGitHubReleaseVersions("yarnpkg/yarn", "Versões de Yarn disponíveis:", "yarn", "yarn-v");
        public static VersionData GetPnpmVersions() => GetGitHubReleaseVersions("pnpm/pnpm", "Versões de pnpm disponíveis:", "pnpm", "pnpm-v");
        public static VersionData GetWPCLIVersions() => GetGitHubReleaseVersions("wp-cli/wp-cli", "Versões de WP-CLI disponíveis:", "wpcli", "wp-cli-");
        public static VersionData GetPoetryVersions() => GetGitHubReleaseVersions("python-poetry/poetry", "Versões de Poetry disponíveis:", "poetry", "poetry-");
        public static VersionData GetRubyVersions() => GetGitHubReleaseVersions("oneclick/rubyinstaller2", "Versões de RubyInstaller2 disponíveis:", "ruby", "ruby-");
        public static VersionData GetCertbotVersions() => GetGitHubReleaseVersions("certbot/certbot", "Versões de Certbot disponíveis:", "certbot", "certbot-");
        public static VersionData GetPHPCsFixerVersions() => GetGitHubReleaseVersions("PHP-CS-Fixer/PHP-CS-Fixer", "Versões de PHP CS Fixer disponíveis:", "php-cs-fixer", "php-cs-fixer-");
        public static VersionData GetComposerVersions() => GetGitHubReleaseVersions("composer/composer", "Versões de Composer disponíveis:", "composer", "composer-");
        public static VersionData GetPhpMyAdminVersions() => GetGitHubReleaseVersions("phpmyadmin/phpmyadmin", "Versões de phpMyAdmin disponíveis:", "phpmyadmin", "phpmyadmin-");
        public static VersionData GetGitVersions() => GetGitHubReleaseVersions("git-for-windows/git", "Versões de Git para Windows disponíveis:", "git", "git-");

        /// <summary>
        /// Obtém versões do PostgreSQL
        /// </summary>
        public static VersionData GetPgSQLVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://www.enterprisedb.com/downloads/postgres-postgresql-downloads").Result;
                var matches = Regex.Matches(response, @"PostgreSQL ([\d\.]+)");
                var versions = matches.Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .Distinct()
                    .OrderByDescending(v => v)
                    .ToList();

                var installed = GetInstalledVersionsForComponent("pgsql", "pgsql-");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de PostgreSQL disponíveis para Windows:",
                    Versions = versions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do PostgreSQL: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões do Go
        /// </summary>
        public static VersionData GetGoVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://go.dev/dl/?mode=json").Result;
                var goReleases = JsonSerializer.Deserialize<JsonElement[]>(response);
                var versions = (goReleases ?? Array.Empty<JsonElement>())
                    .Select(release => release.GetProperty("version").GetString() ?? "")
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                var installed = GetInstalledVersionsForComponent("go", "go");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de Go disponíveis:",
                    Versions = versions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do Go: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões do MySQL
        /// </summary>
        public static VersionData GetMySQLVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://api.github.com/repos/mysql/mysql-server/tags").Result;
                var tags = JsonSerializer.Deserialize<JsonElement[]>(response);
                var versions = (tags ?? Array.Empty<JsonElement>())
                    .Select(tag => tag.GetProperty("name").GetString() ?? "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name => {
                        var match = Regex.Match(name, @"^mysql-(.+)$");
                        return match.Success ? match.Groups[1].Value : null;
                    })
                    .Where(v => !string.IsNullOrEmpty(v))
                    .OrderByDescending(v => v)
                    .Take(30)
                    .ToList();

                var installed = GetInstalledVersionsForComponent("mysql", "mysql-");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de MySQL disponíveis:",
                    Versions = versions!,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do MySQL: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões do Adminer
        /// </summary>
        public static VersionData GetAdminerVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://www.adminer.org/en/").Result;
                var matches = Regex.Matches(response, @"Adminer ([\d\.]+)");
                var versions = matches.Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .Distinct()
                    .OrderByDescending(v => v)
                    .ToList();

                var installed = GetInstalledVersionsForComponent("adminer", "adminer-", ".php");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de Adminer disponíveis:",
                    Versions = versions,
                    Installed = installed
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do Adminer: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões do OpenSSL
        /// </summary>
        public static VersionData GetOpenSSLVersions()
        {
            try
            {
                var response = httpClient.GetStringAsync("https://raw.githubusercontent.com/slproweb/opensslhashes/master/win32_openssl_hashes.json").Result;
                var jsonDoc = JsonSerializer.Deserialize<JsonElement>(response);
                
                var normal = new List<string>();
                var light = new List<string>();
                
                if (jsonDoc.TryGetProperty("files", out var filesElement))
                {
                    foreach (var file in filesElement.EnumerateObject())
                    {
                        var fileName = file.Name;
                        if (fileName.StartsWith("Win64OpenSSL") || fileName.StartsWith("WinUniversalOpenSSL"))
                        {
                            var fileData = file.Value;
                            if (fileData.TryGetProperty("basever", out var baseverElement))
                            {
                                var ver = baseverElement.GetString();
                                if (!string.IsNullOrEmpty(ver))
                                {
                                    if (fileData.TryGetProperty("light", out var lightElement) && lightElement.GetBoolean())
                                    {
                                        light.Add(ver);
                                    }
                                    else
                                    {
                                        normal.Add(ver);
                                    }
                                }
                            }
                        }
                    }
                }

                normal = normal.Distinct().OrderByDescending(v => v).ToList();
                light = light.Distinct().OrderByDescending(v => v).ToList();

                var basevers = new List<string>();
                foreach (var normalVer in normal)
                {
                    basevers.Add(normalVer);
                    if (light.Contains(normalVer))
                    {
                        light.Remove(normalVer);
                        basevers.Add($"light-{normalVer}");
                    }
                }

                foreach (var lightVer in light)
                {
                    if (!normal.Contains(lightVer))
                    {
                        basevers.Add($"light-{lightVer}");
                    }
                }

                var installed = GetInstalledVersionsForComponent("openssl", "openssl-");

                return new VersionData
                {
                    Status = "success",
                    Header = "Versões de OpenSSL (SLProWeb) disponíveis:",
                    Versions = basevers,
                    Installed = installed,
                    OrderDescending = false
                };
            }
            catch (Exception ex)
            {
                return new VersionData
                {
                    Status = "error",
                    Message = $"Erro ao obter versões do OpenSSL: {ex.Message}",
                    Versions = new List<string>(),
                    Installed = new List<string>()
                };
            }
        }

        /// <summary>
        /// Obtém versões instaladas para um componente específico
        /// </summary>
        private static List<string> GetInstalledVersionsForComponent(string componentName, string prefix = "", string suffix = "")
        {
            var baseDir = "C:\\devstack";
            var componentDir = componentName switch
            {
                "git" => baseDir,
                _ => Path.Combine(baseDir, componentName)
            };

            if (!Directory.Exists(componentDir))
                return new List<string>();

            try
            {
                var directories = Directory.GetDirectories(componentDir);
                var versions = new List<string>();

                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    
                    if (componentName == "git")
                    {
                        if (dirName.StartsWith("git-"))
                        {
                            versions.Add(dirName.Substring(4)); // Remove "git-"
                        }
                    }
                    else
                    {
                        var version = dirName;
                        
                        if (!string.IsNullOrEmpty(prefix) && version.StartsWith(prefix))
                        {
                            version = version.Substring(prefix.Length);
                        }
                        
                        if (!string.IsNullOrEmpty(suffix) && version.EndsWith(suffix))
                        {
                            version = version.Substring(0, version.Length - suffix.Length);
                        }
                        
                        versions.Add(version);
                    }
                }

                return versions;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Obtém status de um componente específico
        /// </summary>
        public static ComponentStatus GetComponentStatus(string component)
        {
            var baseDir = "C:\\devstack";
            var dir = component.ToLowerInvariant() switch
            {
                "php" => Path.Combine(baseDir, "php"),
                "nginx" => Path.Combine(baseDir, "nginx"),
                "mysql" => Path.Combine(baseDir, "mysql"),
                "nodejs" => Path.Combine(baseDir, "nodejs"),
                "python" => Path.Combine(baseDir, "python"),
                "composer" => Path.Combine(baseDir, "composer"),
                "git" => baseDir,
                "phpmyadmin" => Path.Combine(baseDir, "phpmyadmin"),
                "mongodb" => Path.Combine(baseDir, "mongodb"),
                "redis" => Path.Combine(baseDir, "redis"),
                "pgsql" => Path.Combine(baseDir, "pgsql"),
                "mailhog" => Path.Combine(baseDir, "mailhog"),
                "elasticsearch" => Path.Combine(baseDir, "elasticsearch"),
                "memcached" => Path.Combine(baseDir, "memcached"),
                "docker" => Path.Combine(baseDir, "docker"),
                "yarn" => Path.Combine(baseDir, "yarn"),
                "pnpm" => Path.Combine(baseDir, "pnpm"),
                "wpcli" => Path.Combine(baseDir, "wpcli"),
                "adminer" => Path.Combine(baseDir, "adminer"),
                "poetry" => Path.Combine(baseDir, "poetry"),
                "ruby" => Path.Combine(baseDir, "ruby"),
                "go" => Path.Combine(baseDir, "go"),
                "certbot" => Path.Combine(baseDir, "certbot"),
                "openssl" => Path.Combine(baseDir, "openssl"),
                "php-cs-fixer" => Path.Combine(baseDir, "phpcsfixer"),
                _ => ""
            };

            if (string.IsNullOrEmpty(dir))
            {
                return new ComponentStatus
                {
                    Installed = false,
                    Versions = new List<string>(),
                    Message = "Componente desconhecido"
                };
            }

            var versions = new List<string>();
            if (Directory.Exists(dir))
            {
                try
                {
                    if (component.ToLowerInvariant() == "git")
                    {
                        versions = Directory.GetDirectories(dir)
                            .Select(d => Path.GetFileName(d))
                            .Where(name => name.StartsWith("git-"))
                            .ToList();
                    }
                    else
                    {
                        versions = Directory.GetDirectories(dir)
                            .Select(d => Path.GetFileName(d))
                            .ToList();
                    }
                }
                catch
                {
                    // Ignorar erros de acesso
                }
            }

            if (versions.Count == 0)
            {
                return new ComponentStatus
                {
                    Installed = false,
                    Versions = new List<string>(),
                    Message = $"{component} não está instalado."
                };
            }

            return new ComponentStatus
            {
                Installed = true,
                Versions = versions,
                Message = $"{component} instalado(s)"
            };
        }

        /// <summary>
        /// Obtém status de todos os componentes
        /// </summary>
        public static Dictionary<string, ComponentStatus> GetAllComponentsStatus()
        {
            var components = new[]
            {
                "php", "nginx", "mysql", "nodejs", "python", "composer", "git", "phpmyadmin",
                "mongodb", "redis", "pgsql", "mailhog", "elasticsearch", "memcached",
                "docker", "yarn", "pnpm", "wpcli", "adminer", "poetry", "ruby", "go",
                "certbot", "openssl", "phpcsfixer"
            };

            var results = new Dictionary<string, ComponentStatus>();
            foreach (var comp in components)
            {
                results[comp] = GetComponentStatus(comp);
            }

            return results;
        }
    }

    /// <summary>
    /// Classe para representar o status de um componente
    /// </summary>
    public class ComponentStatus
    {
        public bool Installed { get; set; } = false;
        public List<string> Versions { get; set; } = new List<string>();
        public string Message { get; set; } = "";
    }
}
