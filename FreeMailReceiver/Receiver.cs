using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FreeMailReceiver
{
    public class Receiver
    {
        private readonly HttpClient _httpClient;
        private string _visitorId = "eliorfoy"; // 访客ID
        private string _currentEmailAddress;

        public Receiver()
        {
            _httpClient = new HttpClient();
            SetupHttpClient();
        }

        public Receiver(string visitorId)
        {
            _httpClient = new HttpClient();
            _visitorId = visitorId;
            SetupHttpClient();
        }

        private void SetupHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Add("accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            _httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("priority", "u=1, i");
            _httpClient.DefaultRequestHeaders.Add("referer", "https://minmail.app/cn");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"137\", \"Chromium\";v=\"137\", \"Not/A)Brand\";v=\"24\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36 Edg/137.0.0.0");
            _httpClient.DefaultRequestHeaders.Add("visitor-id", _visitorId);
        }

        /// <summary>
        /// 刷新并获取新的临时邮箱地址
        /// </summary>
        /// <returns>临时邮箱地址</returns>
        public async Task<string> RefreshEmailAddressAsync()
        {
            string url = "https://minmail.app/api/mail/address?refresh=true&expire=1440&part=main";
            
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            _currentEmailAddress = doc.RootElement.GetProperty("address").GetString();
            
            return _currentEmailAddress;
        }

        /// <summary>
        /// 获取当前邮箱中的邮件列表
        /// </summary>
        /// <returns>邮件列表的JSON响应</returns>
        public async Task<JsonDocument> GetEmailListAsync()
        {
            string url = "https://minmail.app/api/mail/list?part=main";
            
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        /// <summary>
        /// 检测邮件中的验证码并返回
        /// </summary>
        /// <param name="maxAttempts">最大尝试次数</param>
        /// <param name="delaySeconds">每次尝试间隔（秒）</param>
        /// <returns>找到的验证码，如果未找到则返回null</returns>
        public async Task<string> GetVerificationCodeAsync(int maxAttempts = 10, int delaySeconds = 5)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var emailList = await GetEmailListAsync();
                
                if (emailList.RootElement.TryGetProperty("message", out JsonElement messages) && 
                    messages.GetArrayLength() > 0)
                {
                    foreach (JsonElement message in messages.EnumerateArray())
                    {
                        if (message.TryGetProperty("content", out JsonElement content))
                        {
                            string contentStr = content.GetString();
                            if (!string.IsNullOrEmpty(contentStr))
                            {
                                // 尝试从HTML内容中提取验证码
                                // 方法1：查找h1标签中的数字
                                var h1Match = Regex.Match(contentStr, @"<h1[^>]*>(\d+)<\/h1>");
                                if (h1Match.Success)
                                {
                                    return h1Match.Groups[1].Value;
                                }
                                
                                // 方法2：查找任何看起来像验证码的4-6位数字
                                var codeMatch = Regex.Match(contentStr, @"(\d{4,6})");
                                if (codeMatch.Success)
                                {
                                    return codeMatch.Groups[1].Value;
                                }
                            }
                        }
                    }
                }
                
                // 如果没有找到验证码，等待一段时间后再次尝试
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            
            return null; // 未找到验证码
        }

        /// <summary>
        /// 获取当前使用的邮箱地址
        /// </summary>
        public string CurrentEmailAddress => _currentEmailAddress;
    }
}
