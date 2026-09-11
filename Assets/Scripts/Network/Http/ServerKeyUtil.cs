using System;
using System.Security.Cryptography;
using System.Text;

namespace AChen.Networking
{
    /// <summary>把后端地址映射为稳定短键, 用于按服务器隔离本地缓存与会话文件.</summary>
    public static class ServerKeyUtil
    {
        public static string Create(string baseUrl)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseUrl));
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
