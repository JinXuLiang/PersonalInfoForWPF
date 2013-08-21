using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Markup;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;

namespace WPFSuperRichTextBox
{
    /// <summary>
    /// 辅助类，封装一些公用函数
    /// </summary>
   class MySuperEditorHelper
    {
        /// <summary>
        ///判断字串是否可打印内容（字母和数字，标点认为是可打印的）
        /// </summary>
        /// <param name="DocumentContent">要判断的字串</param>
        /// <returns></returns>
        public static  bool IsPrintableString(string value)
        {
            if (value.Length == 0)
                return false;
            char[] chs = value.ToCharArray();
            foreach (char c in chs)
            {
                if (Char.IsLetterOrDigit(c) || Char.IsPunctuation(c))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 获取根据当地文化(比如汉语)显示的字体名称
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static  string GetLocaliteFontName(FontFamily font)
        {
            if (font == null)
                return "";
            LanguageSpecificStringDictionary dic = font.FamilyNames;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            if (dic.ContainsKey(XmlLanguage.GetLanguage(cultureInfo.Name)))
                return font.FamilyNames[XmlLanguage.GetLanguage(cultureInfo.Name)];
            else
                return font.FamilyNames[XmlLanguage.GetLanguage("en-us")];
        }


        /// <summary>
        /// 从指定的文本流中读取文本，采用系统默认编码
        /// </summary>
        /// <param name="fs">包含文本的流</param>
        /// <returns></returns>
        public static string LoadStringFromTextFile(FileStream fs)
        {
            if (fs == null)
                return "";
            if (fs.CanRead == false)
                return "";
            if (fs.CanSeek)
                fs.Seek(0, SeekOrigin.Begin);

            StreamReader sr = new StreamReader(fs, Encoding.Default);
            string str = sr.ReadToEnd();
            return str;
        }


        /// <summary>
        /// 通过编码长度判断一个字符是英文还是汉字
        /// </summary>
        /// <param name="str">一个字串,只提取第一个字符</param>
        /// <returns>是汉字,返回true,否则,返回false,空字串也返回false</returns>
        public static bool IsTwoByteChineseChar(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            string c = str.Substring(0, 1);
            byte[] arr = Encoding.Default.GetBytes(c);
            return arr.Length == 2 ? true : false;

        }
    }
}
