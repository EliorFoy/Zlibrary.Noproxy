# Zlibrary.Noproxy Tool 类说明文档

## 概述
Tool.cs 是 Zlibrary.Noproxy 项目的核心工具类，提供与 Z-Library 服务交互的主要功能，包括书籍搜索、下载以及相关网络请求处理。该类实现了自定义的 HTTP 客户端配置，支持绕过常规代理限制直接连接到 Z-Library 服务。

## 主要功能
- 自定义 HTTP 客户端配置，支持 SSL 证书验证禁用和直接 IP 连接
- 域名 IP 地址解析与管理
- 书籍信息搜索与解析
- 带进度显示的书籍下载功能
- 文件大小和时间格式化工具方法

## 核心组件

### BookInfo 类
用于存储书籍信息的模型类，包含以下属性：
- Id: 书籍唯一标识符
- Isbn: ISBN 编号
- Title: 书名
- Author: 作者
- DownloadUrl: 下载链接
- DetailUrl: 详情页链接
- Publisher: 出版社
- Language: 语言
- Year: 出版年份
- Extension: 文件格式
- FileSize: 文件大小
- Rating: 评分
- Quality: 质量
- ImageUrl: 封面图片 URL

### Tool 静态类
提供主要功能实现的静态工具类，包含以下核心方法：

#### 网络配置方法
- `GetIpAddress(string hostname = "z-library.sk", bool useCookie = false)`: 查询指定域名的 IP 地址
- `SetDefaultClient(HttpClient client)`: 配置 HTTP 客户端默认请求头
- `CreateNewClient(bool allowAutoRedirect = true, int timeout = 10)`: 创建新的 HTTP 客户端实例

#### HTTP 请求方法
- `Get(string url)`: 发送 GET 请求
- `Post(string url, Dictionary<string, string> formData)`: 发送表单 POST 请求
- `PostJson(string url, object data)`: 发送 JSON 格式 POST 请求

#### 书籍操作方法
- `SearchBooks(string query, int page = 1)`: 搜索书籍并返回 BookInfo 列表
- `DownloadBook(BookInfo book, string savePath)`: 下载指定书籍到本地路径
- `DownloadBook(string downloadUrl, string savePath, string suggestedFileName = null, string suggestedExtension = null)`: 通过 URL 下载书籍

#### 辅助方法
- `ParseBooksFromHtml(string html)`: 从 HTML 内容解析书籍信息
- `FormatFileSize(long bytes)`: 格式化文件大小显示
- `FormatTime(double seconds)`: 格式化时间显示
- `DownloadFileWithProgressAsync(HttpClient client, string url, string destinationPath)`: 带进度条的文件下载

## 使用示例

### 搜索书籍
```csharp
var books = await Tool.SearchBooks("dotnet", 1);
foreach (var book in books)
{
    Console.WriteLine(book.ToString());
}
```

### 下载书籍
```csharp
string savePath = @"C:\Books\Downloads";
var books = await Tool.SearchBooks("C# programming");
if (books.Any())
{
    bool success = await Tool.DownloadBook(books[0], savePath);
    Console.WriteLine(success ? "下载成功" : "下载失败");
}
```

## 配置说明
- `ORIGIN_DOMAIN`: 默认域名，默认为 "z-library.sk"
- `TARGET_DOMAIN`: SSL 目标主机名
- `ACTUAL_IP`: Z-Library 实际 IP 地址，需要根据实际情况配置

## 注意事项
1. 该工具类禁用了 SSL 证书验证，可能存在安全风险
2. IP 地址初始化是异步进行的，首次使用前建议确保 IP 已正确解析
3. 下载功能包含详细的进度显示和错误处理
4. 默认 HTTP 超时时间为 10 秒，大文件下载可适当调整

## 进度条样式
支持多种进度条样式，可通过 `SetProgressBarStyle` 方法设置，可选样式包括：
- Block (默认)
- Simple
- Equals
- Arrow
- Block2
- Block3
- Star