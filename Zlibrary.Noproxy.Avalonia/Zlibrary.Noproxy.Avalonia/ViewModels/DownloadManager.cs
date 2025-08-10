using System;
using System.IO;
using System.Threading.Tasks;
using Zlibrary.Noproxy;
using Zlibrary.Noproxy.Avalonia.ViewModels;

namespace Zlibrary.Noproxy.Avalonia.ViewModels
{
    public class DownloadManager
    {
        private static DownloadManager? _instance;
        private static readonly object _lock = new object();
        
        private BookInfo? _selectedBook;
        private readonly IFileService _fileService;
        
        public event Action<BookInfo?>? DownloadStarted;
        public event Action<double, string>? DownloadProgressChanged;
        public event Action<string>? DownloadCompleted;
        public event Action<string>? DownloadFailed;
        
        // 通过构造函数注入IFileService
        public DownloadManager(IFileService fileService)
        {
            _fileService = fileService;
            
            // 订阅Tool的下载进度事件
            Tool.DownloadProgressChanged += OnToolDownloadProgressChanged;
        }
        
        public static DownloadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            throw new InvalidOperationException("DownloadManager尚未通过依赖注入初始化");
                        }
                    }
                }
                return _instance;
            }
        }
        
        private void OnToolDownloadProgressChanged(double progress, string message)
        {
            // 确保在UI线程上触发事件
            DownloadProgressChanged?.Invoke(progress, message);
        }
        
        public void SetSelectedBook(BookInfo book)
        {
            _selectedBook = book;
        }
        
        public BookInfo? GetSelectedBook()
        {
            return _selectedBook;
        }
        
        public async Task<bool> StartDownload()
        {
            if (_selectedBook == null)
            {
                DownloadFailed?.Invoke("未选择要下载的书籍");
                return false;
            }
                
            DownloadStarted?.Invoke(_selectedBook);
            
            try
            {
                // 触发下载进度事件
                DownloadProgressChanged?.Invoke(0, "开始下载...");
                
                // 使用Tool下载书籍并获取字节数组（支持进度报告）
                byte[]? fileData = await Tool.DownloadBookWithProgress(_selectedBook);
                if (fileData == null || fileData.Length == 0)
                {
                    DownloadFailed?.Invoke("下载失败: 未获取到文件数据");
                    return false;
                }
                
                DownloadProgressChanged?.Invoke(20, $"已接收 {fileData.Length} 字节数据");
                
                // 构造文件名
                string fileName = $"{SanitizeFileName(_selectedBook.Title)}.{_selectedBook.Extension}";
                
                // 使用文件服务保存文件
                DownloadProgressChanged?.Invoke(40, "正在保存文件...");
                string fullPath = await _fileService.CreateAndWriteBinaryFileAsync(fileName, fileData);
                DownloadProgressChanged?.Invoke(100, "文件保存完成");
                DownloadCompleted?.Invoke($"下载完成！文件已保存到: {fullPath} (大小: {fileData.Length} 字节)");
                return true;
            }
            catch (Exception ex)
            {
                DownloadFailed?.Invoke($"下载过程中发生错误: {ex.Message}");
                return false;
            }
        }

        // 打开已下载的文件
        public async Task<bool> OpenDownloadedFile(string filePath)
        {
            try
            {
                return await _fileService.OpenFileAsync(filePath);
            }
            catch
            {
                return false;
            }
        }
        
        // 清理文件名中的非法字符
        private string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "unknown_book";
                
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            
            // 确保文件名不为空
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "unknown_book";
            }
            
            return fileName.Trim();
        }
    }
}