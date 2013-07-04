using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace WPFSuperRichTextBox
{
    /// <summary>
    /// 自定义一些标准预定义命令中没有的命令，专用于此应用程序
    /// </summary>
    public class MySuperEditorCommands
    {
        /// <summary>
        /// 打开文档
        /// </summary>
        public static RoutedUICommand OpenDocument = new RoutedUICommand();

        /// <summary>
        /// 插入文档
        /// </summary>
        public static RoutedUICommand InsertDocument = new RoutedUICommand();

        /// <summary>
        /// 导出选择的部分到文件
        /// </summary>
        public static RoutedUICommand ExportSelectionToFile = new RoutedUICommand();

        /// <summary>
        /// 显示流文档的XAML代码
        /// </summary>
        public static RoutedCommand ShowDocumentXAML = new RoutedCommand();

        /// <summary>
        /// 根据流文档的XAML代码重新刷新显示RichTextBox
        /// </summary>
        public static RoutedCommand RefreshRichTextBoxFromXAML = new RoutedCommand();
        /// <summary>
        /// 以默认格式粘贴
        /// </summary>
        public static RoutedCommand PasteAndClearAllProperties = new RoutedCommand();
    }

  
}
