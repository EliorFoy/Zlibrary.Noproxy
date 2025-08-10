using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Zlibrary.Noproxy;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public partial class BookSearchViewModel : ViewModelBase
    {
        private readonly DownloadManager _downloadManager;
        private readonly MainViewModel? _mainViewModel;
        
        [ObservableProperty]
        private string _searchKeyword = "";
        
        [ObservableProperty]
        private ObservableCollection<BookInfo> _searchResults = new();
        
        [ObservableProperty]
        private BookInfo? _selectedBook;
        
        [ObservableProperty]
        private bool _isSearching = false;
        
        [ObservableProperty]
        private string _searchMessage = "";
        
        [ObservableProperty]
        private int _currentPage = 1;
        
        [ObservableProperty]
        private bool _hasNextPage = false;
        
        [ObservableProperty]
        private bool _hasPreviousPage = false;
        public BookSearchViewModel(DownloadManager downloadManager, MainViewModel? mainViewModel = null)
        {
            _downloadManager = downloadManager;
            _mainViewModel = mainViewModel;
        }
        
        [RelayCommand]
        private async Task SearchBooks()
        {
            if (string.IsNullOrWhiteSpace(SearchKeyword))
            {
                SearchMessage = "请输入搜索关键词";
                return;
            }
            
            IsSearching = true;
            SearchMessage = "正在搜索...";
            
            try
            {
                var books = await Tool.SearchBooks(SearchKeyword, CurrentPage);
                SearchResults = new ObservableCollection<BookInfo>(books);
                HasNextPage = books.Count > 0; // 简化处理，实际应该根据总结果数判断
                HasPreviousPage = CurrentPage > 1;
                SearchMessage = $"找到 {books.Count} 本书籍";
            }
            catch (Exception ex)
            {
                SearchMessage = $"搜索失败: {ex.Message}";
            }
            finally
            {
                IsSearching = false;
            }
        }
        
        [RelayCommand]
        private async Task NextPage()
        {
            CurrentPage++;
            await SearchBooksCommand.ExecuteAsync(null);
        }
        
        [RelayCommand]
        private async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await SearchBooksCommand.ExecuteAsync(null);
            }
        }
        
        [RelayCommand]
        private async Task RefreshSearch()
        {
            CurrentPage = 1;
            await SearchBooksCommand.ExecuteAsync(null);
        }
        
        [RelayCommand]
        private async Task DownloadBook(BookInfo book)
        {
            // 设置选中的书籍到DownloadManager
            _downloadManager.SetSelectedBook(book);
            
            // 如果有主视图模型，切换到下载页面
            if (_mainViewModel != null)
            {
                _mainViewModel.SelectedViewModel = _mainViewModel.BookDownloadViewModel;
            }
            
            // 启动下载
            await _downloadManager.StartDownload();
        }
        
        // 清理文件名中的非法字符
        private string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "unknown_book";
                
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            
            // 确保文件名不为空
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "unknown_book";
            }
            
            return fileName.Trim();
        }
    }
}