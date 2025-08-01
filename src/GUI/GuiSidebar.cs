using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela sidebar da aplicação - navegação e título
    /// </summary>
    public static class GuiSidebar
    {
        /// <summary>
        /// Cria a sidebar completa com título, ícone e navegação
        /// </summary>
        public static void CreateSidebar(Grid contentGrid, DevStackGui mainWindow)
        {
            var sidebar = new Border
            {
                Background = GuiTheme.DarkTheme.SidebarBackground,
                BorderBrush = GuiTheme.DarkTheme.Border,
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            Grid.SetColumn(sidebar, 0);

            // Container principal da sidebar com o título no topo
            var sidebarContainer = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Criar título e ícone
            CreateTitlePanel(sidebarContainer);

            // Criar separador
            CreateSeparator(sidebarContainer);

            // Criar navegação
            CreateNavigationList(mainWindow, sidebarContainer);
            
            // Adicionar o container à sidebar
            sidebar.Child = sidebarContainer;
            contentGrid.Children.Add(sidebar);
        }

        /// <summary>
        /// Cria o painel do título com ícone
        /// </summary>
        private static void CreateTitlePanel(StackPanel sidebarContainer)
        {
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 15, 5, 10)
            };

            // Ícone DevStack
            var iconImage = CreateIcon();
            if (iconImage != null)
                titlePanel.Children.Add(iconImage);

            // Título
            var sidebarTitleLabel = GuiTheme.CreateStyledLabel("DevStack Manager");
            sidebarTitleLabel.FontSize = 20;
            sidebarTitleLabel.FontWeight = FontWeights.Bold;
            sidebarTitleLabel.HorizontalAlignment = HorizontalAlignment.Left;
            sidebarTitleLabel.VerticalAlignment = VerticalAlignment.Center;
            sidebarTitleLabel.Margin = new Thickness(0);
            sidebarTitleLabel.Padding = new Thickness(0);
            titlePanel.Children.Add(sidebarTitleLabel);
            
            sidebarContainer.Children.Add(titlePanel);
        }

        /// <summary>
        /// Cria o ícone da aplicação
        /// </summary>
        private static Image? CreateIcon()
        {
            var iconImage = new Image
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(0, 6, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DevStack.ico");
                if (File.Exists(iconPath))
                {
                    iconImage.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                    return iconImage;
                }
            }
            catch 
            {
                // Ignorar erros de carregamento do ícone
            }

            return null;
        }

        /// <summary>
        /// Cria o separador visual
        /// </summary>
        private static void CreateSeparator(StackPanel sidebarContainer)
        {
            var separator = new Border
            {
                Height = 1,
                Margin = new Thickness(10, 0, 10, 10),
                Background = GuiTheme.DarkTheme.Border
            };
            sidebarContainer.Children.Add(separator);
        }

        /// <summary>
        /// Cria a lista de navegação
        /// </summary>
        private static void CreateNavigationList(DevStackGui mainWindow, StackPanel sidebarContainer)
        {
            var navList = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Margin = new Thickness(8, 5, 8, 5),
                SelectedIndex = 0
            };

            // Criar itens de navegação com ícones modernos
            var navItems = GetNavigationItems();

            foreach (var item in navItems)
            {
                var listItem = CreateNavigationItem(item);
                navList.Items.Add(listItem);
            }

            // Bind da seleção
            var binding = new Binding("SelectedNavIndex") { Source = mainWindow };
            navList.SetBinding(ListBox.SelectedIndexProperty, binding);

            // Adicionar a navList ao container da sidebar ANTES de aplicar o tema
            sidebarContainer.Children.Add(navList);
            
            // Apply theme to the navigation list DEPOIS que todos os itens foram adicionados e a lista foi adicionada ao container
            GuiTheme.ApplySidebarListBoxTheme(navList);
            
            // Forçar atualização da UI para garantir que o tema seja aplicado
            navList.UpdateLayout();
        }

        /// <summary>
        /// Obtém a lista de itens de navegação
        /// </summary>
        private static List<GuiNavigation.NavigationItem> GetNavigationItems()
        {
            return new List<GuiNavigation.NavigationItem>
            {
                new() { Title = "Instalados", Icon = "📦", Description = "Ferramentas instaladas" },
                new() { Title = "Instalar", Icon = "📥", Description = "Instalar novos componentes" },
                new() { Title = "Desinstalar", Icon = "🗑️", Description = "Remover componentes" },
                new() { Title = "Serviços", Icon = "⚙️", Description = "Controle de serviços" },
                new() { Title = "Configurações", Icon = "🔧", Description = "Configurações do sistema" },
                new() { Title = "Sites", Icon = "🌐", Description = "Gerenciar sites Nginx" },
                new() { Title = "Utilitários", Icon = "🛠️", Description = "Ferramentas e console" }
            };
        }

        /// <summary>
        /// Cria um item de navegação individual
        /// </summary>
        private static ListBoxItem CreateNavigationItem(GuiNavigation.NavigationItem item)
        {
            var listItem = new ListBoxItem();
            
            // Limpar qualquer estilo padrão que possa interferir
            listItem.ClearValue(ListBoxItem.StyleProperty);
            
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var iconLabel = new Label
            {
                Content = item.Icon,
                FontSize = 18,
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = GuiTheme.DarkTheme.Foreground,
                Background = Brushes.Transparent
            };
            
            var titleLabel = new Label
            {
                Content = item.Title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = GuiTheme.DarkTheme.Foreground,
                Background = Brushes.Transparent
            };
            
            panel.Children.Add(iconLabel);
            panel.Children.Add(titleLabel);
            listItem.Content = panel;
            
            return listItem;
        }
    }
}
