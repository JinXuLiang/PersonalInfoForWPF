using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace WPFUserControlLibrary
{
    /// <summary>
    /// 当Tab页面被关闭时，激发TabPageClosed事件，这是事件的参数对象
    /// </summary>
    public class TabPageClosedEventArgs:EventArgs
    {
        /// <summary>
        /// 被关闭的TabItem的索引
        /// </summary>
        public int TabItemIndex { get; set; }
        /// <summary>
        /// 被关闭的TabItem对象
        /// </summary>
        public TabItem ClosedTabItem { get; set; }

       
    }
}
