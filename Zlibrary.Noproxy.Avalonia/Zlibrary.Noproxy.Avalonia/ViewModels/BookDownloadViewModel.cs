using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Zlibrary.Noproxy;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public partial class BookDownloadViewModel : ViewModelBase
    {
        private readonly DownloadManager _downloadManager;
        
        [ObservableProperty]
        private BookInfo? _selectedBook;
        
        [ObservableProperty]
        private bool _isDownloading = false;
        
        [ObservableProperty]
        private double _downloadProgress;
        
        [ObservableProperty]
        private string _downloadMessage = "";
        
        [ObservableProperty]
        private string _downloadedFilePath = "";
        
        [ObservableProperty]
        private string _downloadPath = "";
        
        public BookDownloadViewModel(DownloadManager downloadManager)
        {
            _downloadManager = downloadManager;
            
            // 订阅下载管理器的事件
            _downloadManager.DownloadStarted += OnDownloadStarted;
            _downloadManager.DownloadProgressChanged += OnDownloadProgressChanged;
            _downloadManager.DownloadCompleted += OnDownloadCompleted;
            _downloadManager.DownloadFailed += OnDownloadFailed;
            
            // 设置当前选中的书籍
            RefreshSelectedBook();
            
            // 设置下载路径
            DownloadPath = "使用系统默认下载位置";
        }
        
        // 刷新选中的书籍信息
        public void RefreshSelectedBook()
        {
            SelectedBook = _downloadManager.GetSelectedBook();
        }
        
        private void OnDownloadStarted(BookInfo? book)
        {
            SelectedBook = book;
            IsDownloading = true;
            DownloadProgress = 0;
            DownloadMessage = book != null ? $"正在下载 {book.Title}..." : "正在下载...";
        }
        
        private void OnDownloadProgressChanged(double progress, string message)
        {
            DownloadProgress = progress;
            DownloadMessage = message;
        }
        
        private void OnDownloadCompleted(string message)
        {
            // 从下载完成消息中提取文件路径
            if (message.StartsWith("下载完成！文件已保存到: "))
            {
                int prefixLength = "下载完成！文件已保存到: ".Length;
                int suffixStart = message.IndexOf(" (大小:");
                if (suffixStart > prefixLength)
                {
                    DownloadedFilePath = message.Substring(prefixLength, suffixStart - prefixLength);
                    // 确保路径中没有多余空格
                    DownloadedFilePath = DownloadedFilePath.Trim();
                }
            }
            
            DownloadMessage = message;
            IsDownloading = false;
        }
        
        private void OnDownloadFailed(string message)
        {
            DownloadMessage = message;
            IsDownloading = false;
        }
        
        [RelayCommand]
        private async Task StartDownload()
        {
            if (SelectedBook == null)
            {
                DownloadMessage = "未选择要下载的书籍";
                return;
            }
            
            // 启动下载
            await _downloadManager.StartDownload();
        }
        
        [RelayCommand]
        private async Task OpenDownloadedFile()
        {
        
            try
            {
                // 通过DownloadManager处理文件打开操作
                await _downloadManager.OpenDownloadedFile(DownloadedFilePath);
            }
            catch (Exception ex)
            {
                DownloadMessage = $"打开文件时出错: {ex.Message}";
            }
        }
    }
}