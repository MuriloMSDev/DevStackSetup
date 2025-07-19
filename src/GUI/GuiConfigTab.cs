using System;
using System.Windows;
using System.Windows.Controls;

namespace DevStackManager
{
    /// <summary>
    /// Componente responsável pela aba "Configurações" - gerencia configurações do sistema
    /// </summary>
    public static class GuiConfigTab
    {
        /// <summary>
        /// Cria o conteúdo completo da aba "Configurações"
        /// </summary>
        public static Grid CreateConfigContent(DevStackGui mainWindow)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Painel esquerdo - Configurações
            var leftPanel = CreateConfigSelectionPanel(mainWindow);
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            // Painel direito - Console de saída
            var rightPanel = GuiConsolePanel.CreateConsoleOutputPanel(mainWindow);
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            return grid;
        }

        /// <summary>
        /// Cria o painel de seleção de configurações
        /// </summary>
        private static ScrollViewer CreateConfigSelectionPanel(DevStackGui mainWindow)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            var panel = new StackPanel();

            // Configurações de Path
            var pathPanel = CreatePathConfigPanel(mainWindow);
            panel.Children.Add(pathPanel);

            scrollViewer.Content = panel;
            return scrollViewer;
        }

        /// <summary>
        /// Cria o painel de configurações do PATH (StackPanel estilizado)
        /// </summary>
        private static StackPanel CreatePathConfigPanel(DevStackGui mainWindow)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Título
            var titleLabel = GuiTheme.CreateStyledLabel("Gerenciamento do PATH", true);
            titleLabel.FontSize = 18;
            titleLabel.Margin = new Thickness(0, 0, 0, 20);
            panel.Children.Add(titleLabel);

            // Descrição
            var pathLabel = GuiTheme.CreateStyledLabel("Adicionar ferramentas ao PATH do sistema");
            pathLabel.FontWeight = FontWeights.Bold;
            panel.Children.Add(pathLabel);

            // Botão Adicionar
            var addPathButton = GuiTheme.CreateStyledButton("➕ Adicionar ao PATH", (s, e) => AddToPath(mainWindow));
            addPathButton.Width = 200;
            addPathButton.Height = 35;
            addPathButton.Margin = new Thickness(0, 10, 0, 5);
            addPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(addPathButton);

            // Botão Remover
            var removePathButton = GuiTheme.CreateStyledButton("➖ Remover do PATH", (s, e) => RemoveFromPath(mainWindow));
            removePathButton.Width = 200;
            removePathButton.Height = 35;
            removePathButton.Margin = new Thickness(0, 5, 0, 5);
            removePathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(removePathButton);

            // Botão Listar
            var listPathButton = GuiTheme.CreateStyledButton("📋 Listar PATH Atual", (s, e) => ListCurrentPath(mainWindow));
            listPathButton.Width = 200;
            listPathButton.Height = 35;
            listPathButton.Margin = new Thickness(0, 5, 0, 10);
            listPathButton.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(listPathButton);

            // Info
            var infoLabel = GuiTheme.CreateStyledLabel("ℹ️ As alterações no PATH afetam o terminal e o sistema.");
            infoLabel.FontStyle = FontStyles.Italic;
            infoLabel.Margin = new Thickness(0, 20, 0, 0);
            panel.Children.Add(infoLabel);

            return panel;
        }

        /// <summary>
        /// Adiciona as ferramentas do DevStack ao PATH do sistema
        /// </summary>
        private static void AddToPath(DevStackGui mainWindow)
        {
            try
            {
                DevStackConfig.pathManager?.AddBinDirsToPath();
                
                mainWindow.StatusMessage = "PATH atualizado com sucesso";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao adicionar ao PATH: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao atualizar PATH";
            }
        }

        /// <summary>
        /// Remove as ferramentas do DevStack do PATH do sistema
        /// </summary>
        private static void RemoveFromPath(DevStackGui mainWindow)
        {
            try
            {
                DevStackConfig.pathManager?.RemoveAllDevStackFromPath();
                
                mainWindow.StatusMessage = "PATH limpo com sucesso";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao remover do PATH: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao limpar PATH";
            }
        }

        /// <summary>
        /// Lista o PATH atual do sistema
        /// </summary>
        private static void ListCurrentPath(DevStackGui mainWindow)
        {
            try
            {
                DevStackConfig.pathManager?.ListCurrentPath();

                mainWindow.StatusMessage = "PATH listado";
            }
            catch (Exception ex)
            {
                GuiConsolePanel.AppendToConsole(mainWindow, $"❌ Erro ao listar PATH: {ex.Message}");
                mainWindow.StatusMessage = "Erro ao listar PATH";
            }
        }
    }
}
