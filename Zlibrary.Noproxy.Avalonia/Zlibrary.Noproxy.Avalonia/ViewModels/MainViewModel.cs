using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zlibrary.Noproxy;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly DownloadManager _downloadManager;
    private readonly IFileService _fileService;
    
    private readonly BookSearchViewModel _bookSearchViewModel;
    private readonly BookDownloadViewModel _downloadViewModel;
    private readonly LogViewModel _logViewModel;

    [ObservableProperty]
    private string _currentPage = "图书搜索";

    [ObservableProperty]
    private bool _isTitleBarVisible = true;

    [ObservableProperty]
    private bool _windowLocked = false;

    [ObservableProperty]
    private bool _animationsEnabled = true;

    [ObservableProperty]
    private bool _transitionsEnabled = true;

    [ObservableProperty]
    private ViewModelBase? _selectedViewModel;

    // 底部导航选中状态
    [ObservableProperty]
    private bool _isChatSelected = true; // 搜索页面

    [ObservableProperty]
    private bool _isContactsSelected = false; // 下载页面

    [ObservableProperty]
    private bool _isDiscoverSelected = false; // 日志页面

    [ObservableProperty]
    private bool _isMomentsSelected = false;

    [ObservableProperty]
    private bool _isProfileSelected = false;

    public BookSearchViewModel BookSearchViewModel => _bookSearchViewModel;

    public BookDownloadViewModel BookDownloadViewModel => _downloadViewModel;

    public LogViewModel LogViewModel => _logViewModel;

    public MainViewModel(DownloadManager downloadManager, IFileService fileService)
    {
        _downloadManager = downloadManager;
        _fileService = fileService;
        
        _bookSearchViewModel = new BookSearchViewModel(_downloadManager, this);
        _downloadViewModel = new BookDownloadViewModel(_downloadManager); // 不再传递fileService
        _logViewModel = new LogViewModel();
        
        SelectedViewModel = BookSearchViewModel;
    }

    public void AddLog(string message)
    {
    }
    
    [RelayCommand]
    private void Navigate(string page)
    {
        // 重置所有选择状态
        IsChatSelected = false;
        IsContactsSelected = false;
        IsDiscoverSelected = false;
        IsMomentsSelected = false;
        IsProfileSelected = false;

        // 设置当前页面和选择状态
        switch (page)
        {
            case "Chat":
                CurrentPage = "图书搜索";
                IsChatSelected = true;
                SelectedViewModel = _bookSearchViewModel;
                break;
            case "Contacts":
                CurrentPage = "下载页面";
                IsContactsSelected = true;
                // 刷新下载页面的书籍信息
                _downloadViewModel.RefreshSelectedBook();
                SelectedViewModel = _downloadViewModel;
                break;
            case "Discover":
                CurrentPage = "日志页面";
                IsDiscoverSelected = true;
                SelectedViewModel = _logViewModel;
                break;
        }
    }

    [RelayCommand]
    private void ToggleTitleBar()
    {
        IsTitleBarVisible = !IsTitleBarVisible;
    }

    [RelayCommand]
    private void ToggleWindowLock()
    {
        WindowLocked = !WindowLocked;
    }

    [RelayCommand]
    private void ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
    }

    [RelayCommand]
    private void ToggleTransitions()
    {
        TransitionsEnabled = !TransitionsEnabled;
    }
}