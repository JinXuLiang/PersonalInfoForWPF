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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFUserControlLibrary
{
    /// <summary>
    /// Interaction logic for SuperTextBox.xaml
    /// </summary>
    public partial class SuperTextBox : UserControl
    {
        public SuperTextBox()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 设定文本框的文本
        /// </summary>
        public String Text
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }


        //public String Text
        //{
        //    get { 
                
        //        return textBox1.Text; }
        //    set {
        //        textBox1.Text = value;
        //        SetValue(TextProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TextProperty =
        //    DependencyProperty.Register("Text", typeof(String), typeof(SuperTextBox), new PropertyMetadata(""));


        /// <summary>
        /// 是否只允许输入整数
        /// 为false时，可以输入小数点，为True时，只允许输入数字
        /// </summary>
        public bool IsInteger
        {
            get { return (bool)GetValue(IsIntegerProperty); }
            set { SetValue(IsIntegerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInteger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsIntegerProperty =
            DependencyProperty.Register("IsInteger", typeof(bool), typeof(SuperTextBox), new PropertyMetadata(false));


        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;

            //屏蔽非法按键
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal)
            {
                if (IsInteger)
                {
                    if (e.Key == Key.Decimal)
                    {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                }
                else
                {
                    if (txt.Text.Contains(".") && e.Key == Key.Decimal)
                    {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                }
                
            }
            else if (((e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                if (IsInteger)
                {
                    if (e.Key == Key.OemPeriod)
                    {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                }
                else
                {
                    if (txt.Text.Contains(".") && e.Key == Key.OemPeriod)
                    {
                        e.Handled = true;
                        return;
                    }
                    e.Handled = false;
                }
                
            }
            else
            {
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //屏蔽中文输入和非法字符粘贴输入
            TextBox textBox = sender as TextBox;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textBox.Text, out num))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                }
            }
        }

        

        private void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        

       
    }
}
