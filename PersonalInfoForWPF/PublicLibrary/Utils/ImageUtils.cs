using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PublicLibrary.Utils
{
    /// <summary>
    /// 用于封装一些与图片处理有关的功能
    /// </summary>
    public class ImageUtils
    {
        /// <summary>
        /// 将图像文件(使用相对路径）转为BitmapSource对象
        /// </summary>
        /// <param name="ImageFileFullName"></param>
        /// <returns></returns>
        public static BitmapSource GetBitmapSourceFromImageFileName(string ImageFileName, UriKind uriKind)
        {
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(ImageFileName, uriKind);
            myBitmapImage.EndInit();
            return myBitmapImage;
        }


    }
}
