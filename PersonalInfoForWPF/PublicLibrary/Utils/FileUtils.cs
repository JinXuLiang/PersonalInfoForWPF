using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicLibrary.Utils
{
    /// <summary>
    /// 封装一些基类库没提供的，与文件相关的一些功能
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// 将文件尺寸转换为G,M,K和字节的字符串
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static String FileSizeFormater(long fileSize)
        {
            

            double temp = 0;
            //大于1k,小于1M
            if ((fileSize >= 1024) && (fileSize < 1024 * 1024))
            {
                temp = (double)fileSize / 1024;
                return temp.ToString("N") + "K";
            }
            //大于1M，小于1G
            if ((fileSize >= 1024 * 1024) && (fileSize < 1024 * 1024 * 1024))
            {
                temp = (double)fileSize / (1024 * 1024);
                return temp.ToString("N") + "M";
            }
            //大于1G
            if ((fileSize >= 1024 * 1024 * 1024))
            {
                temp = (double)fileSize / (1024 * 1024 * 1024);
                return temp.ToString("N") + "G";
            }
            //小于1K的，返回真实大小
            return fileSize.ToString() + "字节";
        }
    }
}
