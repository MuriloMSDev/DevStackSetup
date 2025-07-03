using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevStackManager
{
    public static class UninstallManager
    {
        public static void UninstallCommands(params string[] components)
        {
            if (components == null || components.Length == 0)
            {
            DevStackConfig.WriteColoredLine("Nenhum componente especificado para desinstalar.", ConsoleColor.Red);
            return;
            }

            string tool = components[0].ToLowerInvariant();
            string? version = components.Length > 1 ? components[1] : null;

            switch (tool)
            {
                case "php":
                    UninstallPHP(version);
                    break;
                case "nginx":
                    if (version == null)
                    UninstallNginx();
                    else
                    UninstallNginx(version);
                    break;
                case "mysql":
                    UninstallMySQL(version);
                    break;
                case "nodejs":
                case "node":
                    UninstallNodeJS(version);
                    break;
                case "python":
                    UninstallPython(version);
                    break;
                case "composer":
                    UninstallComposer(version);
                    break;
                case "phpmyadmin":
                    UninstallPhpMyAdmin(version);
                    break;
                case "git":
                    UninstallGit(version);
                    break;
                case "mongodb":
                    UninstallMongoDB(version);
                    break;
                case "redis":
                    UninstallRedis(version);
                    break;
                case "pgsql":
                    UninstallPgSQL(version);
                    break;
                case "mailhog":
                    UninstallMailHog(version);
                    break;
                case "elasticsearch":
                    UninstallElasticsearch(version);
                    break;
                case "memcached":
                    UninstallMemcached(version);
                    break;
                case "docker":
                    UninstallDocker(version);
                    break;
                case "yarn":
                    UninstallYarn(version);
                    break;
                case "pnpm":
                    UninstallPnpm(version);
                    break;
                case "wpcli":
                    UninstallWPCLI(version);
                    break;
                case "adminer":
                    UninstallAdminer(version);
                    break;
                case "poetry":
                    UninstallPoetry(version);
                    break;
                case "ruby":
                    UninstallRuby(version);
                    break;
                case "go":
                    UninstallGo(version);
                    break;
                case "certbot":
                    UninstallCertbot(version);
                    break;
                case "openssl":
                    UninstallOpenSSL(version);
                    break;
                case "php-cs-fixer":
                    UninstallPHPCsFixer(version);
                    break;
                default:
                    DevStackConfig.WriteColoredLine($"Componente desconhecido: {tool}", ConsoleColor.Red);
                    break;
            }
            DevStackConfig.WriteColoredLine("Uninstall finalizado.", ConsoleColor.Green);
        }

        public static void UninstallGenericTool(string toolDir, string subDir)
        {
            string targetDir = Path.Combine(toolDir, subDir);
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
                DevStackConfig.pathManager?.RemoveFromPath(new[] { targetDir });
                DevStackConfig.WriteColoredLine($"{subDir} removido.", ConsoleColor.Green);
            }
            else
            {
                DevStackConfig.WriteColoredLine($"{subDir} não está instalado.", ConsoleColor.Yellow);
            }
        }

        public static void RollbackInstall(string dirToRemove)
        {
            if (!string.IsNullOrEmpty(dirToRemove) && Directory.Exists(dirToRemove))
            {
                Directory.Delete(dirToRemove, true);
                DevStackConfig.WriteColoredLine($"Rollback: pasta {dirToRemove} removida.", ConsoleColor.Yellow);
            }
        }

        public static void UninstallPHP(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do PHP deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall php 8.4.10", ConsoleColor.Yellow);
                return;
            }

            string phpSubDir = $"php-{version}";
            UninstallGenericTool(DevStackConfig.phpDir, phpSubDir);
        }

        public static void UninstallMySQL(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do MySQL deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall mysql 8.0.36", ConsoleColor.Yellow);
                return;
            }

            string mysqlSubDir = $"mysql-{version}-winx64";
            UninstallGenericTool(DevStackConfig.mysqlDir, mysqlSubDir);
        }

        public static void UninstallNodeJS(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Node.js deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall nodejs 22.17.0", ConsoleColor.Yellow);
                return;
            }

            string nodeSubDir = $"node-v{version}-win-x64";
            UninstallGenericTool(DevStackConfig.nodeDir, nodeSubDir);
        }

        public static void UninstallPython(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Python deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall python 3.12.0", ConsoleColor.Yellow);
                return;
            }

            string pySubDir = $"python-{version}";
            UninstallGenericTool(DevStackConfig.pythonDir, pySubDir);
        }

        public static void UninstallComposer(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Composer deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall composer 2.8.9", ConsoleColor.Yellow);
                return;
            }

            string composerSubDir = $"composer-{version}";
            UninstallGenericTool(DevStackConfig.composerDir, composerSubDir);
        }

        public static void UninstallNginx()
        {
            DevStackConfig.WriteColoredLine("Erro: Versão do Nginx deve ser especificada para desinstalar.", ConsoleColor.Red);
            DevStackConfig.WriteColoredLine("Exemplo: uninstall nginx 1.27.5", ConsoleColor.Yellow);
            DevStackConfig.WriteColoredLine("Use 'list --installed' para ver as versões instaladas.", ConsoleColor.Cyan);
        }

        public static void UninstallNginx(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                UninstallNginx();
                return;
            }

            string nginxSubDir = $"nginx-{version}";
            UninstallGenericTool(DevStackConfig.nginxDir, nginxSubDir);
        }

        public static void UninstallPhpMyAdmin(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do phpMyAdmin deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall phpmyadmin 5.2.1", ConsoleColor.Yellow);
                return;
            }

            string pmaSubDir = $"phpmyadmin-{version}";
            UninstallGenericTool(DevStackConfig.pmaDir, pmaSubDir);
        }

        public static void UninstallGit(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Git deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall git 2.43.0", ConsoleColor.Yellow);
                return;
            }

            string gitSubDir = $"git-{version}";
            UninstallGenericTool(DevStackConfig.baseDir, gitSubDir);
        }

        public static void UninstallMongoDB(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do MongoDB deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall mongodb 7.0.5", ConsoleColor.Yellow);
                return;
            }

            string mongoSubDir = $"mongodb-{version}";
            UninstallGenericTool(DevStackConfig.mongoDir, mongoSubDir);
        }

        public static void UninstallRedis(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Redis deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall redis 7.2.4", ConsoleColor.Yellow);
                return;
            }

            string redisSubDir = $"redis-{version}";
            UninstallGenericTool(DevStackConfig.redisDir, redisSubDir);
        }

        public static void UninstallPgSQL(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do PostgreSQL deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall pgsql 15.5", ConsoleColor.Yellow);
                return;
            }

            string pgsqlSubDir = $"pgsql-{version}";
            UninstallGenericTool(DevStackConfig.pgsqlDir, pgsqlSubDir);
        }

        public static void UninstallMailHog(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do MailHog deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall mailhog 1.0.1", ConsoleColor.Yellow);
                return;
            }

            string mailhogSubDir = $"mailhog-{version}";
            UninstallGenericTool(DevStackConfig.mailhogDir, mailhogSubDir);
        }

        public static void UninstallElasticsearch(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Elasticsearch deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall elasticsearch 8.11.0", ConsoleColor.Yellow);
                return;
            }

            string elasticSubDir = $"elasticsearch-{version}";
            UninstallGenericTool(DevStackConfig.elasticDir, elasticSubDir);
        }

        public static void UninstallMemcached(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Memcached deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall memcached 1.6.22", ConsoleColor.Yellow);
                return;
            }

            string memcachedSubDir = $"memcached-{version}";
            UninstallGenericTool(DevStackConfig.memcachedDir, memcachedSubDir);
        }

        public static void UninstallDocker(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Docker deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall docker 24.0.7", ConsoleColor.Yellow);
                return;
            }

            string dockerSubDir = $"docker-{version}";
            UninstallGenericTool(DevStackConfig.dockerDir, dockerSubDir);
        }

        public static void UninstallYarn(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Yarn deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall yarn 1.22.22", ConsoleColor.Yellow);
                return;
            }

            string yarnSubDir = $"yarn-{version}";
            UninstallGenericTool(DevStackConfig.yarnDir, yarnSubDir);
        }

        public static void UninstallPnpm(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do pnpm deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall pnpm 8.15.0", ConsoleColor.Yellow);
                return;
            }

            string pnpmSubDir = $"pnpm-{version}";
            UninstallGenericTool(DevStackConfig.pnpmDir, pnpmSubDir);
        }

        public static void UninstallWPCLI(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do WP-CLI deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall wpcli 2.10.0", ConsoleColor.Yellow);
                return;
            }

            string wpcliSubDir = $"wpcli-{version}";
            UninstallGenericTool(DevStackConfig.wpcliDir, wpcliSubDir);
        }

        public static void UninstallAdminer(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Adminer deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall adminer 4.8.1", ConsoleColor.Yellow);
                return;
            }

            string adminerSubDir = $"adminer-{version}";
            UninstallGenericTool(DevStackConfig.adminerDir, adminerSubDir);
        }

        public static void UninstallPoetry(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Poetry deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall poetry 1.7.1", ConsoleColor.Yellow);
                return;
            }

            string poetrySubDir = $"poetry-{version}";
            UninstallGenericTool(DevStackConfig.poetryDir, poetrySubDir);
        }

        public static void UninstallRuby(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Ruby deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall ruby 3.2.0", ConsoleColor.Yellow);
                return;
            }

            string rubySubDir = $"ruby-{version}";
            UninstallGenericTool(DevStackConfig.rubyDir, rubySubDir);
        }

        public static void UninstallGo(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Go deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall go 1.21.5", ConsoleColor.Yellow);
                return;
            }

            string goSubDir = $"go-{version}";
            UninstallGenericTool(DevStackConfig.goDir, goSubDir);
        }

        public static void UninstallCertbot(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do Certbot deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall certbot 2.8.0", ConsoleColor.Yellow);
                return;
            }

            string certbotSubDir = $"certbot-{version}";
            UninstallGenericTool(DevStackConfig.certbotDir, certbotSubDir);
        }

        public static void UninstallOpenSSL(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do OpenSSL deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall openssl 3.2.0", ConsoleColor.Yellow);
                return;
            }

            string opensslSubDir = $"openssl-{version}";
            UninstallGenericTool(DevStackConfig.openSSLDir, opensslSubDir);
        }

        public static void UninstallPHPCsFixer(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
            {
                DevStackConfig.WriteColoredLine("Erro: Versão do PHP CS Fixer deve ser especificada para desinstalar.", ConsoleColor.Red);
                DevStackConfig.WriteColoredLine("Exemplo: uninstall php-cs-fixer 3.75.0", ConsoleColor.Yellow);
                return;
            }

            string phpcsfixerSubDir = $"php-cs-fixer-{version}";
            UninstallGenericTool(DevStackConfig.phpcsfixerDir, phpcsfixerSubDir);
        }
    }
}
