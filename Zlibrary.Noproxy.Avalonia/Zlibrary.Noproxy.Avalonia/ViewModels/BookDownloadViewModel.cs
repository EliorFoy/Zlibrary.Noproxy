using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zlibrary.Noproxy;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public partial class BookDownloadViewModel : ViewModelBase
    {
        [ObservableProperty]
        private BookInfo? _selectedBook;

        [ObservableProperty]
        private bool _isDownloading = false;

        [ObservableProperty]
        private string _downloadMessage = "";

        [ObservableProperty]
        private double _downloadProgress = 0;

        [ObservableProperty]
        private string _downloadPath = Path.Combine(Environment.CurrentDirectory, "Downloads");

        public BookDownloadViewModel()
        {
            // 确保下载目录存在
            if (!Directory.Exists(DownloadPath))
            {
                Directory.CreateDirectory(DownloadPath);
            }
            
            // 订阅下载进度事件
            Tool.DownloadProgressChanged += OnDownloadProgressChanged;
        }
        
        private void OnDownloadProgressChanged(double progress, string message)
        {
            DownloadProgress = progress;
            DownloadMessage = message;
        }

        [RelayCommand]
        private async Task DownloadSelectedBook()
        {
            if (SelectedBook == null)
            {
                DownloadMessage = "请选择要下载的书籍";
                return;
            }

            if (!Directory.Exists(DownloadPath))
            {
                try
                {
                    Directory.CreateDirectory(DownloadPath);
                }
                catch (Exception ex)
                {
                    DownloadMessage = $"创建下载目录失败: {ex.Message}";
                    return;
                }
            }

            IsDownloading = true;
            DownloadProgress = 0;
            DownloadMessage = $"正在下载 {SelectedBook.Title}...";

            try
            {
                // 使用Tool类的GUI下载功能
                bool success = await Tool.DownloadBookForGUI(SelectedBook, DownloadPath);
                
                if (success)
                {
                    DownloadMessage = $"下载完成！文件已保存到: {DownloadPath}";
                }
                else
                {
                    DownloadMessage = "下载失败";
                }
            }
            catch (Exception ex)
            {
                DownloadMessage = $"下载出错: {ex.Message}";
            }
            finally
            {
                IsDownloading = false;
            }
        }

        public void SetSelectedBook(BookInfo book)
        {
            SelectedBook = book ?? throw new ArgumentNullException(nameof(book));
        }
    }
}