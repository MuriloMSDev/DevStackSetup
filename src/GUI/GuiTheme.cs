using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DevStackManager
{
    /// <summary>
    /// Sistema de temas e estilos para o DevStackManager GUI
    /// Contém as definições de cores, estilos e helpers para criação de controles temáticos
    /// </summary>
    public static class GuiTheme
    {
        #region Theme Classes
        /// <summary>
        /// Cores do tema
        /// </summary>
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
            public SolidColorBrush DropdownBackground { get; set; } = null!;

            // Texto secundário
            public SolidColorBrush TextMuted { get; set; } = null!;
            public SolidColorBrush TextSecondary { get; set; } = null!;
        }
        #endregion

        #region Theme Definition
        /// <summary>
        /// Tema escuro personalizado
        /// </summary>
        public static readonly ThemeColors DarkTheme = new()
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
            SidebarSelected = new SolidColorBrush(Color.FromRgb(36, 46, 59)),
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
            DropdownBackground = new SolidColorBrush(Color.FromRgb(27, 32, 40)),

            // Texto com hierarquia bem definida
            TextMuted = new SolidColorBrush(Color.FromRgb(139, 148, 158)),
            TextSecondary = new SolidColorBrush(Color.FromRgb(166, 173, 186))
        };

        public static ThemeColors CurrentTheme => DarkTheme;
        #endregion

        #region UI Helper Methods
        /// <summary>
        /// Aplica o tema escuro a um DataGrid
        /// </summary>
        public static void SetDataGridDarkTheme(DataGrid dataGrid)
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

        /// <summary>
        /// Cria um botão estilizado com tema escuro
        /// </summary>
        public static Button CreateStyledButton(string content, RoutedEventHandler? clickHandler = null)
        {
            var button = new Button
            {
                Content = content
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
            else if (content.Contains("Não") ||content.Contains("Desinstalar") || content.Contains("🗑") || content.Contains("❌"))
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

            // Create style for hover and pressed effects
            var buttonStyle = new Style(typeof(Button));

            // Set base properties to override default template
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, backgroundColor));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.ButtonForeground));
            buttonStyle.Setters.Add(new Setter(Button.BorderBrushProperty, CurrentTheme.Border));
            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            buttonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(12, 8, 12, 8)));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Medium));

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

            // Add hover trigger - força override do template padrão
            var hoverTrigger = new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, hoverColor));
            hoverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, CurrentTheme.BorderHover));
            hoverTrigger.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            // Força a cor de foreground para garantir contraste
            hoverTrigger.Setters.Add(new Setter(Button.ForegroundProperty, CurrentTheme.ButtonForeground));

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

            // Aplica o estilo ao botão
            button.Style = buttonStyle;

            // Add subtle shadow effect for visual depth
            button.Effect = new DropShadowEffect
            {
                BlurRadius = 4,
                ShadowDepth = 2,
                Opacity = 0.6,
                Color = Colors.Black
            };

            return button;
        }

        /// <summary>
        /// Cria um TextBox estilizado com tema escuro
        /// </summary>
        public static TextBox CreateStyledTextBox(bool isConsole = false)
        {
            var textBox = new TextBox
            {
                BorderThickness = new Thickness(1),
                Padding = isConsole ? new Thickness(10, 8, 10, 8) : new Thickness(10, 4, 10, 4),
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

        /// <summary>
        /// Cria um ComboBox estilizado com tema escuro
        /// </summary>
        public static ComboBox CreateStyledComboBox()
        {
            var comboBox = new ComboBox
            {
                Background = CurrentTheme.InputBackground,
                Foreground = CurrentTheme.InputForeground,
                BorderBrush = CurrentTheme.InputBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 14,
                MinHeight = 35
            };

            // Evento para rolar o dropdown para o início ao abrir
            comboBox.DropDownOpened += (s, e) =>
            {
                // Tenta encontrar o ScrollViewer do dropdown
                if (comboBox.Template != null)
                {
                    comboBox.ApplyTemplate();
                    var popup = comboBox.Template.FindName("Popup", comboBox) as System.Windows.Controls.Primitives.Popup;
                    if (popup != null && popup.Child is Border border)
                    {
                        var scrollViewer = FindScrollViewer(border);
                        if (scrollViewer != null)
                        {
                            scrollViewer.ScrollToTop();
                        }
                    }
                }
            };

            // Helper para buscar ScrollViewer dentro do Border
            static ScrollViewer? FindScrollViewer(DependencyObject parent)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is ScrollViewer sv)
                        return sv;
                    var result = FindScrollViewer(child);
                    if (result != null)
                        return result;
                }
                return null;
            }

            // Create a simplified but effective style for dark theme
            var comboStyle = new Style(typeof(ComboBox));

            // Basic styling - force dark colors
            comboStyle.Setters.Add(new Setter(ComboBox.BackgroundProperty, CurrentTheme.InputBackground));
            comboStyle.Setters.Add(new Setter(ComboBox.ForegroundProperty, CurrentTheme.InputForeground));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputBorder));
            comboStyle.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(1)));
            comboStyle.Setters.Add(new Setter(ComboBox.PaddingProperty, new Thickness(10, 8, 10, 8)));
            comboStyle.Setters.Add(new Setter(ComboBox.FontSizeProperty, 14.0));
            comboStyle.Setters.Add(new Setter(ComboBox.MinHeightProperty, 35.0));

            // Create simplified template using XAML string
            var templateXaml = @"
                <ControlTemplate TargetType='ComboBox' xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Border Name='MainBorder' 
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'
                            CornerRadius='3'>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width='*'/>
                                <ColumnDefinition Width='20'/>
                            </Grid.ColumnDefinitions>
                            
                            <!-- Content presenter for selected item -->
                            <ContentPresenter Name='ContentSite'
                                            Grid.Column='0'
                                            Margin='10,5,10,8'
                                            VerticalAlignment='Top'
                                            HorizontalAlignment='Left'
                                            Content='{TemplateBinding SelectionBoxItem}'
                                            ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}'
                                            ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}'
                                            IsHitTestVisible='False'/>
                            
                            <!-- Toggle button for dropdown -->
                            <ToggleButton Name='ToggleButton'
                                        Grid.Column='0'
                                        Grid.ColumnSpan='2'
                                        Background='Transparent'
                                        BorderBrush='Transparent'
                                        BorderThickness='0'
                                        Focusable='False'
                                        ClickMode='Press'
                                        IsChecked='{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}}'>
                                <ToggleButton.Template>
                                    <ControlTemplate TargetType='ToggleButton'>
                                        <Border Background='Transparent'>
                                            <Path Name='Arrow'
                                                  Data='M 0 0 L 4 4 L 8 0 Z'
                                                  Fill='{Binding Foreground, RelativeSource={RelativeSource AncestorType=ComboBox}}'
                                                  HorizontalAlignment='Right'
                                                  VerticalAlignment='Center'
                                                  Margin='0,0,8,0'/>
                                        </Border>
                                    </ControlTemplate>
                                </ToggleButton.Template>
                            </ToggleButton>
                            
                            <!-- Popup for dropdown -->
                            <Popup Name='Popup'
                                   Placement='Bottom'
                                   IsOpen='{TemplateBinding IsDropDownOpen}'
                                   AllowsTransparency='True'
                                   Focusable='False'
                                   PopupAnimation='Slide'>
                                <Border Name='DropDownBorder'
                                        Background='#FF2D2D30'
                                        BorderBrush='#FF3F3F46'
                                        BorderThickness='1'
                                        CornerRadius='3'
                                        MinWidth='{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}'
                                        MaxHeight='{TemplateBinding MaxDropDownHeight}'>
                                    <ScrollViewer Name='DropDownScrollViewer'
                                                  CanContentScroll='True'>
                                        <ItemsPresenter KeyboardNavigation.DirectionalNavigation='Contained'/>
                                    </ScrollViewer>
                                </Border>
                            </Popup>
                        </Grid>
                    </Border>
                    
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#FF007ACC'/>
                        </Trigger>
                        <Trigger Property='IsFocused' Value='True'>
                            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#FF007ACC'/>
                            <Setter TargetName='MainBorder' Property='BorderThickness' Value='2'/>
                        </Trigger>
                        <Trigger Property='IsDropDownOpen' Value='True'>
                            <Setter TargetName='MainBorder' Property='BorderBrush' Value='#FF007ACC'/>
                            <Setter TargetName='MainBorder' Property='BorderThickness' Value='2'/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>";

            try
            {
                var template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(templateXaml);
                comboStyle.Setters.Add(new Setter(ComboBox.TemplateProperty, template));
            }
            catch
            {
                // Fallback to basic triggers if XAML parsing fails
                var hoverTrigger = new Trigger { Property = ComboBox.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.BorderHover));
                comboStyle.Triggers.Add(hoverTrigger);

                var focusTrigger = new Trigger { Property = ComboBox.IsFocusedProperty, Value = true };
                focusTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
                focusTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
                comboStyle.Triggers.Add(focusTrigger);

                var dropdownTrigger = new Trigger { Property = ComboBox.IsDropDownOpenProperty, Value = true };
                dropdownTrigger.Setters.Add(new Setter(ComboBox.BorderBrushProperty, CurrentTheme.InputFocusBorder));
                dropdownTrigger.Setters.Add(new Setter(ComboBox.BorderThicknessProperty, new Thickness(2)));
                comboStyle.Triggers.Add(dropdownTrigger);
            }

            comboBox.Style = comboStyle;

            // Style the dropdown items for dark theme
            var itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.DropdownBackground));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, CurrentTheme.InputForeground));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(10, 6, 10, 6)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BorderThicknessProperty, new Thickness(0)));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.FontSizeProperty, 14.0));

            // Hover trigger for items
            var itemHoverTrigger = new Trigger
            {
                Property = ComboBoxItem.IsMouseOverProperty,
                Value = true
            };
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.SidebarHover));
            itemHoverTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, CurrentTheme.InputForeground));

            // Selected trigger for items
            var itemSelectedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsSelectedProperty,
                Value = true
            };
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.ButtonBackground));
            itemSelectedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

            // Highlighted trigger for items
            var itemHighlightedTrigger = new Trigger
            {
                Property = ComboBoxItem.IsHighlightedProperty,
                Value = true
            };
            itemHighlightedTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, CurrentTheme.ButtonBackground));
            itemHighlightedTrigger.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));

            itemStyle.Triggers.Add(itemHoverTrigger);
            itemStyle.Triggers.Add(itemSelectedTrigger);
            itemStyle.Triggers.Add(itemHighlightedTrigger);

            comboBox.ItemContainerStyle = itemStyle;

            // Override system colors to force dark theme
            comboBox.Resources.Clear();
            comboBox.Resources.Add(SystemColors.WindowBrushKey, CurrentTheme.DropdownBackground);
            comboBox.Resources.Add(SystemColors.ControlBrushKey, CurrentTheme.InputBackground);
            comboBox.Resources.Add(SystemColors.ControlTextBrushKey, CurrentTheme.InputForeground);
            comboBox.Resources.Add(SystemColors.HighlightBrushKey, CurrentTheme.ButtonBackground);
            comboBox.Resources.Add(SystemColors.HighlightTextBrushKey, new SolidColorBrush(Colors.White));

            return comboBox;
        }

        /// <summary>
        /// Cria um Label estilizado com tema escuro
        /// </summary>
        public static Label CreateStyledLabel(string content, bool isTitle = false, bool isMuted = false)
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

        /// <summary>
        /// Exibe uma MessageBox estilizada com tema escuro
        /// </summary>
        public static MessageBoxResult CreateStyledMessageBox(string message, string title = "Mensagem", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            // Cria uma janela customizada para garantir tema escuro real
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Background = Brushes.Transparent, // Para permitir borda arredondada
                AllowsTransparency = true,
                Foreground = CurrentTheme.Foreground,
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current?.MainWindow
            };

            var border = new Border
            {
                Background = CurrentTheme.FormBackground,
                CornerRadius = new CornerRadius(12),
                BorderBrush = CurrentTheme.Border,
                BorderThickness = new Thickness(1),
                SnapsToDevicePixels = true
            };

            var grid = new Grid { Margin = new Thickness(0) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Mensagem
            var msg = new TextBlock
            {
                Text = message,
                Foreground = CurrentTheme.Foreground,
                Background = Brushes.Transparent,
                FontSize = 16,
                Margin = new Thickness(55, 20, 30, 10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(msg, 0);
            grid.Children.Add(msg);

            // Botões
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 20, 20)
            };

            MessageBoxResult result = MessageBoxResult.None;
            void CloseAndSetResult(MessageBoxResult r) { result = r; dialog.DialogResult = true; dialog.Close(); }

            void AddButton(string text, MessageBoxResult r, bool isDefault = false)
            {
                var btn = CreateStyledButton(text, (s, e) => CloseAndSetResult(r));
                btn.MinWidth = 80;
                btn.Margin = new Thickness(8, 0, 0, 0);
                btn.Padding = new Thickness(10, 5, 10, 5);
                btn.FontWeight = isDefault ? FontWeights.Bold : FontWeights.Normal;
                buttonPanel.Children.Add(btn);
            }

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, true);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, true);
                    AddButton("Cancelar", MessageBoxResult.Cancel);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Sim", MessageBoxResult.Yes, true);
                    AddButton("Não", MessageBoxResult.No);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Sim", MessageBoxResult.Yes, true);
                    AddButton("Não", MessageBoxResult.No);
                    AddButton("Cancelar", MessageBoxResult.Cancel);
                    break;
            }

            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            // Ícone
            if (icon != MessageBoxImage.None)
            {
                var iconText = new TextBlock
                {
                    Margin = new Thickness(5, 5, 0, 0),
                    FontSize = 32,
                    VerticalAlignment = VerticalAlignment.Top
                };
                switch (icon)
                {
                    case MessageBoxImage.Information:
                        iconText.Text = "❕";
                        iconText.Foreground = CurrentTheme.Accent;
                        break;
                    case MessageBoxImage.Warning:
                        iconText.Text = "⚠️";
                        iconText.Foreground = CurrentTheme.Warning;
                        break;
                    case MessageBoxImage.Error:
                        iconText.Text = "⛔";
                        iconText.Foreground = CurrentTheme.Danger;
                        break;
                    case MessageBoxImage.Question:
                        iconText.Text = "❔";
                        iconText.Foreground = CurrentTheme.Accent;
                        break;
                }
                grid.Children.Add(iconText);
            }

            border.Child = grid;
            dialog.Content = border;
            dialog.ShowDialog();
            return result;
        }

        /// <summary>
        /// Aplica o tema escuro personalizado a um ListBox usado na barra lateral
        /// </summary>
        public static void ApplySidebarListBoxTheme(ListBox listBox)
        {
            listBox.Background = CurrentTheme.SidebarBackground;
            listBox.Foreground = CurrentTheme.Foreground;
            listBox.BorderBrush = CurrentTheme.Border;
            listBox.BorderThickness = new Thickness(0);
            ScrollViewer.SetCanContentScroll(listBox, false);

            // Create custom template para garantir controle total sobre o visual
            var itemTemplate = new ControlTemplate(typeof(ListBoxItem));

            // Container principal (Border para background e borda direita)
            var mainBorder = new FrameworkElementFactory(typeof(Border));
            mainBorder.Name = "MainBorder";
            mainBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 4, 0));
            mainBorder.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            mainBorder.SetValue(Border.PaddingProperty, new Thickness(15, 12, 15, 12));
            mainBorder.SetValue(Border.MarginProperty, new Thickness(4, 2, 4, 2));

            // ContentPresenter para o conteúdo do item
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            mainBorder.AppendChild(contentPresenter);
            itemTemplate.VisualTree = mainBorder;

            // Triggers no template para garantir prioridade máxima

            // 1. Hover trigger (baixa prioridade)
            var hoverTrigger = new Trigger { Property = ListBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarHover) { TargetName = "MainBorder" });
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "MainBorder" });
            itemTemplate.Triggers.Add(hoverTrigger);

            // 2. Selected trigger (MÁXIMA PRIORIDADE - deve ser o último)
            var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarSelected) { TargetName = "MainBorder" });
            selectedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderFocus) { TargetName = "MainBorder" });
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
            selectedTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
            itemTemplate.Triggers.Add(selectedTrigger);

            // 3. Selected + Hover trigger (sobrescreve hover quando selecionado)
            var selectedHoverTrigger = new MultiTrigger();
            selectedHoverTrigger.Conditions.Add(new Condition(ListBoxItem.IsSelectedProperty, true));
            selectedHoverTrigger.Conditions.Add(new Condition(ListBoxItem.IsMouseOverProperty, true));
            selectedHoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, CurrentTheme.SidebarHover) { TargetName = "MainBorder" });
            selectedHoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, CurrentTheme.BorderHover) { TargetName = "MainBorder" });
            selectedHoverTrigger.Setters.Add(new Setter(ListBoxItem.ForegroundProperty, new SolidColorBrush(Colors.White)));
            selectedHoverTrigger.Setters.Add(new Setter(ListBoxItem.FontWeightProperty, FontWeights.SemiBold));
            itemTemplate.Triggers.Add(selectedHoverTrigger);

            // Style com template customizado
            var itemStyle = new Style(typeof(ListBoxItem));
            itemStyle.Setters.Add(new Setter(ListBoxItem.TemplateProperty, itemTemplate));
            itemStyle.Setters.Add(new Setter(ListBoxItem.FontSizeProperty, 14.0));
            itemStyle.Setters.Add(new Setter(ListBoxItem.CursorProperty, Cursors.Hand));
            itemStyle.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            itemStyle.Setters.Add(new Setter(ListBoxItem.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            // Aplicar o estilo
            listBox.ItemContainerStyle = itemStyle;
        }
        
        /// <summary>
        /// Cria um overlay de loading com spinner centralizado
        /// </summary>
        public static Border CreateLoadingOverlay()
        {
            double spinnerSize = 20;
            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Child = new Viewbox
                {
                    Width = 500,
                    Height = 500,
                    Child = CreateSpinner(spinnerSize, spinnerSize)
                }
            };
            return overlay;
        }
        
        /// <summary>
        /// Cria um spinner animado circular, usado para overlays de loading (tamanho customizável)
        /// </summary>
        public static UIElement CreateSpinner(double width, double height)
        {
            int dotCount = 11;
            double minDim = Math.Min(width, height);
            double radius = minDim / 4.0;
            double dotRadius = minDim / 20.0;
            double centerX = width / 2.0;
            double centerY = height / 2.0;

            var canvas = new System.Windows.Controls.Canvas
            {
                Width = width,
                Height = height
            };

            double[] opacities = new double[] { 0.12, 0.18, 0.24, 0.32, 0.44, 0.56, 0.68, 0.8, 0.86, 0.92, 1.0 };

            for (int i = 0; i < dotCount; i++)
            {
                double angle = i * 360.0 / 12.0;
                double rad = angle * Math.PI / 180.0;
                double x = centerX + radius * Math.Sin(rad) - dotRadius;
                double y = centerY - radius * Math.Cos(rad) - dotRadius;

                var ellipse = new System.Windows.Shapes.Ellipse
                {
                    Width = dotRadius * 2,
                    Height = dotRadius * 2,
                    Fill = System.Windows.Media.Brushes.White,
                    Opacity = opacities[i % opacities.Length]
                };
                System.Windows.Controls.Canvas.SetLeft(ellipse, x);
                System.Windows.Controls.Canvas.SetTop(ellipse, y);
                canvas.Children.Add(ellipse);
            }

            var rotate = new System.Windows.Media.RotateTransform(0, centerX, centerY);
            canvas.RenderTransform = rotate;

            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(1.1)),
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };
            canvas.Loaded += (s, e) =>
            {
                rotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animation);
            };
            return canvas;
        }
        #endregion
    }
}
