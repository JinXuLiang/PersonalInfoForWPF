using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// winFindAndReplace.xaml 的交互逻辑
    /// </summary>
    partial class winFindAndReplace : Window
    {
        public winFindAndReplace(RichTextBox ctl)
        {
            InitializeComponent();
            rtb = ctl;
            manager = new FindAndReplaceManager(rtb.Document);
           
            searchStringBox.ItemsSource =SuperRichTextBoxResourses.searchStrings;
            replaceStringBox.ItemsSource =SuperRichTextBoxResourses.replaceStrings;
        }
        /// <summary>
        /// 用于控制是否关闭窗体
        /// </summary>
        public bool CloseTag = false;

        /// <summary>
        ///从外界传入的RichTextBox控件
        /// </summary>
        private RichTextBox rtb = null;

        /// <summary>
        /// 用于实现搜索和替换功能的功能对象
        /// </summary>
        private FindAndReplaceManager manager = null;

        /// <summary>
        /// 查找选项
        /// </summary>
        private FindOptions findOptions = new FindOptions();

        //private static ObservableCollection<String> searchStrings = null;
        //private static ObservableCollection<String> replaceStrings = null;

        private void FindText(object sender, RoutedEventArgs e)
        {
            String findText = searchStringBox.Text;
            if (String.IsNullOrEmpty(findText))
            {
                return;
            }
            //设置搜索选项
            findOptions.MatchCase = chkMatchCase.IsChecked.Value;
            findOptions.MatchWholeWord = chkMatchWholeWord.IsChecked.Value;

            manager.CurrentPosition = rtb.CaretPosition;

            TextRange textRange = manager.FindNext(findText, findOptions);
            if (textRange != null)
            {
                rtb.Focus();
                rtb.Selection.Select(textRange.Start, textRange.End);
            }
            else
            {
                if (manager.CurrentPosition.CompareTo(rtb.Document.ContentEnd) == 0)
                {
                    MessageBox.Show("从当前光标处开始，已搜索完全部文档。下一次搜索将从文档起始处进行。", "提示信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    rtb.CaretPosition = rtb.Document.ContentStart;
                    manager.CurrentPosition = rtb.CaretPosition;
                }
            }
            addStringToSearchStrings(findText);
        }

        private void ReplaceText(object sender, RoutedEventArgs e)
        {
            String findText = searchStringBox.Text;
            String replaceText = replaceStringBox.Text;
            if (String.IsNullOrEmpty(findText))
            {
                return;
            }
            //如果要替换的文本与原文本完全一致，不进行操作
            if (replaceText == findText)
            {
                return;
            }
            //如果要替换的文本与原文本忽略大小写后完全一致，则认为必须区分大小写的
            if (replaceText.ToUpper() == findText.ToUpper())
            {
                chkMatchCase.IsChecked = true;
            }

            //如果当前选中的就是要替换的字串，直接替换之
            if (rtb.Selection.IsEmpty == false && rtb.Selection.Text==findText)
            {
                rtb.Selection.Text = replaceText;
                addStringToSearchStrings(findText);
                AddStringToReplaceStrings(replaceText);
                //“联动”查找与替换词语
                SuperRichTextBoxResourses.SearchReplaceDictionary[findText] = replaceText;
                rtb.Focus();
                return;
            }


            manager.CurrentPosition = rtb.CaretPosition;

            //设置搜索选项
            findOptions.MatchCase = chkMatchCase.IsChecked.Value;
            findOptions.MatchWholeWord = chkMatchWholeWord.IsChecked.Value;

            TextRange textRange = manager.Replace(findText, replaceText, findOptions);
            if (textRange != null)
            {
                rtb.Focus();
                rtb.Selection.Select(textRange.Start, textRange.End);
            }
            else
            {
                if (manager.CurrentPosition.CompareTo(rtb.Document.ContentEnd) == 0)
                {
                    MessageBox.Show("己处理完全部文档，没有找到要替换的文字。");
                    rtb.CaretPosition = rtb.Document.ContentStart;
                    manager.CurrentPosition = rtb.CaretPosition;
                }
            }
            //将用户输入的替换字串保存起来
            AddStringToReplaceStrings(replaceText);
            addStringToSearchStrings(findText);
            //“联动”查找与替换词语
            SuperRichTextBoxResourses.SearchReplaceDictionary[findText] = replaceText;
        }
        /// <summary>
        /// 将一个字串加入到替换字串集合中，表示用户使用过此字串进行替换操作
        /// </summary>
        /// <param name="replaceText"></param>
        private void AddStringToReplaceStrings(String replaceText)
        {
            if (String.IsNullOrEmpty(replaceText) == false)
            {
                int index = SuperRichTextBoxResourses.replaceStrings.IndexOf(replaceText);
                if (index == -1)
                {
                    SuperRichTextBoxResourses.replaceStrings.Insert(0, replaceText);
                }
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseTag == false)  //未设置关闭标记,不允许关闭窗体
            {
                Visibility = Visibility.Collapsed; //改关闭为隐藏
                e.Cancel = true;
            }
        }

        private void btnReplaceAll_Click(object sender, RoutedEventArgs e)
        {

            String findText = searchStringBox.Text;
            String replaceText = replaceStringBox.Text;
            if (String.IsNullOrEmpty(findText))
            {
                return;
            }
            //如果要替换的文本与原文本完全一致，不进行操作
            if (replaceText == findText)
            {
                return;
            }
            //如果要替换的文本与原文本忽略大小写后完全一致，则认为必须区分大小写的
            if (replaceText.ToUpper() == findText.ToUpper())
            {
                chkMatchCase.IsChecked = true;
            }

            findOptions.MatchCase = chkMatchCase.IsChecked.Value;
            findOptions.MatchWholeWord = chkMatchWholeWord.IsChecked.Value;

            Cursor curCursor = this.Cursor;
            this.Cursor = Cursors.Wait;  //变换鼠标指针,提示用户等待
            int result = manager.ReplaceAll(findText, replaceText, findOptions);
            this.Cursor = curCursor;//还原鼠标指针
            

            //将用户输入的替换字串保存起来
            AddStringToReplaceStrings(replaceText);
            addStringToSearchStrings(findText);
            //“联动”查找与替换词语
            SuperRichTextBoxResourses.SearchReplaceDictionary[findText] = replaceText;
            MessageBox.Show("一共完成了" + result.ToString() + "处替换工作");

        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //当显示窗体时，自动地将RichTextBox中当前选中的文本加入到下拉列表中，为了避免太长的文本，最多取20个字符
            if (winFind.Visibility == Visibility.Visible && rtb.Selection.IsEmpty == false)
            {
                String selectedText=rtb.Selection.Text;
                String appendText=selectedText.Length>20?selectedText.Substring(0, 20):selectedText;
                addStringToSearchStrings(appendText);
            }
        }
        /// <summary>
        /// 将一个字串加入搜索历史列表中
        /// </summary>
        /// <param name="appendText"></param>
        private void addStringToSearchStrings(String appendText)
        {
            int index = SuperRichTextBoxResourses.searchStrings.IndexOf(appendText);
            if (index == -1)
            {
                SuperRichTextBoxResourses.searchStrings.Insert(0, appendText);
                searchStringBox.SelectedIndex = 0;
            }
            else
            {
                searchStringBox.SelectedIndex = index;
            }
        }

        private void searchStringBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchStringBox.SelectedItem != null)
            {
                String key = searchStringBox.SelectedItem.ToString();
                if (SuperRichTextBoxResourses.SearchReplaceDictionary.Keys.Contains(key))
                {
                    replaceStringBox.Text = SuperRichTextBoxResourses.SearchReplaceDictionary[key];
                }
            }
        }

        
    }
}
