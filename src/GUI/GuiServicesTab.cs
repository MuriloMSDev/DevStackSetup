using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DevStackManager
{
    /// <summary>
    /// Converter personalizado para converter bool em Visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Componente responsável pela aba "Serviços" - gerencia e controla serviços do DevStack
    /// </summary>
    public static class GuiServicesTab
    {
        // Overlays de loading usados apenas nesta tab
        private static Border? ServicesLoadingOverlay;
        /// <summary>
        /// Cria o conteúdo completo da aba "Serviços"
        /// </summary>
        public static Grid CreateServicesContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerPanel = CreateServicesHeader(mainWindow);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            // DataGrid de Serviços
            var servicesGrid = CreateServicesDataGrid(mainWindow);
            Grid.SetRow(servicesGrid, 1);
            grid.Children.Add(servicesGrid);

            // Botões de controle
            var controlPanel = CreateServicesControlPanel(mainWindow);
            Grid.SetRow(controlPanel, 2);
            grid.Children.Add(controlPanel);


            // Overlay de loading único cobrindo toda a área de serviços
            var overlay = GuiTheme.CreateLoadingOverlay();
            // Overlay sempre visível se carregando serviços
            overlay.Visibility = mainWindow.IsLoadingServices ? Visibility.Visible : Visibility.Collapsed;
            mainWindow.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindow.IsLoadingServices))
                {
                    overlay.Visibility = mainWindow.IsLoadingServices ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            Grid.SetRowSpan(overlay, 3);
            grid.Children.Add(overlay);
            ServicesLoadingOverlay = overlay;

            return grid;
        }

        /// <summary>
        /// Cria o cabeçalho da aba de serviços
        /// </summary>
        private static StackPanel CreateServicesHeader(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };

            var titleLabel = GuiTheme.CreateStyledLabel("Gerenciamento de Serviços", true);
            titleLabel.FontSize = 18;
            panel.Children.Add(titleLabel);

            var refreshButton = GuiTheme.CreateStyledButton("🔄 Atualizar", async (s, e) => await LoadServices(mainWindow));
            refreshButton.Width = 100;
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(20, 0, 0, 0);
            panel.Children.Add(refreshButton);

            return panel;
        }

        /// <summary>
        /// Cria o DataGrid para exibir os serviços
        /// </summary>
        private static DataGrid CreateServicesDataGrid(DevStackGui mainWindow)
        {
            var dataGrid = new DataGrid
            {
                Margin = new Thickness(10),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single,
                Name = "ServicesDataGrid"
            };

            // Coluna Componente usando DataTemplate
            var componentColumn = new DataGridTemplateColumn
            {
                Header = "Componente",
                Width = new DataGridLength(120)
            };

            var componentTemplate = new DataTemplate();
            var componentTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            componentTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            componentTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
            componentTemplate.VisualTree = componentTextBlockFactory;
            componentColumn.CellTemplate = componentTemplate;

            dataGrid.Columns.Add(componentColumn);

            // Coluna Versão usando DataTemplate
            var versionColumn = new DataGridTemplateColumn
            {
                Header = "Versão",
                Width = new DataGridLength(100)
            };

            var versionTemplate = new DataTemplate();
            var versionTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            versionTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Version"));
            versionTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
            versionTemplate.VisualTree = versionTextBlockFactory;
            versionColumn.CellTemplate = versionTemplate;

            dataGrid.Columns.Add(versionColumn);

            // Coluna Status com colorização usando DataTemplate
            var statusColumn = new DataGridTemplateColumn
            {
                Header = "Status",
                Width = new DataGridLength(120)
            };

            var statusTemplate = new DataTemplate();
            var statusTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            statusTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Status"));
            statusTextBlockFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("StatusColor"));
            statusTextBlockFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            statusTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
            statusTemplate.VisualTree = statusTextBlockFactory;
            statusColumn.CellTemplate = statusTemplate;

            dataGrid.Columns.Add(statusColumn);

            // Coluna PID usando DataTemplate
            var pidColumn = new DataGridTemplateColumn
            {
                Header = "PID",
                Width = new DataGridLength(330)
            };

            var pidTemplate = new DataTemplate();
            var pidTextBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            pidTextBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("Pid"));
            pidTextBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(12, 0, 0, 0));
            pidTemplate.VisualTree = pidTextBlockFactory;
            pidColumn.CellTemplate = pidTemplate;

            dataGrid.Columns.Add(pidColumn);

            // Coluna Copiar PID (botão) - Aparece apenas quando executando
            var copyButtonTemplate = new DataTemplate();
            var copyButtonFactory = new FrameworkElementFactory(typeof(Button));
            copyButtonFactory.SetValue(Button.ContentProperty, "📋");
            copyButtonFactory.SetValue(Button.HeightProperty, 25.0);
            copyButtonFactory.SetValue(Button.WidthProperty, 35.0);
            copyButtonFactory.SetValue(Button.FontSizeProperty, 12.0);
            copyButtonFactory.SetValue(Button.ToolTipProperty, "Copiar PID");
            copyButtonFactory.SetValue(Button.BackgroundProperty, GuiTheme.CurrentTheme.ButtonBackground);
            copyButtonFactory.SetValue(Button.ForegroundProperty, GuiTheme.CurrentTheme.ButtonForeground);
            copyButtonFactory.SetValue(Button.BorderBrushProperty, GuiTheme.CurrentTheme.Border);
            copyButtonFactory.SetValue(Button.BorderThicknessProperty, new Thickness(1));
            copyButtonFactory.SetValue(Button.FontWeightProperty, FontWeights.Medium);
            copyButtonFactory.SetValue(Button.CursorProperty, Cursors.Hand);
            
            // Usar binding com converter personalizado para visibilidade
            var visibilityBinding = new Binding("IsRunning");
            visibilityBinding.Converter = new BooleanToVisibilityConverter();
            copyButtonFactory.SetBinding(Button.VisibilityProperty, visibilityBinding);
            
            copyButtonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => CopyPidButton_Click(sender, e, mainWindow)));
            copyButtonTemplate.VisualTree = copyButtonFactory;

            dataGrid.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Copiar PID",
                CellTemplate = copyButtonTemplate,
                Width = new DataGridLength(100)
            });

            // Coluna Ações (Start, Stop, Restart)
            var actionsTemplate = new DataTemplate();
            var actionsPanel = new FrameworkElementFactory(typeof(StackPanel));
            actionsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            actionsPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            // Criar estilos personalizados para os botões de ação
            var startButtonStyle = CreateActionButtonStyle(GuiTheme.CurrentTheme.Success, GuiTheme.CurrentTheme.AccentHover);
            var stopButtonStyle = CreateActionButtonStyle(GuiTheme.CurrentTheme.Warning, new SolidColorBrush(Color.FromRgb(217, 164, 6)));
            var restartButtonStyle = CreateActionButtonStyle(GuiTheme.CurrentTheme.ButtonBackground, GuiTheme.CurrentTheme.ButtonHover);

            // Botão Start
            var startButton = new FrameworkElementFactory(typeof(Button));
            startButton.SetValue(Button.ContentProperty, "▶️");
            startButton.SetValue(Button.WidthProperty, 30.0);
            startButton.SetValue(Button.HeightProperty, 25.0);
            startButton.SetValue(Button.MarginProperty, new Thickness(2));
            startButton.SetValue(Button.ToolTipProperty, "Iniciar");
            startButton.SetValue(Button.StyleProperty, startButtonStyle);
            startButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => StartServiceButton_Click(sender, e, mainWindow)));
            actionsPanel.AppendChild(startButton);

            // Botão Stop
            var stopButton = new FrameworkElementFactory(typeof(Button));
            stopButton.SetValue(Button.ContentProperty, "⏹️");
            stopButton.SetValue(Button.WidthProperty, 30.0);
            stopButton.SetValue(Button.HeightProperty, 25.0);
            stopButton.SetValue(Button.MarginProperty, new Thickness(2));
            stopButton.SetValue(Button.ToolTipProperty, "Parar");
            stopButton.SetValue(Button.StyleProperty, stopButtonStyle);
            stopButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => StopServiceButton_Click(sender, e, mainWindow)));
            actionsPanel.AppendChild(stopButton);

            // Botão Restart
            var restartButton = new FrameworkElementFactory(typeof(Button));
            restartButton.SetValue(Button.ContentProperty, "🔄");
            restartButton.SetValue(Button.WidthProperty, 30.0);
            restartButton.SetValue(Button.HeightProperty, 25.0);
            restartButton.SetValue(Button.MarginProperty, new Thickness(2));
            restartButton.SetValue(Button.ToolTipProperty, "Reiniciar");
            restartButton.SetValue(Button.StyleProperty, restartButtonStyle);
            restartButton.AddHandler(Button.ClickEvent, new RoutedEventHandler((sender, e) => RestartServiceButton_Click(sender, e, mainWindow)));
            actionsPanel.AppendChild(restartButton);

            actionsTemplate.VisualTree = actionsPanel;


            // Coluna Ações usando DataTemplate
            var actionsColumn = new DataGridTemplateColumn
            {
                Header = "Ações",
                Width = new DataGridLength(120)
            };
            
            
            // Centralizar o header da coluna Ações
            var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, GuiTheme.CurrentTheme.GridHeaderBackground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, GuiTheme.CurrentTheme.GridHeaderForeground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 14.0));
            actionsColumn.HeaderStyle = headerStyle;

            actionsColumn.CellTemplate = actionsTemplate;

            dataGrid.Columns.Add(actionsColumn);

            var servicesBinding = new Binding("Services") { Source = mainWindow };
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, servicesBinding);

            // Apply dark theme to DataGrid
            GuiTheme.SetDataGridDarkTheme(dataGrid);

            return dataGrid;
        }

        /// <summary>
        /// Manipula o clique no botão de copiar PID
        /// </summary>
        private static void CopyPidButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    if (service.Pid != "-" && !string.IsNullOrEmpty(service.Pid))
                    {
                        Clipboard.SetText(service.Pid);
                        
                        mainWindow.StatusMessage = $"PID {service.Pid} copiado para a área de transferência";
                    }
                    else
                    {
                        GuiTheme.CreateStyledMessageBox("Serviço não está em execução, não há PID para copiar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao copiar PID: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao copiar PID";
            }
        }

        /// <summary>
        /// Manipula o clique no botão de iniciar serviço individual
        /// </summary>
        private static async void StartServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    mainWindow.StatusMessage = $"Iniciando {service.Name} versão {service.Version}...";
                    
                    await Task.Run(() =>
                    {
                        ProcessManager.StartComponent(service.Name, service.Version);
                    });

                    mainWindow.StatusMessage = $"{service.Name} iniciado com sucesso";
                    await LoadServices(mainWindow); // Recarregar lista
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao iniciar serviço: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao iniciar serviço";
                DevStackConfig.WriteLog($"Erro ao iniciar serviço na GUI: {ex}");
            }
        }

        /// <summary>
        /// Manipula o clique no botão de parar serviço individual
        /// </summary>
        private static async void StopServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    mainWindow.StatusMessage = $"Parando {service.Name} versão {service.Version}...";
                    
                    await Task.Run(() =>
                    {
                        ProcessManager.StopComponent(service.Name, service.Version);
                    });

                    mainWindow.StatusMessage = $"{service.Name} parado com sucesso";
                    await LoadServices(mainWindow); // Recarregar lista
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao parar serviço: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao parar serviço";
                DevStackConfig.WriteLog($"Erro ao parar serviço na GUI: {ex}");
            }
        }

        /// <summary>
        /// Manipula o clique no botão de reiniciar serviço individual
        /// </summary>
        private static async void RestartServiceButton_Click(object sender, RoutedEventArgs e, DevStackGui mainWindow)
        {
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    mainWindow.StatusMessage = $"Reiniciando {service.Name} versão {service.Version}...";
                    
                    await Task.Run(() =>
                    {
                        ProcessManager.RestartComponent(service.Name, service.Version);
                    });

                    mainWindow.StatusMessage = $"{service.Name} reiniciado com sucesso";
                    await LoadServices(mainWindow); // Recarregar lista
                }
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao reiniciar serviço: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao reiniciar serviço";
                DevStackConfig.WriteLog($"Erro ao reiniciar serviço na GUI: {ex}");
            }
        }

        /// <summary>
        /// Cria o painel de controle dos serviços
        /// </summary>
        private static StackPanel CreateServicesControlPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Botões para todos os serviços
            var startAllButton = GuiTheme.CreateStyledButton("▶️ Iniciar Todos", async (s, e) =>
            {
                mainWindow.IsLoadingServices = true;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
                try { await StartAllServices(mainWindow); }
                finally
                {
                    mainWindow.IsLoadingServices = false;
                    if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            });
            startAllButton.Width = 140;
            startAllButton.Height = 40;
            startAllButton.Margin = new Thickness(10);
            panel.Children.Add(startAllButton);

            var stopAllButton = GuiTheme.CreateStyledButton("⏹️ Parar Todos", async (s, e) =>
            {
                mainWindow.IsLoadingServices = true;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
                try { await StopAllServices(mainWindow); }
                finally
                {
                    mainWindow.IsLoadingServices = false;
                    if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            });
            stopAllButton.Width = 140;
            stopAllButton.Height = 40;
            stopAllButton.Margin = new Thickness(10);
            panel.Children.Add(stopAllButton);

            var restartAllButton = GuiTheme.CreateStyledButton("🔄 Reiniciar Todos", async (s, e) =>
            {
                mainWindow.IsLoadingServices = true;
                if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Visible;
                try { await RestartAllServices(mainWindow); }
                finally
                {
                    mainWindow.IsLoadingServices = false;
                    if (ServicesLoadingOverlay != null) ServicesLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            });
            restartAllButton.Width = 140;
            restartAllButton.Height = 40;
            restartAllButton.Margin = new Thickness(10);
            panel.Children.Add(restartAllButton);

            return panel;
        }

        /// <summary>
        /// Carrega a lista de serviços
        /// </summary>
        public static async Task LoadServices(DevStackGui mainWindow)
        {
            await Task.Run(() =>
            {
                try
                {
                    mainWindow.StatusMessage = "Carregando serviços...";
                    
                    var services = new ObservableCollection<ServiceViewModel>();
                    
                    // Usar o diretório base do DevStackConfig
                    var devStackPath = DevStackConfig.baseDir;
                    
                    // Verificar se as configurações foram inicializadas
                    if (string.IsNullOrEmpty(devStackPath))
                    {
                        DevStackConfig.Initialize();
                        devStackPath = DevStackConfig.baseDir;
                    }
                    
                    DevStackConfig.WriteLog($"DevStack base directory: {devStackPath}");
                    DevStackConfig.WriteLog($"PHP directory: {DevStackConfig.phpDir}");
                    DevStackConfig.WriteLog($"Nginx directory: {DevStackConfig.nginxDir}");
                    
                    // Debug: Listar todos os processos que começam com php ou nginx
                    var allProcesses = Process.GetProcesses();
                    var debugProcesses = allProcesses
                        .Where(p => p.ProcessName.StartsWith("php", StringComparison.OrdinalIgnoreCase) ||
                                   p.ProcessName.StartsWith("nginx", StringComparison.OrdinalIgnoreCase) ||
                                   p.ProcessName.StartsWith("mysql", StringComparison.OrdinalIgnoreCase) ||
                                   p.ProcessName.Equals("node", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    
                    DevStackConfig.WriteLog($"Processos encontrados para debug: {debugProcesses.Count}");
                    foreach (var proc in debugProcesses)
                    {
                        try
                        {
                            var path = proc.MainModule?.FileName ?? "N/A";
                            DevStackConfig.WriteLog($"  - {proc.ProcessName} (PID: {proc.Id}) - Path: {path}");
                        }
                        catch (Exception ex)
                        {
                            DevStackConfig.WriteLog($"  - {proc.ProcessName} (PID: {proc.Id}) - Path: Erro ao acessar ({ex.Message})");
                        }
                    }
                    
                    // Detectar serviços PHP-FPM
                    if (Directory.Exists(DevStackConfig.phpDir))
                    {
                        var phpDirs = Directory.GetDirectories(DevStackConfig.phpDir);
                        DevStackConfig.WriteLog($"Encontrados {phpDirs.Length} diretórios PHP: {string.Join(", ", phpDirs.Select(Path.GetFileName))}");
                        
                        foreach (var dir in phpDirs)
                        {
                            var dirName = Path.GetFileName(dir);
                            var versionNumber = dirName.Replace("php-", "");
                            
                            DevStackConfig.WriteLog($"Verificando PHP versão {versionNumber} no diretório {dirName}");
                            
                            try
                            {
                                // Buscar processos PHP usando wildcards como no PowerShell
                                var phpProcesses = allProcesses
                                    .Where(p => {
                                        try
                                        {
                                            if (p.ProcessName.StartsWith("php", StringComparison.OrdinalIgnoreCase))
                                            {
                                                var processPath = p.MainModule?.FileName;
                                                var contains = !string.IsNullOrEmpty(processPath) && processPath.Contains(dirName, StringComparison.OrdinalIgnoreCase);
                                                if (contains)
                                                {
                                                    DevStackConfig.WriteLog($"  - Processo PHP encontrado: {p.ProcessName} (PID: {p.Id}) - Path: {processPath}");
                                                }
                                                return contains;
                                            }
                                            return false;
                                        }
                                        catch (Exception ex)
                                        {
                                            DevStackConfig.WriteLog($"  - Erro ao verificar processo {p.ProcessName}: {ex.Message}");
                                            return false;
                                        }
                                    })
                                    .ToList();
                                
                                if (phpProcesses.Any())
                                {
                                    var pids = string.Join(", ", phpProcesses.Select(p => p.Id));
                                    DevStackConfig.WriteLog($"PHP {versionNumber} está executando com PIDs: {pids}");
                                    services.Add(new ServiceViewModel 
                                    { 
                                        Name = "php", 
                                        Version = versionNumber,
                                        Status = "Em execução", 
                                        Type = "PHP-FPM", 
                                        Description = $"PHP {versionNumber} FastCGI",
                                        Pid = pids,
                                        IsRunning = true
                                    });
                                }
                                else
                                {
                                    DevStackConfig.WriteLog($"PHP {versionNumber} não está executando");
                                    services.Add(new ServiceViewModel 
                                    { 
                                        Name = "php", 
                                        Version = versionNumber,
                                        Status = "Parado", 
                                        Type = "PHP-FPM", 
                                        Description = $"PHP {versionNumber} FastCGI",
                                        Pid = "-",
                                        IsRunning = false
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                DevStackConfig.WriteLog($"Erro ao verificar processos PHP: {ex.Message}");
                            }
                        }
                    }
                    
                    // Detectar serviços Nginx
                    if (Directory.Exists(DevStackConfig.nginxDir))
                    {
                        var nginxDirs = Directory.GetDirectories(DevStackConfig.nginxDir);
                        DevStackConfig.WriteLog($"Encontrados {nginxDirs.Length} diretórios Nginx: {string.Join(", ", nginxDirs.Select(Path.GetFileName))}");
                        
                        foreach (var dir in nginxDirs)
                        {
                            var dirName = Path.GetFileName(dir);
                            var versionNumber = dirName.Replace("nginx-", "");
                            
                            DevStackConfig.WriteLog($"Verificando Nginx versão {versionNumber} no diretório {dirName}");
                            
                            try
                            {
                                // Buscar processos Nginx usando wildcards como no PowerShell
                                var nginxProcesses = allProcesses
                                    .Where(p => {
                                        try
                                        {
                                            if (p.ProcessName.StartsWith("nginx", StringComparison.OrdinalIgnoreCase))
                                            {
                                                var processPath = p.MainModule?.FileName;
                                                var contains = !string.IsNullOrEmpty(processPath) && processPath.Contains(dirName, StringComparison.OrdinalIgnoreCase);
                                                if (contains)
                                                {
                                                    DevStackConfig.WriteLog($"  - Processo Nginx encontrado: {p.ProcessName} (PID: {p.Id}) - Path: {processPath}");
                                                }
                                                return contains;
                                            }
                                            return false;
                                        }
                                        catch (Exception ex)
                                        {
                                            DevStackConfig.WriteLog($"  - Erro ao verificar processo {p.ProcessName}: {ex.Message}");
                                            return false;
                                        }
                                    })
                                    .ToList();
                                
                                if (nginxProcesses.Any())
                                {
                                    var mainProcess = nginxProcesses.First();
                                    DevStackConfig.WriteLog($"Nginx {versionNumber} está executando com PID: {mainProcess.Id}");
                                    services.Add(new ServiceViewModel 
                                    { 
                                        Name = "nginx", 
                                        Version = versionNumber,
                                        Status = "Em execução", 
                                        Type = "Web Server", 
                                        Description = $"Nginx {versionNumber}",
                                        Pid = mainProcess.Id.ToString(),
                                        IsRunning = true
                                    });
                                }
                                else
                                {
                                    DevStackConfig.WriteLog($"Nginx {versionNumber} não está executando");
                                    services.Add(new ServiceViewModel 
                                    { 
                                        Name = "nginx", 
                                        Version = versionNumber,
                                        Status = "Parado", 
                                        Type = "Web Server", 
                                        Description = $"Nginx {versionNumber}",
                                        Pid = "-",
                                        IsRunning = false
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                DevStackConfig.WriteLog($"Erro ao verificar processos Nginx: {ex.Message}");
                            }
                        }
                    }
                    
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.Services.Clear();
                        foreach (var service in services)
                        {
                            mainWindow.Services.Add(service);
                        }
                        mainWindow.StatusMessage = $"{mainWindow.Services.Count} serviços carregados";
                    });
                }
                catch (Exception ex)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.StatusMessage = $"Erro ao carregar serviços: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar serviços na GUI: {ex}");
                    });
                }
            });
        }

        /// <summary>
        /// Inicia todos os serviços
        /// </summary>
        private static async Task StartAllServices(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = "Iniciando todos os serviços...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StartAllComponents();
                });

                mainWindow.StatusMessage = "Todos os serviços iniciados";
                await LoadServices(mainWindow); // Recarregar lista
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao iniciar todos os serviços: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao iniciar todos os serviços";
                DevStackConfig.WriteLog($"Erro ao iniciar todos os serviços na GUI: {ex}");
            }
        }

        /// <summary>
        /// Para todos os serviços
        /// </summary>
        private static async Task StopAllServices(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = "Parando todos os serviços...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StopAllComponents();
                });

                mainWindow.StatusMessage = "Todos os serviços parados";
                await LoadServices(mainWindow); // Recarregar lista
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao parar todos os serviços: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao parar todos os serviços";
                DevStackConfig.WriteLog($"Erro ao parar todos os serviços na GUI: {ex}");
            }
        }

        /// <summary>
        /// Reinicia todos os serviços
        /// </summary>
        private static async Task RestartAllServices(DevStackGui mainWindow)
        {
            mainWindow.StatusMessage = "Reiniciando todos os serviços...";
            
            try
            {
                await Task.Run(() =>
                {
                    // Parar todos e depois iniciar todos
                    ProcessManager.StopAllComponents();
                    System.Threading.Thread.Sleep(2000); // Aguardar 2 segundos
                    ProcessManager.StartAllComponents();
                });

                mainWindow.StatusMessage = "Todos os serviços reiniciados";
                await LoadServices(mainWindow); // Recarregar lista
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao reiniciar todos os serviços: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao reiniciar todos os serviços";
                DevStackConfig.WriteLog($"Erro ao reiniciar todos os serviços na GUI: {ex}");
            }
        }

        /// <summary>
        /// Cria um estilo personalizado para botões de ação com hover funcional
        /// </summary>
        private static Style CreateActionButtonStyle(SolidColorBrush backgroundColor, SolidColorBrush hoverColor)
        {
            var buttonStyle = new Style(typeof(Button));
            
            // Set base properties
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, backgroundColor));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, GuiTheme.CurrentTheme.ButtonForeground));
            buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, GuiTheme.CurrentTheme.Border));
            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Medium));
            buttonStyle.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            
            // Template customizado para garantir que triggers funcionem
            var buttonTemplate = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "ButtonBorder";
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenterFactory.SetBinding(ContentPresenter.MarginProperty, new Binding("Padding") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            
            borderFactory.AppendChild(contentPresenterFactory);
            buttonTemplate.VisualTree = borderFactory;
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));
            
            // Add hover trigger
            var hoverTrigger = new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, hoverColor));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, GuiTheme.CurrentTheme.BorderHover));
            hoverTrigger.Setters.Add(new Setter(Button.ForegroundProperty, GuiTheme.CurrentTheme.ButtonForeground));
            
            // Add pressed trigger
            var pressedTrigger = new Trigger
            {
                Property = Button.IsPressedProperty,
                Value = true
            };
            
            var pressedColor = new SolidColorBrush(Color.FromArgb(
                255,
                (byte)(hoverColor.Color.R * 0.9),
                (byte)(hoverColor.Color.G * 0.9),
                (byte)(hoverColor.Color.B * 0.9)
            ));
            
            pressedTrigger.Setters.Add(new Setter(Button.BackgroundProperty, pressedColor));
            
            // Add disabled trigger
            var disabledTrigger = new Trigger
            {
                Property = Button.IsEnabledProperty,
                Value = false
            };
            disabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, GuiTheme.CurrentTheme.ButtonDisabled));
            disabledTrigger.Setters.Add(new Setter(Button.ForegroundProperty, GuiTheme.CurrentTheme.TextMuted));
            disabledTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.6));
            
            buttonStyle.Triggers.Add(hoverTrigger);
            buttonStyle.Triggers.Add(pressedTrigger);
            buttonStyle.Triggers.Add(disabledTrigger);
            
            return buttonStyle;
        }
    }
}
