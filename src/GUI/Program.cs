using System;
using System.IO;
using System.Text;
using System.Windows;

namespace DevStackManager
{
    /// <summary>
    /// Ponto de entrada exclusivo para a interface gráfica (DevStackGUI.exe)
    /// Este programa será compilado como WinExe para não mostrar console
    /// </summary>
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                // Carregar configurações necessárias
                LoadConfiguration();

                // Inicializar aplicação WPF
                var app = new Application();
                
                // Criar e mostrar a janela principal
                var mainWindow = new DevStackGui();
                app.MainWindow = mainWindow;
                
                // Executar aplicação
                var result = app.Run(mainWindow);
                
                // Parar captura do console ao finalizar
                GuiConsolePanel.StopConsoleCapture();
                
                return result;
            }
            catch (Exception ex)
            {
                GuiTheme.CreateStyledMessageBox($"Erro ao inicializar DevStack GUI: {ex.Message}",
                              "DevStack Manager - Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                return 1;
            }
        }

        private static void LoadConfiguration()
        {
            DevStackConfig.Initialize();
        }
    }
}
