using DataAccessLayer.Folder;
using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public FolderAccess accessObj
        {
            get;
            set;
        }
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


        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            AddFiles();
        }
        /// <summary>
        /// 添加文件
        /// </summary>
        private void AddFiles()
        {
            WinForm.OpenFileDialog openFileDialog = new WinForm.OpenFileDialog();
            openFileDialog.Multiselect = true;
            String info = "";
            if (openFileDialog.ShowDialog() == WinForm.DialogResult.OK)
            {
                foreach (var FileName in openFileDialog.FileNames)
                {
                    FileInfo fi = new FileInfo(FileName);
                    if (fi.Length > 5 * 1024 * 1024)
                    {
                        MessageBoxResult result = MessageBox.Show("要加入的文件:“" + System.IO.Path.GetFileName(FileName) + "”大小为：" + FileUtils.FileSizeFormater(fi.Length) + "，将大文件加入数据库会导致程序性能下降，点击“是”添加此文件，点击“否”跳过此文件", "加入大文件", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
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
                    //当加入文件时，有可能因为另一进程也使用此文件而导致加载失败
                    try
                    {
                        accessObj.AddFile(_dataObject.Path, fileInfo, System.IO.File.ReadAllBytes(FileName));
                        _dataObject.AttachFiles.Add(fileInfo);
                        info = string.Format("正在加入文件{0}", System.IO.Path.GetFileName(FileName));

                        if (_dataObject.MainWindow != null)
                        {
                            _dataObject.MainWindow.ShowInfo(info);
                        }
                    }

                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    if (_dataObject.MainWindow != null)
                    {
                        _dataObject.MainWindow.ShowInfo("文件添加完毕");
                    }
                }
            }
        }

        private void btnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            RemoveFiles();
        }
        /// <summary>
        /// 删除用户选择的文件
        /// </summary>
        private void RemoveFiles()
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
            ExportToDisk();
        }
        /// <summary>
        /// 将选中的文件导出到指定文件夹
        /// </summary>
        private void ExportToDisk()
        {
            WinForm.FolderBrowserDialog folderDialog = null;
            String SavePath = "";
            String SaveFileNameWithPath = "";
            String SaveFileName = "";
            byte[] fileContent = null;
            if (dgFiles.SelectedItems.Count > 0)
            {

                var fileInfo = dgFiles.SelectedItems[0] as DBFileInfo;
                //设置要导出的文件夹
                folderDialog = new WinForm.FolderBrowserDialog();
                folderDialog.Description = "选择用于保存文件的文件夹";
                SavePath = System.IO.Path.GetDirectoryName(fileInfo.FilePath);
                //提取用户选择的第一个文件路径，如果其存在，则将其作为保存导出文件的文件夹
                //如果不存在，导出到"我的文档"文件夹
                if (System.IO.Directory.Exists(SavePath))
                {
                    folderDialog.SelectedPath = SavePath;
                }
                else
                {
                    folderDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                }

                if (folderDialog.ShowDialog() == WinForm.DialogResult.OK)
                {
                    SavePath = folderDialog.SelectedPath;
                    try
                    {
                        int exportedFiles = 0;
                        //循环导出所有文件。
                        foreach (DBFileInfo dbFile in dgFiles.SelectedItems)
                        {
                            SaveFileName = System.IO.Path.GetFileName(dbFile.FilePath);
                            SaveFileNameWithPath = SavePath + "\\" + SaveFileName;
                            fileContent = accessObj.getFileContent(fileInfo.ID);
                            if (fileContent != null)
                            {
                                File.WriteAllBytes(SaveFileNameWithPath, fileContent);
                            }
                            exportedFiles++;
                            //在主窗体上显示相关信息
                            if (_dataObject.MainWindow != null)
                            {
                                _dataObject.MainWindow.ShowInfo("正在导出" + SaveFileName);
                            }
                        }

                    }
                    catch (IOException)
                    {
                        //忽略不能导出的文件（比如正在尝试覆盖的文件正在使用中，不能被覆盖）
                    }
                    //导出结束
                    String info = string.Format("文件导出结束，共导出{0}个文件到{1}", dgFiles.SelectedItems.Count, SavePath);

                    if (_dataObject.MainWindow != null)
                    {
                        _dataObject.MainWindow.ShowInfo(info);
                    }
                    //自动在Explorer中打开文件夹
                    Process.Start(SavePath);
                }

            }




        }
        /// <summary>
        /// 失去焦点时，更新数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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
    }
}
