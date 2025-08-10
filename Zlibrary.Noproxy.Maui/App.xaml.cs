using Microsoft.Extensions.DependencyInjection;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // 获取服务提供者
            var serviceProvider = MauiProgram.CreateMauiApp().Services;
            
            // 创建主页面并设置DataContext
            var mainPage = new AppShell();
            mainPage.BindingContext = serviceProvider.GetRequiredService<MainViewModel>();
            
            return new Window(new AppShell());
        }
    }
}