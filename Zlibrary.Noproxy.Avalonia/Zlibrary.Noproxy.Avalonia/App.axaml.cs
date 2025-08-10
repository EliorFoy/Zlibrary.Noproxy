using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Zlibrary.Noproxy.Avalonia.Services;
using Zlibrary.Noproxy.Avalonia.ViewModels;
using Zlibrary.Noproxy.Avalonia.Views;

namespace Zlibrary.Noproxy.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public static ServiceCollection Services { get; private set; } = new ServiceCollection();
    public override void OnFrameworkInitializationCompleted()
    {
        ConfigureServices(Services);
        var serviceProvider = Services.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // 注册文件服务
        // 根据运行时平台选择适当的文件服务实现
        if (IsAndroid())
        {
            System.Console.WriteLine("正在使用Android平台");
            // Android平台使用特定的文件服务实现
            try
            {
                // 尝试加载Android特定的文件服务
                var androidServiceType = Type.GetType("Zlibrary.Noproxy.Avalonia.Android.FileServiceAndroid, Zlibrary.Noproxy.Avalonia.Android");
                if (androidServiceType != null)
                {
                    services.AddSingleton(typeof(IFileService), androidServiceType);
                }
                else
                {
                    System.Console.WriteLine("找不到Android特定实现，回退到默认实现");
                    // 如果找不到Android特定实现，回退到默认实现
                    services.AddSingleton<IFileService, DefaultFileService>();
                }
            }
            catch
            {
                // 出现异常时回退到默认实现
                services.AddSingleton<IFileService, DefaultFileService>();
            }
        }
        else
        {
            // 其他平台使用默认文件服务实现
            services.AddSingleton<IFileService, DefaultFileService>();
        }

        // 注册DownloadManager为单例
        services.AddSingleton<DownloadManager>();

        // 注册MainViewModel
        services.AddSingleton<MainViewModel>();
    }
    
    private bool IsAndroid()
    {
        // 检测是否在Android平台上运行
        try
        {
            var runtimeInfoType = Type.GetType("Avalonia.Platform.RuntimePlatformInfo, Avalonia");
            if (runtimeInfoType != null)
            {
                var isAndroidProperty = runtimeInfoType.GetProperty("IsAndroid");
                if (isAndroidProperty != null)
                {
                    var isAndroid = isAndroidProperty.GetValue(null);
                    return isAndroid as bool? == true;
                }
            }
        }
        catch
        {
            // 忽略异常，返回false
        }
        
        // 简单的回退方法：检查是否存在Android特定的程序集
        try
        {
            return Type.GetType("Android.App.Application, Mono.Android") != null;
        }
        catch
        {
            return false;
        }
    }
}