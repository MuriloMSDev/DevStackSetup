using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Sites" - gerencia configurações de sites Nginx
    /// </summary>
    public static class GuiSitesTab
    {
        /// <summary>
        /// Cria o conteúdo completo da aba "Sites"
        /// </summary>
        public static Grid CreateSitesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Criar site
            var leftPanel = CreateSiteCreationPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console
            var rightPanel = GuiConsolePanel.CreateConsoleOutputPanel(mainWindow);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de criação de sites
        /// </summary>
        private static UIElement CreateSiteCreationPanel(DevStackGui mainWindow)
        {
            // Usar Grid para permitir overlay
            var grid = new Grid();
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = GuiTheme.CreateStyledLabel("Criar Configuração de Site Nginx", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Domínio
            var domainLabel = GuiTheme.CreateStyledLabel("Domínio do site:");
            panel.Children.Add(domainLabel);

            var domainTextBox = GuiTheme.CreateStyledTextBox();
            domainTextBox.Height = 30;
            domainTextBox.Name = "DomainTextBox";
            panel.Children.Add(domainTextBox);

            // Diretório raiz com botão procurar
            var rootLabel = GuiTheme.CreateStyledLabel("Diretório raiz:");
            panel.Children.Add(rootLabel);

            var rootPanel = new Grid
            {
                Margin = new Thickness(0, 5, 0, 15)
            };
            rootPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rootPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var rootTextBox = GuiTheme.CreateStyledTextBox();
            rootTextBox.Height = 30;
            rootTextBox.Name = "RootTextBox";
            Grid.SetColumn(rootTextBox, 0);
            rootPanel.Children.Add(rootTextBox);

            var browseButton = GuiTheme.CreateStyledButton("📁 Procurar", (s, e) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.ValidateNames = false;
                dialog.CheckFileExists = false;
                dialog.CheckPathExists = true;
                dialog.FileName = "Selecionar Pasta";
                dialog.Filter = "Pastas|*.";
                dialog.Title = "Selecionar Pasta do Site";

                if (dialog.ShowDialog() == true)
                {
                    var selectedPath = Path.GetDirectoryName(dialog.FileName);
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        rootTextBox.Text = selectedPath;
                    }
                }
            });
            browseButton.Width = 100;
            browseButton.Height = 30;
            browseButton.Padding = new Thickness(12, 3, 12, 3);
            Grid.SetColumn(browseButton, 1);
            rootPanel.Children.Add(browseButton);
            panel.Children.Add(rootPanel);

            // PHP Upstream com ComboBox para versões instaladas
            var phpLabel = GuiTheme.CreateStyledLabel("PHP Upstream:");
            panel.Children.Add(phpLabel);

            var phpComboBox = GuiTheme.CreateStyledComboBox();
            phpComboBox.Height = 30;
            phpComboBox.Margin = new Thickness(0, 5, 0, 15);
            phpComboBox.Name = "PhpComboBox";
            
            // Carregar versões PHP instaladas
            LoadPhpVersions(phpComboBox);
            panel.Children.Add(phpComboBox);

            // Nginx Version ComboBox
            var nginxLabel = GuiTheme.CreateStyledLabel("Versão Nginx:");
            panel.Children.Add(nginxLabel);

            var nginxComboBox = GuiTheme.CreateStyledComboBox();
            nginxComboBox.Height = 30;
            nginxComboBox.Margin = new Thickness(0, 5, 0, 15);
            nginxComboBox.Name = "NginxComboBox";

            // Carregar versões Nginx instaladas
            LoadNginxVersions(nginxComboBox);
            panel.Children.Add(nginxComboBox);

            // Overlay de loading (spinner)
            var overlay = GuiTheme.CreateLoadingOverlay();
            // Overlay sempre visível se criando site
            overlay.Visibility = mainWindow.IsCreatingSite ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsCreatingSite))
                {
                    overlay.Visibility = mainWindow.IsCreatingSite ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            // Botão Criar Site
            var createButton = GuiTheme.CreateStyledButton("🌐 Criar Configuração de Site", (s, e) =>
            {
                mainWindow.IsCreatingSite = true;
                overlay.Visibility = Visibility.Visible;
                try
                {
                    var domain = domainTextBox.Text.Trim();
                    var root = rootTextBox.Text.Trim();
                    var phpUpstream = phpComboBox.SelectedItem?.ToString() ?? "";
                    var nginxVersion = nginxComboBox.SelectedItem?.ToString() ?? "";

                    CreateSite(mainWindow, domain, root, phpUpstream, nginxVersion);
                    phpComboBox.SelectedIndex = -1;
                    nginxComboBox.SelectedIndex = -1;
                }
                finally
                {
                    mainWindow.IsCreatingSite = false;
                    overlay.Visibility = Visibility.Collapsed;
                }
            });
            createButton.Height = 40;
            createButton.FontSize = 14;
            createButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(createButton);

            // Seção SSL
            var sslSeparator = new Separator { Margin = new Thickness(0, 20, 0, 10) };
            panel.Children.Add(sslSeparator);

            var sslTitle = GuiTheme.CreateStyledLabel("Certificados SSL", true);
            sslTitle.FontSize = 16;
            sslTitle.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(sslTitle);

            var sslDomainLabel = GuiTheme.CreateStyledLabel("Domínio para SSL:");
            panel.Children.Add(sslDomainLabel);

            var sslDomainTextBox = GuiTheme.CreateStyledTextBox();
            sslDomainTextBox.Height = 30;
            sslDomainTextBox.Margin = new Thickness(0, 5, 0, 15);
            sslDomainTextBox.Name = "SslDomainTextBox";
            panel.Children.Add(sslDomainTextBox);

            Button? generateSslButton = null;
            generateSslButton = GuiTheme.CreateStyledButton("🔒 Gerar Certificado SSL", async (s, e) =>
            {
                mainWindow.IsCreatingSite = true;
                overlay.Visibility = Visibility.Visible;
                if (generateSslButton != null)
                    generateSslButton.IsEnabled = false;
                try
                {
                    var domain = sslDomainTextBox.Text;
                    await Task.Run(async () =>
                    {
                        await GenerateSslCertificate(mainWindow, domain);
                    });
                    await GuiInstalledTab.LoadInstalledComponents(mainWindow);
                }
                finally
                {
                    sslDomainTextBox.Text = "";
                    if (generateSslButton != null)
                        generateSslButton.IsEnabled = true;
                    overlay.Visibility = Visibility.Collapsed;
                    mainWindow.IsCreatingSite = false;
                }
            });
            generateSslButton.Height = 40;
            generateSslButton.FontSize = 14;
            generateSslButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(generateSslButton);

            // Info
            var infoLabel = GuiTheme.CreateStyledLabel("ℹ️ Os arquivos de configuração serão criados automaticamente");
            infoLabel.FontStyle = FontStyles.Italic;
            infoLabel.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(infoLabel);

            // Adiciona painel e overlay ao grid
            grid.Children.Add(panel);
            grid.Children.Add(overlay);

            return grid;
        }

        /// <summary>
        /// Carrega as versões PHP instaladas no ComboBox
        /// </summary>
        private static void LoadPhpVersions(ComboBox phpComboBox)
        {
            try
            {
                var phpDir = Path.Combine("C:\\devstack", "php");
                if (Directory.Exists(phpDir))
                {
                    var phpVersions = Directory.GetDirectories(phpDir)
                        .Select(d => Path.GetFileName(d))
                        .Where(name => name.StartsWith("php-"))
                        .Select(name => name.Replace("php-", ""))
                        .ToList();

                    foreach (var version in phpVersions)
                    {
                        phpComboBox.Items.Add(version);
                    }
                }
            }
            catch {}
        }

        /// <summary>
        /// Carrega as versões Nginx instaladas no ComboBox
        /// </summary>
        private static void LoadNginxVersions(ComboBox nginxComboBox)
        {
            try
            {
                var nginxDir = Path.Combine("C:\\devstack", "nginx");
                if (Directory.Exists(nginxDir))
                {
                    var nginxVersions = Directory.GetDirectories(nginxDir)
                        .Select(d => Path.GetFileName(d))
                        .Where(name => name.StartsWith("nginx-"))
                        .Select(name => name.Replace("nginx-", ""))
                        .ToList();

                    foreach (var version in nginxVersions)
                    {
                        nginxComboBox.Items.Add(version);
                    }
                }
            }
            catch {}
        }

        /// <summary>
        /// Cria uma configuração de site Nginx
        /// </summary>
        private static void CreateSite(DevStackGui mainWindow, string domain, string root, string phpUpstream, string nginxVersion)
        {
            if (string.IsNullOrEmpty(domain))
            {
                GuiTheme.CreateStyledMessageBox("Digite um domínio para o site.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(root))
            {
                GuiTheme.CreateStyledMessageBox("Digite um diretório raiz para o site.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(phpUpstream))
            {
                GuiTheme.CreateStyledMessageBox("Selecione uma versão do PHP para o site.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(nginxVersion))
            {
                GuiTheme.CreateStyledMessageBox("Selecione uma versão do Nginx para o site.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                mainWindow.StatusMessage = $"Criando configuração para o site {domain}...";
                InstallManager.CreateNginxSiteConfig(domain, root, $"127.{phpUpstream}:9000", nginxVersion);
                
                // Reiniciar serviços do Nginx após criar a configuração
                mainWindow.StatusMessage = $"Reiniciando serviços do Nginx...";
                RestartNginxServices(mainWindow);
                
                mainWindow.StatusMessage = $"Site {domain} criado";
                
                // Limpar os campos após sucesso
                var domainTextBox = GuiHelpers.FindChild<TextBox>(mainWindow, "DomainTextBox");
                var rootTextBox = GuiHelpers.FindChild<TextBox>(mainWindow, "RootTextBox");
                if (domainTextBox != null) domainTextBox.Text = "";
                if (rootTextBox != null) rootTextBox.Text = "";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao criar site {domain}: {ex.Message}");
                mainWindow.StatusMessage = $"Erro ao criar site {domain}";
                GuiTheme.CreateStyledMessageBox($"Erro ao criar configuração do site: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gera um certificado SSL para o domínio especificado
        /// </summary>
        private static async Task GenerateSslCertificate(DevStackGui mainWindow, string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GuiTheme.CreateStyledMessageBox("Digite um domínio para gerar o certificado SSL.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
                return;
            }

            // Validação extra: checar se o domínio existe (resolve DNS) em thread separada
            bool domainResolves = false;
            try
            {
                domainResolves = await Task.Run(() =>
                {
                    try
                    {
                        var hostEntry = System.Net.Dns.GetHostEntry(domain);
                        return hostEntry != null && hostEntry.AddressList.Length > 0;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch { domainResolves = false; }

            if (!domainResolves)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GuiTheme.CreateStyledMessageBox($"O domínio '{domain}' não existe ou não está resolvendo para nenhum IP.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
                return;
            }

            try
            {
                mainWindow.StatusMessage = $"Gerando certificado SSL para {domain}...";
                // Chama a lógica compartilhada para geração de certificado
                var args = new string[] { domain };
                await GenerateManager.GenerateSslCertificate(args);
                // O método já faz o output no console, mas podemos adicionar feedback extra se necessário
                mainWindow.StatusMessage = $"Processo de geração de SSL para {domain} finalizado.";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao gerar certificado SSL: {ex.Message}");
                mainWindow.StatusMessage = $"Erro ao gerar SSL para {domain}";
                GuiTheme.CreateStyledMessageBox($"Erro ao gerar certificado SSL: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Reinicia os serviços do Nginx
        /// </summary>
        private static void RestartNginxServices(DevStackGui mainWindow)
        {
            try
            {
                GuiConsolePanel.AppendToConsole(mainWindow, "🔄 Reiniciando serviços do Nginx...");
                
                // Encontrar todas as versões instaladas do Nginx usando os componentes carregados na memória
                var nginxComponents = mainWindow.InstalledComponents
                    .Where(component => component.Name.Equals("nginx", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (nginxComponents.Any())
                {
                    int restartedCount = 0;
                    
                    // Reiniciar cada versão instalada do Nginx
                    foreach (var nginxComponent in nginxComponents)
                    {
                        foreach (var version in nginxComponent.Versions)
                        {
                            try
                            {
                                GuiConsolePanel.AppendToConsole(mainWindow, $"🔄 Reiniciando Nginx v{version}...");
                                ProcessManager.RestartComponent("nginx", version);
                                GuiConsolePanel.AppendToConsole(mainWindow, $"✅ Nginx v{version} reiniciado com sucesso");
                                restartedCount++;
                            }
                            catch (Exception ex)
                            {
                                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao reiniciar Nginx v{version}: {ex.Message}");
                            }
                        }
                    }
                    
                    if (restartedCount == 0)
                    {
                        GuiConsolePanel.AppendToConsole(mainWindow, "ℹ️ Nenhuma versão do Nginx foi reiniciada (podem não estar em execução)");
                    }
                    else
                    {
                        GuiConsolePanel.AppendToConsole(mainWindow, $"✅ {restartedCount} versão(ões) do Nginx reiniciadas");
                    }
                }
                else
                {
                    GuiConsolePanel.AppendToConsole(mainWindow, "❌ Nenhuma versão do Nginx instalada encontrada");
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao reiniciar Nginx: {ex.Message}");
                // Não propagar a exceção para não interromper o fluxo principal
            }
        }
    }
}
