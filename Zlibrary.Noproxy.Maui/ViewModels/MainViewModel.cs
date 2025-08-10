using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Maui.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private BookSearchViewModel? _bookSearchViewModel;
        private BookDownloadViewModel? _downloadViewModel;
        private LogViewModel? _logViewModel;

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

        public BookSearchViewModel BookSearchViewModel
        {
            get => _bookSearchViewModel ??= new BookSearchViewModel(DownloadManager.Instance, this);
        }

        public BookDownloadViewModel BookDownloadViewModel
        {
            get => _downloadViewModel ??= new BookDownloadViewModel(); // 修复：属性名应与View匹配
        }

        public LogViewModel LogViewModel
        {
            get => _logViewModel ??= new LogViewModel();
        }

        public MainViewModel()
        {
            SelectedViewModel = BookSearchViewModel;
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
                    SelectedViewModel = BookSearchViewModel;
                    break;
                case "Contacts":
                    CurrentPage = "下载页面";
                    IsContactsSelected = true;
                    SelectedViewModel = BookDownloadViewModel; // 修复：使用正确的属性名
                    break;
                case "Discover":
                    CurrentPage = "日志页面";
                    IsDiscoverSelected = true;
                    SelectedViewModel = LogViewModel;
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
}