using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
    }

    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LogViewModel logViewModel)
        {
        }
    }
}