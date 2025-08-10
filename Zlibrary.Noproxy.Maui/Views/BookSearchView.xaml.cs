using Microsoft.Maui.Controls;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Maui.Views
{
    public partial class BookSearchView : ContentPage
    {
        public BookSearchView()
        {
            InitializeComponent();
        }

        private void SearchBox_KeyDown(object sender, Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.EntryEventArgs e)
        {
            // 在MAUI中，我们使用不同的事件处理方式
            // 这里可以添加回车键搜索功能
        }
    }
}