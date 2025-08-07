using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _currentPage = "图书搜索";

        [ObservableProperty]
        private bool _isChatSelected = true;

        [ObservableProperty]
        private bool _isContactsSelected = false;

        [ObservableProperty]
        private bool _isDiscoverSelected = false;

        [ObservableProperty]
        private bool _isMomentsSelected = false;

        [ObservableProperty]
        private bool _isProfileSelected = false;

        // 添加用于控制窗口功能的属性
        [ObservableProperty]
        private bool _isTitleBarVisible = true;

        [ObservableProperty]
        private bool _windowLocked = false;

        [ObservableProperty]
        private bool _animationsEnabled = true;

        [ObservableProperty]
        private bool _transitionsEnabled = true;

        // 添加子视图模型
        public BookSearchViewModel BookSearchViewModel { get; } = new BookSearchViewModel();
        public BookDownloadViewModel BookDownloadViewModel { get; } = new BookDownloadViewModel();
        public LogViewModel LogViewModel { get; } = new LogViewModel();

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
                    break;
                case "Contacts":
                    CurrentPage = "联系人页面";
                    IsContactsSelected = true;
                    break;
                case "Discover":
                    CurrentPage = "发现页面";
                    IsDiscoverSelected = true;
                    break;
                case "Moments":
                    CurrentPage = "朋友圈页面";
                    IsMomentsSelected = true;
                    break;
                case "Profile":
                    CurrentPage = "我的页面";
                    IsProfileSelected = true;
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