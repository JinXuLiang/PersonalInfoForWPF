using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;

namespace PrintDocument
{
    class PrintingPaginator : DocumentPaginator
    {
        private readonly DocumentPaginator _originalPaginator;
        private readonly Size _pageSize;
        private readonly Size _pageMargin;

        public PrintingPaginator(
            DocumentPaginator paginator,
            Size pageSize,
            Size margin)
        {
            _originalPaginator = paginator;
            _pageSize = pageSize;
            _pageMargin = margin;

            _originalPaginator.PageSize = new Size(
                _pageSize.Width - _pageMargin.Width * 2,
                _pageSize.Height - _pageMargin.Height * 2
                );

            _originalPaginator.ComputePageCount();
        }

        public override bool IsPageCountValid
        {
            get { return _originalPaginator.IsPageCountValid; }
        }

        public override int PageCount
        {
            get { return _originalPaginator.PageCount; }
        }

        public override Size PageSize
        {
            get { return _originalPaginator.PageSize; }
            set { _originalPaginator.PageSize = value; }
        }

        public override IDocumentPaginatorSource Source
        {
            get { return _originalPaginator.Source; }
        }
        /// <summary>
        /// 每次打印一页时，此方法都会被调用一次
        /// 打印的内容由它提供
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public override DocumentPage GetPage(int pageNumber)
        {
            DocumentPage originalPage =
                _originalPaginator.GetPage(pageNumber);

            ContainerVisual fixedPage = new ContainerVisual();

            // 创建页眉 
            DrawingVisual header = new DrawingVisual();
            using (DrawingContext context = header.RenderOpen())
            {
                Typeface typeface = new Typeface("Times New Roman");
                FormattedText text = new FormattedText("第" + (pageNumber + 1).ToString()+"页",
                  CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                  typeface, 14, Brushes.Black);

                context.DrawText(text, new Point(0, 0));
            }

            //将页面主体下移0.25 inch,以腾出空间显示页眉
            ContainerVisual PageArea = new ContainerVisual();
            PageArea.Children.Add(originalPage.Visual);
            PageArea.Transform = new TranslateTransform(
               0,
               0.25*96
               );

            //组合页眉
            fixedPage.Children.Add(header);
            //组合页主体          
            fixedPage.Children.Add(PageArea);

            //平移全页，为Margin腾出空间
            fixedPage.Transform = new TranslateTransform(
                _pageMargin.Width,
                _pageMargin.Height
                );
           
            //根据新的纸张可打印区域生成调整后页面
            return new DocumentPage(
                fixedPage,
                _pageSize,
                AdjustForMargins(originalPage.BleedBox),
                AdjustForMargins(originalPage.ContentBox)
                );
        }

        private Rect AdjustForMargins(Rect rect)
        {
            if (rect.IsEmpty) return rect;
            else
            {
                //有Margin时，自动调整打印区域
                return new Rect(
                    rect.Left + _pageMargin.Width,
                    rect.Top + _pageMargin.Height,
                    rect.Width-_pageMargin.Width,
                    rect.Height-_pageMargin.Height
                    );
            }
        }
    }

}
