using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicLibrary.Utils
{
    /// <summary>
    /// 封装一些与字符串相关的辅助功能
    /// </summary>
    public class StringUtils
    {
        /// <summary>
        /// 将一个字串转换为字节数组长，使用UTF8编码
        /// 如果字串为空，返回null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] getBytesUsingUTF8(String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// 从字节数组中创建字符串对象，使用UTF8编码
        /// </summary>
        /// <param name="stringData"></param>
        /// <returns></returns>
        public static String getStringUsingUTF8(byte[] stringData)
        {
            if (stringData == null || stringData.Length == 0)
            {
                return "";
            }
            return Encoding.UTF8.GetString(stringData);
        }

        /// <summary>
        /// 获取指定字符串的第一行内容，其方法是查找\r或\n的位置
        /// 如果只有一个，则直接依据其索引从0开始截取子串即可
        /// 如果找到两个，则取最小的那个作为索此截取子串
        /// 如果没有找到，返回整个字串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static String getFirstLineOfString(String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }

            int indexOfR = str.IndexOf("\r");
            int indexOfN = str.IndexOf("\n");
            if (indexOfN == -1 && indexOfR == -1)
            {
                return str;
            }

            if (indexOfN != -1 && indexOfR != -1)
            {
                int length = (indexOfN < indexOfR ? indexOfR : indexOfN) - 1;
                return str.Substring(0, length);

            }

            if (indexOfN != -1 && indexOfR == -1)
            {
                return str.Substring(0, indexOfN - 1);

            }

            if (indexOfN == -1 && indexOfR != -1)
            {
                return str.Substring(0, indexOfR - 1);

            }
            return null;

        }
    }
}
