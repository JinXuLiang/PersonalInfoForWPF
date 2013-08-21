using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFUserControlLibrary
{
    /// <summary>
    /// Interaction logic for InfoTabItem.xaml
    /// </summary>
    public partial class InfoTabControl : TabControl
    {
        public InfoTabControl()
        {
            InitializeComponent();
           
        }
    
        /// <summary>
        /// 添加新选项卡，指定选项卡标题和内容
        /// </summary>
        /// <param name="TabHeaderText"></param>
        /// <param name="content"></param>
        public void Add(String TabHeaderText,Object content)
        {
            TabItem tabItem = new TabItem();
            tabItem.Content = content;
            if (String.IsNullOrEmpty(TabHeaderText))
            {
                TabHeaderText = "NewTable " + Items.Count;
            }
            InfoTabHeader header = new InfoTabHeader(TabHeaderText);
            header.onClose += header_TabPageClose;
            tabItem.Header = header;
            Items.Add(tabItem);
            SelectedIndex = Items.Count-1;
         
        }

        /// <summary>
        /// 当页被关闭时，激发此事件
        /// </summary>
        public event EventHandler<TabPageClosedEventArgs> TabPageClosed;

        void header_TabPageClose(TabItem closedTabItem)
        {
            int index = Items.IndexOf(closedTabItem);
            if (index == -1)
            {
                return;
            }

            //激发事件
           
             TabPageClosedEventArgs args = new TabPageClosedEventArgs()
             {
                 TabItemIndex=index, ClosedTabItem=closedTabItem
             };
             if (TabPageClosed != null)
             {
                 TabPageClosed(closedTabItem, args);
             }
            //移除选项卡
             Items.RemoveAt(index);

        }

        public void RemoveTabItem(int index)
        {
            if (Items.Count > 0 && index >= 0 && index < Items.Count)
            {
                header_TabPageClose(Items.GetItemAt(index) as TabItem);
            }
        }
    }
}
