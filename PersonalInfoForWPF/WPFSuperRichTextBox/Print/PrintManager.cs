using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Printing;
using System.IO;
using System.Windows;
using PrintDocument;

namespace WPFSuperRichTextBox
{
    /// <summary>
    /// 提供打印RichTextBox中文档的功能
    /// </summary>
    public class PrintManager
    {
        public static readonly int DPI = 96;
        private readonly RichTextBox _textBox;
        public PrintManager(RichTextBox textBox)
        {
            _textBox = textBox;
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <returns></returns>
        public bool Print()
        {
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                //获取用户所选择的打印机
                PrintQueue printQueue = dlg.PrintQueue;
                //UserPrintTicket代表了特定打印机的相关参数
                DocumentPaginator paginator = GetPaginator(
                printQueue.UserPrintTicket.PageMediaSize.Width.Value,
                printQueue.UserPrintTicket.PageMediaSize.Height.Value
                );
                //打印
                dlg.PrintDocument(paginator, "TextEditor Printing");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取文档分页器
        /// </summary>
        /// <param name="pageWidth"></param>
        /// <param name="pageHeight"></param>
        /// <returns></returns>
        public DocumentPaginator GetPaginator(double pageWidth,double pageHeight)
        {
            //将RichTextBox的文档内容转为XAML
            TextRange originalRange = new TextRange(
            _textBox.Document.ContentStart,
            _textBox.Document.ContentEnd
            );
            MemoryStream memoryStream = new MemoryStream();
            originalRange.Save(memoryStream, System.Windows.DataFormats.XamlPackage);
            
            //根据XAML将流文档复制一份
            FlowDocument copy = new FlowDocument();

            TextRange copyRange = new TextRange(
            copy.ContentStart,
            copy.ContentEnd
            );
            copyRange.Load(memoryStream, System.Windows.DataFormats.XamlPackage);


            DocumentPaginator paginator =
            ((IDocumentPaginatorSource)copy).DocumentPaginator;

            //转换为新的分页器
            return new PrintingPaginator(
            paginator,new Size( pageWidth,pageHeight),
            new Size(DPI,DPI)
            );
        }

        /// <summary>
        /// 打印预览
        /// </summary>
        public void PrintPreview()
        {
            PrintPreviewDialog dlg = new PrintPreviewDialog(this);
            dlg.ShowDialog();
        }
    }
}

