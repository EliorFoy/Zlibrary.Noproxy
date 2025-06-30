using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Linq;
using System.IO;

namespace Zlibrary.Noproxy
{
    // 添加一个书籍信息的类
    public class BookInfo
    {
        public string Id { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string DownloadUrl { get; set; }
        public string DetailUrl { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }
        public string Year { get; set; }
        public string Extension { get; set; }
        public string FileSize { get; set; }
        public string Rating { get; set; }
        public string Quality { get; set; }
        public string ImageUrl { get; set; }

        public override string ToString()
        {
            return $"书名: {Title}\n作者: {Author}\nID: {Id}\nISBN: {Isbn}\n下载链接: {DownloadUrl}\n详情链接: {DetailUrl}\n出版社: {Publisher}\n语言: {Language}\n年份: {Year}\n格式: {Extension}\n文件大小: {FileSize}\n评分: {Rating}\n质量: {Quality}\n封面: {ImageUrl}";
        }
    }
    
    public static class Tool
    {
        private static string ORIGIN_DOMAIN = "z-library.sk";
        private static string TARGET_DOMAIN = "Megumin";
        private static string ACTUAL_IP = "176.123.7.105"; // 需要设置实际IP地址
        
        private static readonly SocketsHttpHandler _handler = new SocketsHttpHandler
        {
            // 禁用证书验证
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true,
                TargetHost = TARGET_DOMAIN
            },
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(IPAddress.Parse(ACTUAL_IP), context.DnsEndPoint.Port, cancellationToken);
                var stream = new NetworkStream(socket, ownsSocket: true);

                // 创建SSL流并手动认证
                var sslStream = new SslStream(stream, false,
                    (_, _, _, _) => true); // 证书验证回调

                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = TARGET_DOMAIN,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, cancellationToken);

                return sslStream;
            }
        };

        // 静态HttpClient实例
        private static readonly HttpClient _client;

        // 静态构造函数，初始化HttpClient
        static Tool()
        {
            _client = new HttpClient(_handler);
            SetDefaultClient(_client);
            
            // 异步初始化IP地址
            InitializeIpAddressAsync().ConfigureAwait(false);
        }
        
        // 异步初始化IP地址
        private static async Task InitializeIpAddressAsync()
        {
            try
            {
                string? ip = await GetIpAddress();
                if (!string.IsNullOrEmpty(ip))
                {
                    ACTUAL_IP = ip;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化IP地址失败: {ex.Message}");
            }
        }
    
        private static HttpClient SetDefaultClient(HttpClient client){
            // 清除可能存在的旧请求头
            client.DefaultRequestHeaders.Clear();
            
            client.DefaultRequestHeaders.Add("Host", ORIGIN_DOMAIN);
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("Referer", "https://z-library.sk/s/maui?page=2");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            client.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        /// <summary>
        /// 查询指定域名的IP地址
        /// </summary>
        /// <param name="hostname">要查询的域名，默认为z-library.sk</param>
        /// <param name="useCookie">是否使用cookie</param>
        /// <returns>返回查询到的IP地址，失败则返回null</returns>
        public static async Task<string?> GetIpAddress(string hostname = "z-library.sk", bool useCookie = false)
        {
            // 创建HttpClientHandler并配置自动解压缩
            using var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.All
            };

            // 创建HttpClient
            using var client = new HttpClient(handler);

            // 设置请求头
            client.DefaultRequestHeaders.Add("authority", "www.diggui.com");
            client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36 Edg/137.0.0.0");

            // 添加可选的cookie
            if (useCookie)
            {
                client.DefaultRequestHeaders.Add("cookie", "cf_clearance=mEJJoBQj2mWAURdxNMr_z4giz6wwer_F6b8WlmbeIYo-1751273671-1.2.1.1-w6koXTt7Cclad3u3bpUp7oLTbqtSnb.CGj7duBA9S1ryfKlwE_NTntgvMEme9dgFgN7O79ueA6S0UeR9yhGLaI_rHy2oUxozWHrn7LRwzCAyVwB1V_yttJTb_WYzJ6Q0ua7FSV7dnLHoUcx6vNwZ99o.YFntAFz2eGUshK2kdcyd40GrY.ye.I.yXc2XvC7MyWPvoKg7e8SYuDodGuozDBZeQRMBpJk9R30l62q2WTxTcD_G3c1n3dc7umUViuQkpa54nbSTquE8XmYgm7nj86bQcGBgcjD8IjssC6ORKkwS.oF4sZjBCVlm8zNVWGEmipuuyC1mv4ONuLG.SuS8Asx2dngIoVl8mBH8A4j7XWk; __gads=ID=11b3fd4a5b7cb36d:T=1751105188:RT=1751273670:S=ALNI_Mby0-rj2zL0063Ab7KIJ6Vh1Hl-JA; __gpi=UID=0000113b536fc881:T=1751105188:RT=1751273670:S=ALNI_MaFWyj-b9YtxzUUyMqRvKWj2tODyA; __eoi=ID=22229afbcd6ba925:T=1751105188:RT=1751273670:S=AA-AfjZS73u02cj0nkAS-uQHRfew; _ga=GA1.2.405987918.1751105188; _gid=GA1.2.2007293896.1751273703; _gat_gtag_UA_6388236_14=1; _ga_EKW4LY120R=GS2.1.s1751273669$o2$g1$t1751273705$j24$l0$h0");
            }

            try
            {
                // 准备POST数据
                var formData = new Dictionary<string, string>
                {
                    {"type", "A"},
                    {"hostname", hostname},
                    {"nameserver", "public"},
                    {"public", "8.8.8.8"},
                    {"specify", ""},
                    {"clientsubnet", ""},
                    {"tcp", "def"},
                    {"transport", "def"},
                    {"mapped", "def"},
                    {"nssearch", "def"},
                    {"trace", "def"},
                    {"recurse", "def"},
                    {"edns", "def"},
                    {"dnssec", "def"},
                    {"subnet", "def"},
                    {"cookie", "def"},
                    {"all", "def"},
                    {"cmd", "def"},
                    {"question", "def"},
                    {"answer", "def"},
                    {"authority", "def"},
                    {"additional", "def"},
                    {"comments", "def"},
                    {"stats", "def"},
                    {"multiline", "def"},
                    {"short", "def"},
                    {"colorize", "on"}
                };

                // 创建表单内容
                using var content = new FormUrlEncodedContent(formData);

                // 发送POST请求
                using var response = await client.PostAsync("https://www.diggui.com/", content);

                // 处理响应
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    
                    // 提取IP地址
                    string? ipAddress = ExtractIpAddressFromResponse(responseContent, hostname);
                    
                    if (ipAddress != null)
                    {
                        Console.WriteLine($"[{hostname}] IP地址: {ipAddress}");
                        return ipAddress;
                    }
                    else
                    {
                        Console.WriteLine($"未能从响应中提取[{hostname}]的IP地址");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"请求失败: {response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"请求错误: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从响应内容中提取IP地址
        /// </summary>
        /// <param name="responseContent">响应内容</param>
        /// <param name="hostname">域名</param>
        /// <returns>提取的IP地址</returns>
        private static string? ExtractIpAddressFromResponse(string responseContent, string hostname)
        {
            try
            {
                // 替换域名中的点，防止正则表达式问题
                string escapedHostname = hostname.Replace(".", "\\.");
                
                // 正则表达式匹配ANSWER SECTION中的IP地址
                string pattern = $@"{escapedHostname}\.</a>\s+<span[^>]*>\d+</span>\s+<span[^>]*>IN</span>\s+<a[^>]*>A</a>\s+<a[^>]*>([0-9]{{1,3}}\.[0-9]{{1,3}}\.[0-9]{{1,3}}\.[0-9]{{1,3}})</a>";
                
                // 执行正则匹配
                Match match = Regex.Match(responseContent, pattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    // 返回捕获的IP地址
                    return match.Groups[1].Value;
                }
                
                Console.WriteLine($"未找到匹配的IP地址，域名: {hostname}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"提取IP地址时出错: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> Get(string url)
        {
            url = url.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
            // 使用静态HttpClient实例
            using var response = await _client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> Post(string url, Dictionary<string, string> formData)
        {
            url = url.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
            // 使用静态HttpClient实例
            using var content = new FormUrlEncodedContent(formData);
            using var response = await _client.PostAsync(url, content);
            
            // 检查响应是否成功
            response.EnsureSuccessStatusCode();
            
            // 如果需要返回格式化的JSON
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                using var jsonDoc = JsonDocument.Parse(responseBytes);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                return JsonSerializer.Serialize(jsonDoc, options);
            }
            else
            {
                // 对于非JSON响应，直接返回字符串
                return await response.Content.ReadAsStringAsync();
            }
        }
        
        // 添加一个新的方法，专门用于发送JSON数据
        public static async Task<string> PostJson(string url, object data)
        {
            url = url.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
            
            // 序列化对象为JSON
            var jsonContent = JsonSerializer.Serialize(data);
            Console.WriteLine($"发送的JSON数据: {jsonContent}");
            
            // 创建StringContent，指定内容类型为application/json
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            // 发送请求
            using var response = await _client.PostAsync(url, content);
            
            // 获取响应内容
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // 如果响应不成功，记录错误信息但不抛出异常
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"请求失败: {response.StatusCode}");
                Console.WriteLine($"响应内容: {responseContent}");
                return responseContent;
            }
            
            // 如果是JSON响应，格式化后返回
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    return JsonSerializer.Serialize(jsonDoc, options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"JSON解析错误: {ex.Message}");
                    return responseContent;
                }
            }
            
            return responseContent;
        }

        /// <summary>
        /// 从HTML中解析书籍信息
        /// </summary>
        /// <param name="html">包含书籍信息的HTML字符串</param>
        /// <returns>解析出的书籍信息列表</returns>
        public static List<BookInfo> ParseBooksFromHtml(string html)
        {
            var books = new List<BookInfo>();
            
            // 使用正则表达式匹配每个z-bookcard元素
            var bookCardPattern = @"<z-bookcard\s+(?<attributes>.*?)>(?<content>.*?)</z-bookcard>";
            var bookMatches = Regex.Matches(html, bookCardPattern, RegexOptions.Singleline);
            
            foreach (Match bookMatch in bookMatches)
            {
                try
                {
                    var attributesText = bookMatch.Groups["attributes"].Value;
                    var contentText = bookMatch.Groups["content"].Value;
                    
                    var book = new BookInfo
                    {
                        // 提取属性
                        Id = ExtractAttribute(attributesText, "id"),
                        Isbn = ExtractAttribute(attributesText, "isbn"),
                        DetailUrl = "https://z-library.sk" + ExtractAttribute(attributesText, "href"),
                        DownloadUrl = "https://z-library.sk" + ExtractAttribute(attributesText, "download"),
                        Publisher = ExtractAttribute(attributesText, "publisher"),
                        Language = ExtractAttribute(attributesText, "language"),
                        Year = ExtractAttribute(attributesText, "year"),
                        Extension = ExtractAttribute(attributesText, "extension"),
                        FileSize = ExtractAttribute(attributesText, "filesize"),
                        Rating = ExtractAttribute(attributesText, "rating"),
                        Quality = ExtractAttribute(attributesText, "quality")
                    };
                    
                    // 提取图片URL
                    var imgMatch = Regex.Match(contentText, @"<img\s+data-src=""(?<url>.*?)""");
                    if (imgMatch.Success)
                    {
                        book.ImageUrl = imgMatch.Groups["url"].Value;
                    }
                    
                    // 提取标题
                    var titleMatch = Regex.Match(contentText, @"<div\s+slot=""title"">(?<title>.*?)</div>");
                    if (titleMatch.Success)
                    {
                        book.Title = WebUtility.HtmlDecode(titleMatch.Groups["title"].Value);
                    }
                    
                    // 提取作者
                    var authorMatch = Regex.Match(contentText, @"<div\s+slot=""author"">(?<author>.*?)</div>");
                    if (authorMatch.Success)
                    {
                        book.Author = WebUtility.HtmlDecode(authorMatch.Groups["author"].Value);
                    }
                    
                    books.Add(book);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解析书籍信息时出错: {ex.Message}");
                    // 继续处理下一本书
                }
            }
            
            return books;
        }
        
        /// <summary>
        /// 从属性文本中提取指定属性的值
        /// </summary>
        private static string ExtractAttribute(string attributesText, string attributeName)
        {
            var match = Regex.Match(attributesText, $@"{attributeName}\s*=\s*""(?<value>.*?)""");
            return match.Success ? match.Groups["value"].Value : string.Empty;
        }
        
        /// <summary>
        /// 搜索书籍
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <param name="page">页码，从1开始</param>
        /// <returns>搜索结果，包含书籍列表</returns>
        public static async Task<List<BookInfo>> SearchBooks(string query, int page = 1)
        {
            var encodedQuery = WebUtility.UrlEncode(query);
            var url = $"https://z-library.sk/s/{encodedQuery}?page={page}";
            
            var html = await Get(url);
            return ParseBooksFromHtml(html);
        }
        
        /// <summary>
        /// 下载书籍
        /// </summary>
        /// <param name="book">要下载的书籍信息</param>
        /// <param name="savePath">保存路径</param>
        /// <returns>是否下载成功</returns>
        public static async Task<bool> DownloadBook(BookInfo book, string savePath)
        {
            if (book == null)
            {
                Console.WriteLine("书籍信息为空，无法下载");
                return false;
            }
            
            return await DownloadBook(book.DownloadUrl, savePath, book.Title, book.Extension);
        }
        
        // 仅仅提供id需要访问这本书的主页才能获取到下载链接
        // /// <summary>
        // /// 下载书籍
        // /// </summary>
        // /// <param name="bookId">书籍ID</param>
        // /// <param name="savePath">保存路径</param>
        // /// <returns>是否下载成功</returns>
        // public static async Task<bool> DownloadBook(string bookId, string savePath)
        // {
        //     if (string.IsNullOrEmpty(bookId))
        //     {
        //         Console.WriteLine("书籍ID为空，无法下载");
        //         return false;
        //     }
            
        //     string downloadUrl = $"https://z-library.sk/dl/{bookId}";
        //     return await DownloadBook(downloadUrl, savePath);
        // }
        
        /// <summary>
        /// 下载书籍
        /// </summary>
        /// <param name="downloadUrl">下载链接</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="suggestedFileName">建议的文件名（可选）</param>
        /// <param name="suggestedExtension">建议的文件扩展名（可选）</param>
        /// <returns>是否下载成功</returns>
        public static async Task<bool> DownloadBook(string downloadUrl, string savePath, string suggestedFileName = null, string suggestedExtension = null)
        {
            try
            {
                // 确保保存路径存在
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = Path.Combine(Environment.CurrentDirectory, "Downloads");
                }
                
                // 创建保存目录（如果不存在）
                try
                {
                    Directory.CreateDirectory(savePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"创建目录失败: {ex.Message}，将使用默认下载目录");
                    savePath = Path.Combine(Environment.CurrentDirectory, "Downloads");
                    Directory.CreateDirectory(savePath);
                }
                
                // 替换URL中的域名为IP地址
                downloadUrl = downloadUrl.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
                
                // 使用静态HttpClient实例
                using var response = await _client.GetAsync(downloadUrl);
                
                // 检查响应是否成功
                response.EnsureSuccessStatusCode();
                
                // 获取文件名
                var contentDisposition = response.Content.Headers.ContentDisposition;
                string fileName = contentDisposition?.FileName ?? Path.GetFileName(downloadUrl);
                fileName = WebUtility.UrlDecode(fileName);
                
                // 如果服务器没有提供文件名，尝试使用建议的文件名
                if (string.IsNullOrEmpty(fileName) || fileName == Path.GetFileName(downloadUrl))
                {
                    if (!string.IsNullOrEmpty(suggestedFileName))
                    {
                        fileName = suggestedFileName;
                        
                        // 添加扩展名（如果有）
                        if (!string.IsNullOrEmpty(suggestedExtension) && !fileName.EndsWith($".{suggestedExtension}"))
                        {
                            fileName = $"{fileName}.{suggestedExtension}";
                        }
                    }
                    else
                    {
                        // 从URL中提取ID作为文件名
                        var match = Regex.Match(downloadUrl, @"/dl/(\w+)(?:/|$)");
                        if (match.Success)
                        {
                            fileName = match.Groups[1].Value;
                            
                            // 添加扩展名（如果有）
                            if (!string.IsNullOrEmpty(suggestedExtension))
                            {
                                fileName = $"{fileName}.{suggestedExtension}";
                            }
                            else
                            {
                                fileName = $"{fileName}.unknown";
                            }
                        }
                        else
                        {
                            fileName = $"book_{DateTime.Now:yyyyMMdd_HHmmss}.unknown";
                        }
                    }
                }
                
                // 确保文件名有效
                foreach (var invalidChar in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(invalidChar, '_');
                }
                
                // 组合完整的保存路径
                var fullPath = Path.Combine(savePath, fileName);
                
                // 将响应内容写入文件
                using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fileStream);
                
                Console.WriteLine($"书籍已下载到: {fullPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载书籍时出错: {ex.Message}");
                return false;
            }
        }

        
        public static async Task<string> Test(){
            var url = "https://z-library.sk/s/maui?page=1";
            var html = await Get(url);
            
            // 解析书籍信息
            var books = ParseBooksFromHtml(html);
            Console.WriteLine($"找到 {books.Count} 本书籍");
            
            // 打印前3本书的详细信息
            for (int i = 0; i < Math.Min(3, books.Count); i++)
            {
                Console.WriteLine($"\n===== 书籍 {i+1} =====");
                Console.WriteLine(books[i].ToString());
            }
            await DownloadBook(books[0], "Downloads");
            return JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
