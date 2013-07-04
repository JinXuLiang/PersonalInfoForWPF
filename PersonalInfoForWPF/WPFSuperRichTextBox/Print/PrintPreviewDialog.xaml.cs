using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFSuperRichTextBox
{
    /// <summary>
    /// PrintPreviewDialog.xaml 的交互逻辑
    /// </summary>
    partial class PrintPreviewDialog : Window
    {
        public PrintPreviewDialog()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 当前页码，是一个依赖属性
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", 
                typeof(int), 
                typeof(PrintPreviewDialog)
        );
        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        private readonly PrintManager _manager;
        private int _pageIndex;

        public PrintPreviewDialog(PrintManager printManager)
        {
            InitializeComponent();
            _manager = printManager;
            DataContext = this;
            ChangePage(0);//显示第一页
        }
        
        private void PreviousClick(object sender, RoutedEventArgs e)
        {
            ChangePage(_pageIndex - 1);
        }
        private void NextClick(object sender, RoutedEventArgs e)
        {
            ChangePage(_pageIndex + 1);
        }
        private void ChangePage(int requestedPage)
        {
            pageViewer.DocumentPaginator = _manager.GetPaginator(
            8.5 * PrintManager.DPI,
            11 * PrintManager.DPI
            );
            if (requestedPage < 0)
                _pageIndex = 0;
            else if (requestedPage >= pageViewer.DocumentPaginator.PageCount)
                _pageIndex = pageViewer.DocumentPaginator.PageCount - 1;
            else _pageIndex = requestedPage;
            pageViewer.PageNumber = _pageIndex;
            CurrentPage = _pageIndex + 1;
        }
    }
}
