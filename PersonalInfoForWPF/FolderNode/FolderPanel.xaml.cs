using DataAccessLayer.Folder;
using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
using WinForm = System.Windows.Forms;

namespace FolderNode
{
    /// <summary>
    /// Interaction logic for FolderPanel.xaml
    /// </summary>
    public partial class FolderPanel : UserControl
    {

        private FolderAccess accessObj = new FolderAccess();
        public FolderPanel()
        {
            InitializeComponent();
            //当点击保存按钮时，刷新数据库
            richTextBox1.OnSaveDocument += UpdateDb;
        }
        /// <summary>
        ///  刷新数据库中的内容
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

        private FolderInfo _dataObject = null;

        /// <summary>
        /// 数据对象,读取时自动用当前控件值更新数据对象状态
        /// </summary>
        public FolderInfo DataObject
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

        private void ShowDataObjectInUI(FolderInfo obj)
        {
            if (obj != null)
            {
                richTextBox1.Rtf = obj.RTFText;
                dgFiles.ItemsSource = obj.AttachFiles;
            }
        }

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

                    Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.ToString()); }));
                }
            });
            thread.Start();

        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            WinForm.OpenFileDialog openFileDialog = new WinForm.OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == WinForm.DialogResult.OK)
            {
                foreach (var FileName in openFileDialog.FileNames)
                {
                    

                    FileInfo fi = new FileInfo(FileName);
                    if (fi.Length > 5 * 1024 * 1024)
                    {
                        MessageBoxResult result = MessageBox.Show("要加入的文件:“"+System.IO.Path.GetFileName(FileName)+"”大小为："+FileUtils.FileSizeFormater(fi.Length)+"，将大文件加入数据库会导致程序性能下降，点击“是”添加此文件，点击“否”跳过此文件", "加入大文件", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                        if (result == MessageBoxResult.No)
                        {
                            continue;
                        }
                        
                    }
                    

                    DBFileInfo fileInfo = new DBFileInfo()
                    {
                    AddTime = DateTime.Now,
                    FilePath = FileName,
                    FileSize = fi.Length
                    };
                   //不加入重复的文件
                    if (_dataObject.AttachFiles.IndexOf(fileInfo) != -1)
                    {
                        continue;
                    }
                    accessObj.AddFile(_dataObject.Path, fileInfo, System.IO.File.ReadAllBytes(FileName));
                    _dataObject.AttachFiles.Add(fileInfo);
                }


            }
        }

        private void btnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            List<int> fileIDs = new List<int>();
            List<DBFileInfo> files = new List<DBFileInfo>();
            foreach (var item in dgFiles.SelectedItems)
            {
                DBFileInfo fileInfo = item as DBFileInfo;
                files.Add(fileInfo);
                fileIDs.Add(fileInfo.ID);
            }
            if (fileIDs.Count > 0)
            {
                accessObj.DeleteFilesOfFolderDB(_dataObject.Path, fileIDs);
            }
            foreach (var fileInfo in files)
            {
                _dataObject.AttachFiles.Remove(fileInfo);
            }
        }

        private void btnExportToDisk_Click(object sender, RoutedEventArgs e)
        {
            DBFileInfo fileInfo = dgFiles.SelectedItem as DBFileInfo;
            if (fileInfo != null)
            {
                WinForm.SaveFileDialog saveFileDialog = new WinForm.SaveFileDialog();
                saveFileDialog.InitialDirectory =System.IO.Path.GetDirectoryName(fileInfo.FilePath);
                saveFileDialog.FileName = System.IO.Path.GetFileName(fileInfo.FilePath);
                if (saveFileDialog.ShowDialog() == WinForm.DialogResult.OK)
                {
                    byte[] fileContent = accessObj.getFileContent(fileInfo.ID);
                    if (fileContent != null)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, fileContent);
                        MessageBox.Show("文件导出为：" + saveFileDialog.FileName);
                    }
                }
            }
        }
    }
}
