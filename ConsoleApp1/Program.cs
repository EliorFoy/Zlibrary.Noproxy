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
using System.Threading.Tasks;
using Zlibrary.Noproxy;

class Program
{
    const string TARGET_DOMAIN = "Megumin";   // 伪装的SNI域名
    const string ACTUAL_IP = "176.123.7.105"; // 实际请求的IP
    const string ORIGIN_DOMAIN = "z-library.sk"; // 原始域名
    static async Task Main()
    {
        //var html = await Tool.Test();
        //Console.WriteLine(html);
        await Tool.DownloadBook("https://z-library.sk/dl/5602260/6d6b02", "Downloads");
    }

    //static async Task Main()
    //{
    //    // 创建自定义HttpClientHandler
    //    var handler = new SocketsHttpHandler
    //    {
    //        // 禁用证书验证
    //        SslOptions = new SslClientAuthenticationOptions
    //        {
    //            RemoteCertificateValidationCallback = (_, _, _, _) => true,
    //            TargetHost = TARGET_DOMAIN
    //        },
    //        ConnectCallback = async (context, cancellationToken) =>
    //        {
    //            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    //            await socket.ConnectAsync(IPAddress.Parse(ACTUAL_IP), context.DnsEndPoint.Port, cancellationToken);
    //            var stream = new NetworkStream(socket, ownsSocket: true);

    //            // 创建SSL流并手动认证
    //            var sslStream = new SslStream(stream, false,
    //                (_, _, _, _) => true); // 证书验证回调

    //            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
    //            {
    //                TargetHost = TARGET_DOMAIN,
    //                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
    //                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
    //            }, cancellationToken);

    //            return sslStream;
    //        }
    //    };

    //    // 创建HttpClient
    //    using var client = new HttpClient(handler);

    //    // 设置请求头
    //    client.DefaultRequestHeaders.Add("Host", ORIGIN_DOMAIN);
    //    client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
    //    client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
    //    client.DefaultRequestHeaders.Add("Referer", "https://z-library.sk/s/maui?page=2");
    //    client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
    //    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
    //    client.Timeout = TimeSpan.FromSeconds(10);

    //    // 准备请求数据
    //    var requestData = new
    //    {
    //        bookIds = new List<string>
    //        {
    //            "21461955", "2052226", "27250476", "25286505", "5419633",
    //            "2531163", "2948536", "4531230", "97918590", "5038224",
    //            "969382", "25038925", "27410814", "1627066", "4349316",
    //            "24168681", "6110868", "21636412", "21636415", "21636418"
    //        }
    //    };

    //    var url = $"https://{ACTUAL_IP}/papi/book/recommended/mosaic/30";

    //    // 修复内容类型问题：媒体类型和字符集分开处理
    //    var json = JsonSerializer.Serialize(requestData);
    //    var content = new StringContent(json, Encoding.UTF8, "text/plain");

    //    // 添加字符集参数到内容类型
    //    content.Headers.ContentType.CharSet = "UTF-8";

    //    try
    //    {
    //        // 发送请求
    //        var response = await client.PostAsync(url, content);
    //        Console.WriteLine($"状态码: {(int)response.StatusCode} ({response.StatusCode})");

    //        var responseBytes = await response.Content.ReadAsByteArrayAsync();
    //        Console.WriteLine($"响应大小: {responseBytes.Length} 字节");

    //        // 尝试解析JSON
    //        try
    //        {
    //            using var jsonDoc = JsonDocument.Parse(responseBytes);
    //            var options = new JsonSerializerOptions
    //            {
    //                WriteIndented = true,
    //                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    //            };

    //            Console.WriteLine("\n响应内容:");
    //            Console.WriteLine(JsonSerializer.Serialize(jsonDoc, options));

    //            // 保存到文件
    //            await File.WriteAllBytesAsync("zlibrary_response.json", responseBytes);
    //            Console.WriteLine("\n响应已保存到 zlibrary_response.json 文件");
    //        }
    //        catch (JsonException)
    //        {
    //            Console.WriteLine("响应不是有效的JSON格式");
    //            Console.WriteLine("\n响应内容预览:");
    //            Console.WriteLine(Encoding.UTF8.GetString(responseBytes, 0, Math.Min(500, responseBytes.Length)));

    //            // 保存原始响应
    //            await File.WriteAllBytesAsync("zlibrary_response.txt", responseBytes);
    //            Console.WriteLine("\n原始响应已保存到 zlibrary_response.txt 文件");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"请求失败: {ex.Message}");
    //        Console.WriteLine(ex.StackTrace);
    //    }
    //}
}