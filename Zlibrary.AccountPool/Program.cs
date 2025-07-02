using FreeMailReceiver;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Zlibrary.AccountPool
{
    class Program
    {
        // 用于存储已处理过的邮件ID
        private static HashSet<string> processedEmailIds = new HashSet<string>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("开始测试临时邮箱接收器...");
            
            try
            {
                // 创建接收器实例
                var receiver = new Receiver("eliorfoy");
                
                // 获取新邮箱地址
                Console.WriteLine("获取新邮箱地址...");
                string email = await receiver.RefreshEmailAddressAsync();
                Console.WriteLine($"新邮箱地址: {email}");
                Console.WriteLine("系统将自动监控此邮箱，等待新邮件到达...");
                Console.WriteLine("(按下 'q' 键退出程序)");
                
                // 创建取消令牌源
                var cts = new CancellationTokenSource();
                
                // 启动键盘监听任务
                var keyboardTask = Task.Run(() => {
                    while (true)
                    {
                        var key = Console.ReadKey(true);
                        if (key.KeyChar == 'q' || key.KeyChar == 'Q')
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
            
            Console.WriteLine("程序已退出，按任意键关闭窗口...");
            Console.ReadKey();
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
            var h1Match = System.Text.RegularExpressions.Regex.Match(content, @"<h1[^>]*>(\d+)<\/h1>");
            if (h1Match.Success)
            {
                return h1Match.Groups[1].Value;
            }
            
            // 方法2：查找任何看起来像验证码的4-6位数字
            var codeMatch = System.Text.RegularExpressions.Regex.Match(content, @"(\d{4,6})");
            if (codeMatch.Success)
            {
                return codeMatch.Groups[1].Value;
            }
            
            return null;
        }
    }
}