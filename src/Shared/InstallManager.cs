using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevStackManager
{
    public static class InstallManager
    {
        public static async Task InstallCommands(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Nenhum componente especificado para instalar.");
                return;
            }

            string component = args[0];
            string? version = args.Length > 1 ? args[1] : null;
            var comp = Components.ComponentsFactory.GetComponent(component);
            if (comp != null)
            {
                await comp.Install(version);
            }
            else
            {
                Console.WriteLine($"Componente desconhecido: {component}");
            }
        }

        public static void CreateNginxSiteConfig(string domain, string root, string phpUpstream, string nginxVersion)
        {
            string nginxVersionDir = Path.Combine(DevStackConfig.nginxDir, $"nginx-{nginxVersion}");
            string nginxSitesDirFull = Path.Combine(nginxVersionDir, DevStackConfig.nginxSitesDir);

            if (!Directory.Exists(nginxVersionDir))
            {
                throw new Exception($"A versão do Nginx ({nginxVersion}) não está instalada em {nginxVersionDir}.");
            }

            if (!Directory.Exists(nginxSitesDirFull))
            {
                Directory.CreateDirectory(nginxSitesDirFull);
            }

            string confPath = Path.Combine(nginxSitesDirFull, $"{domain}.conf");
            string serverName = $"{domain}.localhost";

            string template = $@"server {{

    listen 80;
    listen [::]:80;

    server_name {serverName};
    root {root};
    index index.php index.html index.htm;

    location / {{
         try_files $uri $uri/ /index.php$is_args$args;
    }}

    location ~ \.php$ {{
        try_files $uri /index.php =404;
        fastcgi_pass {phpUpstream};
        fastcgi_index index.php;
        fastcgi_buffers 16 16k;
        fastcgi_buffer_size 32k;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        fastcgi_read_timeout 600;
        include fastcgi_params;
    }}

    location ~ /\.ht {{
        deny all;
    }}

    location /.well-known/acme-challenge/ {{
        root /var/www/letsencrypt/;
        log_not_found off;
    }}

    location /api {{
        rewrite ^/api/(\w+).*$ /api.php?type=$1 last;
    }}

    error_log logs\{domain}_error.log;
    access_log logs\{domain}_access.log;
}}";

            File.WriteAllText(confPath, template, new UTF8Encoding(false));
            Console.WriteLine($"Arquivo {confPath} criado/configurado com sucesso!");

            string hostsPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot")!, "System32", "drivers", "etc", "hosts");
            string entry = $"127.0.0.1\t{serverName}";
            
            try
            {
                string[] hostsContent = File.Exists(hostsPath) ? File.ReadAllLines(hostsPath) : Array.Empty<string>();
                if (!hostsContent.Contains(entry))
                {
                    File.AppendAllText(hostsPath, Environment.NewLine + entry, Encoding.UTF8);
                    Console.WriteLine($"Adicionado {serverName} ao arquivo hosts.");
                }
                else
                {
                    Console.WriteLine($"{serverName} já está presente no arquivo hosts.");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao modificar arquivo hosts. Execute como administrador.");
            }
        }
    }
}
