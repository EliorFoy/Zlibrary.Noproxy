using System;
using System.IO;
using System.Threading.Tasks;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.Services
{
    /// <summary>
    /// 默认文件服务实现，用于非Android平台
    /// </summary>
    public class DefaultFileService : IFileService
    {
        /// <summary>
        /// 创建并写入文本文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="content">文件内容</param>
        /// <returns>文件完整路径</returns>
        public async Task<string> CreateAndWriteTextFileAsync(string fileName, string content)
        {
            // 确保使用合适的下载目录
            string downloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "Downloads");
            
            // 确保目录存在
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            
            string filePath = Path.Combine(downloadPath, fileName);
            
            // 写入文件内容
            await File.WriteAllTextAsync(filePath, content);
            
            return filePath;
        }

        /// <summary>
        /// 创建并写入二进制文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="data">文件数据</param>
        /// <returns>文件完整路径</returns>
        public async Task<string> CreateAndWriteBinaryFileAsync(string fileName, byte[] data)
        {
            // 确保使用合适的下载目录
            string downloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Downloads");

            // 确保目录存在
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            string filePath = Path.Combine(downloadPath, fileName);

            // 写入二进制数据
            await File.WriteAllBytesAsync(filePath, data);

            return filePath;
        }

        /// <summary>
        /// 打开指定文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功打开文件</returns>
        public async Task<bool> OpenFileAsync(string filePath)
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(filePath))
                    return false;

                // 使用 Avalonia 的 Launcher API (需要引用 Avalonia.Desktop)
                // 这种方式更加可靠和现代化
                var uri = new Uri(filePath);
                // 注意：这需要根据实际的 Avalonia 版本和可用 API 调整
                // Avalonia.Platform.PlatformManager?.Launcher?.LaunchUri(uri);

                // 或者使用更简单的方式
                if (OperatingSystem.IsWindows())
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start \"\" \"{filePath}\"")
                    {
                        CreateNoWindow = true
                    });
                }
                else if (OperatingSystem.IsMacOS())
                {
                    System.Diagnostics.Process.Start("open", $"\"{filePath}\"");
                }
                else if (OperatingSystem.IsLinux())
                {
                    System.Diagnostics.Process.Start("xdg-open", $"\"{filePath}\"");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}