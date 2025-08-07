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
            // 查找父级控件中的MainWindow
            var mainWindow = this.FindAncestorOfType<MainWindow>();
            if (mainWindow != null)
            {
                var bookDownloadViewModel = mainWindow.GetBookDownloadViewModel();
                if (bookDownloadViewModel != null)
                {
                    bookDownloadViewModel.SetSelectedBook(book);
                    // 切换到下载页面
                    mainWindow.SwitchToDownloadPage();
                }
            }
        }
    }
}