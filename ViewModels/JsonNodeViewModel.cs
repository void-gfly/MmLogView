using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MmLogView.ViewModels;

public sealed class JsonNodeViewModel : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isSelected;

    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public JsonValueKind ValueKind { get; set; }
    
    public int TextStart { get; set; }
    public int TextLength { get; set; }
    
    public JsonNodeViewModel? Parent { get; set; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<JsonNodeViewModel> Children { get; } = new();

    public bool IsEditableLeaf => Children.Count == 0 && ValueKind is not JsonValueKind.Object and not JsonValueKind.Array;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
