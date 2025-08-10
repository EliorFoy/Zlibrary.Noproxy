using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Zlibrary.Noproxy;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.Views;

public partial class BookSearchView : UserControl
{
    public BookSearchView()
    {
        InitializeComponent();
        // DataContext = new BookSearchViewModel(); // 显式设置 DataContext
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is BookSearchViewModel viewModel)
            {
                viewModel.SearchBooksCommand.Execute(null);
            }
        }
    }

    private void DownloadBook_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is BookInfo book)
        {
            // 查找父级控件中的MainViewModel
            var mainView = this.FindAncestorOfType<MainView>();
            var mainWindow = this.FindAncestorOfType<MainWindow>();
            
            MainViewModel? mainViewModel = null;
            if (mainView?.DataContext is MainViewModel mv)
            {
                mainViewModel = mv;
            }
            else if (mainWindow?.DataContext is MainViewModel mw)
            {
                mainViewModel = mw;
            }
            
            // 通过BookSearchViewModel处理下载，而不是直接调用BookDownloadViewModel的SetSelectedBook方法
            if (DataContext is BookSearchViewModel searchViewModel)
            {
                searchViewModel.DownloadBookCommand.Execute(book);
            }
        }
    }
}