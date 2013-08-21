using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFUserControlLibrary
{
    /// <summary>
    /// BrushChanged事件参数
    /// </summary>
    public class BrushChangeEventArgs : EventArgs
    {
        public Brush selectedBrush { get; set; }
    }
    /// <summary>
    /// 显示一个由纯色色块填充的画刷列表，当用户选择一项时，激发BrushChanged事件，在事件参数中可以获取用户选取的画刷
    /// 此画刷列表放置于一个ItemsControl中。
    /// </summary>
    public class ColorBrushList
    {
        /// <summary>
        /// 用于填充矩形的颜色刷子
        /// </summary>
        private Brush[] brushes = { 
                              Brushes.Aquamarine,
                              Brushes.Black,
                              Brushes.BlanchedAlmond,
                              Brushes.Blue,
                              Brushes.BlueViolet,
                              Brushes.Brown,
                              Brushes.BurlyWood,
                              Brushes.CadetBlue,
                              Brushes.Chartreuse,
                              Brushes.Chocolate,
                              Brushes.Coral,
                              Brushes.Crimson,
                              Brushes.Cyan,
                              Brushes.DarkBlue,
                              Brushes.DarkCyan,
                              Brushes.DarkGoldenrod,
                              Brushes.DarkGray,
                              Brushes.DarkGreen,
                              Brushes.DarkKhaki,
                              Brushes.DarkMagenta,
                              //Brushes.DarkOliveGreen,
                              //Brushes.DarkOrange,
                              //Brushes.DarkOrchid,
                              //Brushes.DarkRed,
                              //Brushes.DarkSalmon,
                              //Brushes.DarkSeaGreen,
                              //Brushes.DarkSlateBlue,
                              //Brushes.DarkSlateGray,
                              //Brushes.DarkTurquoise,
                              //Brushes.DarkViolet,
                              //Brushes.DeepPink,
                              //Brushes.DeepSkyBlue,
                              //Brushes.DimGray,
                              //Brushes.DodgerBlue,
                              //Brushes.Firebrick,
                              //Brushes.ForestGreen,
                              //Brushes.Fuchsia,
                              //Brushes.Gold,
                              //Brushes.Goldenrod,
                              //Brushes.Gray,
                              //Brushes.Green,
                              //Brushes.GreenYellow,
                              //Brushes.HotPink,
                              //Brushes.IndianRed,
                              //Brushes.Indigo,
                              //Brushes.Khaki,
                              //Brushes.LawnGreen,
                              //Brushes.Lime,
                              //Brushes.LimeGreen,
                              //Brushes.Magenta,
                              //Brushes.Maroon,
                              //Brushes.MediumAquamarine,
                              //Brushes.MediumBlue,
                              //Brushes.MediumOrchid,
                              //Brushes.MediumPurple,
                              //Brushes.MediumSeaGreen,
                              //Brushes.MediumSlateBlue,
                              //Brushes.MediumSpringGreen,
                              //Brushes.MediumTurquoise,
                              //Brushes.MediumVioletRed,
                              //Brushes.MidnightBlue,
                              //Brushes.Navy,
                              //Brushes.Olive,
                              //Brushes.OliveDrab,
                              //Brushes.Orange,
                              //Brushes.OrangeRed,
                              //Brushes.Orchid,
                              //Brushes.SaddleBrown,
                              //Brushes.PaleGoldenrod,
                              //Brushes.PaleGreen,
                              //Brushes.PaleTurquoise,
                              //Brushes.PaleVioletRed,
                              //Brushes.Peru,
                              //Brushes.Pink,
                              //Brushes.Plum,
                              //Brushes.PowderBlue,
                              //Brushes.Purple,
                              //Brushes.Red,
                              //Brushes.RosyBrown,
                              //Brushes.RoyalBlue,
                              //Brushes.Salmon,
                              //Brushes.SandyBrown,
                              //Brushes.SeaGreen,
                              //Brushes.Sienna,
                              //Brushes.Silver,
                              //Brushes.SkyBlue,
                              //Brushes.SlateBlue,
                              //Brushes.SlateGray,
                              //Brushes.SpringGreen,
                              //Brushes.SteelBlue,
                              //Brushes.Tan,
                              //Brushes.Teal,
                              //Brushes.Thistle,
                              //Brushes.Tomato,
                              //Brushes.Turquoise,
                              //Brushes.Violet,
                              //Brushes.Wheat,
                              //Brushes.Yellow,
                              //Brushes.YellowGreen
                                  };
        //当点击时的事件处理
        public event EventHandler<BrushChangeEventArgs> BrushChanged = null;
        /// <summary>
        /// 用于承载颜色列表的容器
        /// </summary>
        private ItemsControl _container = null;

        public ColorBrushList(ItemsControl container)
        {
            _container = container;
            init();
        }

        private void init()
        {

            foreach (var item in brushes)
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;


                Rectangle rect = new Rectangle();
                rect.Width = 100;
                rect.Height = 15;
                rect.Fill = item;
                rect.Margin = new Thickness(2);
                rect.Stroke = Brushes.Black;
                rect.Tag = item;
                panel.Children.Add(rect);

                TextBlock tb = new TextBlock();
                tb.Text = item.ToString();

                panel.Children.Add(tb);
                _container.Items.Add(panel);
                rect.MouseDown += rect_MouseDown;
            }
        }

        private void rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BrushChangeEventArgs arg = new BrushChangeEventArgs();
            arg.selectedBrush = (sender as Rectangle).Fill;
            if (BrushChanged != null)
            {
                BrushChanged(sender, arg);
            }
        }

    }
}
