<p align="center">
  <img src="https://i.ibb.co/QKrMYFb/Leonardo-Phoenix-Create-a-visually-striking-banner-for-a-Git-Hu-3.jpg" alt="Discord Username Tool Banner" width="75%">
</p>

# Discord Username Tool 🚀

A fast and efficient Discord username tool that allows you to generate and check the availability of usernames. Built with performance in mind, featuring multi-threading, proxy support, and real-time status updates.

⚡ **Looking for the program without building it?**  
→ [Click here to download the latest release](https://github.com/gxqk/DiscordUsernameChecker/releases/latest)

![Version](https://img.shields.io/badge/version-1.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Love](https://img.shields.io/badge/Coded%20with-❤-red)

[![Need Help?](https://img.shields.io/badge/Need%20Help%3F-Click%20Here-orange)](#-support)

## 📥 Installation

1. Install [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime)
2. Install [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
3. Open the `DiscordUsernameChecker.sln` file
4. At the top of Visual Studio, select `Release` and `x64`
5. Click on `Build` then `Build Solution`
6. The compiled program is located in `bin/Release/net8.0/`

All necessary files (config.ini, proxies.txt) are included in the project.

## ⚙️ Configuration

```ini
[AppSettings]
Threads=10    # Number of simultaneous threads
Debug=false   # Debug mode for logging
```

## 📁 Features

### Username Generator
- Complete customization (3-32 characters)
- Special characters support
- Automatic results export

### Availability Checker
- Multi-thread
- Proxy support (HTTP/SOCKS5/SOCKS4)
- Real-time display
- Available usernames export

## 📁 Structure

- `config.ini` - Configuration
- `proxies.txt` - Proxy list (ip:port or ip:port:user:pass)
- `usernames.txt` - Usernames to check
- `output/` - Results and logs

## ⚠️ Important Notes

- Respect Discord rate limits
- Use quality proxies
- Maximum 32 characters per username
- Only one dot (.) and underscore (_) per username

## 💬 Support
For any support or questions, add me on Discord: gxqk

## 🤝 Author
- [gxqk]

## ⚖️ Legal Disclaimer
This tool is provided for educational purposes only. Usage must comply with Discord's Terms of Service.
