using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zlibrary.Noproxy;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public partial class BookSearchViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _searchKeyword = "";

        [ObservableProperty]
        private bool _isSearching = false;

        [ObservableProperty]
        private string _searchMessage = "";

        [ObservableProperty]
        private List<BookInfo> _searchResults = new();

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage = false;

        [ObservableProperty]
        private bool _hasPreviousPage = false;
        
        [ObservableProperty]
        private BookInfo? _selectedBook;

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
                SearchResults = books;
                HasNextPage = books.Count > 0;
                HasPreviousPage = CurrentPage > 1;
                SearchMessage = books.Count > 0 ? $"找到 {books.Count} 本书籍" : "未找到相关书籍";
            }
            catch (Exception ex)
            {
                SearchMessage = $"搜索出错: {ex.Message}";
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
    }
}