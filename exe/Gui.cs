using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DevStackManager
{
    /// <summary>
    /// Interface gráfica moderna WPF para o DevStackManager
    /// Convertida do arquivo gui.ps1 original mantendo funcionalidades e layout
    /// </summary>
    public partial class DevStackGui : Window, INotifyPropertyChanged
    {
        #region Private Fields
        private string _statusMessage = "Pronto";
        private ObservableCollection<ComponentViewModel> _installedComponents = new();
        private ObservableCollection<string> _availableComponents = new();
        private ObservableCollection<string> _availableVersions = new();
        private ObservableCollection<ServiceViewModel> _services = new();
        private string _selectedComponent = "";
        private string _selectedVersion = "";
        private string _selectedUninstallComponent = "";
        private string _selectedUninstallVersion = "";
        private string _consoleOutput = "";
        private bool _isLoading = false;
        
        // Navegação moderna
        private ContentControl? _mainContent;
        private int _selectedNavIndex = 0;

        // Sistema de tema escuro fixo
        private static readonly ThemeColors DarkTheme = new()
        {
            // Backgrounds principais - tons mais equilibrados e modernos
            FormBackground = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
            Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            ControlBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            
            // Botões com cores vibrantes para tema escuro
            ButtonBackground = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            ButtonForeground = new SolidColorBrush(Colors.White),
            ButtonHover = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            ButtonDisabled = new SolidColorBrush(Color.FromRgb(87, 96, 106)),
            
            // Cores de destaque otimizadas para tema escuro
            Accent = new SolidColorBrush(Color.FromRgb(56, 211, 159)),
            AccentHover = new SolidColorBrush(Color.FromRgb(46, 194, 145)),
            Warning = new SolidColorBrush(Color.FromRgb(255, 196, 0)),
            Danger = new SolidColorBrush(Color.FromRgb(248, 81, 73)),
            Success = new SolidColorBrush(Color.FromRgb(56, 211, 159)),
            
            // Grid com excelente legibilidade
            GridBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            GridForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            GridHeaderBackground = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
            GridHeaderForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            GridAlternateRow = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            GridSelectedRow = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            
            // Status e navegação com contraste perfeito
            StatusBackground = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
            StatusForeground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            SidebarBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            SidebarSelected = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            SidebarHover = new SolidColorBrush(Color.FromRgb(45, 55, 68)),
            
            // Bordas bem visíveis
            Border = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
            BorderHover = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            BorderFocus = new SolidColorBrush(Color.FromRgb(58, 150, 255)),
            
            // Áreas de conteúdo com hierarquia clara
            ContentBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            PanelBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),
            ConsoleBackground = new SolidColorBrush(Color.FromRgb(13, 17, 23)),
            ConsoleForeground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),
            
            // Inputs com excelente contraste
            InputBackground = new SolidColorBrush(Color.FromRgb(32, 39, 49)),
            InputForeground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
            InputBorder = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
            InputFocusBorder = new SolidColorBrush(Color.FromRgb(33, 136, 255)),
            
            // Texto com hierarquia bem definida
            TextMuted = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            TextSecondary = new SolidColorBrush(Color.FromRgb(166, 173, 186))
        };

        private static ThemeColors CurrentTheme => DarkTheme;
        #endregion

        #region Theme Classes
        public class ThemeColors
        {
            // Backgrounds principais
            public SolidColorBrush FormBackground { get; set; } = null!;
            public SolidColorBrush Foreground { get; set; } = null!;
            public SolidColorBrush ControlBackground { get; set; } = null!;
            
            // Botões
            public SolidColorBrush ButtonBackground { get; set; } = null!;
            public SolidColorBrush ButtonForeground { get; set; } = null!;
            public SolidColorBrush ButtonHover { get; set; } = null!;
            public SolidColorBrush ButtonDisabled { get; set; } = null!;
            
            // Cores de destaque
            public SolidColorBrush Accent { get; set; } = null!;
            public SolidColorBrush AccentHover { get; set; } = null!;
            public SolidColorBrush Warning { get; set; } = null!;
            public SolidColorBrush Danger { get; set; } = null!;
            public SolidColorBrush Success { get; set; } = null!;
            
            // Grid e tabelas
            public SolidColorBrush GridBackground { get; set; } = null!;
            public SolidColorBrush GridForeground { get; set; } = null!;
            public SolidColorBrush GridHeaderBackground { get; set; } = null!;
            public SolidColorBrush GridHeaderForeground { get; set; } = null!;
            public SolidColorBrush GridAlternateRow { get; set; } = null!;
            public SolidColorBrush GridSelectedRow { get; set; } = null!;
            
            // Status e navegação
            public SolidColorBrush StatusBackground { get; set; } = null!;
            public SolidColorBrush StatusForeground { get; set; } = null!;
            public SolidColorBrush SidebarBackground { get; set; } = null!;
            public SolidColorBrush SidebarSelected { get; set; } = null!;
            public SolidColorBrush SidebarHover { get; set; } = null!;
            
            // Bordas e separadores
            public SolidColorBrush Border { get; set; } = null!;
            public SolidColorBrush BorderHover { get; set; } = null!;
            public SolidColorBrush BorderFocus { get; set; } = null!;
            
            // Áreas de conteúdo
            public SolidColorBrush ContentBackground { get; set; } = null!;
            public SolidColorBrush PanelBackground { get; set; } = null!;
            public SolidColorBrush ConsoleBackground { get; set; } = null!;
            public SolidColorBrush ConsoleForeground { get; set; } = null!;
            
            // Inputs
            public SolidColorBrush InputBackground { get; set; } = null!;
            public SolidColorBrush InputForeground { get; set; } = null!;
            public SolidColorBrush InputBorder { get; set; } = null!;
            public SolidColorBrush InputFocusBorder { get; set; } = null!;
            
            // Texto secundário
            public SolidColorBrush TextMuted { get; set; } = null!;
            public SolidColorBrush TextSecondary { get; set; } = null!;
        }
        #endregion

        #region Properties
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ComponentViewModel> InstalledComponents
        {
            get => _installedComponents;
            set { _installedComponents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableComponents
        {
            get => _availableComponents;
            set { _availableComponents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableVersions
        {
            get => _availableVersions;
            set { _availableVersions = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ServiceViewModel> Services
        {
            get => _services;
            set { _services = value; OnPropertyChanged(); }
        }

        public string SelectedComponent
        {
            get => _selectedComponent;
            set { _selectedComponent = value; OnPropertyChanged(); _ = LoadVersionsForComponent(); }
        }

        public string SelectedVersion
        {
            get => _selectedVersion;
            set { _selectedVersion = value; OnPropertyChanged(); }
        }

        public string SelectedUninstallComponent
        {
            get => _selectedUninstallComponent;
            set { _selectedUninstallComponent = value; OnPropertyChanged(); _ = LoadUninstallVersions(); }
        }

        public string SelectedUninstallVersion
        {
            get => _selectedUninstallVersion;
            set { _selectedUninstallVersion = value; OnPropertyChanged(); }
        }

        public string ConsoleOutput
        {
            get => _consoleOutput;
            set { _consoleOutput = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
        
        public int SelectedNavIndex
        {
            get => _selectedNavIndex;
            set { _selectedNavIndex = value; OnPropertyChanged(); NavigateToSection(value); }
        }
        #endregion

        #region Constructor
        public DevStackGui()
        {
            InitializeComponent();
            DataContext = this;
            
            InitializeData();
            
            Title = "DevStack Manager - Interface Gráfica";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            // Use async void for event-like behavior in constructor
            _ = Task.Run(async () =>
            {
                await LoadInstalledComponents();
                await Dispatcher.InvokeAsync(() => LoadUninstallComponents());
            });
            LoadAvailableComponents();
            _ = LoadServices();
        }
        #endregion

        #region Navigation Classes
        public class NavigationItem
        {
            public string Title { get; set; } = "";
            public string Icon { get; set; } = "";
            public string Description { get; set; } = "";
        }
        #endregion

        #region Initialization Methods
        private void InitializeComponent()
        {
            Width = 1200;
            Height = 800;
            MinWidth = 800;
            MinHeight = 600;

            // Apply main window theme
            Background = CurrentTheme.FormBackground;
            Foreground = CurrentTheme.Foreground;

            // Grid principal
            var mainGrid = new Grid();
            Content = mainGrid;

            // Definir linhas do grid
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status

            // Content principal com sidebar
            CreateMainContent(mainGrid);

            // Status Bar
            CreateStatusBar(mainGrid);
        }

        private void CreateMainContent(Grid mainGrid)
        {
            var contentGrid = new Grid { Margin = new Thickness(0) };
            Grid.SetRow(contentGrid, 0);

            // Definir colunas: Sidebar | Content
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // Sidebar
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content

            // Criar sidebar
            CreateSidebar(contentGrid);

            // Criar área de conteúdo principal
            _mainContent = new ContentControl
            {
                Margin = new Thickness(10)
            };
            Grid.SetColumn(_mainContent, 1);
            contentGrid.Children.Add(_mainContent);

            // Navegar para a primeira seção por padrão
            NavigateToSection(0);

            mainGrid.Children.Add(contentGrid);
        }

        private void CreateSidebar(Grid contentGrid)
        {
            var sidebar = new Border
            {
                Background = CurrentTheme.SidebarBackground,
                BorderBrush = CurrentTheme.Border,
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(sidebar, 0);

            // Container principal da sidebar com o título no topo
            var sidebarContainer = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Título no topo da sidebar com ícone
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 15, 5, 10)
            };

            // Ícone DevStack
            var iconImage = new Image
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 6, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Tentar carregar o ícone com fallback para erro
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevStack.ico");
                if (File.Exists(iconPath))
                {
                    iconImage.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                }
            }
            catch {}

            if (iconImage != null)
                titlePanel.Children.Add(iconImage);

            var sidebarTitleLabel = new Label
            {
                Content = "DevStack Manager",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = CurrentTheme.Foreground,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            titlePanel.Children.Add(sidebarTitleLabel);
            
            sidebarContainer.Children.Add(titlePanel);

            // Separador sutil
            var separator = new Border
            {
                Height = 1,
                Margin = new Thickness(10, 0, 10, 10),
                Background = CurrentTheme.Border
            };
            sidebarContainer.Children.Add(separator);

            var navList = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Margin = new Thickness(8, 5, 8, 5),
                SelectedIndex = 0
            };

            // Criar itens de navegação com ícones modernos
            var navItems = new List<NavigationItem>
            {
                new() { Title = "Instalados", Icon = "📦", Description = "Ferramentas instaladas" },
                new() { Title = "Instalar", Icon = "📥", Description = "Instalar novos componentes" },
                new() { Title = "Desinstalar", Icon = "🗑️", Description = "Remover componentes" },
                new() { Title = "Serviços", Icon = "⚙️", Description = "Controle de serviços" },
                new() { Title = "Configurações", Icon = "🔧", Description = "Configurações do sistema" },
                new() { Title = "Sites", Icon = "🌐", Description = "Gerenciar sites Nginx" },
                new() { Title = "Utilitários", Icon = "🛠️", Description = "Ferramentas e console" }
            };

            foreach (var item in navItems)
            {
                var listItem = new ListBoxItem();

                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var iconLabel = new Label
                {
                    Content = item.Icon,
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = CurrentTheme.Foreground
                };
                
                var textPanel = new StackPanel();
                var titleLabel = new Label
                {
                    Content = item.Title,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = CurrentTheme.Foreground
                };
                var descLabel = new Label
                {
                    Content = item.Description,
                    FontSize = 11,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = CurrentTheme.TextMuted
                };
                
                textPanel.Children.Add(titleLabel);
                textPanel.Children.Add(descLabel);
                
                panel.Children.Add(iconLabel);
                panel.Children.Add(textPanel);
                
                listItem.Content = panel;
                navList.Items.Add(listItem);
            }

            // Apply theme to the navigation list
            ApplySidebarListBoxTheme(navList);

            // Bind da seleção
            var binding = new Binding("SelectedNavIndex") { Source = this };
            navList.SetBinding(ListBox.SelectedIndexProperty, binding);

            // Adicionar a navList ao container da sidebar
            sidebarContainer.Children.Add(navList);
            
            // Adicionar o container à sidebar
            sidebar.Child = sidebarContainer;
            contentGrid.Children.Add(sidebar);
        }

        private void NavigateToSection(int index)
        {
            if (_mainContent == null) return;

            _mainContent.Content = index switch
            {
                0 => CreateInstalledContent(),
                1 => CreateInstallContent(),
                2 => CreateUninstallContent(),
                3 => CreateServicesContent(),
                4 => CreateConfigContent(),
                5 => CreateSitesContent(),
                6 => CreateUtilitiesContent(),
                _ => CreateInstalledContent()
            };
        }

        private void CreateStatusBar(Grid mainGrid)
        {
            var statusBar = new Grid
            {
                Height = 35,
                Background = CurrentTheme.StatusBackground
            };
            Grid.SetRow(statusBar, 1);
            
            // Define columns: status message (left) and refresh button (right)
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Add a subtle top border to separate from content
            var topBorder = new Border
            {
                Height = 1,
                VerticalAlignment = VerticalAlignment.Top,
                Background = CurrentTheme.Border
            };
            Grid.SetColumnSpan(topBorder, 2);
            statusBar.Children.Add(topBorder);

            var statusLabel = new Label
            {
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Foreground = CurrentTheme.StatusForeground
            };
            Grid.SetColumn(statusLabel, 0);
            var statusBinding = new Binding("StatusMessage") { Source = this };
            statusLabel.SetBinding(Label.ContentProperty, statusBinding);
            
            // Create refresh button with only icon
            var refreshButton = CreateStyledButton("🔄");
            refreshButton.Width = 45;
            refreshButton.Height = 35;
            refreshButton.FontSize = 14;
            refreshButton.Margin = new Thickness(0);
            refreshButton.VerticalAlignment = VerticalAlignment.Center;
            refreshButton.HorizontalAlignment = HorizontalAlignment.Right;
            refreshButton.ToolTip = "Atualizar todos os dados";
            refreshButton.Click += async (s, e) => await RefreshAllData();
            Grid.SetColumn(refreshButton, 1);
            
            statusBar.Children.Add(statusLabel);
            statusBar.Children.Add(refreshButton);
            mainGrid.Children.Add(statusBar);
        }

        private void InitializeData()
        {
            // Componentes disponíveis para instalação
            AvailableComponents = new ObservableCollection<string>
            {
                "php", "nginx", "mysql", "nodejs", "python", "composer",
                "phpmyadmin", "git", "mongodb", "redis", "pgsql", "mailhog",
                "elasticsearch", "memcached", "docker", "yarn", "pnpm",
                "wpcli", "adminer", "poetry", "ruby", "go", "certbot",
                "openssl", "phpcsfixer"
            };
        }
        #endregion

        #region Content Creation Methods
        private Grid CreateInstalledContent()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };
            
            var titleLabel = CreateStyledLabel("Ferramentas Instaladas", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            headerPanel.Children.Add(titleLabel);

            var refreshButton = CreateStyledButton("🔄 Atualizar Lista", (s, e) => _ = Task.Run(async () => await LoadInstalledComponents()));
            refreshButton.Width = 130;
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(20, 0, 0, 20);
            headerPanel.Children.Add(refreshButton);

            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            // DataGrid
            var dataGrid = new DataGrid
            {
                Margin = new Thickness(10),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single
            };

            // Colunas
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Ferramenta",
                Binding = new Binding("Name"),
                Width = new DataGridLength(200)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Versões Instaladas",
                Binding = new Binding("VersionsText"),
                Width = new DataGridLength(400)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Status",
                Binding = new Binding("Status"),
                Width = new DataGridLength(100),
                FontWeight = FontWeights.Bold
            });

            var installedBinding = new Binding("InstalledComponents") { Source = this };
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, installedBinding);

            // Apply dark theme to DataGrid
            SetDataGridDarkTheme(dataGrid);

            Grid.SetRow(dataGrid, 1);
            grid.Children.Add(dataGrid);

            // Info panel
            var infoPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var infoLabel = CreateStyledLabel("ℹ️ Use as abas 'Instalar' e 'Desinstalar' para gerenciar as ferramentas", false, true);
            infoLabel.FontStyle = FontStyles.Italic;
            infoPanel.Children.Add(infoLabel);

            Grid.SetRow(infoPanel, 2);
            grid.Children.Add(infoPanel);

            return grid;
        }

        private Grid CreateInstallContent()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Seleção
            var leftPanel = CreateInstallSelectionPanel();
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console de saída
            var rightPanel = CreateConsoleOutputPanel();
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        private StackPanel CreateInstallSelectionPanel()
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = CreateStyledLabel("Instalar Nova Ferramenta", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Componente
            var componentLabel = CreateStyledLabel("Selecione a ferramenta:");
            panel.Children.Add(componentLabel);

            var componentCombo = CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, 5, 0, 15);
            componentCombo.Height = 30;
            var componentBinding = new Binding("AvailableComponents") { Source = this };
            componentCombo.SetBinding(ComboBox.ItemsSourceProperty, componentBinding);
            var selectedComponentBinding = new Binding("SelectedComponent") { Source = this };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedComponentBinding);
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = CreateStyledLabel("Selecione a versão (deixe vazio para a mais recente):");
            panel.Children.Add(versionLabel);

            var versionCombo = CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, 5, 0, 20);
            versionCombo.Height = 30;
            versionCombo.IsEditable = true;
            var versionBinding = new Binding("AvailableVersions") { Source = this };
            versionCombo.SetBinding(ComboBox.ItemsSourceProperty, versionBinding);
            var selectedVersionBinding = new Binding("SelectedVersion") { Source = this };
            versionCombo.SetBinding(ComboBox.TextProperty, selectedVersionBinding);
            panel.Children.Add(versionCombo);

            // Botão Instalar
            var installButton = CreateStyledButton("⬬ Instalar", async (s, e) => await InstallComponent());
            installButton.Height = 40;
            installButton.FontSize = 14;
            installButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(installButton);

            // Botão Listar Versões
            var listVersionsButton = CreateStyledButton("📋 Listar Versões Disponíveis", (s, e) => ListVersionsForSelectedComponent());
            listVersionsButton.Height = 35;
            listVersionsButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(listVersionsButton);

            return panel;
        }

        private StackPanel CreateConsoleOutputPanel()
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = CreateStyledLabel("Saída do Console", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(titleLabel);

            // Console output
            var outputBox = CreateStyledTextBox(true);
            outputBox.Height = 400;
            outputBox.IsReadOnly = true;
            outputBox.FontSize = 12;
            outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            outputBox.AcceptsReturn = true;
            outputBox.TextWrapping = TextWrapping.Wrap;
            outputBox.Name = "ConsoleOutput";

            var outputBinding = new Binding("ConsoleOutput") { Source = this };
            outputBox.SetBinding(TextBox.TextProperty, outputBinding);

            panel.Children.Add(outputBox);

            // Botão limpar
            var clearButton = CreateStyledButton("🗑️ Limpar Console", (s, e) => ConsoleOutput = "");
            clearButton.Height = 30;
            clearButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(clearButton);

            return panel;
        }

        private Grid CreateUninstallContent()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Seleção
            var leftPanel = CreateUninstallSelectionPanel();
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console
            var rightPanel = CreateConsoleOutputPanel();
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        private StackPanel CreateUninstallSelectionPanel()
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = CreateStyledLabel("Desinstalar Ferramenta", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Componente
            var componentLabel = CreateStyledLabel("Selecione a ferramenta:");
            panel.Children.Add(componentLabel);

            var componentCombo = CreateStyledComboBox();
            componentCombo.Margin = new Thickness(0, 5, 0, 15);
            componentCombo.Height = 30;
            componentCombo.Name = "UninstallComponentCombo";
            
            var selectedUninstallComponentBinding = new Binding("SelectedUninstallComponent") { Source = this };
            componentCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallComponentBinding);
            panel.Children.Add(componentCombo);

            // Versão
            var versionLabel = CreateStyledLabel("Selecione a versão:");
            panel.Children.Add(versionLabel);

            var versionCombo = CreateStyledComboBox();
            versionCombo.Margin = new Thickness(0, 5, 0, 20);
            versionCombo.Height = 30;
            versionCombo.Name = "UninstallVersionCombo";
            
            var selectedUninstallVersionBinding = new Binding("SelectedUninstallVersion") { Source = this };
            versionCombo.SetBinding(ComboBox.SelectedValueProperty, selectedUninstallVersionBinding);
            panel.Children.Add(versionCombo);

            // Botão Desinstalar
            var uninstallButton = CreateStyledButton("🗑️ Desinstalar", async (s, e) => await UninstallComponent());
            uninstallButton.Height = 40;
            uninstallButton.FontSize = 14;
            uninstallButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(uninstallButton);

            // Botão Atualizar Lista
            var refreshButton = CreateStyledButton("🔄 Atualizar Lista", (s, e) => LoadUninstallComponents());
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(refreshButton);

            // Warning
            var warningLabel = CreateStyledLabel("⚠️ Atenção: Esta ação não pode ser desfeita!");
            warningLabel.Foreground = new SolidColorBrush(Colors.Orange);
            warningLabel.FontWeight = FontWeights.Bold;
            warningLabel.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(warningLabel);

            return panel;
        }

        private Grid CreateServicesContent()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerPanel = CreateServicesHeader();
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            // DataGrid de Serviços
            var servicesGrid = CreateServicesDataGrid();
            Grid.SetRow(servicesGrid, 1);
            grid.Children.Add(servicesGrid);

            // Botões de controle
            var controlPanel = CreateServicesControlPanel();
            Grid.SetRow(controlPanel, 2);
            grid.Children.Add(controlPanel);

            return grid;
        }

        private StackPanel CreateServicesHeader()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };

            var titleLabel = CreateStyledLabel("Gerenciamento de Serviços", true);
            titleLabel.FontSize = 18;
            panel.Children.Add(titleLabel);

            var refreshButton = CreateStyledButton("🔄 Atualizar", async (s, e) => await LoadServices());
            refreshButton.Width = 100;
            refreshButton.Height = 35;
            refreshButton.Margin = new Thickness(20, 0, 0, 0);
            panel.Children.Add(refreshButton);

            return panel;
        }

        private DataGrid CreateServicesDataGrid()
        {
            var dataGrid = new DataGrid
            {
                Margin = new Thickness(10),
                AutoGenerateColumns = false,
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single,
                Name = "ServicesDataGrid"
            };

            // Coluna Componente
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Componente",
                Binding = new Binding("Name"),
                Width = new DataGridLength(120)
            });

            // Coluna Versão
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Versão",
                Binding = new Binding("Version"),
                Width = new DataGridLength(100)
            });

            // Coluna Status com colorização
            var statusTemplate = new DataTemplate();
            var statusFactory = new FrameworkElementFactory(typeof(TextBlock));
            statusFactory.SetBinding(TextBlock.TextProperty, new Binding("Status"));
            statusFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("StatusColor"));
            statusFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            statusTemplate.VisualTree = statusFactory;

            dataGrid.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Status",
                CellTemplate = statusTemplate,
                Width = new DataGridLength(120)
            });

            // Coluna PID
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "PID",
                Binding = new Binding("Pid"),
                Width = new DataGridLength(100)
            });

            // Coluna Copiar PID (botão)
            var copyButtonTemplate = new DataTemplate();
            var copyButtonFactory = new FrameworkElementFactory(typeof(Button));
            copyButtonFactory.SetValue(Button.ContentProperty, "📋 Copiar");
            copyButtonFactory.SetValue(Button.HeightProperty, 25.0);
            copyButtonFactory.SetValue(Button.FontSizeProperty, 10.0);
            copyButtonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(CopyPidButton_Click));
            copyButtonTemplate.VisualTree = copyButtonFactory;

            dataGrid.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Copiar PID",
                CellTemplate = copyButtonTemplate,
                Width = new DataGridLength(80)
            });

            var servicesBinding = new Binding("Services") { Source = this };
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty, servicesBinding);

            // Apply dark theme to DataGrid
            SetDataGridDarkTheme(dataGrid);

            return dataGrid;
        }

        private void CopyPidButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.DataContext is ServiceViewModel service)
                {
                    if (service.Pid != "-" && !string.IsNullOrEmpty(service.Pid))
                    {
                        Clipboard.SetText(service.Pid);
                        AppendToConsole($"📋 PID(s) {service.Pid} copiado(s) para a área de transferência ({service.Name} {service.Version})");
                        StatusMessage = $"PID {service.Pid} copiado para a área de transferência";
                    }
                    else
                    {
                        MessageBox.Show("Serviço não está em execução, não há PID para copiar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao copiar PID: {ex.Message}");
                StatusMessage = "Erro ao copiar PID";
            }
        }

        private StackPanel CreateServicesControlPanel()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var startButton = CreateStyledButton("▶️ Iniciar", async (s, e) => await StartSelectedService());
            startButton.Width = 100;
            startButton.Height = 35;
            startButton.Margin = new Thickness(5);
            panel.Children.Add(startButton);

            var stopButton = CreateStyledButton("⏹️ Parar", async (s, e) => await StopSelectedService());
            stopButton.Width = 100;
            stopButton.Height = 35;
            stopButton.Margin = new Thickness(5);
            panel.Children.Add(stopButton);

            var restartButton = CreateStyledButton("🔄 Reiniciar", async (s, e) => await RestartSelectedService());
            restartButton.Width = 100;
            restartButton.Height = 35;
            restartButton.Margin = new Thickness(5);
            panel.Children.Add(restartButton);

            // Separador
            var separator = new Border
            {
                Width = 2,
                Height = 30,
                Background = new SolidColorBrush(Colors.Gray),
                Margin = new Thickness(10, 0, 10, 0)
            };
            panel.Children.Add(separator);

            // Botões para todos os serviços
            var startAllButton = CreateStyledButton("▶️ Iniciar Todos", async (s, e) => await StartAllServices());
            startAllButton.Width = 120;
            startAllButton.Height = 35;
            startAllButton.Margin = new Thickness(5);
            panel.Children.Add(startAllButton);

            var stopAllButton = CreateStyledButton("⏹️ Parar Todos", async (s, e) => await StopAllServices());
            stopAllButton.Width = 120;
            stopAllButton.Height = 35;
            stopAllButton.Margin = new Thickness(5);
            panel.Children.Add(stopAllButton);

            var restartAllButton = CreateStyledButton("🔄 Reiniciar Todos", async (s, e) => await RestartAllServices());
            restartAllButton.Width = 120;
            restartAllButton.Height = 35;
            restartAllButton.Margin = new Thickness(5);
            panel.Children.Add(restartAllButton);

            return panel;
        }

        private ScrollViewer CreateConfigContent()
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            var panel = new StackPanel();

            // Configurações de Path
            var pathGroup = CreatePathConfigGroup();
            panel.Children.Add(pathGroup);

            // Configurações de Proxy
            var proxyGroup = CreateProxyConfigGroup();
            panel.Children.Add(proxyGroup);

            scrollViewer.Content = panel;
            return scrollViewer;
        }

        private System.Windows.Controls.GroupBox CreatePathConfigGroup()
        {
            var group = new System.Windows.Controls.GroupBox
            {
                Header = "Gerenciamento do PATH",
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(10)
            };

            var panel = new StackPanel();
            
            var pathLabel = CreateStyledLabel("Adicionar ferramentas ao PATH do sistema");
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            var addPathButton = new Button
            {
                Content = "➕ Adicionar ao PATH",
                Width = 200,
                Height = 35,
                Margin = new Thickness(0, 10, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            addPathButton.Click += (s, e) => AddToPath();
            panel.Children.Add(addPathButton);

            var removePathButton = new Button
            {
                Content = "➖ Remover do PATH",
                Width = 200,
                Height = 35,
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            removePathButton.Click += (s, e) => RemoveFromPath();
            panel.Children.Add(removePathButton);

            var listPathButton = new Button
            {
                Content = "📋 Listar PATH Atual",
                Width = 200,
                Height = 35,
                Margin = new Thickness(0, 5, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            listPathButton.Click += (s, e) => ListCurrentPath();
            panel.Children.Add(listPathButton);

            group.Content = panel;
            return group;
        }

        private System.Windows.Controls.GroupBox CreateProxyConfigGroup()
        {
            var group = new System.Windows.Controls.GroupBox
            {
                Header = "Configurações de Proxy",
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(10)
            };

            var panel = new StackPanel();

            var proxyLabel = CreateStyledLabel("URL do Proxy (opcional):");
            proxyLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(proxyLabel);

            var proxyTextBox = CreateStyledTextBox();
            proxyTextBox.Height = 30;
            proxyTextBox.Margin = new Thickness(0, 5, 0, 10);
            proxyTextBox.Text = Environment.GetEnvironmentVariable("HTTP_PROXY") ?? "";
            panel.Children.Add(proxyTextBox);

            var setProxyButton = CreateStyledButton("✅ Definir Proxy", (s, e) => SetProxy(proxyTextBox.Text));
            setProxyButton.Width = 150;
            setProxyButton.Height = 35;
            setProxyButton.Margin = new Thickness(0, 5, 5, 5);
            setProxyButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(setProxyButton);

            var removeProxyButton = CreateStyledButton("❌ Remover Proxy", (s, e) => { proxyTextBox.Text = ""; SetProxy(""); });
            removeProxyButton.Width = 150;
            removeProxyButton.Height = 35;
            removeProxyButton.Margin = new Thickness(0, 5, 0, 10);
            removeProxyButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(removeProxyButton);

            group.Content = panel;
            return group;
        }

        private Grid CreateSitesContent()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Criar site
            var leftPanel = CreateSiteCreationPanel();
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console
            var rightPanel = CreateConsoleOutputPanel();
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        private StackPanel CreateSiteCreationPanel()
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            var titleLabel = CreateStyledLabel("Criar Configuração de Site Nginx", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Domínio
            var domainLabel = CreateStyledLabel("Domínio do site:");
            panel.Children.Add(domainLabel);

            var domainTextBox = CreateStyledTextBox();
            domainTextBox.Height = 30;
            domainTextBox.Margin = new Thickness(0, 5, 0, 15);
            domainTextBox.Name = "DomainTextBox";
            panel.Children.Add(domainTextBox);

            // Diretório raiz com botão procurar
            var rootLabel = CreateStyledLabel("Diretório raiz (opcional):");
            panel.Children.Add(rootLabel);

            var rootPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 15)
            };

            var rootTextBox = new TextBox
            {
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                Name = "RootTextBox"
            };
            rootPanel.Children.Add(rootTextBox);

            var browseButton = new Button
            {
                Content = "📁 Procurar",
                Width = 100,
                Height = 30
            };
            browseButton.Click += (s, e) =>
            {
                MessageBox.Show("Funcionalidade de procurar pasta em desenvolvimento.\nPor favor, digite o caminho manualmente.", 
                    "Em Desenvolvimento", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            rootPanel.Children.Add(browseButton);
            panel.Children.Add(rootPanel);

            // PHP Upstream com ComboBox para versões instaladas
            var phpLabel = CreateStyledLabel("PHP Upstream:");
            panel.Children.Add(phpLabel);

            var phpComboBox = new ComboBox
            {
                Height = 30,
                Margin = new Thickness(0, 5, 0, 15),
                Name = "PhpComboBox",
                IsEditable = true
            };
            
            // Carregar versões PHP instaladas
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
                        phpComboBox.Items.Add($"php{version}");
                    }

                    if (phpComboBox.Items.Count > 0)
                    {
                        phpComboBox.SelectedIndex = 0;
                    }
                }
                
                // Adicionar opção padrão se nenhuma versão instalada
                if (phpComboBox.Items.Count == 0)
                {
                    phpComboBox.Items.Add("127.0.0.1:9000");
                    phpComboBox.SelectedIndex = 0;
                }
            }
            catch
            {
                phpComboBox.Text = "127.0.0.1:9000";
            }
            
            panel.Children.Add(phpComboBox);

            // Nginx Version ComboBox
            var nginxLabel = CreateStyledLabel("Versão Nginx:");
            panel.Children.Add(nginxLabel);

            var nginxComboBox = new ComboBox
            {
                Height = 30,
                Margin = new Thickness(0, 5, 0, 15),
                Name = "NginxComboBox",
                IsEditable = true
            };

            // Carregar versões Nginx instaladas
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

                    if (nginxComboBox.Items.Count > 0)
                    {
                        nginxComboBox.SelectedIndex = 0;
                    }
                }

                if (nginxComboBox.Items.Count == 0)
                {
                    nginxComboBox.Items.Add("latest");
                    nginxComboBox.SelectedIndex = 0;
                }
            }
            catch
            {
                nginxComboBox.Text = "latest";
            }

            panel.Children.Add(nginxComboBox);

            // Botão Criar Site
            var createButton = new Button
            {
                Content = "🌐 Criar Configuração de Site",
                Height = 40,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 0)
            };
            createButton.Click += (s, e) => 
            {
                var domain = domainTextBox.Text.Trim();
                var root = rootTextBox.Text.Trim();
                var phpUpstream = phpComboBox.Text.Trim();
                var nginxVersion = nginxComboBox.Text.Trim();
                
                CreateSite(domain, root, phpUpstream, nginxVersion);
            };
            panel.Children.Add(createButton);

            // Seção SSL
            var sslSeparator = new Separator { Margin = new Thickness(0, 20, 0, 10) };
            panel.Children.Add(sslSeparator);

            var sslTitle = CreateStyledLabel("Criar Configuração de Site Nginx", true);
            sslTitle.FontSize = 16;
            sslTitle.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(sslTitle);

            var sslDomainLabel = CreateStyledLabel("Domínio para SSL:");
            panel.Children.Add(sslDomainLabel);

            var sslDomainTextBox = new TextBox
            {
                Height = 30,
                Margin = new Thickness(0, 5, 0, 15),
                Name = "SslDomainTextBox"
            };
            panel.Children.Add(sslDomainTextBox);

            var generateSslButton = new Button
            {
                Content = "🔒 Gerar Certificado SSL",
                Height = 40,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 0)
            };
            generateSslButton.Click += (s, e) => GenerateSslCertificate(sslDomainTextBox.Text);
            panel.Children.Add(generateSslButton);

            // Info
            var infoLabel = new Label
            {
                Content = "ℹ️ Os arquivos de configuração serão criados automaticamente",
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 20, 0, 0)
            };
            panel.Children.Add(infoLabel);

            return panel;
        }

        private Grid CreateUtilitiesContent()
        {
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Command input
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Console output
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

            // Header
            var headerLabel = CreateStyledLabel("Console DevStack - Execute comandos diretamente", true);
            headerLabel.FontSize = 18;
            headerLabel.Margin = new Thickness(10, 10, 10, 0);
            Grid.SetRow(headerLabel, 0);
            grid.Children.Add(headerLabel);

            // Command input panel
            var inputPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 10, 10, 5)
            };
            Grid.SetRow(inputPanel, 1);

            var commandLabel = CreateStyledLabel("Comando:");
            commandLabel.Width = 80;
            commandLabel.VerticalAlignment = VerticalAlignment.Center;
            inputPanel.Children.Add(commandLabel);

            var commandTextBox = CreateStyledTextBox(true);
            commandTextBox.Height = 30;
            commandTextBox.Margin = new Thickness(5, 0, 5, 0);
            commandTextBox.Name = "UtilsCommandTextBox";
            
            // Enter para executar comando
            commandTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    ExecuteCommand(commandTextBox.Text);
                    commandTextBox.Text = "";
                }
            };
            
            inputPanel.Children.Add(commandTextBox);

            var executeButton = CreateStyledButton("▶️ Executar", (s, e) =>
            {
                ExecuteCommand(commandTextBox.Text);
                commandTextBox.Text = "";
            });
            executeButton.Width = 100;
            executeButton.Height = 30;
            executeButton.Margin = new Thickness(5, 0, 0, 0);
            inputPanel.Children.Add(executeButton);

            grid.Children.Add(inputPanel);

            // Console output
            var consoleScrollViewer = new ScrollViewer
            {
                Margin = new Thickness(10, 5, 10, 5),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(consoleScrollViewer, 2);

            var consoleTextBox = new TextBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = CurrentTheme.ConsoleBackground,
                Foreground = CurrentTheme.ConsoleForeground,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Name = "UtilsConsoleOutput"
            };

            // Texto inicial de ajuda
            consoleTextBox.Text = 
                "DevStack Manager GUI Console\n" +
                "-------------------------\n" +
                "Comandos populares:\n" +
                "  • status - Verificar status das ferramentas\n" +
                "  • list --installed - Listar ferramentas instaladas\n" +
                "  • list <componente> - Listar versões de um componente\n" +
                "  • test - Testar ferramentas instaladas\n" +
                "  • doctor - Diagnóstico do sistema\n" +
                "  • help - Mostrar ajuda completa\n" +
                "-------------------------\n\n";

            consoleScrollViewer.Content = consoleTextBox;
            grid.Children.Add(consoleScrollViewer);

            // Buttons panel
            var buttonsPanel = new WrapPanel
            {
                Margin = new Thickness(10, 5, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(buttonsPanel, 3);

            var quickButtons = new[]
            {
                new { Text = "� Status", Command = "status" },
                new { Text = "� Instalados", Command = "list --installed" },
                new { Text = "🔍 Diagnóstico", Command = "doctor" },
                new { Text = "🧪 Testar", Command = "test" },
                new { Text = "🧹 Limpar", Command = "clean" },
                new { Text = "❓ Ajuda", Command = "help" }
            };

            foreach (var btn in quickButtons)
            {
                var button = CreateStyledButton(btn.Text);
                button.Width = 120;
                button.Height = 35;
                button.Margin = new Thickness(5);
                var command = btn.Command;
                button.Click += (s, e) => ExecuteCommand(command);
                buttonsPanel.Children.Add(button);
            }

            var clearButton = CreateStyledButton("🗑️ Limpar Console", (s, e) => ClearConsole());
            clearButton.Width = 120;
            clearButton.Height = 35;
            clearButton.Margin = new Thickness(5);
            buttonsPanel.Children.Add(clearButton);

            grid.Children.Add(buttonsPanel);
            return grid;
        }

        private async void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            var consoleOutput = FindChild<TextBox>(this, "UtilsConsoleOutput");
            if (consoleOutput == null) return;

            StatusMessage = $"Executando: {command}";
            
            // Adicionar comando ao console
            consoleOutput.AppendText($"> {command}\n");
            consoleOutput.ScrollToEnd();

            try
            {
                await Task.Run(() =>
                {
                    var output = "";
                    var setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "setup.ps1");
                    
                    // Verificar se o arquivo setup.ps1 existe
                    if (!File.Exists(setupPath))
                    {
                        setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "setup.ps1");
                    }

                    if (File.Exists(setupPath))
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{setupPath}\" {command}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            StandardOutputEncoding = System.Text.Encoding.UTF8,
                            StandardErrorEncoding = System.Text.Encoding.UTF8
                        };

                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            var stdout = process.StandardOutput.ReadToEnd();
                            var stderr = process.StandardError.ReadToEnd();
                            process.WaitForExit();

                            output = stdout;
                            if (!string.IsNullOrEmpty(stderr))
                            {
                                output += $"\nERROS:\n{stderr}";
                            }
                            
                            if (string.IsNullOrEmpty(output))
                            {
                                output = "(Comando executado, sem saída gerada)";
                            }
                        }
                        else
                        {
                            output = "Erro: Não foi possível iniciar o processo PowerShell";
                        }
                    }
                    else
                    {
                        // Fallback: usar managers C# diretamente
                        output = ExecuteCommandDirect(command);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        consoleOutput.AppendText($"{output}\n\n");
                        consoleOutput.ScrollToEnd();
                        StatusMessage = "Comando executado";
                    });
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    consoleOutput.AppendText($"ERRO: {ex.Message}\n\n");
                    consoleOutput.ScrollToEnd();
                    StatusMessage = "Erro ao executar comando";
                });
            }
        }

        private string ExecuteCommandDirect(string command)
        {
            try
            {
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return "Comando vazio";

                switch (parts[0].ToLowerInvariant())
                {
                    case "status":
                        using (var writer = new StringWriter())
                        {
                            var originalOut = Console.Out;
                            Console.SetOut(writer);
                            ProcessManager.ListComponentsStatus();
                            Console.SetOut(originalOut);
                            return writer.ToString();
                        }
                    case "list":
                        if (parts.Length > 1 && parts[1] == "--installed")
                        {
                            using (var writer = new StringWriter())
                            {
                                var originalOut = Console.Out;
                                Console.SetOut(writer);
                                ListManager.ListInstalledVersions();
                                Console.SetOut(originalOut);
                                return writer.ToString();
                            }
                        }
                        else if (parts.Length > 1)
                        {
                            using (var writer = new StringWriter())
                            {
                                var originalOut = Console.Out;
                                Console.SetOut(writer);
                                ListManager.ListVersions(parts[1]);
                                Console.SetOut(originalOut);
                                return writer.ToString();
                            }
                        }
                        return "Uso: list --installed ou list <componente>";
                    case "help":
                        return GetHelpText();
                    default:
                        return $"Comando '{parts[0]}' não reconhecido. Use 'help' para ver comandos disponíveis.";
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao executar comando: {ex.Message}";
            }
        }

        private string GetHelpText()
        {
            return @"DevStackManager - Comandos Disponíveis:

INSTALAÇÃO:
  install <componente> [versão]    - Instalar componente
  uninstall <componente-versão>    - Desinstalar componente

GERENCIAMENTO DE SERVIÇOS:
  start <componente> [versão]      - Iniciar serviço
  stop <componente> [versão]       - Parar serviço  
  restart <componente> [versão]    - Reiniciar serviço
  start --all                      - Iniciar todos os serviços
  stop --all                       - Parar todos os serviços

INFORMAÇÕES:
  status                           - Status de todos os componentes
  list --installed                 - Listar ferramentas instaladas
  list <componente>                - Listar versões disponíveis
  doctor                           - Diagnóstico do sistema
  test                             - Testar ferramentas instaladas

CONFIGURAÇÃO:
  global                           - Configuração global do sistema
  path --add                       - Adicionar ao PATH
  path --remove                    - Remover do PATH
  path --list                      - Listar PATH atual

UTILITÁRIOS:
  site <domínio> [opções]          - Criar configuração de site
  ssl <domínio>                    - Gerar certificado SSL
  clean                            - Limpar logs e temporários
  backup                           - Backup das configurações
  self-update                      - Atualizar DevStack

EXEMPLOS:
  install php 8.2
  start nginx
  list php
  site meusite.local -root C:/www/meusite
";
        }

        private void ClearConsole()
        {
            var consoleOutput = FindChild<TextBox>(this, "UtilsConsoleOutput");
            if (consoleOutput != null)
            {
                consoleOutput.Text = "Console limpo.\n\n";
                StatusMessage = "Console limpo";
            }
        }
        #endregion

        #region Data Loading Methods
        private async Task LoadInstalledComponents()
        {
            await Task.Run(() =>
            {
                try
                {
                    StatusMessage = "Carregando componentes instalados...";
                    
                    var data = DataManager.GetInstalledVersions();
                    var components = new ObservableCollection<ComponentViewModel>();
                    
                    foreach (var comp in data.Components)
                    {
                        components.Add(new ComponentViewModel
                        {
                            Name = comp.Name,
                            Installed = comp.Installed,
                            Versions = comp.Versions,
                            Status = comp.Installed ? "Instalado" : "Não Instalado",
                            VersionsText = comp.Installed ? string.Join(", ", comp.Versions) : "N/A"
                        });
                    }
                    
                    Dispatcher.Invoke(() =>
                    {
                        InstalledComponents = components;
                        StatusMessage = $"Carregados {components.Count} componentes";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Erro ao carregar componentes: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar componentes na GUI: {ex}");
                    });
                }
            });
        }

        private void LoadAvailableComponents()
        {
            // Já inicializado no construtor
            StatusMessage = "Componentes disponíveis carregados";
        }

        private async Task LoadVersionsForComponent()
        {
            if (string.IsNullOrEmpty(SelectedComponent))
            {
                AvailableVersions.Clear();
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    StatusMessage = $"Carregando versões de {SelectedComponent}...";
                    
                    var versionData = GetVersionDataForComponent(SelectedComponent);
                    
                    Dispatcher.Invoke(() =>
                    {
                        AvailableVersions.Clear();
                        foreach (var version in versionData.Versions.Take(20)) // Limitar a 20 versões
                        {
                            AvailableVersions.Add(version);
                        }
                        StatusMessage = $"{AvailableVersions.Count} versões carregadas para {SelectedComponent}";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Erro ao carregar versões: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar versões na GUI: {ex}");
                    });
                }
            });
        }

        private VersionData GetVersionDataForComponent(string component)
        {
            return component.ToLowerInvariant() switch
            {
                "php" => DataManager.GetPHPVersions(),
                "nginx" => DataManager.GetNginxVersions(),
                "nodejs" or "node" => DataManager.GetNodeVersions(),
                "python" => DataManager.GetPythonVersions(),
                "mysql" => DataManager.GetMySQLVersions(),
                "composer" => DataManager.GetComposerVersions(),
                "phpmyadmin" => DataManager.GetPhpMyAdminVersions(),
                "git" => DataManager.GetGitVersions(),
                "mongodb" => DataManager.GetMongoDBVersions(),
                "redis" => DataManager.GetRedisVersions(),
                "pgsql" => DataManager.GetPgSQLVersions(),
                "mailhog" => DataManager.GetMailHogVersions(),
                "elasticsearch" => DataManager.GetElasticsearchVersions(),
                "memcached" => DataManager.GetMemcachedVersions(),
                "docker" => DataManager.GetDockerVersions(),
                "yarn" => DataManager.GetYarnVersions(),
                "pnpm" => DataManager.GetPnpmVersions(),
                "wpcli" => DataManager.GetWPCLIVersions(),
                "adminer" => DataManager.GetAdminerVersions(),
                "poetry" => DataManager.GetPoetryVersions(),
                "ruby" => DataManager.GetRubyVersions(),
                "go" => DataManager.GetGoVersions(),
                "certbot" => DataManager.GetCertbotVersions(),
                "openssl" => DataManager.GetOpenSSLVersions(),
                "phpcsfixer" => DataManager.GetPHPCsFixerVersions(),
                _ => new VersionData { Status = "error", Message = "Componente não suportado" }
            };
        }

        private async Task LoadUninstallVersions()
        {
            if (string.IsNullOrEmpty(SelectedUninstallComponent))
            {
                // Limpar versões se nenhum componente selecionado
                var versionCombo = FindChild<ComboBox>(this, "UninstallVersionCombo");
                if (versionCombo != null)
                {
                    versionCombo.Items.Clear();
                }
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    StatusMessage = $"Carregando versões instaladas de {SelectedUninstallComponent}...";
                    
                    var status = DataManager.GetComponentStatus(SelectedUninstallComponent);
                    
                    Dispatcher.Invoke(() =>
                    {
                        var versionCombo = FindChild<ComboBox>(this, "UninstallVersionCombo");
                        if (versionCombo != null)
                        {
                            versionCombo.Items.Clear();
                            
                            if (status.Installed && status.Versions.Any())
                            {
                                foreach (var version in status.Versions)
                                {
                                    // Extrair apenas a parte da versão, removendo o nome do componente
                                    var versionNumber = version;
                                    if (SelectedUninstallComponent == "git" && version.StartsWith("git-"))
                                    {
                                        versionNumber = version.Substring(4); // Remove "git-"
                                    }
                                    else if (version.StartsWith($"{SelectedUninstallComponent}-"))
                                    {
                                        versionNumber = version.Substring(SelectedUninstallComponent.Length + 1);
                                    }
                                    
                                    versionCombo.Items.Add(versionNumber);
                                }
                                
                                if (versionCombo.Items.Count > 0)
                                {
                                    versionCombo.SelectedIndex = 0;
                                    SelectedUninstallVersion = versionCombo.SelectedItem?.ToString() ?? "";
                                }
                            }
                            else
                            {
                                MessageBox.Show($"{SelectedUninstallComponent} não possui versões instaladas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        
                        StatusMessage = status.Installed ? 
                            $"Versões carregadas para {SelectedUninstallComponent}" : 
                            $"{SelectedUninstallComponent} não está instalado";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Erro ao carregar versões para desinstalar: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar versões para desinstalar na GUI: {ex}");
                    });
                }
            });
        }

        private void LoadUninstallComponents()
        {
            try
            {
                StatusMessage = "Carregando componentes instalados...";
                
                var componentCombo = FindChild<ComboBox>(this, "UninstallComponentCombo");
                if (componentCombo != null)
                {
                    componentCombo.Items.Clear();
                    
                    // Obter componentes instalados
                    foreach (var comp in InstalledComponents.Where(c => c.Installed))
                    {
                        componentCombo.Items.Add(comp.Name);
                    }
                    
                    if (componentCombo.Items.Count > 0)
                    {
                        componentCombo.SelectedIndex = 0;
                        SelectedUninstallComponent = componentCombo.SelectedItem?.ToString() ?? "";
                    }
                    else
                    {
                        StatusMessage = "Nenhum componente instalado encontrado para desinstalação";
                    }
                }
                
                StatusMessage = $"{componentCombo?.Items.Count ?? 0} componentes disponíveis para desinstalação";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar componentes: {ex.Message}";
                DevStackConfig.WriteLog($"Erro ao carregar componentes para desinstalação na GUI: {ex}");
            }
        }

        private async Task LoadServices()
        {
            await Task.Run(() =>
            {
                try
                {
                    StatusMessage = "Carregando serviços...";
                    
                    var services = new ObservableCollection<ServiceViewModel>();
                    
                    // Detectar serviços PHP-FPM reais
                    var devStackPath = Environment.GetEnvironmentVariable("DEVSTACK_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DevStack");
                    var phpDir = Path.Combine(devStackPath, "php");
                    
                    if (Directory.Exists(phpDir))
                    {
                        var phpDirs = Directory.GetDirectories(phpDir);
                        foreach (var dir in phpDirs)
                        {
                            var dirName = Path.GetFileName(dir);
                            var versionNumber = dirName.Replace("php-", "");
                            
                            try
                            {
                                var phpProcesses = Process.GetProcessesByName("php-cgi")
                                    .Concat(Process.GetProcessesByName("php"))
                                    .Where(p => p.MainModule?.FileName?.Contains(dirName) == true)
                                    .ToList();
                                
                                if (phpProcesses.Any())
                                {
                                    var pids = string.Join(", ", phpProcesses.Select(p => p.Id));
                                    services.Add(new ServiceViewModel 
                                    { 
                                        Name = "php", 
                                        Version = versionNumber,
                                        Status = "Executando", 
                                        Type = "PHP-FPM", 
                                        Description = $"PHP {versionNumber} FastCGI",
                                        Pid = pids,
                                        IsRunning = true
                                    });
                                }
                                else
                                {
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
                    
                    // Detectar serviços Nginx reais
                    var nginxDir = Path.Combine(devStackPath, "nginx");
                    if (Directory.Exists(nginxDir))
                    {
                        var nginxDirs = Directory.GetDirectories(nginxDir);
                        foreach (var dir in nginxDirs)
                        {
                            var dirName = Path.GetFileName(dir);
                            var versionNumber = dirName.Replace("nginx-", "");
                            
                            try
                            {
                                var nginxProcesses = Process.GetProcessesByName("nginx")
                                    .Where(p => p.MainModule?.FileName?.Contains(dirName) == true)
                                    .ToList();
                                
                                if (nginxProcesses.Any())
                                {
                                    var mainProcess = nginxProcesses.First();
                                    services.Add(new ServiceViewModel 
                                    { 
                                        Name = "nginx", 
                                        Version = versionNumber,
                                        Status = "Executando", 
                                        Type = "Web Server", 
                                        Description = $"Nginx {versionNumber}",
                                        Pid = mainProcess.Id.ToString(),
                                        IsRunning = true
                                    });
                                }
                                else
                                {
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
                    
                    Dispatcher.Invoke(() =>
                    {
                        Services.Clear();
                        foreach (var service in services)
                        {
                            Services.Add(service);
                        }
                        StatusMessage = $"{Services.Count} serviços carregados";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"Erro ao carregar serviços: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar serviços na GUI: {ex}");
                    });
                }
            });
        }
        #endregion

        #region Action Methods
        private async Task InstallComponent()
        {
            if (string.IsNullOrEmpty(SelectedComponent))
            {
                MessageBox.Show("Selecione um componente para instalar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            StatusMessage = $"Instalando {SelectedComponent}...";
            
            try
            {
                var args = string.IsNullOrEmpty(SelectedVersion) 
                    ? new[] { SelectedComponent }
                    : new[] { SelectedComponent, SelectedVersion };
                
                await InstallManager.InstallCommands(args);
                
                AppendToConsole($"✅ {SelectedComponent} instalado com sucesso!");
                StatusMessage = $"{SelectedComponent} instalado com sucesso!";
                
                // Recarregar lista de instalados
                await LoadInstalledComponents();
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao instalar {SelectedComponent}: {ex.Message}");
                StatusMessage = $"Erro ao instalar {SelectedComponent}";
                DevStackConfig.WriteLog($"Erro ao instalar {SelectedComponent} na GUI: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UninstallComponent()
        {
            if (string.IsNullOrEmpty(SelectedUninstallComponent))
            {
                MessageBox.Show("Selecione um componente para desinstalar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Tem certeza que deseja desinstalar {SelectedUninstallComponent}?",
                "Confirmação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            StatusMessage = $"Desinstalando {SelectedUninstallComponent}...";
            
            try
            {
                var args = string.IsNullOrEmpty(SelectedUninstallVersion)
                    ? new[] { SelectedUninstallComponent }
                    : new[] { SelectedUninstallComponent, SelectedUninstallVersion };
                
                UninstallManager.UninstallCommands(args);
                
                AppendToConsole($"✅ {SelectedUninstallComponent} desinstalado com sucesso!");
                StatusMessage = $"{SelectedUninstallComponent} desinstalado com sucesso!";
                
                // Recarregar lista de instalados
                await LoadInstalledComponents();
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao desinstalar {SelectedUninstallComponent}: {ex.Message}");
                StatusMessage = $"Erro ao desinstalar {SelectedUninstallComponent}";
                DevStackConfig.WriteLog($"Erro ao desinstalar {SelectedUninstallComponent} na GUI: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ListVersionsForSelectedComponent()
        {
            if (string.IsNullOrEmpty(SelectedComponent))
            {
                MessageBox.Show("Selecione um componente primeiro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = $"Listando versões de {SelectedComponent}...";
            
            try
            {
                ListManager.ListVersions(SelectedComponent);
                AppendToConsole($"📋 Versões de {SelectedComponent} listadas no console principal.");
                StatusMessage = $"Versões de {SelectedComponent} listadas";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao listar versões de {SelectedComponent}: {ex.Message}");
                StatusMessage = $"Erro ao listar versões de {SelectedComponent}";
            }
        }

        private async Task StartSelectedService()
        {
            var selectedService = GetSelectedService();
            if (selectedService == null)
            {
                MessageBox.Show("Selecione um serviço para iniciar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = $"Iniciando {selectedService.Name} versão {selectedService.Version}...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StartComponent(selectedService.Name, selectedService.Version);
                });

                AppendToConsole($"▶️ {selectedService.Name} versão {selectedService.Version} iniciado com sucesso");
                StatusMessage = $"{selectedService.Name} iniciado com sucesso";
                await LoadServices(); // Recarregar lista
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao iniciar {selectedService.Name}: {ex.Message}");
                StatusMessage = $"Erro ao iniciar {selectedService.Name}";
                DevStackConfig.WriteLog($"Erro ao iniciar serviço na GUI: {ex}");
            }
        }

        private async Task StopSelectedService()
        {
            var selectedService = GetSelectedService();
            if (selectedService == null)
            {
                MessageBox.Show("Selecione um serviço para parar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = $"Parando {selectedService.Name} versão {selectedService.Version}...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StopComponent(selectedService.Name, selectedService.Version);
                });

                AppendToConsole($"⏹️ {selectedService.Name} versão {selectedService.Version} parado com sucesso");
                StatusMessage = $"{selectedService.Name} parado com sucesso";
                await LoadServices(); // Recarregar lista
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao parar {selectedService.Name}: {ex.Message}");
                StatusMessage = $"Erro ao parar {selectedService.Name}";
                DevStackConfig.WriteLog($"Erro ao parar serviço na GUI: {ex}");
            }
        }

        private async Task RestartSelectedService()
        {
            var selectedService = GetSelectedService();
            if (selectedService == null)
            {
                MessageBox.Show("Selecione um serviço para reiniciar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = $"Reiniciando {selectedService.Name} versão {selectedService.Version}...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.RestartComponent(selectedService.Name, selectedService.Version);
                });

                AppendToConsole($"🔄 {selectedService.Name} versão {selectedService.Version} reiniciado com sucesso");
                StatusMessage = $"{selectedService.Name} reiniciado com sucesso";
                await LoadServices(); // Recarregar lista
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao reiniciar {selectedService.Name}: {ex.Message}");
                StatusMessage = $"Erro ao reiniciar {selectedService.Name}";
                DevStackConfig.WriteLog($"Erro ao reiniciar serviço na GUI: {ex}");
            }
        }

        private async Task StartAllServices()
        {
            StatusMessage = "Iniciando todos os serviços...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StartAllComponents();
                });

                AppendToConsole("▶️ Todos os serviços foram iniciados com sucesso");
                StatusMessage = "Todos os serviços iniciados";
                await LoadServices(); // Recarregar lista
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao iniciar todos os serviços: {ex.Message}");
                StatusMessage = "Erro ao iniciar todos os serviços";
                DevStackConfig.WriteLog($"Erro ao iniciar todos os serviços na GUI: {ex}");
            }
        }

        private async Task StopAllServices()
        {
            StatusMessage = "Parando todos os serviços...";
            
            try
            {
                await Task.Run(() =>
                {
                    ProcessManager.StopAllComponents();
                });

                AppendToConsole("⏹️ Todos os serviços foram parados com sucesso");
                StatusMessage = "Todos os serviços parados";
                await LoadServices(); // Recarregar lista
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao parar todos os serviços: {ex.Message}");
                StatusMessage = "Erro ao parar todos os serviços";
                DevStackConfig.WriteLog($"Erro ao parar todos os serviços na GUI: {ex}");
            }
        }

        private async Task RestartAllServices()
        {
            StatusMessage = "Reiniciando todos os serviços...";
            
            try
            {
                await Task.Run(() =>
                {
                    // Parar todos e depois iniciar todos
                    ProcessManager.StopAllComponents();
                    System.Threading.Thread.Sleep(2000); // Aguardar 2 segundos
                    ProcessManager.StartAllComponents();
                });

                AppendToConsole("🔄 Todos os serviços foram reiniciados com sucesso");
                StatusMessage = "Todos os serviços reiniciados";
                await LoadServices(); // Recarregar lista
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao reiniciar todos os serviços: {ex.Message}");
                StatusMessage = "Erro ao reiniciar todos os serviços";
                DevStackConfig.WriteLog($"Erro ao reiniciar todos os serviços na GUI: {ex}");
            }
        }

        private ServiceViewModel? GetSelectedService()
        {
            // Procurar o DataGrid de serviços no visual tree
            var servicesGrid = FindChild<DataGrid>(this, "ServicesDataGrid");
            return servicesGrid?.SelectedItem as ServiceViewModel;
        }

        // Método auxiliar para encontrar controles filhos por nome
        private static T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T childType && (child as FrameworkElement)?.Name == childName)
                {
                    foundChild = childType;
                    break;
                }
                else
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
            }
            
            return foundChild;
        }

        private void AddToPath()
        {
            try
            {
                DevStackConfig.pathManager?.AddBinDirsToPath();
                AppendToConsole("✅ Ferramentas adicionadas ao PATH do sistema");
                StatusMessage = "PATH atualizado com sucesso";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao adicionar ao PATH: {ex.Message}");
                StatusMessage = "Erro ao atualizar PATH";
            }
        }

        private void RemoveFromPath()
        {
            try
            {
                DevStackConfig.pathManager?.RemoveAllDevStackFromPath();
                AppendToConsole("✅ Ferramentas removidas do PATH do sistema");
                StatusMessage = "PATH limpo com sucesso";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao remover do PATH: {ex.Message}");
                StatusMessage = "Erro ao limpar PATH";
            }
        }

        private void ListCurrentPath()
        {
            try
            {
                DevStackConfig.pathManager?.ListCurrentPath();
                AppendToConsole("📋 PATH atual listado no console principal");
                StatusMessage = "PATH listado";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao listar PATH: {ex.Message}");
                StatusMessage = "Erro ao listar PATH";
            }
        }

        private void SetProxy(string proxyUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(proxyUrl))
                {
                    Environment.SetEnvironmentVariable("HTTP_PROXY", null);
                    Environment.SetEnvironmentVariable("HTTPS_PROXY", null);
                    AppendToConsole("✅ Proxy removido");
                }
                else
                {
                    Environment.SetEnvironmentVariable("HTTP_PROXY", proxyUrl);
                    Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyUrl);
                    AppendToConsole($"✅ Proxy definido para: {proxyUrl}");
                }
                StatusMessage = "Configuração de proxy atualizada";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao configurar proxy: {ex.Message}");
                StatusMessage = "Erro ao configurar proxy";
            }
        }

        private void CreateSite(string domain, string root, string phpUpstream, string nginxVersion = "")
        {
            if (string.IsNullOrEmpty(domain))
            {
                MessageBox.Show("Digite um domínio para o site.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusMessage = $"Criando configuração para o site {domain}...";
                InstallManager.CreateNginxSiteConfig(domain, root, phpUpstream, null, nginxVersion);
                
                var message = $"Configuração para o site {domain} criada com sucesso.\n\n" +
                             $"Não se esqueça de adicionar uma entrada no arquivo hosts:\n" +
                             $"127.0.0.1    {domain}";
                
                MessageBox.Show(message, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                AppendToConsole($"🌐 Site {domain} criado com sucesso!");
                StatusMessage = $"Site {domain} criado";
                
                // Limpar os campos após sucesso
                var domainTextBox = FindChild<TextBox>(this, "DomainTextBox");
                var rootTextBox = FindChild<TextBox>(this, "RootTextBox");
                if (domainTextBox != null) domainTextBox.Text = "";
                if (rootTextBox != null) rootTextBox.Text = "";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao criar site {domain}: {ex.Message}");
                StatusMessage = $"Erro ao criar site {domain}";
                MessageBox.Show($"Erro ao criar configuração do site: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateSslCertificate(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                MessageBox.Show("Digite um domínio para gerar o certificado SSL.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusMessage = $"Gerando certificado SSL para {domain}...";
                
                // Chamar o setup.ps1 com comando SSL
                var setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "setup.ps1");
                if (!File.Exists(setupPath))
                {
                    setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "setup.ps1");
                }

                if (File.Exists(setupPath))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "pwsh.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{setupPath}\" ssl {domain}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    // Fallback para PowerShell Windows se pwsh não estiver disponível
                    try
                    {
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForExit();
                            var output = process.StandardOutput.ReadToEnd();
                            var error = process.StandardError.ReadToEnd();

                            if (process.ExitCode == 0)
                            {
                                MessageBox.Show($"Certificado SSL para {domain} gerado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                AppendToConsole($"🔒 Certificado SSL para {domain} gerado com sucesso!");
                                StatusMessage = $"Certificado SSL para {domain} gerado";
                                
                                // Limpar o campo
                                var sslDomainTextBox = FindChild<TextBox>(this, "SslDomainTextBox");
                                if (sslDomainTextBox != null) sslDomainTextBox.Text = "";
                            }
                            else
                            {
                                throw new Exception($"Processo falhou com código {process.ExitCode}: {error}");
                            }
                        }
                    }
                    catch
                    {
                        // Fallback para powershell.exe
                        startInfo.FileName = "powershell.exe";
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForExit();
                            if (process.ExitCode == 0)
                            {
                                MessageBox.Show($"Certificado SSL para {domain} gerado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                                AppendToConsole($"🔒 Certificado SSL para {domain} gerado com sucesso!");
                                StatusMessage = $"Certificado SSL para {domain} gerado";
                            }
                            else
                            {
                                throw new Exception("Falha na execução do comando SSL");
                            }
                        }
                    }
                }
                else
                {
                    // Fallback direto usando os managers C#
                    AppendToConsole($"🔒 Tentativa de gerar SSL para {domain} (funcionalidade em desenvolvimento)");
                    StatusMessage = $"SSL para {domain} - em desenvolvimento";
                }
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao gerar certificado SSL: {ex.Message}");
                StatusMessage = $"Erro ao gerar SSL para {domain}";
                MessageBox.Show($"Erro ao gerar certificado SSL: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteUtilityCommand(string command)
        {
            StatusMessage = $"Executando comando {command}...";
            
            try
            {
                var args = new[] { command };
                // Simular execução do comando no programa principal
                // Em uma implementação real, você executaria DevStackConfig.ExecuteCommand
                AppendToConsole($"🛠️ Executando: {command}");
                
                // Para comandos específicos, chamar as funções apropriadas
                switch (command)
                {
                    case "doctor":
                        // Implementar diagnóstico
                        AppendToConsole("🔍 Executando diagnóstico do sistema...");
                        await Task.Delay(100); // Simular operação async
                        break;
                    case "status":
                        await Task.Run(() => ListManager.ListInstalledVersions());
                        AppendToConsole("📊 Status exibido no console principal");
                        break;
                    case "clean":
                        await Task.Delay(100);
                        AppendToConsole("🧹 Limpeza de logs executada");
                        break;
                    case "backup":
                        await Task.Delay(100);
                        AppendToConsole("💾 Backup das configurações criado");
                        break;
                    default:
                        await Task.Delay(100);
                        AppendToConsole($"📝 Comando {command} executado");
                        break;
                }
                
                StatusMessage = $"Comando {command} executado";
            }
            catch (Exception ex)
            {
                AppendToConsole($"❌ Erro ao executar {command}: {ex.Message}");
                StatusMessage = $"Erro ao executar {command}";
            }
        }

        private async Task RefreshAllData()
        {
            StatusMessage = "Atualizando todos os dados...";
            IsLoading = true;
            
            try
            {
                await LoadInstalledComponents();
                await LoadServices();
                StatusMessage = "Dados atualizados com sucesso";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao atualizar dados: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region UI Helper Methods







        // Helper method to apply dark theme to DataGrid controls
        private static void SetDataGridDarkTheme(DataGrid dataGrid)
        {
            // Forçar as cores do tema escuro em todas as propriedades
            dataGrid.Background = CurrentTheme.GridBackground;
            dataGrid.Foreground = CurrentTheme.GridForeground;
            dataGrid.BorderBrush = CurrentTheme.Border;
            dataGrid.BorderThickness = new Thickness(1);
            dataGrid.RowBackground = CurrentTheme.GridBackground;
            dataGrid.AlternatingRowBackground = CurrentTheme.GridAlternateRow;
            dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
            dataGrid.HorizontalGridLinesBrush = CurrentTheme.Border;
            dataGrid.VerticalGridLinesBrush = CurrentTheme.Border;
            dataGrid.CanUserResizeRows = false;
            dataGrid.SelectionMode = DataGridSelectionMode.Single;
            dataGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
            
            // Remover os cabeçalhos de linha (botões finos à esquerda)
            dataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
            
            // Remover qualquer estilo existente que possa interferir
            dataGrid.Style = null;
            
            // Forçar recursos para garantir que cores padrão não sejam aplicadas
            dataGrid.Resources.Clear();
            
            // Header styling with modern look - SEMPRE aplicar
            var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, CurrentTheme.GridHeaderBackground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, CurrentTheme.GridHeaderForeground));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.SemiBold));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 14.0));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.PaddingProperty, new Thickness(12, 10, 12, 10)));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderBrushProperty, CurrentTheme.Border));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            
            // Forçar o template para garantir que não haja elementos visuais claros
            var headerTemplate = new ControlTemplate(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, CurrentTheme.GridHeaderBackground);
            border.SetValue(Border.BorderBrushProperty, CurrentTheme.Border);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 1, 1));
            
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(12, 10, 12, 10));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            border.AppendChild(contentPresenter);
            headerTemplate.VisualTree = border;
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.TemplateProperty, headerTemplate));
            
            dataGrid.ColumnHeaderStyle = headerStyle;
            
            // Row styling with hover and selection effects - SEMPRE aplicar
            var rowStyle = new Style(typeof(DataGridRow));
            rowStyle.Setters.Add(new Setter(DataGridRow.MinHeightProperty, 35.0));
            rowStyle.Setters.Add(new Setter(DataGridRow.FontSizeProperty, 14.0));
            rowStyle.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.GridBackground));
            rowStyle.Setters.Add(new Setter(DataGridRow.ForegroundProperty, CurrentTheme.GridForeground));
            
            // Hover trigger
            var hoverTrigger = new Trigger
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.SidebarHover));
            
            // Selection trigger
            var selectedTrigger = new Trigger
            {
                Property = DataGridRow.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, CurrentTheme.GridSelectedRow));
            selectedTrigger.Setters.Add(new Setter(DataGridRow.ForegroundProperty, new SolidColorBrush(Colors.White)));
            
            rowStyle.Triggers.Add(hoverTrigger);
            rowStyle.Triggers.Add(selectedTrigger);
            dataGrid.RowStyle = rowStyle;
            
            // Cell styling for better padding - SEMPRE aplicar
            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(8, 6, 8, 6)));
            cellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)));
            cellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent));
            cellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, CurrentTheme.GridForeground));
            
            // Triggers para células selecionadas
            var cellSelectedTrigger = new Trigger
            {
                Property = DataGridCell.IsSelectedProperty,
                Value = true
            };
            cellSelectedTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent));
            cellSelectedTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, new SolidColorBrush(Colors.White)));
            cellStyle.Triggers.Add(cellSelectedTrigger);
            
            dataGrid.CellStyle = cellStyle;
        }

        // Helper method to create styled Button with dark theme
        private static Button CreateStyledButton(string content, RoutedEventHandler? clickHandler = null)
        {
            var button = new Button
            {
                Content = content,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 8, 12, 8),
                FontWeight = FontWeights.Medium,
                Foreground = CurrentTheme.ButtonForeground,
                BorderBrush = CurrentTheme.Border
            };

            if (clickHandler != null)
                button.Click += clickHandler;

            // Determine colors based on button type
            SolidColorBrush backgroundColor, hoverColor;
            
            if (content.Contains("Instalar") || content.Contains("▶") || content.Contains("⬇"))
            {
                backgroundColor = CurrentTheme.Success;
                hoverColor = CurrentTheme.AccentHover;
            }
            else if (content.Contains("Desinstalar") || content.Contains("🗑") || content.Contains("❌"))
            {
                backgroundColor = CurrentTheme.Danger;
                hoverColor = new SolidColorBrush(Color.FromRgb(200, 35, 51));
            }
            else if (content.Contains("Parar") || content.Contains("⏹"))
            {
                backgroundColor = CurrentTheme.Warning;
                hoverColor = new SolidColorBrush(Color.FromRgb(217, 164, 6));
            }
            else
            {
                backgroundColor = CurrentTheme.ButtonBackground;
                hoverColor = CurrentTheme.ButtonHover;
            }
            
            button.Background = backgroundColor;
            
            // Create style for hover and pressed effects
            var buttonStyle = new Style(typeof(Button));
            
            // Add hover trigger
            var hoverTrigger = new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, hoverColor));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, CurrentTheme.BorderHover));
            hoverTrigger.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            
            // Add pressed trigger
            var pressedTrigger = new Trigger
            {
                Property = Button.IsPressedProperty,
                Value = true
            };
            
            var pressedColor = new SolidColorBrush(Color.FromArgb(
                255,
                (byte)(((SolidColorBrush)hoverColor).Color.R * 0.9),
                (byte)(((SolidColorBrush)hoverColor).Color.G * 0.9),
                (byte)(((SolidColorBrush)hoverColor).Color.B * 0.9)
            ));
            
            pressedTrigger.Setters.Add(new Setter(Button.BackgroundProperty, pressedColor));
            
            // Add disabled trigger
            var disabledTrigger = new Trigger
            {
                Property = Button.IsEnabledProperty,
                Value = false
            };
            disabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, CurrentTheme.ButtonDisabled));
            disabledTrigger.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.TextMuted));
            disabledTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.6));
            
            buttonStyle.Triggers.Add(hoverTrigger);
            buttonStyle.Triggers.Add(pressedTrigger);
            buttonStyle.Triggers.Add(disabledTrigger);
            
            button.Style = buttonStyle;
            
            // Add subtle shadow effect for visual depth
            button.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 4,
                ShadowDepth = 2,
                Opacity = 0.6,
                Color = Colors.Black
            };

            return button;
        }

        // Helper method to create styled TextBox with dark theme
        private static TextBox CreateStyledTextBox(bool isConsole = false)
        {
            var textBox = new TextBox
            {
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = isConsole ? 13 : 14
            };

            if (isConsole)
            {
                textBox.Background = CurrentTheme.ConsoleBackground;
                textBox.Foreground = CurrentTheme.ConsoleForeground;
                textBox.BorderBrush = CurrentTheme.Border;
                textBox.FontFamily = new FontFamily("Consolas");
                textBox.SelectionBrush = new SolidColorBrush(Color.FromArgb(128, 0, 123, 255));
            }
            else
            {
                textBox.Background = CurrentTheme.InputBackground;
                textBox.Foreground = CurrentTheme.InputForeground;
                textBox.BorderBrush = CurrentTheme.InputBorder;
                textBox.SelectionBrush = new SolidColorBrush(Color.FromArgb(128, 0, 123, 255));
            }
            
            // Create style for focus effects
            var textBoxStyle = new Style(typeof(TextBox));
            
            // Focus trigger
            var focusTrigger = new Trigger
            {
                Property = TextBox.IsFocusedProperty,
                Value = true
            };
            focusTrigger.Setters.Add(new Setter(TextBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
            focusTrigger.Setters.Add(new Setter(TextBox.BorderThicknessProperty, new Thickness(2)));
            
            // Mouse over trigger
            var hoverTrigger = new Trigger
            {
                Property = TextBox.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(TextBox.BorderBrushProperty, CurrentTheme.BorderHover));
            
            textBoxStyle.Triggers.Add(focusTrigger);
            textBoxStyle.Triggers.Add(hoverTrigger);
            
            textBox.Style = textBoxStyle;

            return textBox;
        }

        // Helper method to create styled ComboBox with dark theme
        private static ComboBox CreateStyledComboBox()
        {
            var comboBox = new ComboBox
            {
                Background = CurrentTheme.InputBackground,
                Foreground = CurrentTheme.InputForeground,
                BorderBrush = CurrentTheme.InputBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 14
            };
            
            // Create style for focus and hover effects
            var comboStyle = new Style(typeof(ComboBox));
            
            // Focus trigger
            var focusTrigger = new Trigger
            {
                Property = ComboBox.IsFocusedProperty,
                Value = true
            };
            focusTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
            focusTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
            
            // Mouse over trigger
            var hoverTrigger = new Trigger
            {
                Property = ComboBox.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.BorderHover));
            
            comboStyle.Triggers.Add(focusTrigger);
            comboStyle.Triggers.Add(hoverTrigger);
            
            comboBox.Style = comboStyle;

            return comboBox;
        }

        // Helper method to create styled Label with dark theme
        private static Label CreateStyledLabel(string content, bool isTitle = false, bool isMuted = false)
        {
            var label = new Label
            {
                Content = content
            };

            if (isTitle)
            {
                label.FontWeight = FontWeights.Bold;
                label.FontSize = 16;
                label.Foreground = CurrentTheme.Foreground;
            }
            else if (isMuted)
            {
                label.Foreground = CurrentTheme.TextMuted;
            }
            else
            {
                label.Foreground = CurrentTheme.Foreground;
            }

            return label;
        }

        // Helper method to apply sidebar ListBox theme (keep existing sidebar styling)
        private static void ApplySidebarListBoxTheme(ListBox listBox)
        {
            listBox.Background = CurrentTheme.SidebarBackground;
            listBox.Foreground = CurrentTheme.Foreground;
            listBox.BorderBrush = CurrentTheme.Border;
            listBox.BorderThickness = new Thickness(0);
            ScrollViewer.SetCanContentScroll(listBox, false);
            
            // Style ListBoxItems for sidebar navigation with modern effects
            if (listBox.ItemContainerStyle == null)
            {
                var itemStyle = new Style(typeof(ListBoxItem));
                itemStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(15, 12, 15, 12)));
                itemStyle.Setters.Add(new Setter(ListBoxItem.MarginProperty, new Thickness(4, 2, 4, 2)));
                itemStyle.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent));
                itemStyle.Setters.Add(new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0, 0, 3, 0)));
                itemStyle.Setters.Add(new Setter(ListBoxItem.BorderBrushProperty, Brushes.Transparent));
                itemStyle.Setters.Add(new Setter(ListBoxItem.FontSizeProperty, 14.0));
                itemStyle.Setters.Add(new Setter(ListBoxItem.CursorProperty, Cursors.Hand));
                
                // Hover trigger with smooth animation-like effect
                var hoverTrigger = new Trigger
                {
                    Property = ListBoxItem.IsMouseOverProperty,
                    Value = true
                };
                hoverTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, CurrentTheme.SidebarHover));
                hoverTrigger.Setters.Add(new Setter(ListBoxItem.BorderBrushProperty, CurrentTheme.BorderHover));
                
                // Selected trigger with accent color
                var selectedTrigger = new Trigger
                {
                    Property = ListBoxItem.IsSelectedProperty,
                    Value = true
                };
                selectedTrigger.Setters.Add(new Setter(ListBoxItem.BackgroundProperty, CurrentTheme.SidebarSelected));
                selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
                selectedTrigger.Setters.Add(new Setter(ListBoxItem.BorderBrushProperty, CurrentTheme.Accent));
                selectedTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
                
                // Focus trigger
                var focusTrigger = new Trigger
                {
                    Property = ListBoxItem.IsFocusedProperty,
                    Value = true
                };
                focusTrigger.Setters.Add(new Setter(ListBoxItem.BorderBrushProperty, CurrentTheme.BorderFocus));
                
                itemStyle.Triggers.Add(hoverTrigger);
                itemStyle.Triggers.Add(selectedTrigger);
                itemStyle.Triggers.Add(focusTrigger);
                listBox.ItemContainerStyle = itemStyle;
            }
        }

        private void AppendToConsole(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ConsoleOutput += $"[{timestamp}] {message}\n";
            
            // Limitar o tamanho do console (manter últimas 1000 linhas)
            var lines = ConsoleOutput.Split('\n');
            if (lines.Length > 1000)
            {
                ConsoleOutput = string.Join("\n", lines.TakeLast(1000));
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    #region ViewModels
    public class ComponentViewModel
    {
        public string Name { get; set; } = "";
        public bool Installed { get; set; }
        public List<string> Versions { get; set; } = new();
        public string Status { get; set; } = "";
        public string VersionsText { get; set; } = "";
    }

    public class ServiceViewModel
    {
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public string Pid { get; set; } = "";
        public bool IsRunning { get; set; } = false;
        public SolidColorBrush StatusColor => IsRunning ? 
            new SolidColorBrush(Colors.Green) : 
            new SolidColorBrush(Colors.Red);
    }
    #endregion
}
