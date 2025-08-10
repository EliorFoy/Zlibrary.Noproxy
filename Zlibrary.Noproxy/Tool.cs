// 在文件开头添加nullable启用
#nullable enable
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
        public string? Id { get; set; }
        public string? Isbn { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? DownloadUrl { get; set; }
        public string? DetailUrl { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public string? Year { get; set; }
        public string? Extension { get; set; }
        public string? FileSize { get; set; }
        public string? Rating { get; set; }
        public string? Quality { get; set; }
        public string? ImageUrl { get; set; }

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
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Cookie", "remix_userkey=a097500143c397d1c09c8c4c459bb142; remix_userid=35246529; selectedSiteMode=books");
            client.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        /// <summary>
        /// 创建一个新的HttpClient实例，配置与默认_client相同
        /// </summary>
        /// <param name="allowAutoRedirect">是否允许自动重定向</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>配置好的新HttpClient实例</returns>
        private static HttpClient CreateNewClient(bool allowAutoRedirect = true, int timeout = 10)
        {
            // 创建与_handler相同配置的新handler
            var handler = new SocketsHttpHandler
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
                },
                AllowAutoRedirect = allowAutoRedirect
            };

            // 创建新的HttpClient
            var client = new HttpClient(handler);
            
            // 设置相同的默认请求头
            client.DefaultRequestHeaders.Add("Host", ORIGIN_DOMAIN);
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Cookie", "remix_userkey=a097500143c397d1c09c8c4c459bb142; remix_userid=35246529; selectedSiteMode=books");
            
            // 设置超时
            client.Timeout = TimeSpan.FromSeconds(timeout);
            
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
            
            return await DownloadBook(book.DownloadUrl!, savePath, book.Title, book.Extension);
        }
        
        // 仅仅提供id需要访问这本书的主页才能获取到下载链接
        // /// <summary>
        // /// 下载书籍
        // /// </summary>
        // // <param name="bookId">书籍ID</param>
        // // <param name="savePath">保存路径</param>
        // // <returns>是否下载成功</returns>
        // public static async Task<bool> DownloadBook(string bookId, string savePath)
        // {
        //     if (string.IsNullOrEmpty(bookId))
        //     {
        //         Console.WriteLine("书籍ID为空，无法下载");
        //         return false;
        //     }
        //     
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
        public static async Task<bool> DownloadBook(string downloadUrl, string savePath, string? suggestedFileName = null, string? suggestedExtension = null)
        {
            try
            {
                Console.WriteLine($"开始下载书籍: {downloadUrl}");
                
                // 确保保存路径存在
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                    Console.WriteLine($"创建保存目录: {savePath}");
                }
                
                // 替换URL中的域名为IP地址
                string url = downloadUrl.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
                
                // 创建不自动重定向的HttpClient
                using var redirectClient = CreateNewClient(allowAutoRedirect: false);
                
                try
                {
                    // 发送请求获取重定向URL
                    var response = await redirectClient.GetAsync(url);
                    Console.WriteLine($"响应状态码: {(int)response.StatusCode} {response.StatusCode}");
                    
                    // 检查是否为重定向
                    if (response.StatusCode == HttpStatusCode.Found || 
                        response.StatusCode == HttpStatusCode.Redirect || 
                        response.StatusCode == HttpStatusCode.MovedPermanently || 
                        response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        var location = response.Headers.Location;
                        if (location != null)
                        {
                            string redirectUrl = location.ToString();
                            Console.WriteLine($"获取到重定向URL: {redirectUrl}");
                            
                            // 从URL查询参数中提取文件名
                            string? fileName = null;
                            
                            // 尝试从URL的查询参数中获取文件名
                            if (redirectUrl.Contains("filename="))
                            {
                                try
                                {
                                    // 解析URL查询参数
                                    var uri = new Uri(redirectUrl);
                                    var queryParams = uri.Query
                                        .TrimStart('?')
                                        .Split('&')
                                        .Select(p => p.Split('='))
                                        .ToDictionary(
                                            p => p[0], 
                                            p => p.Length > 1 ? WebUtility.UrlDecode(p[1]) : null
                                        );
                                    
                                    if (queryParams.ContainsKey("filename") && !string.IsNullOrEmpty(queryParams["filename"]))
                                    {
                                        fileName = queryParams["filename"];
                                        Console.WriteLine($"从URL查询参数获取到文件名: {fileName}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"解析URL查询参数时出错: {ex.Message}");
                                }
                            }
                            
                            // 如果URL查询参数中没有文件名，尝试从Content-Disposition头获取
                            if (string.IsNullOrEmpty(fileName) && response.Content.Headers.ContentDisposition != null)
                            {
                                fileName = response.Content.Headers.ContentDisposition.FileName?.Trim('"');
                                Console.WriteLine($"从响应头获取到文件名: {fileName}");
                            }
                            
                            // 如果仍然没有文件名，尝试从URL或建议的文件名构建
                            if (string.IsNullOrEmpty(fileName))
                            {
                                if (!string.IsNullOrEmpty(suggestedFileName))
                                {
                                    // 使用建议的文件名和扩展名
                                    fileName = suggestedFileName;
                                    if (!string.IsNullOrEmpty(suggestedExtension) && !fileName.EndsWith($".{suggestedExtension}"))
                                    {
                                        fileName = $"{fileName}.{suggestedExtension}";
                                    }
                                }
                                else
                                {
                                    // 从URL中提取文件名（不包括查询参数）
                                    var uri = new Uri(redirectUrl);
                                    fileName = Path.GetFileName(uri.AbsolutePath);
                                    
                                    // 如果URL中没有文件名，使用GUID
                                    if (string.IsNullOrEmpty(fileName) || fileName.Length < 3 || fileName.Equals("redirection", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fileName = $"book_{Guid.NewGuid().ToString().Substring(0, 8)}";
                                        if (!string.IsNullOrEmpty(suggestedExtension))
                                        {
                                            fileName = $"{fileName}.{suggestedExtension}";
                                        }
                                        else
                                        {
                                            fileName = $"{fileName}.epub"; // 默认EPUB格式
                                        }
                                    }
                                }
                                Console.WriteLine($"构建的文件名: {fileName}");
                            }
                            
                            // 构建完整的保存路径
                            string fullPath = Path.Combine(savePath, fileName!);
                            Console.WriteLine($"保存路径: {fullPath}");
                            
                            // 创建新的HttpClient下载文件（使用较长的超时时间）
                            using var downloadClient = new HttpClient
                            {
                                Timeout = TimeSpan.FromMinutes(10) // 设置10分钟超时
                            };
                            
                            Console.WriteLine($"开始下载文件: {redirectUrl}");
                            
                            // 使用带进度报告的下载方法
                            await DownloadFileWithProgressAsync(downloadClient, redirectUrl, fullPath);
                            
                            Console.WriteLine($"\n文件已保存: {fullPath}");
                            
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("重定向响应中没有Location头");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"未检测到重定向，状态码: {response.StatusCode}");
                    }
                    
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"请求过程中出错: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载书籍时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 带进度显示的文件下载方法
        /// </summary>
        /// <param name="client">HttpClient实例</param>
        /// <param name="url">下载URL</param>
        /// <param name="destinationPath">保存路径</param>
        /// <returns>异步任务</returns>
        private static async Task DownloadFileWithProgressAsync(HttpClient client, string url, string destinationPath)
        {
            try
            {
                // 发送请求获取响应
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                
                // 获取文件总大小
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                if (totalBytes > 0)
                {
                    Console.WriteLine($"文件大小: {FormatFileSize(totalBytes)}");
                }
                
                // 创建文件流
                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var contentStream = await response.Content.ReadAsStreamAsync();
                
                // 准备下载
                byte[] buffer = new byte[8192]; // 8KB缓冲区
                long totalBytesRead = 0;
                int bytesRead;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long lastReportTime = 0;
                int progressBarWidth = 50; // 进度条宽度
                
                // 读取数据流并写入文件
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    
                    // 每100毫秒更新一次进度，避免过于频繁的控制台输出
                    if (sw.ElapsedMilliseconds - lastReportTime > 100 || totalBytes == totalBytesRead)
                    {
                        lastReportTime = sw.ElapsedMilliseconds;
                        
                        if (totalBytes > 0)
                        {
                            // 计算进度百分比
                            double percentage = (double)totalBytesRead / totalBytes;
                            
                            // 计算下载速度
                            double speed = totalBytesRead / (sw.ElapsedMilliseconds / 1000.0);
                            
                            // 估算剩余时间
                            string remainingTime = "未知";
                            if (speed > 0)
                            {
                                long remainingBytes = totalBytes - totalBytesRead;
                                double remainingSeconds = remainingBytes / speed;
                                remainingTime = FormatTime(remainingSeconds);
                            }
                            
                            // 构建进度条
                            int progressChars = (int)(percentage * progressBarWidth);
                            var (filledChar, emptyChar) = GetProgressBarChars();
                            string progressBar = new string(filledChar, progressChars) + new string(emptyChar, progressBarWidth - progressChars);
                            
                            // 清除当前行并显示进度
                            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
                            Console.Write($"下载进度: [{progressBar}] {percentage:P1} | {FormatFileSize(totalBytesRead)}/{FormatFileSize(totalBytes)} | {FormatFileSize((long)speed)}/s | 剩余: {remainingTime}");
                        }
                        else
                        {
                            // 如果不知道总大小，只显示已下载大小和速度
                            double speed = totalBytesRead / (sw.ElapsedMilliseconds / 1000.0);
                            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
                            Console.Write($"已下载: {FormatFileSize(totalBytesRead)} | {FormatFileSize((long)speed)}/s");
                        }
                    }
                }
                
                sw.Stop();
                double totalSpeed = totalBytesRead / (sw.ElapsedMilliseconds / 1000.0);
                Console.WriteLine($"\n下载完成 | 总大小: {FormatFileSize(totalBytesRead)} | 平均速度: {FormatFileSize((long)totalSpeed)}/s | 用时: {FormatTime(sw.ElapsedMilliseconds / 1000.0)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n下载过程中出错: {ex.Message}");
                throw;
            }
        }
        
        // 添加一个委托用于报告下载进度
        public delegate void DownloadProgressHandler(double progress, string message);
        
        // 添加一个事件用于报告下载进度
        public static event DownloadProgressHandler? DownloadProgressChanged;
        

        /// <summary>
        /// 下载书籍并返回字节数组
        /// </summary>
        /// <param name="book">要下载的书籍信息</param>
        /// <returns>书籍文件的字节数组</returns>
        public static async Task<byte[]?> DownloadBook(BookInfo book)
        {
            if (book == null)
            {
                Console.WriteLine("书籍信息为空，无法下载");
                return null;
            }
            
            return await DownloadBook(book.DownloadUrl!);
        }
        
        /// <summary>
        /// 下载书籍并返回字节数组
        /// </summary>
        /// <param name="downloadUrl">下载链接</param>
        /// <returns>书籍文件的字节数组</returns>
        public static async Task<byte[]?> DownloadBook(string downloadUrl)
        {
            try
            {
                // 替换URL中的域名为IP地址
                string url = downloadUrl.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
                
                // 创建不自动重定向的HttpClient
                using var redirectClient = CreateNewClient(allowAutoRedirect: false);
                
                // 发送请求获取重定向URL
                var response = await redirectClient.GetAsync(url);
                
                // 检查是否为重定向
                if (response.StatusCode == HttpStatusCode.Found || 
                    response.StatusCode == HttpStatusCode.Redirect || 
                    response.StatusCode == HttpStatusCode.MovedPermanently || 
                    response.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    var location = response.Headers.Location;
                    if (location != null)
                    {
                        string redirectUrl = location.ToString();
                        
                        // 创建新的HttpClient下载文件（使用较长的超时时间）
                        using var downloadClient = new HttpClient
                        {
                            Timeout = TimeSpan.FromMinutes(10) // 设置10分钟超时
                        };
                        
                        // 下载并返回字节数组
                        byte[] fileBytes = await downloadClient.GetByteArrayAsync(redirectUrl);
                        return fileBytes;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载书籍时出错: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 下载书籍并返回字节数组（支持进度报告）
        /// </summary>
        /// <param name="book">要下载的书籍信息</param>
        /// <returns>书籍文件的字节数组</returns>
        public static async Task<byte[]?> DownloadBookWithProgress(BookInfo book)
        {
            if (book == null)
            {
                DownloadProgressChanged?.Invoke(-1, "书籍信息为空，无法下载");
                return null;
            }
            
            return await DownloadBookWithProgress(book.DownloadUrl!);
        }
        
        /// <summary>
        /// 下载书籍并返回字节数组（支持进度报告）
        /// </summary>
        /// <param name="downloadUrl">下载链接</param>
        /// <returns>书籍文件的字节数组</returns>
        public static async Task<byte[]?> DownloadBookWithProgress(string downloadUrl)
        {
            try
            {
                // 替换URL中的域名为IP地址
                string url = downloadUrl.Replace(ORIGIN_DOMAIN, ACTUAL_IP);
                
                // 创建不自动重定向的HttpClient
                using var redirectClient = CreateNewClient(allowAutoRedirect: false);
                
                // 发送请求获取重定向URL
                var response = await redirectClient.GetAsync(url);
                
                // 检查是否为重定向
                if (response.StatusCode == HttpStatusCode.Found || 
                    response.StatusCode == HttpStatusCode.Redirect || 
                    response.StatusCode == HttpStatusCode.MovedPermanently || 
                    response.StatusCode == HttpStatusCode.TemporaryRedirect)
                {
                    var location = response.Headers.Location;
                    if (location != null)
                    {
                        string redirectUrl = location.ToString();
                        
                        // 创建新的HttpClient下载文件（使用较长的超时时间）
                        using var downloadClient = new HttpClient
                        {
                            Timeout = TimeSpan.FromMinutes(10) // 设置10分钟超时
                        };
                        
                        // 使用带进度报告的下载方法
                        byte[] fileBytes = await DownloadFileWithProgressAsync(downloadClient, redirectUrl);
                        return fileBytes;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                DownloadProgressChanged?.Invoke(-1, $"下载书籍时出错: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 带进度报告的文件下载方法
        /// </summary>
        /// <param name="client">HttpClient实例</param>
        /// <param name="url">下载URL</param>
        /// <returns>文件字节数组</returns>
        private static async Task<byte[]> DownloadFileWithProgressAsync(HttpClient client, string url)
        {
            try
            {
                // 发送请求获取响应
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                
                // 获取文件总大小
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                
                // 创建内存流来存储文件数据
                using var memoryStream = new MemoryStream();
                using var contentStream = await response.Content.ReadAsStreamAsync();
                
                // 准备下载
                byte[] buffer = new byte[8192]; // 8KB缓冲区
                long totalBytesRead = 0;
                int bytesRead;
                
                // 读取数据流并写入内存流
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    
                    // 报告进度
                    if (totalBytes > 0)
                    {
                        // 计算进度百分比
                        double percentage = (double)totalBytesRead / totalBytes * 100;
                        string message = $"已下载: {FormatFileSize(totalBytesRead)}/{FormatFileSize(totalBytes)}";
                        DownloadProgressChanged?.Invoke(percentage, message);
                    }
                    else
                    {
                        // 如果不知道总大小，只显示已下载大小
                        string message = $"已下载: {FormatFileSize(totalBytesRead)}";
                        DownloadProgressChanged?.Invoke(-1, message); // -1表示未知进度
                    }
                }
                
                DownloadProgressChanged?.Invoke(100, "下载完成");
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                DownloadProgressChanged?.Invoke(-1, $"下载出错: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// 格式化时间
        /// </summary>
        private static string FormatTime(double seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours}小时{timeSpan.Minutes}分";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}分{timeSpan.Seconds}秒";
            }
            else
            {
                return $"{timeSpan.Seconds}秒";
            }
        }

        public static async Task<string> Test(string keyword, int page = 1){
            // 使用SearchBooks函数搜索书籍
            var books = await SearchBooks(keyword, page);
            Console.WriteLine($"找到 {books.Count} 本书籍");
            
            // 打印所有书籍的详细信息
            for (int i = 0; i < books.Count; i++)
            {
                Console.WriteLine($"\n===== 书籍 {i+1} =====");
                Console.WriteLine(books[i].ToString());
            }
            
            return JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// 进度条样式
        /// </summary>
        public enum ProgressBarStyle
        {
            /// <summary>
            /// 方块样式 (█░)
            /// </summary>
            Block,
            
            /// <summary>
            /// 简单样式 (#-)
            /// </summary>
            Simple,
            
            /// <summary>
            /// 等号样式 (=.)
            /// </summary>
            Equals,
            
            /// <summary>
            /// 箭头样式 (>.)
            /// </summary>
            Arrow,
            
            /// <summary>
            /// 方块样式2 (■□)
            /// </summary>
            Block2,
            
            /// <summary>
            /// 方块样式3 (▓░)
            /// </summary>
            Block3,
            
            /// <summary>
            /// 星号样式 (*.)
            /// </summary>
            Star
        }
        
        // 当前进度条样式
        private static ProgressBarStyle _progressBarStyle = ProgressBarStyle.Block;
        
        /// <summary>
        /// 设置进度条样式
        /// </summary>
        /// <param name="style">进度条样式</param>
        public static void SetProgressBarStyle(ProgressBarStyle style)
        {
            _progressBarStyle = style;
        }
        
        /// <summary>
        /// 获取进度条字符
        /// </summary>
        private static (char Filled, char Empty) GetProgressBarChars()
        {
            return _progressBarStyle switch
            {
                ProgressBarStyle.Block => ('█', '░'),
                ProgressBarStyle.Simple => ('#', '-'),
                ProgressBarStyle.Equals => ('=', '.'),
                ProgressBarStyle.Arrow => ('>', '.'),
                ProgressBarStyle.Block2 => ('■', '□'),
                ProgressBarStyle.Block3 => ('▓', '░'),
                ProgressBarStyle.Star => ('*', '.'),
                _ => ('=', '.')
            };
        }
    }
}