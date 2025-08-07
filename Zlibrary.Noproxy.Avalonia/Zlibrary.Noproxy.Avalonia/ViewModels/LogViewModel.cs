using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public partial class LogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<string> _logMessages = new();

        [ObservableProperty]
        private string _newLogMessage = "";

        public LogViewModel()
        {
            // 初始化一些示例日志
            LogMessages.Add($"应用启动: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogMessages.Add("ZLibrary.Noproxy GUI 版本已初始化");
        }

        public void AddLogMessage(string message)
        {
            LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public void ClearLogs()
        {
            LogMessages.Clear();
        }
    }
}