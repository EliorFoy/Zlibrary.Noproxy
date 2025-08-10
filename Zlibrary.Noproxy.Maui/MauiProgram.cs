using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // 配置服务
            ConfigureServices(builder.Services);

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // 注册DownloadManager为单例，让DI容器自动管理
            services.AddSingleton<DownloadManager>();

            // 只注册MainViewModel，其他ViewModel通过MainViewModel属性访问
            services.AddSingleton<MainViewModel>();
        }
    }
}