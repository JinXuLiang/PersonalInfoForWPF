using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PublicLibrary.Security
{
    /// <summary>
    /// 提供一些与安全相关的公用方法
    /// </summary>
    public static class SecurityUtils
    {
        /// <summary>
        /// 针对一个字串值生成MD5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("要散列的数据不能为空");
            }
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            md5Hasher.Clear();
            return sBuilder.ToString();
        }
        /// <summary>
        /// 计算一个密码的Hash值
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        /// <summary>
        /// 比较用户传来的密码与本地保存的Hash码是否一致
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                throw new ArgumentNullException("hashedPassword");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((a == null) || (b == null)) || (a.Length != b.Length))
            {
                return false;
            }
            bool flag = true;
            for (int i = 0; i < a.Length; i++)
            {
                flag &= a[i] == b[i];
            }
            return flag;
        }


        /// <summary>
        /// 使用对称算法加密，密钥至少要8位，字符串采用UTF8编码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encryptKey"></param>
        /// <returns></returns>
        public static string SymmetricEncrypts(string str, string encryptKey)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new CryptographicException("要加密的数据不能为空");
            }
            if (String.IsNullOrEmpty(encryptKey) || encryptKey.Length < 8)
            {
                throw new CryptographicException("密钥长度至少为8位");
            }
            string result = string.Empty;
            byte[] inputData = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] IV = { 0x77, 0x70, 0x50, 0xD9, 0xE1, 0x7F, 0x23, 0x13, 0x7A, 0xB3, 0xC7, 0xA7, 0x48, 0x2A, 0x4B, 0x39 };

            byte[] byKey = System.Text.Encoding.UTF8.GetBytes(encryptKey);
            //如需指定加密算法，可在Create()参数中指定字符串
            //Create()方法中的参数可以是：DES、RC2 System、Rijndael、TripleDES 
            //采用不同的实现类对IV向量的要求不一样(可以用GenerateIV()方法生成)，无参数表示用Rijndael
            SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create();//产生一种加密算法
            MemoryStream msTarget = new MemoryStream();
            //定义将数据流链接到加密转换的流。
            CryptoStream encStream = new CryptoStream(msTarget, Algorithm.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
            encStream.Write(inputData, 0, inputData.Length);
            encStream.FlushFinalBlock();
            result = Convert.ToBase64String(msTarget.ToArray());


            return result;
        }

        /// <summary>
        /// 使用对称算法解密，字符串采用Base64和UTF8编码
        /// 非法的Base64编码，抛出FormatException
        /// 解密失败时，抛出CryptographicException
        /// 
        /// </summary>
        /// <param name="encryptStr"></param>
        /// <param name="encryptKey"></param>
        /// <returns></returns>
        public static string SymmectricDecrypts(string encryptStr, string encryptKey)
        {
            if (String.IsNullOrEmpty(encryptStr))
            {
                throw new CryptographicException("要解密的数据不能为空");
            }
            if (String.IsNullOrEmpty(encryptKey) || encryptKey.Length < 8)
            {
                throw new CryptographicException("密钥长度至少为8位");
            }
            string result = string.Empty;
            //加密时使用的是Convert.ToBase64String(),解密时必须使用Convert.FromBase64String()

            byte[] encryptData = Convert.FromBase64String(encryptStr);
            byte[] byKey = System.Text.Encoding.UTF8.GetBytes(encryptKey);
            byte[] IV = { 0x77, 0x70, 0x50, 0xD9, 0xE1, 0x7F, 0x23, 0x13, 0x7A, 0xB3, 0xC7, 0xA7, 0x48, 0x2A, 0x4B, 0x39 };
            SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create();
            MemoryStream msTarget = new MemoryStream();
            CryptoStream decStream = new CryptoStream(msTarget, Algorithm.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
            decStream.Write(encryptData, 0, encryptData.Length);
            decStream.FlushFinalBlock();
            result = System.Text.Encoding.UTF8.GetString(msTarget.ToArray());

            return result;
        }


    }
}
