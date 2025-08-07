using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Zlibrary.Noproxy;

class Program
{
    const string TARGET_DOMAIN = "Megumin";   // 伪装的SNI域名
    const string ACTUAL_IP = "176.123.7.105"; // 实际请求的IP
    const string ORIGIN_DOMAIN = "z-library.sk"; // 原始域名
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // 使用简单的ASCII艺术文本替代Figgle
        Console.WriteLine(@"
    _________            ______               _____   ___ __                             ______            __
   / ____/ (_)___  _____/ ____/___  __  __   /__  /  / (_) /_  _________ ________  __   /_  __/___  ____  / /
  / __/ / / / __ \/ ___/ /_  / __ \/ / / /     / /  / / / __ \/ ___/ __ `/ ___/ / / /    / / / __ \/ __ \/ / 
 / /___/ / / /_/ / /  / __/ / /_/ / /_/ /     / /__/ / / /_/ / /  / /_/ / /  / /_/ /    / / / /_/ / /_/ / /  
/_____/_/_/\____/_/  /_/    \____/\__, /     /____/_/_/_.___/_/   \__,_/_/   \__, /    /_/  \____/\____/_/   
                                 /____/                                     /____/                           
");
        
        try
        {
            while (true)
            {
                Console.WriteLine("\n===== Z-Library 图书搜索 =====");
                
                // 获取搜索关键词
                Console.Write("请输入搜索关键词（输入'exit'退出）: ");
                string keyword = SafeReadLineWithRetry();
                
                if (string.IsNullOrWhiteSpace(keyword) || keyword.ToLower() == "exit")
                {
                    Console.WriteLine("程序已退出。");
                    break;
                }
                
                int currentPage = 1;
                List<BookInfo> books = null;
                
                while (true)
                {
                    Console.WriteLine($"\n正在搜索 \"{keyword}\"，第 {currentPage} 页...");
                    
                    try
                    {
                        // 搜索书籍
                        books = await Tool.SearchBooks(keyword, currentPage);
                        
                        if (books.Count == 0)
                        {
                            Console.WriteLine("没有找到相关书籍。");
                            break;
                        }
                        
                        // 显示搜索结果
                        Console.WriteLine($"\n找到 {books.Count} 本书籍:");
                        
                        for (int i = 0; i < books.Count; i++)
                        {
                            var book = books[i];
                            Console.WriteLine($"{i + 1}. {book.Title} - {book.Author} ({book.Year}) [{book.Extension}] {book.FileSize}");
                        }
                        
                        // 用户操作菜单
                        Console.WriteLine("\n操作选项:");
                        Console.WriteLine("- 输入数字(1-{0})选择要下载的书籍", books.Count);
                        Console.WriteLine("- 输入'n'查看下一页");
                        Console.WriteLine("- 输入'p'查看上一页");
                        Console.WriteLine("- 输入'q'返回搜索");
                        
                        Console.Write("\n请选择操作: ");
                        string choice = SafeReadLineWithRetry().Trim().ToLower();
                        
                        if (choice == "q")
                        {
                            // 返回搜索
                            break;
                        }
                        else if (choice == "n")
                        {
                            // 下一页
                            currentPage++;
                        }
                        else if (choice == "p")
                        {
                            // 上一页
                            if (currentPage > 1)
                            {
                                currentPage--;
                            }
                            else
                            {
                                Console.WriteLine("已经是第一页了。");
                            }
                        }
                        else if (int.TryParse(choice, out int bookIndex) && bookIndex >= 1 && bookIndex <= books.Count)
                        {
                            // 下载选中的书籍
                            var selectedBook = books[bookIndex - 1];
                            
                            // 确认下载
                            Console.WriteLine($"\n您选择了: {selectedBook.Title} - {selectedBook.Author}");
                            Console.Write("确认下载? (y/n): ");
                            
                            if (SafeReadLineWithRetry().Trim().ToLower() == "y")
                            {
                                // 创建下载目录
                                string downloadDir = "Downloads";
                                if (!Directory.Exists(downloadDir))
                                {
                                    Directory.CreateDirectory(downloadDir);
                                }
                                
                                Console.WriteLine($"\n开始下载 \"{selectedBook.Title}\"...");
                                
                                try
                                {
                                    bool success = await Tool.DownloadBook(selectedBook, downloadDir);
                                    
                                    if (success)
                                    {
                                        Console.WriteLine($"\n下载完成！文件已保存到 {downloadDir} 目录。");
                                    }
                                    else
                                    {
                                        Console.WriteLine("\n下载失败。");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"\n下载过程中出错: {ex.Message}");
                                }
                                
                                Console.WriteLine("\n按任意键继续...");
                                SafeReadKeyWithRetry();
                            }
                        }
                        else
                        {
                            Console.WriteLine("无效的选择，请重试。");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"发生错误: {ex.Message}");
                        break;
                    }
                }
            }
        }
        catch (IOException ex) when (ex.Message.Contains("管道的另一端上无任何进程"))
        {
            // 在非交互式环境中，使用默认值进行测试
            Console.WriteLine("检测到非交互式环境，将使用默认值进行测试。");
            await RunNonInteractiveTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序发生异常: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
    
    /// <summary>
    /// 带有重试机制的安全读取一行输入
    /// </summary>
    private static string SafeReadLineWithRetry(int maxRetries = 3, string defaultValue = null)
    {
        int retries = 0;
        var inputBuilder = new System.Text.StringBuilder();
        
        while (retries <= maxRetries)
        {
            try
            {
                inputBuilder.Clear();
                ConsoleKeyInfo key;
                
                do
                {
                    key = Console.ReadKey(intercept: true);
                    
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (inputBuilder.Length > 0)
                        {
                            // 获取最后一个字符的宽度
                            var lastChar = inputBuilder[inputBuilder.Length - 1].ToString();
                            int charWidth = Console.OutputEncoding.GetByteCount(lastChar) > 1 ? 2 : 1;

                            // 删除缓冲区字符
                            inputBuilder.Remove(inputBuilder.Length - 1, 1);

                            // 退格并清除字符
                            Console.Write("\b");
                            if (charWidth == 2)
                            {
                                Console.Write("\b"); // 中文额外退格一次
                            }
                            Console.Write(" ");
                            if (charWidth == 2)
                            {
                                Console.Write(" "); // 清除中文的第二个字符位置
                            }
                            // 重新定位光标
                            for (int i = 0; i < charWidth; i++)
                            {
                                Console.Write("\b");
                            }
                        }
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        inputBuilder.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                } while (key.Key != ConsoleKey.Enter);

                string input = inputBuilder.ToString().Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    return input;
                }
                
                retries++;
                if (retries > maxRetries)
                {
                    return defaultValue;
                }
                
                Console.WriteLine("输入不能为空，请重试...");
            }
            catch (Exception ex)
            {
                retries++;
                if (retries > maxRetries)
                {
                    Console.WriteLine($"读取输入失败: {ex.Message}");
                    return defaultValue;
                }
                Console.WriteLine($"输入错误: {ex.Message}, 请重试...");
            }
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// 安全的读取一个按键，如果发生IO异常则忽略
    /// </summary>
    private static void SafeReadKey()
    {
        try
        {
            Console.ReadKey(intercept: true);
        }
        catch (IOException)
        {
            // 忽略IO异常
        }
    }
    
    /// <summary>
    /// 带有重试机制的安全读取按键
    /// </summary>
    private static void SafeReadKeyWithRetry(int maxRetries = 3)
    {
        int retries = 0;
        while (retries < maxRetries)
        {
            try
            {
                // 等待按键可用
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(50);
                }
                
                Console.ReadKey(intercept: true);
                return;
            }
            catch (IOException)
            {
                retries++;
                Thread.Sleep(200 * retries);
                
                if (retries >= maxRetries)
                {
                    Console.WriteLine("\n按键读取失败，继续执行。");
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// 在非交互式环境中运行的测试
    /// </summary>
    private static async Task RunNonInteractiveTest()
    {
        // 使用默认搜索关键词
        string defaultKeyword = "dotnet";
        Console.WriteLine($"使用默认搜索关键词: {defaultKeyword}");
        
        try
        {
            // 搜索书籍
            var books = await Tool.SearchBooks(defaultKeyword, 1);
            
            Console.WriteLine($"找到 {books.Count} 本书籍");
            
            // 显示前5本书籍
            int displayCount = Math.Min(5, books.Count);
            for (int i = 0; i < displayCount; i++)
            {
                var book = books[i];
                Console.WriteLine($"{i + 1}. {book.Title} - {book.Author} ({book.Year}) [{book.Extension}] {book.FileSize}");
            }
            
            // 如果有书籍，尝试下载第一本
            if (books.Count > 0)
            {
                var selectedBook = books[0];
                Console.WriteLine($"\n自动选择第一本书: {selectedBook.Title}");
                
                string downloadDir = "Downloads";
                if (!Directory.Exists(downloadDir))
                {
                    Directory.CreateDirectory(downloadDir);
                }
                
                Console.WriteLine($"开始下载...");
                bool success = await Tool.DownloadBook(selectedBook, downloadDir);
                
                if (success)
                {
                    Console.WriteLine($"下载完成！文件已保存到 {downloadDir} 目录。");
                }
                else
                {
                    Console.WriteLine("下载失败。");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"自动测试过程中出错: {ex.Message}");
        }
    }
}