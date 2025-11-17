using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using BlueMeter.WPF.ViewModels;
using BlueMeter.WPF.Config;

namespace BlueMeter.WPF.Views;

/// <summary>
///     SettingForm.xaml 的交互逻辑
/// </summary>
public partial class SettingsView : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        _vm = vm;
        _vm.RequestClose += Vm_RequestClose;

        // Set initial active theme button when window loads
        Loaded += SettingsView_Loaded;
    }

    private void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        // Populate theme buttons dynamically from ThemeDefinitions
        PopulateThemeButtons();

        // Find and mark the currently active theme button
        InitializeActiveThemeButton();
    }

    // Dynamically generate theme buttons from ThemeDefinitions
    private void PopulateThemeButtons()
    {
        if (ThemeButtonsPanel == null) return;

        // Clear any existing buttons
        ThemeButtonsPanel.Children.Clear();

        // Add buttons for each theme definition
        foreach (var theme in ThemeDefinitions.Themes)
        {
            Button themeButton;

            // Handle special gradient themes
            if (theme.Id == "Rainbow" || theme.Id == "Sunset" || theme.Id == "Cyberpunk")
            {
                themeButton = CreateGradientButton(theme.Id);
            }
            else if (theme.Id == "Transparent")
            {
                themeButton = CreateTransparentButton();
            }
            else
            {
                themeButton = CreateSolidColorButton(theme.ColorHex);
            }

            // Store theme ID in Tag for easier identification
            themeButton.Tag = theme.Id;
            themeButton.ToolTip = theme.DisplayName;
            themeButton.Click += ThemeButton_Click;

            ThemeButtonsPanel.Children.Add(themeButton);
        }
    }

    private Button CreateSolidColorButton(string colorHex)
    {
        var button = new Button
        {
            Style = (Style)FindResource("ThemeButton")
        };

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            button.Background = new SolidColorBrush(color);
        }
        catch
        {
            button.Background = new SolidColorBrush(Colors.Gray);
        }

        return button;
    }

    private Button CreateTransparentButton()
    {
        return new Button
        {
            Style = (Style)FindResource("ThemeButton"),
            Background = Brushes.Transparent,
            Tag = "Transparent",
            ToolTip = "Transparent (No Color Overlay)"
        };
    }

    private Button CreateGradientButton(string gradientType)
    {
        var button = new Button
        {
            Style = (Style)FindResource("ThemeButton")
        };

        // Create gradient template
        var template = new ControlTemplate(typeof(Button));
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.StyleProperty, FindResource("ThemeButtonBorder"));

        var gridFactory = new FrameworkElementFactory(typeof(Grid));

        // Define gradient colors based on type
        var colors = gradientType switch
        {
            "Rainbow" => new[] { "#FF0000", "#FFA500", "#FFFF00", "#00FF00" },
            "Sunset" => new[] { "#FF6B6B", "#FFA500", "#FFD700", "#FF69B4" },
            "Cyberpunk" => new[] { "#FF006E", "#00FFFF", "#39FF14", "#BF40BF" },
            _ => new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00" }
        };

        for (int i = 0; i < 4; i++)
        {
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Grid.RowProperty, i / 2);
            borderFactory.SetValue(Grid.ColumnProperty, i % 2);

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colors[i]);
                borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(color));
            }
            catch
            {
                borderFactory.SetValue(Border.BackgroundProperty, Brushes.Gray);
            }

            // Set corner radius for gradient tiles
            borderFactory.SetValue(Border.CornerRadiusProperty, i switch
            {
                0 => new CornerRadius(5, 0, 0, 0),
                1 => new CornerRadius(0, 5, 0, 0),
                2 => new CornerRadius(0, 0, 0, 5),
                3 => new CornerRadius(0, 0, 5, 0),
                _ => new CornerRadius(0)
            });

            gridFactory.AppendChild(borderFactory);
        }

        // Add row/column definitions
        var rowDef1 = new FrameworkElementFactory(typeof(RowDefinition));
        rowDef1.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        var rowDef2 = new FrameworkElementFactory(typeof(RowDefinition));
        rowDef2.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        gridFactory.AppendChild(rowDef1);
        gridFactory.AppendChild(rowDef2);

        var colDef1 = new FrameworkElementFactory(typeof(ColumnDefinition));
        colDef1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var colDef2 = new FrameworkElementFactory(typeof(ColumnDefinition));
        colDef2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        gridFactory.AppendChild(colDef1);
        gridFactory.AppendChild(colDef2);

        factory.AppendChild(gridFactory);
        template.VisualTree = factory;
        button.Template = template;

        return button;
    }

    // Initialize the active theme button indicator based on current AppConfig.ThemeColor
    private void InitializeActiveThemeButton()
    {
        // Access the named WrapPanel directly
        if (ThemeButtonsPanel == null) return;

        var currentThemeColor = _vm.AppConfig.ThemeColor;

        foreach (var child in ThemeButtonsPanel.Children)
        {
            if (child is Button button)
            {
                bool isActive = false;

                // Check if this button matches the current theme using Tag (theme ID)
                if (button.Tag is string themeId)
                {
                    // Try to find the theme in definitions
                    var theme = ThemeDefinitions.GetTheme(themeId);
                    if (theme != null)
                    {
                        // Match by ID or ColorHex
                        isActive = theme.Id.Equals(currentThemeColor, StringComparison.OrdinalIgnoreCase) ||
                                   theme.ColorHex.Equals(currentThemeColor, StringComparison.OrdinalIgnoreCase);
                    }
                }
                // Fallback: check background color for backwards compatibility
                else if (button.Background is SolidColorBrush brush)
                {
                    isActive = brush.Color.ToString().Equals(currentThemeColor, StringComparison.OrdinalIgnoreCase);
                }

                // Set appropriate style
                if (isActive)
                {
                    button.Style = (Style)FindResource("ThemeButton_Active");
                }
                else
                {
                    button.Style = (Style)FindResource("ThemeButton");
                }
            }
        }
    }

    private void Vm_RequestClose()
    {
        Close();
    }

    // FIX: Theme color button click handler - allows users to change window theme color
    // Updates the AppConfig property which triggers PropertyChanged notification with live preview
    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string? newThemeColor = null;

            // Get theme from Tag (theme ID)
            if (button.Tag is string themeId)
            {
                var theme = ThemeDefinitions.GetTheme(themeId);
                if (theme != null)
                {
                    // Use the theme's ColorHex as the theme color
                    newThemeColor = theme.ColorHex;
                }
            }
            // Fallback: use background color for backwards compatibility
            else if (button.Background is SolidColorBrush brush)
            {
                newThemeColor = brush.Color.ToString();
            }

            if (newThemeColor != null)
            {
                // Update the property - this will trigger PropertyChanged through ObservableProperty
                // This provides live preview without needing to click "Save"
                _vm.AppConfig.ThemeColor = newThemeColor;

                // Update visual indicator for active theme button
                UpdateActiveThemeButton(button);
            }
        }
    }

    // Update the visual indicator to show which theme is currently active
    private void UpdateActiveThemeButton(Button activeButton)
    {
        // Find the parent WrapPanel containing all theme buttons
        if (activeButton.Parent is WrapPanel wrapPanel)
        {
            // Reset all buttons to normal style
            foreach (var child in wrapPanel.Children)
            {
                if (child is Button btn && btn != activeButton)
                {
                    // Only update if it's currently using the active style
                    if (btn.Style == FindResource("ThemeButton_Active"))
                    {
                        btn.Style = (Style)FindResource("ThemeButton");
                    }
                }
            }

            // Set clicked button to active style
            activeButton.Style = (Style)FindResource("ThemeButton_Active");
        }
    }

    // FIX: Background image selection handler - opens file dialog to select image
    private void SelectBackgroundImage_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
            Title = "Select Background Image"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _vm.AppConfig.BackgroundImagePath = openFileDialog.FileName;
        }
    }

    private void Footer_ConfirmClick(object sender, RoutedEventArgs e) { /* handled by VM command */ }

    private void Footer_CancelClick(object sender, RoutedEventArgs e) { /* handled by VM command */ }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void ScrollToSection(FrameworkElement? target)
    {
        if (target == null) return;

        if (ContentScrollViewer?.Content is FrameworkElement content)
        {
            var p = target.TransformToVisual(content).Transform(new Point(0, 0));
            var y = Math.Max(p.Y, 0);
            ContentScrollViewer.ScrollToVerticalOffset(y);
        }
        else
        {
            target.BringIntoView();
        }
    }

    private void Nav_Language_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionLanguage);
    private void Nav_Basic_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionBasic);
    private void Nav_Shortcut_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionShortcut);
    private void Nav_Combat_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionCombat);
    private void Nav_Theme_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionTheme);

    /// <summary>
    /// Opens settings window, scrolls to UID field, and highlights it in red
    /// to indicate that the user needs to enter their UID for Solo Training mode
    /// </summary>
    public void ShowAndHighlightUidField()
    {
        // Show the window and bring it to front
        Show();
        Activate();

        // Wait for window to be fully loaded before scrolling
        Dispatcher.InvokeAsync(() =>
        {
            // Scroll to Combat section where UID field is located
            ScrollToSection(SectionCombat);

            // Focus the UID TextBox
            UidTextBox?.Focus();

            // Create red flash animation
            if (UidInputBorder != null)
            {
                var originalBrush = UidInputBorder.BorderBrush;
                var originalThickness = UidInputBorder.BorderThickness;

                // Create red color animation
                var colorAnimation = new System.Windows.Media.Animation.ColorAnimation
                {
                    To = Colors.Red,
                    Duration = TimeSpan.FromMilliseconds(300),
                    AutoReverse = true,
                    RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3) // Flash 3 times
                };

                // Create thickness animation to make border more visible
                var thicknessAnimation = new System.Windows.Media.Animation.ThicknessAnimation
                {
                    To = new Thickness(2),
                    Duration = TimeSpan.FromMilliseconds(300),
                    AutoReverse = true,
                    RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3)
                };

                // Apply animations
                var brush = new SolidColorBrush(originalBrush is SolidColorBrush sb ? sb.Color : Colors.Gray);
                UidInputBorder.BorderBrush = brush;

                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                UidInputBorder.BeginAnimation(Border.BorderThicknessProperty, thicknessAnimation);
            }
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    // MEMORY LEAK FIX: Dispose ViewModel when window closes to clean up event subscriptions.
    // SettingsViewModel subscribes to LocalizationManager and NetworkChange static events.
    // Without this, the ViewModel would never be garbage collected, causing memory leaks.
    protected override void OnClosed(EventArgs e)
    {
        _vm.RequestClose -= Vm_RequestClose;
        _vm.Dispose();
        base.OnClosed(e);
    }
}
