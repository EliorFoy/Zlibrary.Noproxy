using SukiUI.Controls;
using Avalonia.Interactivity;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.Views
{
    public partial class MainWindow : SukiWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public BookDownloadViewModel? GetBookDownloadViewModel()
        {
            // 通过SideMenu访问BookDownloadView并获取其DataContext
            if (SideMenu.Items != null)
            {
                foreach (var item in SideMenu.Items)
                {
                    if (item is SukiSideMenuItem sideMenuItem && sideMenuItem.Header?.ToString() == "下载")
                    {
                        if (sideMenuItem.PageContent is BookDownloadView bookDownloadView)
                        {
                            return bookDownloadView.DataContext as BookDownloadViewModel;
                        }
                    }
                }
            }
            
            return null;
        }

        public void SwitchToDownloadPage()
        {
            // 切换到下载页面
            if (SideMenu.Items != null)
            {
                foreach (var item in SideMenu.Items)
                {
                    if (item is SukiSideMenuItem sideMenuItem && sideMenuItem.Header?.ToString() == "下载")
                    {
                        SideMenu.SelectedItem = sideMenuItem;
                        break;
                    }
                }
            }
        }

        private void ThemeMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            // 主题切换逻辑
        }

        private void BackgroundMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            // 背景切换逻辑
        }
    }
}