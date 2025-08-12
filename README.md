# Z-Library NoProxy

Z-Library NoProxy 是一个基于 .NET 的 Z-Library 客户端工具，允许用户绕过网络限制直接访问 Z-Library 的内容。该项目提供了多种平台的实现，包括命令行界面、Avalonia 桌面应用、MAUI 跨平台应用等。

## 功能特点

- 📚 搜索 Z-Library 上的电子书
- ⚡ 通过 IP 直连绕过网络限制
- 🔄 自动处理 SSL/TLS 连接和证书验证
- 📥 下载电子书到本地
- 📱 多平台支持（Windows、Linux、macOS、Android、iOS）
- 🎨 现代化 UI 界面（Avalonia 和 MAUI 版本）
- 🤖 自动账号注册和管理

## 项目结构

```
├── Zlibrary.Noproxy              # 核心库
├── Zlibrary.Noproxy.CLI          # 命令行界面版本
├── Zlibrary.Noproxy.Avalonia     # Avalonia 桌面应用版本
├── Zlibrary.Noproxy.Maui         # MAUI 跨平台应用版本
├── Zlibrary.AccountPool          # 账号池管理
└── FreeMailReceiver              # 临时邮箱接收器
```

## 核心组件

### Zlibrary.Noproxy (核心库)

核心库提供了以下功能：

- [Tool.cs](file:///c%3A/Users/DELL123/Desktop/dotnets/Zlibrary.Noproxy/Zlibrary.Noproxy/Tool.cs): 主要工具类，包含搜索、下载等核心功能
- [AccountPool.cs](file:///c%3A/Users/DELL123/Desktop/dotnets/Zlibrary.Noproxy/Zlibrary.Noproxy/AccountPool.cs): 账号池管理，自动注册和管理 Z-Library 账号

主要特性：
- 通过 IP 直连绕过 DNS 污染
- 自动处理 SSL/TLS 连接
- 解析书籍信息
- 下载电子书文件
- 进度显示和错误处理

### Zlibrary.AccountPool (账号池)

- 自动注册 Z-Library 账号
- 使用临时邮箱接收验证码
- SQLite 数据库存储账号信息
- 智能账号管理和轮换

### 命令行版本 (CLI)

基于控制台的 Z-Library 客户端，提供完整的搜索和下载功能：
- 交互式搜索界面
- 分页浏览搜索结果
- 选择并下载电子书
- 显示下载进度

### Avalonia 版本

现代化的桌面应用程序，具有图形用户界面：
- 搜索和浏览书籍
- 下载管理
- 日志查看
- 跨平台支持 (Windows, Linux, macOS)

### MAUI 版本

跨平台移动应用，支持 Android 和 iOS：
- 移动端优化的用户界面
- 触摸友好的交互设计
- 本地文件存储

## 使用方法

### 命令行版本

```bash
cd Zlibrary.Noproxy.CLI
dotnet run
```

运行后按照提示输入搜索关键词，选择书籍并下载。

### Avalonia 桌面版本

```bash
cd Zlibrary.Noproxy.Avalonia/Zlibrary.Noproxy.Avalonia.Desktop
dotnet run
```

### MAUI 移动版本

```bash
cd Zlibrary.Noproxy.Maui
dotnet build -t:Run -f net8.0-android
```

## 技术细节

### 网络连接

项目通过以下方式处理网络连接：

1. 使用 `SocketsHttpHandler` 直接连接到 Z-Library 的 IP 地址
2. 手动处理 SSL/TLS 握手，绕过证书验证问题
3. 伪装 SNI 信息以避免被识别和阻止

### 账号系统

账号系统通过以下方式工作：

1. 使用临时邮箱服务接收验证邮件
2. 自动解析邮件中的验证码
3. 完成账号注册并保存到本地 SQLite 数据库
4. 智能轮换使用账号，避免单个账号被限制

### 下载功能

下载功能包括：

1. 通过账号认证获取下载链接
2. 处理重定向获取真实下载地址
3. 显示下载进度和速度
4. 自动命名和保存文件

## 注意事项

1. 本项目仅供学习和研究使用
2. 请遵守当地法律法规
3. 尊重知识产权，合理使用电子书资源
4. 不要滥用账号注册功能，避免给服务造成负担

## 许可证

本项目基于 GNU General Public License v3.0 许可证发布，这意味着：

1. 任何使用、修改、再分发的衍生作品都必须整体继续使用 GPL-3.0 开源
2. 必须提供完整源代码
3. 禁止将本项目用于闭源商业用途
4. 任何包含本项目代码的软件都必须同样采用 GPL-3.0 许可证开源

详情请查看 [LICENSE](file:///c%3A/Users/DELL123/Desktop/dotnets/Zlibrary.Noproxy/LICENSE.txt) 文件。