using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Zlibrary.Noproxy.Avalonia.ViewModels;

public partial class LogViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> _logs = new();

    [ObservableProperty]
    private bool _isAutoScroll = true;

    [ObservableProperty]
    private string _filterText = "";

    public LogViewModel()
    {

        
        // 添加初始日志
        Logs.Add($"日志系统初始化完成 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    private void OnLogMessageReceived(string message)
    {

    }

    [RelayCommand]
    private async Task ExportLogs()
    {
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        Logs.Add($"日志已清空 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

}