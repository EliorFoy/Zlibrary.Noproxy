using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Zlibrary.Noproxy.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Zlibrary.Noproxy.Avalonia.Android
{
    [Activity(
        Label = "Zlibrary下载器",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/zlibIcon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App>
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder)
                .WithInterFont();
            //                 .AfterPlatformServicesSetup(_ =>
            // {
            //     App.Services.AddSingleton<IFileService, FileServiceAndroid>();
            //     App.Services.AddTransient<MainViewModel>();
            // });
        }
    }
}
