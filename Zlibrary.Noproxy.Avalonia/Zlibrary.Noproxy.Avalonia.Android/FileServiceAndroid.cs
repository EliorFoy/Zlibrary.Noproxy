using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.Android
{
    public class FileServiceAndroid : IFileService
    {
        /// <summary>
        /// 创建并写入文本文件（外部存储）。
        /// </summary>
        public async Task<string> CreateAndWriteTextFileAsync(string fileName, string content)
        {
            System.Console.WriteLine("正在创建并写入文件：" + fileName);
            return await Task.Run(() =>
            {
                // 获取Android应用的外部文件目录
                var externalFilesDirectory = Application.Context.GetExternalFilesDir(null)?.AbsolutePath
                    ?? Application.Context.FilesDir.AbsolutePath; // 如果外部存储不可用，则使用内部存储
                var filePath = Path.Combine(externalFilesDirectory, fileName);

                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // 写入文件
                System.IO.File.WriteAllText(filePath, content);

                return filePath;
            });
        }

        /// <summary>
        /// 创建并写入二进制文件（外部存储）。
        /// </summary>
        public async Task<string> CreateAndWriteBinaryFileAsync(string fileName, byte[] data)
        {
            System.Console.WriteLine("正在创建并写入文件：" + fileName);
            return await Task.Run(() =>
            {
                // 获取Android应用的外部文件目录
                var externalFilesDirectory = Application.Context.GetExternalFilesDir(null)?.AbsolutePath
                    ?? Application.Context.FilesDir.AbsolutePath; // 如果外部存储不可用，则使用内部存储
                var filePath = Path.Combine(externalFilesDirectory, fileName);

                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // 写入二进制数据
                System.IO.File.WriteAllBytes(filePath, data);

                return filePath;
            });
        }

        /// <summary>
        /// 使用系统应用打开指定文件（text/plain）。
        /// </summary>
        public async Task<bool> OpenFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // 检查文件是否存在
                    if (!System.IO.File.Exists(filePath))
                        return false;

                    // 获取当前上下文
                    var context = Application.Context;

                    // 创建打开文件的Intent
                    var intent = new Intent(Intent.ActionView);
                    
                    // 使用FileProvider来安全地共享文件
                    var javaFile = new Java.IO.File(filePath);
                    var uri = FileProvider.GetUriForFile(context, $"{context.PackageName}.fileprovider", javaFile);

                    // 根据文件扩展名确定 MIME 类型
                    var mimeType = GetMimeType(filePath);
                    intent.SetDataAndType(uri, mimeType);
                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    intent.AddFlags(ActivityFlags.NewTask);

                    // 检查是否有应用可以处理这个Intent
                    if (intent.ResolveActivity(context.PackageManager) != null)
                    {
                        context.StartActivity(intent);
                        return true;
                    }
                    
                    return false;
                }
                catch (Java.Lang.IllegalArgumentException)
                {
                    // 如果FileProvider配置有问题，尝试直接打开文件（仅适用于外部存储）
                    try
                    {
                        var context = Application.Context;
                        var intent = new Intent(Intent.ActionView);
                        
                        var javaFile = new Java.IO.File(filePath);
                        var uri = global::Android.Net.Uri.FromFile(javaFile);
                        
                        // 根据文件扩展名确定 MIME 类型
                        var mimeType = GetMimeType(filePath);
                        intent.SetDataAndType(uri, mimeType);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.AddFlags(ActivityFlags.NewTask);
                        
                        context.StartActivity(intent);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            });
        }
        
        private string GetMimeType(string filePath)
        {
            System.Console.WriteLine("正在获取文件 MIME 类型：" + filePath);
            var extension = System.IO.Path.GetExtension(filePath)?.ToLower();
                
            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".epub" => "application/epub+zip",
                ".mobi" => "application/x-mobipocket-ebook",
                ".azw3" => "application/vnd.amazon.ebook",
                ".djvu" => "image/vnd.djvu",
                ".fb2" => "text/xml",
                ".fb2.zip" => "application/zip",
                ".chm" => "application/vnd.ms-htmlhelp",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".rtf" => "application/rtf",
                _ => "application/octet-stream" // 默认 MIME 类型
            };
        }
    }
}