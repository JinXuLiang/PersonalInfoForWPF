using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SystemLibrary;

namespace DetailTextNode
{
    /// <summary>
    /// Interaction logic for DetailTextPanel.xaml
    /// </summary>
    public partial class DetailTextPanel : UserControl
    {
        private DetailTextAccess accessObj = new DetailTextAccess();
        public DetailTextPanel()
        {
            InitializeComponent();
            //当点击保存按钮时，刷新数据库
            richTextBox1.OnSaveDocument += UpdateDb;

        }
        /// <summary>
        /// 刷新数据库中的内容
        /// </summary>
        private void UpdateDb()
        {
            UpdateDataObjectInMemory();
            Task task = new Task(() =>
            {
               
                try
                {
                    accessObj.UpdateDataInfoObject(_dataObject);

                    Dispatcher.Invoke(new Action(() => { MessageBox.Show("数据己保存"); }));
                   
                }
                catch (Exception ex)
                {

                    Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.ToString()); }));
                }

            });
            task.Start();
           
        }

        private DetailTextInfo _dataObject = null;
        /// <summary>
        /// 数据对象,读取时自动用当前控件值更新数据对象状态
        /// </summary>
        public DetailTextInfo DataObject
        {
            get
            {
                UpdateDataObjectInMemory();   
                return _dataObject;
            }
            set
            {
                _dataObject = value;
                
            }
        }
        /// <summary>
        /// 在UI上显示数据对象
        /// </summary>
        private void ShowDataObjectInUI(DetailTextInfo obj)
        {
            if (obj != null)
            {
                richTextBox1.Rtf = obj.RTFText;
            }
        }
        /// <summary>
        /// 使用控件的当前值更新数据对象
        /// </summary>
        public void UpdateDataObjectInMemory()
        {
            _dataObject.RTFText = richTextBox1.Rtf;
            _dataObject.Text = richTextBox1.Text;
            _dataObject.ModifyTime = DateTime.Now;
            
        }
        /// <summary>
        /// 刷新显示
        /// </summary>
        public void RefreshDisplay()
        {
           //因为配置有可能改变，所以重设默认字体
           richTextBox1.FontSize = SystemConfig.configArgus.RichTextEditorDefaultFontSize;
           ShowDataObjectInUI(_dataObject);
           
        }
        /// <summary>
        /// 失去焦点时，更新数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
          
            UpdateDataObjectInMemory();

            Thread thread = new Thread(() =>
            {
                try
                {
                    accessObj.UpdateDataInfoObject(_dataObject);
                }
                catch (Exception ex)
                {

                   // Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.Message); }));
                    Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.ToString()); }));
                }
            });
            thread.Start();
            
        }
        
    }
}
