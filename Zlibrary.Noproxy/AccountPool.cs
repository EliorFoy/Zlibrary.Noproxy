using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FreeMailReceiver;

namespace Zlibrary.Noproxy
{
    /// <summary>
    /// 账号池管理类，用于构建和管理Z-Library账号池
    /// </summary>
    public class AccountPool
    {
        // 数据库连接字符串
        private readonly string _connectionString = "Data Source=zlibrary_accounts.db";
        private readonly string _visitorId;

        public AccountPool(string visitorId = "eliorfoy")
        {
            _visitorId = visitorId;
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化SQLite数据库
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // 如果数据库文件不存在，则创建
                if (!File.Exists("zlibrary_accounts.db"))
                {
                    // 创建空数据库文件
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();
                    }
                }

                // 创建连接并打开
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    // 创建账号表（如果不存在）
                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS Accounts (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Email TEXT NOT NULL UNIQUE,
                            Password TEXT NOT NULL,
                            Username TEXT NOT NULL,
                            UserId INTEGER NOT NULL,
                            UserKey TEXT NOT NULL,
                            RegistrationDate TEXT NOT NULL,
                            LastUsed TEXT,
                            UsageCount INTEGER NOT NULL DEFAULT 10
                        )";

                    using (var command = new SqliteCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"初始化数据库时出错: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 自动注册一个账号并保存到数据库
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>注册是否成功</returns>
        public async Task<bool> RegisterSingleAccountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // 创建新的接收器实例
                var receiver = new Receiver(_visitorId);

                // 获取新邮箱地址
                string email = await receiver.RefreshEmailAddressAsync();

                // 生成随机用户名和密码
                string password = $"ZLib{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                string name = $"User_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

                // 发送注册请求
                bool canContinue = await SendZLibraryRegistrationRequest(email, password, name);

                // 如果达到注册上限，则停止注册流程
                if (!canContinue)
                {
                    return false;
                }

                // 等待并处理验证码邮件
                bool registrationSuccess = await WaitForVerificationCodeAndRegister(receiver, email, password, name, cancellationToken);

                return registrationSuccess;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 自动注册多个账号并保存到数据库
        /// </summary>
        /// <param name="count">要注册的账号数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>注册结果</returns>
        public async Task<RegistrationResult> RegisterMultipleAccountsAsync(int count, CancellationToken cancellationToken = default)
        {
            var result = new RegistrationResult
            {
                TotalRequested = count,
                SuccessCount = 0,
                FailCount = 0
            };

            for (int i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result.IsCancelled = true;
                    break;
                }

                bool success = await RegisterSingleAccountAsync(cancellationToken);
                if (success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailCount++;
                }

                // 等待一段时间再继续下一个注册，避免触发反爬虫机制
                if (i < count - 1) // 不是最后一个账号
                {
                    int waitTime = new Random().Next(3000, 8000);
                    await Task.Delay(waitTime, cancellationToken);
                }
            }

            return result;
        }

        /// <summary>
        /// 从数据库中随机取出一个账号，遵循特定的存取规则
        /// 如果没有可用账号，则自动注册指定数量的新账号
        /// </summary>
        /// <param name="autoRegisterCount">当没有可用账号时自动注册的账号数量</param>
        /// <returns>账号信息</returns>
        public async Task<AccountInfo?> GetRandomAccountWithAutoRegisterAsync(int autoRegisterCount = 3)
        {
            var account = GetRandomAccount();
            
            // 如果有可用账号，直接返回
            if (account != null)
            {
                return account;
            }
            
            // 没有可用账号，自动注册新账号
            var result = await RegisterMultipleAccountsAsync(autoRegisterCount);
            
            // 注册完成后再次尝试获取账号
            if (result.SuccessCount > 0)
            {
                return GetRandomAccount();
            }
            
            // 注册失败，返回null
            return null;
        }

        /// <summary>
        /// 从数据库中随机取出一个账号，遵循特定的存取规则
        /// </summary>
        /// <returns>账号信息</returns>
        public AccountInfo? GetRandomAccount()
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    // 首先查找符合条件的账号：
                    // 1. 使用次数大于0
                    // 2. 或者距离上次使用时间超过1天
                    string selectSql = @"
                        SELECT Id, UserId, UserKey, LastUsed, UsageCount 
                        FROM Accounts 
                        WHERE UsageCount > 0 
                           OR (LastUsed IS NULL OR datetime(LastUsed) < datetime('now', '-1 day'))
                        ORDER BY 
                            CASE 
                                WHEN datetime(LastUsed) < datetime('now', '-1 day') OR LastUsed IS NULL THEN 0 
                                ELSE 1 
                            END,
                            LastUsed ASC";

                    List<AccountInfo> eligibleAccounts = new List<AccountInfo>();

                    using (var command = new SqliteCommand(selectSql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                eligibleAccounts.Add(new AccountInfo
                                {
                                    Id = reader.GetInt32(0),
                                    UserId = reader.GetInt32(1),
                                    UserKey = reader.GetString(2),
                                    LastUsed = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    UsageCount = reader.GetInt32(4)
                                });
                            }
                        }
                    }

                    if (eligibleAccounts.Count == 0)
                    {
                        return null; // 没有符合条件的账号
                    }

                    // 优先选择时间最远的账号
                    var selectedAccount = eligibleAccounts[0];

                    // 更新选中账号的使用信息
                    string updateSql;
                    if (selectedAccount.LastUsed == null || 
                        DateTime.Parse(selectedAccount.LastUsed) < DateTime.Now.AddDays(-1))
                    {
                        // 如果距离上次使用时间超过1天，重置使用次数为9
                        updateSql = @"
                            UPDATE Accounts 
                            SET LastUsed = $LastUsed, UsageCount = 9 
                            WHERE Id = $Id";
                    }
                    else
                    {
                        // 否则减少使用次数1次
                        updateSql = @"
                            UPDATE Accounts 
                            SET LastUsed = $LastUsed, UsageCount = $UsageCount 
                            WHERE Id = $Id";
                    }

                    using (var updateCommand = new SqliteCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("$LastUsed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        updateCommand.Parameters.AddWithValue("$Id", selectedAccount.Id);
                        
                        if (!(selectedAccount.LastUsed == null || 
                              DateTime.Parse(selectedAccount.LastUsed) < DateTime.Now.AddDays(-1)))
                        {
                            // 只有在减少次数的情况下才需要这个参数
                            updateCommand.Parameters.AddWithValue("$UsageCount", selectedAccount.UsageCount - 1);
                        }

                        updateCommand.ExecuteNonQuery();
                    }

                    return new AccountInfo
                    {
                        Id = selectedAccount.Id,
                        UserId = selectedAccount.UserId,
                        UserKey = selectedAccount.UserKey,
                        LastUsed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        UsageCount = (selectedAccount.LastUsed == null || 
                                     DateTime.Parse(selectedAccount.LastUsed) < DateTime.Now.AddDays(-1)) ? 9 : 
                                     selectedAccount.UsageCount - 1
                    };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 等待验证码邮件并完成注册
        /// </summary>
        /// <returns>注册是否成功</returns>
        private async Task<bool> WaitForVerificationCodeAndRegister(Receiver receiver, string email, string password, string name, CancellationToken cancellationToken)
        {
            int maxAttempts = 30; // 最多尝试30次，每次间隔2秒，总共约1分钟
            int attempts = 0;

            while (attempts < maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                attempts++;

                try
                {
                    // 获取邮件列表
                    var emailList = await receiver.GetEmailListAsync();

                    // 解析邮件列表
                    if (emailList.RootElement.TryGetProperty("message", out JsonElement messages) &&
                        messages.GetArrayLength() > 0)
                    {
                        // 检查是否有新邮件
                        foreach (JsonElement message in messages.EnumerateArray())
                        {
                            string from = message.GetProperty("from").GetString();

                            // 如果是Z-Library的验证邮件
                            if (from.Contains("z-lib"))
                            {
                                // 显示邮件内容
                                if (message.TryGetProperty("content", out JsonElement content))
                                {
                                    string contentStr = content.GetString();

                                    // 尝试提取验证码
                                    string verificationCode = ExtractVerificationCode(contentStr);
                                    if (!string.IsNullOrEmpty(verificationCode))
                                    {
                                        // 自动提交验证码
                                        await SubmitZLibraryVerificationCode(email, password, name, verificationCode);

                                        // 注册成功
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    // 等待2秒后再次检查
                    await Task.Delay(2000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(3000, cancellationToken);
                }
            }

            return false;
        }

        /// <summary>
        /// 发送Z-Library注册请求，向临时邮箱发送验证码
        /// </summary>
        /// <returns>是否成功发送注册请求，false表示达到注册上限</returns>
        private async Task<bool> SendZLibraryRegistrationRequest(string email, string password, string name)
        {
            try
            {
                using (var httpClient = Tool.CreateNewClient(noCookie:true))
                {
                    
                    // 生成随机边界
                    string boundary = "----WebKitFormBoundary" + Guid.NewGuid().ToString("N").Substring(0, 16);

                    // 构建multipart/form-data内容
                    var content = new MultipartFormDataContent(boundary);
                    content.Add(new StringContent(email), "email");
                    content.Add(new StringContent(password), "password");
                    content.Add(new StringContent(name), "name");
                    content.Add(new StringContent("215"), "rx");
                    content.Add(new StringContent("registration"), "action");
                    content.Add(new StringContent(""), "redirectUrl");

                    // 发送请求
                    var response = await httpClient.PostAsync("https://z-library.sk/papi/user/verification/send-code", content);

                    // 检查响应
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // 检查是否达到注册上限
                        if (responseBody.Contains("Too many registrations"))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return true; // 发生异常时也继续尝试
            }
        }

        /// <summary>
        /// 提交验证码完成Z-Library注册
        /// </summary>
        private async Task SubmitZLibraryVerificationCode(string email, string password, string name, string verificationCode)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 设置请求头
                    httpClient.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");

                    // 构建表单数据
                    var formData = new Dictionary<string, string>
                    {
                        { "isModal", "true" },
                        { "email", email },
                        { "password", password },
                        { "name", name },
                        { "rx", "114" },
                        { "action", "registration" },
                        { "redirectUrl", "" },
                        { "verifyCode", verificationCode },
                        { "gg_json_mode", "1" }
                    };

                    // 编码表单数据
                    var content = new FormUrlEncodedContent(formData);

                    // 发送请求
                    var response = await httpClient.PostAsync("https://z-library.sk/rpc.php", content);

                    // 检查响应
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // 解析响应，提取用户ID和密钥
                        using JsonDocument doc = JsonDocument.Parse(responseBody);
                        if (doc.RootElement.TryGetProperty("response", out JsonElement responseObj))
                        {
                            // 尝试从priorityRedirectUrl提取
                            if (responseObj.TryGetProperty("priorityRedirectUrl", out JsonElement redirectUrl))
                            {
                                string redirectUrlStr = redirectUrl.GetString();
                                ExtractAndSaveUserCredentials(redirectUrlStr, email, password, name);
                            }
                            // 直接从响应中提取
                            else if (responseObj.TryGetProperty("user_id", out JsonElement userId) &&
                                     responseObj.TryGetProperty("user_key", out JsonElement userKey))
                            {
                                int userIdValue = userId.GetInt32();
                                string userKeyValue = userKey.GetString();
                                SaveAccountToDatabase(email, password, name, userIdValue, userKeyValue);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理异常
            }
        }

        /// <summary>
        /// 从重定向URL中提取用户ID和密钥并保存
        /// </summary>
        private void ExtractAndSaveUserCredentials(string redirectUrl, string email, string password, string name)
        {
            try
            {
                // 使用正则表达式提取用户ID和密钥
                var userIdMatch = Regex.Match(redirectUrl, @"remix_userid=(\d+)");
                var userKeyMatch = Regex.Match(redirectUrl, @"remix_userkey=([a-f0-9]+)");

                if (userIdMatch.Success && userKeyMatch.Success)
                {
                    int userId = int.Parse(userIdMatch.Groups[1].Value);
                    string userKey = userKeyMatch.Groups[1].Value;

                    // 保存到数据库
                    SaveAccountToDatabase(email, password, name, userId, userKey);
                }
            }
            catch (Exception)
            {
                // 静默处理异常
            }
        }

        /// <summary>
        /// 将账号信息保存到数据库
        /// </summary>
        private void SaveAccountToDatabase(string email, string password, string name, int userId, string userKey)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    // 检查是否已存在相同的邮箱或用户ID
                    string checkSql = "SELECT COUNT(*) FROM Accounts WHERE Email = $Email OR UserId = $UserId";
                    using (var checkCommand = new SqliteCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("$Email", email);
                        checkCommand.Parameters.AddWithValue("$UserId", userId);

                        long count = (long)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            return;
                        }
                    }

                    // 插入新记录，初始使用次数为10
                    string insertSql = @"
                        INSERT INTO Accounts (Email, Password, Username, UserId, UserKey, RegistrationDate, UsageCount)
                        VALUES ($Email, $Password, $Username, $UserId, $UserKey, $RegistrationDate, 10)";

                    using (var insertCommand = new SqliteCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("$Email", email);
                        insertCommand.Parameters.AddWithValue("$Password", password);
                        insertCommand.Parameters.AddWithValue("$Username", name);
                        insertCommand.Parameters.AddWithValue("$UserId", userId);
                        insertCommand.Parameters.AddWithValue("$UserKey", userKey);
                        insertCommand.Parameters.AddWithValue("$RegistrationDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理异常
            }
        }

        /// <summary>
        /// 从邮件内容中提取验证码
        /// </summary>
        private string? ExtractVerificationCode(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            // 方法1：查找h1标签中的数字
            var h1Match = Regex.Match(content, @"<h1[^>]*>(\d+)<\/h1>");
            if (h1Match.Success)
            {
                return h1Match.Groups[1].Value;
            }

            // 方法2：查找任何看起来像验证码的4-6位数字
            var codeMatch = Regex.Match(content, @"(\d{4,6})");
            if (codeMatch.Success)
            {
                return codeMatch.Groups[1].Value;
            }

            return null;
        }
    }

    /// <summary>
    /// 账号信息类
    /// </summary>
    public class AccountInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserKey { get; set; } = string.Empty;
        public string? LastUsed { get; set; }
        public int UsageCount { get; set; }
    }

    /// <summary>
    /// 注册结果类
    /// </summary>
    public class RegistrationResult
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public bool IsCancelled { get; set; }

        public int ProcessedCount => SuccessCount + FailCount;
    }
}