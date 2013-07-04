using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace FolderNode
{
    /// <summary>
    /// 文件大小尺寸转换器（即将字节数转换为G、M、K等字符串）
    /// </summary>
    public class FileSizeConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long fileSize = (long)value;
            return FileUtils.FileSizeFormater(fileSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
