using FreeMailReceiver;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Zlibrary.AccountPool
{
    class Program
    {
        // 用于存储已处理过的邮件ID
        private static HashSet<string> processedEmailIds = new HashSet<string>();
        // 存储注册信息
        private static string registrationEmail;
        private static string registrationPassword;
        private static string registrationName;
        // 数据库连接字符串
        private static string connectionString = "Data Source=zlibrary_accounts.db;Version=3;";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Z-Library账号池工具");
            Console.WriteLine("1. 手动注册单个账号");
            Console.WriteLine("2. 自动批量注册账号");
            Console.WriteLine("3. 随机获取一个账号");
            Console.WriteLine("请选择操作模式 (1/2/3):");
            
            var key = Console.ReadKey(true);
            if (key.KeyChar == '2')
            {
                Console.WriteLine("已选择: 自动批量注册账号");
                Console.WriteLine("请输入要注册的账号数量:");
                if (int.TryParse(Console.ReadLine(), out int accountCount) && accountCount > 0)
                {
                    await AutoRegisterMultipleAccounts(accountCount);
                }
                else
                {
                    Console.WriteLine("输入无效，请输入大于0的整数");
                }
            }
            else if (key.KeyChar == '3')
            {
                Console.WriteLine("已选择: 随机获取一个账号");
                await GetRandomAccount();
            }
            else
            {
                Console.WriteLine("已选择: 手动注册单个账号");
                await ManualRegisterSingleAccount();
            }
            
            Console.WriteLine("程序已退出，按任意键关闭窗口...");
            Console.ReadKey();
        }
        
        /// <summary>
        /// 手动注册单个账号的原始流程
        /// </summary>
        static async Task ManualRegisterSingleAccount()
        {
            Console.WriteLine("开始测试临时邮箱接收器...");
            
            try
            {
                // 初始化数据库
                InitializeDatabase();
                
                // 创建接收器实例
                var receiver = new Receiver("eliorfoy");
                
                // 获取新邮箱地址
                Console.WriteLine("获取新邮箱地址...");
                string email = await receiver.RefreshEmailAddressAsync();
                Console.WriteLine($"新邮箱地址: {email}");
                
                // 询问用户是否要注册Z-Library账号
                Console.WriteLine("是否要使用此邮箱注册Z-Library账号？(y/n)");
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    Console.WriteLine("开始注册Z-Library账号...");
                    string password = "WenHao0425"; // 默认密码
                    string name = "Elior"; // 默认用户名
                    
                    // 保存注册信息，以便后续提交验证码时使用
                    registrationEmail = email;
                    registrationPassword = password;
                    registrationName = name;
                    
                    await SendZLibraryRegistrationRequest(email, password, name);
                    Console.WriteLine($"已向邮箱 {email} 发送验证码，等待接收...");
                }
                
                Console.WriteLine("系统将自动监控此邮箱，等待新邮件到达...");
                Console.WriteLine("(按下 'q' 键退出程序)");
                
                // 创建取消令牌源
                var cts = new CancellationTokenSource();
                
                // 启动键盘监听任务
                var keyboardTask = Task.Run(() => {
                    while (true)
                    {
                        var keyPress = Console.ReadKey(true);
                        if (keyPress.KeyChar == 'q' || keyPress.KeyChar == 'Q')
                        {
                            cts.Cancel();
                            return;
                        }
                    }
                });
                
                // 启动邮件监控任务
                var monitorTask = MonitorEmailsAsync(receiver, cts.Token);
                
                // 等待任务完成（当用户按下q键时）
                await Task.WhenAny(monitorTask, keyboardTask);
                
                Console.WriteLine("程序退出中...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// 自动批量注册多个Z-Library账号
        /// </summary>
        /// <param name="accountCount">要注册的账号数量</param>
        static async Task AutoRegisterMultipleAccounts(int accountCount)
        {
            Console.WriteLine($"开始自动批量注册 {accountCount} 个Z-Library账号...");
            
            try
            {
                // 初始化数据库
                InitializeDatabase();
                
                // 创建取消令牌源
                var cts = new CancellationTokenSource();
                
                // 启动键盘监听任务
                var keyboardTask = Task.Run(() => {
                    Console.WriteLine("自动注册进行中...(按下 'q' 键停止注册过程)");
                    while (true)
                    {
                        var keyPress = Console.ReadKey(true);
                        if (keyPress.KeyChar == 'q' || keyPress.KeyChar == 'Q')
                        {
                            Console.WriteLine("用户请求停止注册过程...");
                            cts.Cancel();
                            return;
                        }
                    }
                });
                
                int successCount = 0;
                int failCount = 0;
                
                for (int i = 0; i < accountCount; i++)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("注册过程已被用户中断");
                        break;
                    }
                    
                    Console.WriteLine($"========== 开始注册第 {i+1}/{accountCount} 个账号 ==========");
                    
                    try
                    {
                        // 创建新的接收器实例
                        var receiver = new Receiver("eliorfoy");
                        
                        // 获取新邮箱地址
                        Console.WriteLine("获取新邮箱地址...");
                        string email = await receiver.RefreshEmailAddressAsync();
                        Console.WriteLine($"新邮箱地址: {email}");
                        
                        // 生成随机用户名和密码
                        string password = $"ZLib{DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
                        string name = $"User_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

                        
                        Console.WriteLine($"生成的账号信息 - 用户名: {name}, 密码: {password}");
                        
                        // 保存注册信息，以便后续提交验证码时使用
                        registrationEmail = email;
                        registrationPassword = password;
                        registrationName = name;
                        
                        // 发送注册请求
                        Console.WriteLine("发送Z-Library注册请求...");
                        bool canContinue = await SendZLibraryRegistrationRequest(email, password, name);
                        
                        // 如果达到注册上限，则停止整个注册流程
                        if (!canContinue)
                        {
                            // 当前账号计入失败
                            failCount++;
                            Console.WriteLine($"账号注册失败! 当前成功: {successCount}, 失败: {failCount}");
                            
                            Console.WriteLine("已达到当日注册上限，停止批量注册");
                            // 将当前批次中剩余的所有账号都计入失败数（不包括当前账号，因为已经计数）
                            failCount += (accountCount - i - 1);
                            break;
                        }
                        
                        Console.WriteLine($"已向邮箱 {email} 发送验证码，等待接收...");
                        
                        // 等待并处理验证码邮件
                        bool registrationSuccess = await WaitForVerificationCodeAndRegister(receiver, cts.Token);
                        
                        if (registrationSuccess)
                        {
                            successCount++;
                            Console.WriteLine($"账号注册成功! 当前成功: {successCount}, 失败: {failCount}");
                        }
                        else
                        {
                            failCount++;
                            Console.WriteLine($"账号注册失败! 当前成功: {successCount}, 失败: {failCount}");
                            
                            // 先保存未验证的账号信息
                            await SaveAccountToFile(email, password, name);
                        }
                        
                        // 等待一段时间再继续下一个注册，避免触发反爬虫机制
                        int waitTime = new Random().Next(3000, 8000);
                        Console.WriteLine($"等待 {waitTime/1000} 秒后继续下一个注册...");
                        await Task.Delay(waitTime, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("注册过程被取消");
                        break;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Console.WriteLine($"注册过程中发生错误: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                        
                        // 等待稍长时间后重试
                        await Task.Delay(10000, cts.Token);
                    }
                }
                
                // 取消键盘监听任务
                cts.Cancel();
                
                Console.WriteLine($"========== 批量注册完成 ==========");
                Console.WriteLine($"成功: {successCount}, 失败: {failCount}, 总计: {accountCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"批量注册过程中发生错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// 等待验证码邮件并完成注册
        /// </summary>
        /// <returns>注册是否成功</returns>
        private static async Task<bool> WaitForVerificationCodeAndRegister(Receiver receiver, CancellationToken cancellationToken)
        {
            int maxAttempts = 30; // 最多尝试30次，每次间隔2秒，总共约1分钟
            int attempts = 0;
            
            while (attempts < maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                attempts++;
                Console.WriteLine($"[{attempts}/{maxAttempts}] 检查验证码邮件...");
                
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
                            string emailId = message.GetProperty("id").GetString();
                            string subject = message.GetProperty("subject").GetString();
                            string from = message.GetProperty("from").GetString();
                            
                            // 如果是Z-Library的验证邮件
                            if ((from.Contains("z-lib") || subject.Contains("verification")))
                            {
                                Console.WriteLine($"找到Z-Library验证邮件: {subject}");
                                
                                // 显示邮件内容
                                if (message.TryGetProperty("content", out JsonElement content))
                                {
                                    string contentStr = content.GetString();
                                    Console.WriteLine("邮件内容:");
                                    Console.WriteLine(contentStr);
                                    
                                    // 尝试提取验证码
                                    string verificationCode = ExtractVerificationCode(contentStr);
                                    if (!string.IsNullOrEmpty(verificationCode))
                                    {
                                        Console.WriteLine($"找到验证码: {verificationCode}");
                                        
                                        // 自动提交验证码
                                        Console.WriteLine("自动提交验证码完成注册...");
                                        await SubmitZLibraryVerificationCode(
                                            registrationEmail, 
                                            registrationPassword, 
                                            registrationName, 
                                            verificationCode);
                                        
                                        // 注册成功
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("无法从邮件中提取验证码");
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
                    Console.WriteLine("等待验证码过程被取消");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"检查验证码邮件时出错: {ex.Message}");
                    await Task.Delay(3000, cancellationToken);
                }
            }
            
            Console.WriteLine("等待验证码超时，注册失败");
            return false;
        }
        
        /// <summary>
        /// 初始化SQLite数据库
        /// </summary>
        private static void InitializeDatabase()
        {
            try
            {
                // 如果数据库文件不存在，则创建
                if (!File.Exists("zlibrary_accounts.db"))
                {
                    SQLiteConnection.CreateFile("zlibrary_accounts.db");
                }
                
                // 创建连接并打开
                using (var connection = new SQLiteConnection(connectionString))
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
                            RegistrationDate TEXT NOT NULL
                        )";
                    
                    using (var command = new SQLiteCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    Console.WriteLine("数据库初始化完成");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化数据库时出错: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 发送Z-Library注册请求，向临时邮箱发送验证码
        /// </summary>
        /// <returns>是否成功发送注册请求，false表示达到注册上限</returns>
        private static async Task<bool> SendZLibraryRegistrationRequest(string email, string password, string name)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 设置请求头
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    
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
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("注册请求发送成功，响应内容:");
                        Console.WriteLine(responseBody);
                        return true;
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"注册请求失败，状态码: {response.StatusCode}");
                        Console.WriteLine($"错误响应: {responseBody}");
                        
                        // 检查是否达到注册上限
                        if (responseBody.Contains("Too many registrations"))
                        {
                            Console.WriteLine("检测到已达到当日注册上限，停止注册流程");
                            return false;
                        }
                    }
                }
                
                return true; // 默认返回true，即使请求失败也尝试继续流程
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送注册请求时出错: {ex.Message}");
                return true; // 发生异常时也继续尝试
            }
        }
        
        /// <summary>
        /// 提交验证码完成Z-Library注册
        /// </summary>
        private static async Task SubmitZLibraryVerificationCode(string email, string password, string name, string verificationCode)
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
                        { "rx", "215" },
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
                        Console.WriteLine("验证码提交成功，响应内容:");
                        Console.WriteLine(responseBody);
                        
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
                            else
                            {
                                Console.WriteLine("无法从响应中提取用户ID和密钥");
                            }
                        }
                        else
                        {
                            Console.WriteLine("注册失败，请检查响应内容。");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"验证码提交失败，状态码: {response.StatusCode}");
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"错误响应: {responseBody}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"提交验证码时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 从重定向URL中提取用户ID和密钥并保存
        /// </summary>
        private static void ExtractAndSaveUserCredentials(string redirectUrl, string email, string password, string name)
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
                    
                    Console.WriteLine("成功提取用户凭据:");
                    Console.WriteLine($"用户ID: {userId}");
                    Console.WriteLine($"用户密钥: {userKey}");
                    
                    // 保存到数据库
                    SaveAccountToDatabase(email, password, name, userId, userKey);
                }
                else
                {
                    Console.WriteLine("无法从URL中提取用户ID和密钥");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"提取用户凭据时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 将账号信息保存到数据库
        /// </summary>
        private static void SaveAccountToDatabase(string email, string password, string name, int userId, string userKey)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // 检查是否已存在相同的邮箱或用户ID
                    string checkSql = "SELECT COUNT(*) FROM Accounts WHERE Email = @Email OR UserId = @UserId";
                    using (var checkCommand = new SQLiteCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Email", email);
                        checkCommand.Parameters.AddWithValue("@UserId", userId);
                        
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (count > 0)
                        {
                            Console.WriteLine("数据库中已存在相同的邮箱或用户ID，跳过保存");
                            return;
                        }
                    }
                    
                    // 插入新记录
                    string insertSql = @"
                        INSERT INTO Accounts (Email, Password, Username, UserId, UserKey, RegistrationDate)
                        VALUES (@Email, @Password, @Username, @UserId, @UserKey, @RegistrationDate)";
                    
                    using (var insertCommand = new SQLiteCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Email", email);
                        insertCommand.Parameters.AddWithValue("@Password", password);
                        insertCommand.Parameters.AddWithValue("@Username", name);
                        insertCommand.Parameters.AddWithValue("@UserId", userId);
                        insertCommand.Parameters.AddWithValue("@UserKey", userKey);
                        insertCommand.Parameters.AddWithValue("@RegistrationDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        
                        insertCommand.ExecuteNonQuery();
                    }
                    
                    Console.WriteLine("账号信息已成功保存到数据库");
                    
                    // 同时保存到文本文件作为备份
                    SaveAccountToFile(email, password, name, userId, userKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存账号到数据库时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存账号信息到文件（作为备份）
        /// </summary>
        private static async Task SaveAccountToFile(string email, string password, string name)
        {
            try
            {
                string accountInfo = $"Email: {email}\r\nPassword: {password}\r\nName: {name}\r\nDate: {DateTime.Now}\r\n\r\n";
                await File.AppendAllTextAsync("zlibrary_accounts.txt", accountInfo);
                Console.WriteLine("账号信息已保存到 zlibrary_accounts.txt 文件");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存账号信息到文件时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存完整账号信息到文件（包含用户ID和密钥）
        /// </summary>
        private static void SaveAccountToFile(string email, string password, string name, int userId, string userKey)
        {
            try
            {
                string accountInfo = $"Email: {email}\r\nPassword: {password}\r\nName: {name}\r\nUserId: {userId}\r\nUserKey: {userKey}\r\nDate: {DateTime.Now}\r\n\r\n";
                File.AppendAllText("zlibrary_accounts_full.txt", accountInfo);
                Console.WriteLine("完整账号信息已保存到 zlibrary_accounts_full.txt 文件");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存完整账号信息到文件时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 监控邮箱并处理新邮件
        /// </summary>
        private static async Task MonitorEmailsAsync(Receiver receiver, CancellationToken cancellationToken)
        {
            int checkCount = 0;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    checkCount++;
                    Console.Write($"\r检查新邮件中... (第{checkCount}次检查)");
                    
                    // 获取邮件列表
                    var emailList = await receiver.GetEmailListAsync();
                    
                    // 解析邮件列表
                    if (emailList.RootElement.TryGetProperty("message", out JsonElement messages) && 
                        messages.GetArrayLength() > 0)
                    {
                        // 检查是否有新邮件
                        bool hasNewEmails = false;
                        List<JsonElement> newEmails = new List<JsonElement>();
                        
                        foreach (JsonElement message in messages.EnumerateArray())
                        {
                            string emailId = message.GetProperty("id").GetString();
                            
                            // 如果这个ID没有处理过，说明是新邮件
                            if (!processedEmailIds.Contains(emailId))
                            {
                                hasNewEmails = true;
                                newEmails.Add(message);
                                processedEmailIds.Add(emailId);
                            }
                        }
                        
                        // 如果有新邮件，处理它们
                        if (hasNewEmails)
                        {
                            Console.WriteLine($"\n收到 {newEmails.Count} 封新邮件！");
                            
                            // 显示新邮件列表
                            int index = 0;
                            foreach (JsonElement message in newEmails)
                            {
                                string subject = message.GetProperty("subject").GetString();
                                string from = message.GetProperty("from").GetString();
                                string date = message.GetProperty("date").GetString();
                                
                                Console.WriteLine($"[{index}] 主题: {subject}, 发件人: {from}, 日期: {date}");
                                index++;
                                
                                // 显示邮件内容
                                if (message.TryGetProperty("content", out JsonElement content))
                                {
                                    string contentStr = content.GetString();
                                    Console.WriteLine("邮件内容:");
                                    Console.WriteLine(contentStr);
                                    
                                    // 尝试提取验证码
                                    string verificationCode = ExtractVerificationCode(contentStr);
                                    if (!string.IsNullOrEmpty(verificationCode))
                                    {
                                        Console.WriteLine($"找到验证码: {verificationCode}");
                                        
                                        // 如果是Z-Library的验证邮件，询问用户是否要使用此验证码
                                        if ((from.Contains("z-lib") || subject.Contains("verification")) && 
                                            !string.IsNullOrEmpty(registrationEmail))
                                        {
                                            Console.WriteLine("检测到Z-Library验证码，是否要使用此验证码完成注册？(y/n)");
                                            var key = Console.ReadKey(true);
                                            if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                                            {
                                                Console.WriteLine("正在提交验证码完成注册...");
                                                await SubmitZLibraryVerificationCode(
                                                    registrationEmail, 
                                                    registrationPassword, 
                                                    registrationName, 
                                                    verificationCode);
                                            }
                                        }
                                    }
                                }
                                
                                Console.WriteLine(new string('-', 50));
                            }
                            
                            Console.WriteLine("按任意键继续监控邮箱...");
                            Console.ReadKey(true);
                            Console.WriteLine("继续监控邮箱中...");
                        }
                    }
                    
                    // 等待一段时间再检查
                    await Task.Delay(5000, cancellationToken); // 5秒检查一次
                }
                catch (OperationCanceledException)
                {
                    // 取消操作，正常退出
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n检查邮件时出错: {ex.Message}");
                    await Task.Delay(10000, cancellationToken); // 出错后等待较长时间
                }
            }
        }
        
        /// <summary>
        /// 从邮件内容中提取验证码
        /// </summary>
        private static string ExtractVerificationCode(string content)
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
        
        /// <summary>
        /// 随机获取一个账号并打印相关信息
        /// </summary>
        static async Task GetRandomAccount()
        {
            try
            {
                // 初始化数据库
                InitializeDatabase();
                
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // 检查数据库中是否有账号
                    string countSql = "SELECT COUNT(*) FROM Accounts";
                    using (var countCommand = new SQLiteCommand(countSql, connection))
                    {
                        int accountCount = Convert.ToInt32(countCommand.ExecuteScalar());
                        if (accountCount == 0)
                        {
                            Console.WriteLine("数据库中没有任何账号，请先注册账号");
                            return;
                        }
                        
                        Console.WriteLine($"数据库中共有 {accountCount} 个账号");
                        
                        // 随机选择一个账号
                        string randomSql = "SELECT * FROM Accounts ORDER BY RANDOM() LIMIT 1";
                        using (var randomCommand = new SQLiteCommand(randomSql, connection))
                        {
                            using (var reader = randomCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // 获取账号信息
                                    int id = reader.GetInt32(0);
                                    string email = reader.GetString(1);
                                    string password = reader.GetString(2);
                                    string username = reader.GetString(3);
                                    int userId = reader.GetInt32(4);
                                    string userKey = reader.GetString(5);
                                    string registrationDate = reader.GetString(6);
                                    
                                    // 打印账号信息
                                    Console.WriteLine("========== 随机账号信息 ==========");
                                    Console.WriteLine($"ID: {id}");
                                    Console.WriteLine($"邮箱: {email}");
                                    Console.WriteLine($"密码: {password}");
                                    Console.WriteLine($"用户名: {username}");
                                    Console.WriteLine($"用户ID: {userId}");
                                    Console.WriteLine($"用户密钥: {userKey}");
                                    Console.WriteLine($"注册日期: {registrationDate}");
                                    Console.WriteLine("================================");
                                    
                                    // 生成登录URL
                                    string loginUrl = $"https://z-library.se/elogin.php?remix_userid={userId}&remix_userkey={userKey}";
                                    Console.WriteLine($"一键登录URL: {loginUrl}");
                                    
                                    // 询问是否要将此账号标记为已使用
                                    Console.WriteLine("是否要将此账号标记为已使用? (y/n)");
                                    var markKey = Console.ReadKey(true);
                                    if (markKey.KeyChar == 'y' || markKey.KeyChar == 'Y')
                                    {
                                        // 添加LastUsed字段，如果不存在的话
                                        try
                                        {
                                            string addColumnSql = "ALTER TABLE Accounts ADD COLUMN LastUsed TEXT";
                                            using (var addColumnCommand = new SQLiteCommand(addColumnSql, connection))
                                            {
                                                addColumnCommand.ExecuteNonQuery();
                                            }
                                        }
                                        catch
                                        {
                                            // 列已存在，忽略错误
                                        }
                                        
                                        // 更新LastUsed字段
                                        string updateSql = "UPDATE Accounts SET LastUsed = @LastUsed WHERE Id = @Id";
                                        using (var updateCommand = new SQLiteCommand(updateSql, connection))
                                        {
                                            updateCommand.Parameters.AddWithValue("@LastUsed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                            updateCommand.Parameters.AddWithValue("@Id", id);
                                            updateCommand.ExecuteNonQuery();
                                        }
                                        
                                        Console.WriteLine("账号已标记为已使用");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("无法获取随机账号");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取随机账号时出错: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}