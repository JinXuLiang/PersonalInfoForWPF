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
using System.Windows.Media.Imaging;

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

       /// <summary>
       /// 从设备无关位图中创建Bitmap
       /// 当剪贴板上是图片时，其格式中有Dib格式。
       /// </summary>
       /// <param name="dib"></param>
       /// <returns></returns>
        public static System.Drawing.Bitmap CreateBitmapFromDib(Stream dib)
        {

            BinaryReader reader = new BinaryReader(dib);



            int headerSize = reader.ReadInt32();

            int pixelSize = (int)dib.Length - headerSize;

            int fileSize = 14 + headerSize + pixelSize;



            MemoryStream bmp = new MemoryStream(fileSize);

            BinaryWriter writer = new BinaryWriter(bmp);



            // 1. 把位图的一些元数据写进去，下面这几次Write相当于填写Win32的

            // BITMAPFILEHEADER结构        

            writer.Write((byte)'B');

            writer.Write((byte)'M');

            writer.Write(fileSize);

            writer.Write((int)0);

            writer.Write(14 + headerSize);



            // 2. 把DIB位图中的像素矩阵拷贝出来到我们指定的MemoryStream里。 

            // 因为我们要从MemoryStream里面生成System.Drawing.Bitmap对象

            // 然后再颇为曲折地从Bitmap对象生成WPF的BitmapImage对象

            dib.Position = 0;

            byte[] data = new byte[(int)dib.Length];

            dib.Read(data, 0, (int)dib.Length);

            writer.Write(data, 0, (int)data.Length);



            // 3. 生成Bitmap对象—这个是Winform里面的Bitmap对象

            bmp.Position = 0;

            var bp = new System.Drawing.Bitmap(bmp);
            writer.Dispose();
            bmp.Close();
            data = null;
            return bp;

        }
       /// <summary>
       /// 从Bitmap创建可供WPF Image控件使用的BitmapSource对象
       /// </summary>
       /// <param name="bitmap"></param>
       /// <returns></returns>
        public static BitmapSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {

            BitmapSource destination;

            IntPtr hBitmap = bitmap.GetHbitmap();

            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();

            destination = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, sizeOptions);

            destination.Freeze();

            return destination;

        }
    }
}
