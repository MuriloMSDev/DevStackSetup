using System;
using System.IO;

namespace DevStackManager
{
    /// <summary>
    /// Classe que contém as configurações e diretórios compartilhados entre CLI e GUI
    /// </summary>
    public static class DevStackConfig
    {
        public static string baseDir = "";
        public static string phpDir = "";
        public static string nginxDir = "";
        public static string mysqlDir = "";
        public static string nodeDir = "";
        public static string pythonDir = "";
        public static string composerDir = "";
        public static string pmaDir = "";
        public static string mongoDir = "";
        public static string redisDir = "";
        public static string pgsqlDir = "";
        public static string mailhogDir = "";
        public static string elasticDir = "";
        public static string memcachedDir = "";
        public static string dockerDir = "";
        public static string yarnDir = "";
        public static string pnpmDir = "";
        public static string wpcliDir = "";
        public static string adminerDir = "";
        public static string poetryDir = "";
        public static string rubyDir = "";
        public static string goDir = "";
        public static string certbotDir = "";
        public static string openSSLDir = "";
        public static string phpcsfixerDir = "";
        public static string nginxSitesDir = "";
        public static string tmpDir = "";
        
        public static PathManager? pathManager;

        /// <summary>
        /// Inicializa todas as configurações de diretórios
        /// </summary>
        public static void Initialize()
        {
            baseDir = "C:\\devstack";
            phpDir = Path.Combine(baseDir, "php");
            nginxDir = Path.Combine(baseDir, "nginx");
            mysqlDir = Path.Combine(baseDir, "mysql");
            nodeDir = Path.Combine(baseDir, "nodejs");
            pythonDir = Path.Combine(baseDir, "python");
            composerDir = Path.Combine(baseDir, "composer");
            pmaDir = Path.Combine(baseDir, "phpmyadmin");
            mongoDir = Path.Combine(baseDir, "mongodb");
            redisDir = Path.Combine(baseDir, "redis");
            pgsqlDir = Path.Combine(baseDir, "pgsql");
            mailhogDir = Path.Combine(baseDir, "mailhog");
            elasticDir = Path.Combine(baseDir, "elasticsearch");
            memcachedDir = Path.Combine(baseDir, "memcached");
            dockerDir = Path.Combine(baseDir, "docker");
            yarnDir = Path.Combine(baseDir, "yarn");
            pnpmDir = Path.Combine(baseDir, "pnpm");
            wpcliDir = Path.Combine(baseDir, "wpcli");
            adminerDir = Path.Combine(baseDir, "adminer");
            poetryDir = Path.Combine(baseDir, "poetry");
            rubyDir = Path.Combine(baseDir, "ruby");
            goDir = Path.Combine(baseDir, "go");
            certbotDir = Path.Combine(baseDir, "certbot");
            openSSLDir = Path.Combine(baseDir, "openssl");
            phpcsfixerDir = Path.Combine(baseDir, "phpcsfixer");
            nginxSitesDir = "conf\\sites-enabled";
            tmpDir = Path.Combine(baseDir, "tmp");
            
            // Inicializar PathManager
            pathManager = new PathManager(baseDir, phpDir, nodeDir, pythonDir, nginxDir, mysqlDir);
        }
        
        /// <summary>
        /// Escreve mensagem no log
        /// </summary>
        public static void WriteLog(string message)
        {
            try
            {
                var logFile = Path.Combine(baseDir, "devstack.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logFile) ?? baseDir);
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
            catch
            {
                // Ignore log errors
            }
        }
        
        /// <summary>
        /// Escreve linha colorida no console (para compatibilidade com CLI)
        /// </summary>
        public static void WriteColoredLine(string message, ConsoleColor color)
        {
            try
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
            catch
            {
                // Fallback para texto simples
                Console.WriteLine(message);
            }
        }
    }
}
