using DataAccessLayer.MainTree;
using InterfaceLibrary;
using NodeFactoryLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using WPFSuperTreeView;

namespace PersonalInfoForWPF
{
    /// <summary>
    /// 用于放置一些从MainWindows中移出的代码，以减少主窗体的代码量
    /// </summary>
    public partial class MainWindow
    {
        #region "更换节点类型"


        /// <summary>
        /// 从纯文本型转换为详细信息型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToDetailText(object sender, ExecutedRoutedEventArgs e)
        {

            curDbInfoTab.ChangeNodeType("DetailText");


        }

        

        private void ToOnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.ChangeNodeType("OnlyText");
        }
        private void ToFolder(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.ChangeNodeType("Folder");
        }

        #endregion

    }
}
