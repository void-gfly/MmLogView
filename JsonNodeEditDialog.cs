using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MmLogView.Properties;
using MmLogView.ViewModels;

namespace MmLogView;

public class JsonNodeEditDialog : Window
{
    private readonly JsonNodeViewModel _node;
    private readonly TextBox _textBox;

    public string EditedText => _textBox.Text;

    public JsonNodeEditDialog(JsonNodeViewModel node)
    {
        _node = node;

        var res = ResourcesExtension.Instance;
        Title = res.JsonEditDialogTitle;
        Width = 980;
        Height = 620;
        MinWidth = 900;
        MinHeight = 540;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        ShowInTaskbar = false;
        Background = GetBrush("WindowBackground", Brushes.White);

        var windowForeground = GetBrush("WindowForeground", Brushes.Black);
        var dimForeground = GetBrush("DimForeground", Brushes.Gray);
        var borderBrush = GetBrush("BorderBrush", Brushes.Gray);
        var toolbarBrush = GetBrush("ToolbarBackground", GetBrush("WindowBackground", Brushes.White));
        var editorBackground = GetBrush("EditorBackground", Brushes.White);
        var editorForeground = GetBrush("EditorForeground", windowForeground);
        var accentBrush = GetBrush("StatusBarBackground", Brushes.SteelBlue);
        var accentForeground = GetBrush("StatusBarForeground", Brushes.White);
        var selectionBrush = GetBrush("SelectionBackground", Brushes.SteelBlue);

        var shellBorder = new Border
        {
            Margin = new Thickness(10),
            CornerRadius = new CornerRadius(12),
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Background = GetBrush("WindowBackground", Brushes.White),
            SnapsToDevicePixels = true
        };

        var rootGrid = new Grid();
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var headerBorder = new Border
        {
            Background = toolbarBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(24, 16, 24, 16),
            CornerRadius = new CornerRadius(12, 12, 0, 0)
        };
        Grid.SetRow(headerBorder, 0);

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titlePanel = new StackPanel();
        titlePanel.Children.Add(new TextBlock
        {
            Text = res.JsonEditDialogTitle,
            FontSize = 21,
            FontWeight = FontWeights.SemiBold,
            Foreground = windowForeground
        });
        titlePanel.Children.Add(new TextBlock
        {
            Text = string.Format(res.JsonEditNodeLabel, node.Name),
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = dimForeground,
            FontSize = 12
        });
        Grid.SetColumn(titlePanel, 0);
        headerGrid.Children.Add(titlePanel);

        var metaCard = new Border
        {
            Background = editorBackground,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12, 8, 12, 8),
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 164
        };
        var metaPanel = new StackPanel();
        metaPanel.Children.Add(new TextBlock
        {
            Text = res.JsonEditDialogTitle,
            Foreground = dimForeground,
            FontSize = 10,
            Opacity = 0.9
        });
        metaPanel.Children.Add(new TextBlock
        {
            Text = GetTypeText(node.ValueKind),
            Margin = new Thickness(0, 2, 0, 0),
            Foreground = windowForeground,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold
        });
        metaCard.Child = metaPanel;
        Grid.SetColumn(metaCard, 1);
        headerGrid.Children.Add(metaCard);

        headerBorder.Child = headerGrid;
        rootGrid.Children.Add(headerBorder);

        var contentGrid = new Grid { Margin = new Thickness(24, 16, 24, 14) };
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(contentGrid, 1);

        var editorTitle = new TextBlock
        {
            Text = res.JsonEditHint,
            Margin = new Thickness(0, 0, 0, 10),
            Foreground = dimForeground,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12
        };
        Grid.SetRow(editorTitle, 0);
        contentGrid.Children.Add(editorTitle);

        var editorCard = new Border
        {
            Background = editorBackground,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(1)
        };
        Grid.SetRow(editorCard, 1);

        _textBox = new TextBox
        {
            Text = GetInitialText(node),
            AcceptsReturn = node.ValueKind == JsonValueKind.String,
            AcceptsTab = true,
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0),
            Padding = new Thickness(14, 12, 14, 12),
            BorderThickness = new Thickness(0),
            FontSize = 14,
            FontFamily = new FontFamily("Cascadia Mono, Consolas, Courier New"),
            Background = Brushes.Transparent,
            Foreground = editorForeground,
            CaretBrush = editorForeground,
            SelectionBrush = selectionBrush,
            SelectionOpacity = 0.35
        };
        _textBox.KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                TryAccept();
            }
        };
        editorCard.Child = _textBox;
        contentGrid.Children.Add(editorCard);

        var footerHint = new Border
        {
            Background = toolbarBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 10, 14, 10),
            Margin = new Thickness(0, 10, 0, 0)
        };
        footerHint.Child = new TextBlock
        {
            Text = node.ValueKind == JsonValueKind.String
                ? res.JsonEditStringFooterHint
                : res.JsonEditTypedFooterHint,
            Foreground = dimForeground,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11
        };
        Grid.SetRow(footerHint, 2);
        contentGrid.Children.Add(footerHint);

        rootGrid.Children.Add(contentGrid);

        var actionBar = new Border
        {
            Background = toolbarBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(24, 12, 24, 12),
            CornerRadius = new CornerRadius(0, 0, 12, 12)
        };
        Grid.SetRow(actionBar, 2);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var cancelButton = new Button
        {
            Content = res.BtnCancel,
            Width = 108,
            Height = 36,
            Margin = new Thickness(0, 0, 10, 0),
            IsCancel = true
        };
        cancelButton.Style = CreateDialogButtonStyle(false, toolbarBrush, windowForeground, borderBrush, accentBrush, accentForeground);
        var okButton = new Button
        {
            Content = res.BtnOk,
            Width = 124,
            Height = 36
        };
        okButton.Style = CreateDialogButtonStyle(true, toolbarBrush, windowForeground, borderBrush, accentBrush, accentForeground);
        okButton.Click += (_, _) => TryAccept();
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Insert(0, cancelButton);
        actionBar.Child = buttonPanel;
        rootGrid.Children.Add(actionBar);

        shellBorder.Child = rootGrid;
        Content = shellBorder;
        Loaded += (_, _) =>
        {
            _textBox.Focus();
            _textBox.SelectAll();
        };
    }

    private void TryAccept()
    {
        if (!IsValidInput(_node.ValueKind, _textBox.Text))
        {
            var res = ResourcesExtension.Instance;
            MessageBox.Show(res.JsonEditInvalidInput, res.InvalidInputTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private static string GetInitialText(JsonNodeViewModel node)
    {
        return node.ValueKind == JsonValueKind.String
            ? JsonSerializer.Deserialize<string>(node.Value) ?? string.Empty
            : node.Value;
    }

    private static bool IsValidInput(JsonValueKind valueKind, string text)
    {
        switch (valueKind)
        {
            case JsonValueKind.String:
                return true;
            case JsonValueKind.Number:
                try
                {
                    using var document = JsonDocument.Parse(text);
                    return document.RootElement.ValueKind == JsonValueKind.Number;
                }
                catch
                {
                    return false;
                }
            case JsonValueKind.True:
            case JsonValueKind.False:
                return bool.TryParse(text, out _);
            case JsonValueKind.Null:
                return string.Equals(text.Trim(), "null", StringComparison.Ordinal);
            default:
                return false;
        }
    }

    private static string GetTypeText(JsonValueKind valueKind)
    {
        return valueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Null => "null",
            _ => valueKind.ToString()
        };
    }

    private static Brush GetBrush(string resourceKey, Brush fallback)
    {
        return (Brush?)Application.Current.TryFindResource(resourceKey) ?? fallback;
    }

    private static Style CreateDialogButtonStyle(bool isPrimary, Brush surfaceBrush, Brush foregroundBrush, Brush borderBrush, Brush accentBrush, Brush accentForeground)
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, isPrimary ? accentBrush : surfaceBrush));
        style.Setters.Add(new Setter(Control.ForegroundProperty, isPrimary ? accentForeground : foregroundBrush));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, isPrimary ? accentBrush : borderBrush));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.CursorProperty, System.Windows.Input.Cursors.Hand));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(18, 10, 18, 10)));
        style.Setters.Add(new Setter(Control.FontSizeProperty, 13.0));
        style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateButtonTemplate(isPrimary)));
        return style;
    }

    private static ControlTemplate CreateButtonTemplate(bool isPrimary)
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "ButtonBorder";
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));

        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(contentPresenter);

        var template = new ControlTemplate(typeof(Button)) { VisualTree = border };

        var hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Control.OpacityProperty, isPrimary ? 0.92 : 0.88));
        if (!isPrimary)
        {
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)), "ButtonBorder"));
        }
        template.Triggers.Add(hoverTrigger);

        var pressedTrigger = new Trigger { Property = ButtonBase.IsPressedProperty, Value = true };
        pressedTrigger.Setters.Add(new Setter(Control.OpacityProperty, 0.82));
        template.Triggers.Add(pressedTrigger);

        var disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabledTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 0.45));
        template.Triggers.Add(disabledTrigger);

        return template;
    }
}