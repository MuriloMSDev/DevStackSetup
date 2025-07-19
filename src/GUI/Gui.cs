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
        private bool _isInstallingComponent = false;
        private bool _isUninstallingComponent = false;
        private bool _isLoadingServices = false;
        private bool _isCreatingSite = false;
        public bool IsInstallingComponent
        {
            get => _isInstallingComponent;
            set { _isInstallingComponent = value; OnPropertyChanged(); }
        }
        public bool IsUninstallingComponent
        {
            get => _isUninstallingComponent;
            set { _isUninstallingComponent = value; OnPropertyChanged(); }
        }
        public bool IsLoadingServices
        {
            get => _isLoadingServices;
            set { _isLoadingServices = value; OnPropertyChanged(); }
        }
        public bool IsCreatingSite
        {
            get => _isCreatingSite;
            set { _isCreatingSite = value; OnPropertyChanged(); }
        }
        public ContentControl? _mainContent;
        private int _selectedNavIndex = 0;
        private static GuiTheme.ThemeColors CurrentTheme => GuiTheme.DarkTheme;
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
            set { _selectedComponent = value; OnPropertyChanged(); _ = Task.Run(async () => await GuiInstallTab.LoadVersionsForComponent(this)); }
        }

        public string SelectedVersion
        {
            get => _selectedVersion;
            set { _selectedVersion = value; OnPropertyChanged(); }
        }

        public string SelectedUninstallComponent
        {
            get => _selectedUninstallComponent;
            set { _selectedUninstallComponent = value; OnPropertyChanged(); _ = Task.Run(async () => await GuiUninstallTab.LoadUninstallVersions(this)); }
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
        
        public int SelectedNavIndex
        {
            get => _selectedNavIndex;
            set { 
                _selectedNavIndex = value; 
                OnPropertyChanged();

                GuiNavigation.NavigateToSection(this, value);
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Constructor
        public DevStackGui()
        {
            InitializeComponent();
            DataContext = this;
            
            // Inicializar dados
            _ = Task.Run(async () => await GuiInstalledTab.LoadInstalledComponents(this));
            _ = Task.Run(async () => await GuiServicesTab.LoadServices(this));
        }
        #endregion

        #region Initialization Methods
        private void InitializeComponent()
        {
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion ?? "Unknown";
            Title = $"DevStack Manager v{version}";
            Width = 1200;
            Height = 800;
            MinWidth = 1000;
            MinHeight = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = CurrentTheme.FormBackground;
            Foreground = CurrentTheme.Foreground;

            // Grid principal
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Conteúdo principal
            CreateMainContent(mainGrid);

            // Barra de status
            CreateStatusBar(mainGrid);

            Content = mainGrid;
        }

        private void CreateMainContent(Grid mainGrid)
        {
            GuiMainContent.CreateMainContent(mainGrid, this);
        }

        private void CreateStatusBar(Grid mainGrid)
        {
            GuiStatusBar.CreateStatusBar(mainGrid, this);
        }
        #endregion

        #region Helper Methods
        public void RefreshAllData()
        {
            _ = Task.Run(async () =>
            {
                await GuiInstalledTab.LoadInstalledComponents(this);
                await GuiServicesTab.LoadServices(this);
            });
        }
        #endregion
    }
}
