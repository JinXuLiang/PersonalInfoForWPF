using DataAccessLayer.Folder;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderNode
{
    public class FolderInfo:IDataInfo
    {
        private const string NoteText = "文件夹型节点，压住Control键可以选择多个文件。";
        public bool HasBeenLoadFromStorage
        {
            get;
            set;
        }

        public int ID
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public string Text { get; set; }

        public string RTFText { get; set; }


        private ObservableCollection<DBFileInfo> _files = new ObservableCollection<DBFileInfo>();
        /// <summary>
        /// 本文件夹节点所关联的文件对象集合
        /// </summary>
        public ObservableCollection<DBFileInfo> AttachFiles
        {
            get
            {
                return _files;
            }
            set
            {
                _files = value;
            }

        }

        private DateTime _ModifyTime = DateTime.Now;
        public DateTime ModifyTime
        {
            get
            {
                return _ModifyTime;
            }
            set
            {
                _ModifyTime = value;
            }
        }

        public string NodeType
        {
            get { return "Folder"; }
        }

        public System.Windows.Media.ImageSource NormalIcon
        {
            get { return FolderResources.normalIcon; }
        }

        public System.Windows.Media.ImageSource SelectedIcon
        {
            get { return FolderResources.selectedIcon; }
        }

        public System.Windows.Controls.Control RootControl
        {
            get { return FolderResources.RootControl; }
        }

        public bool ShouldEmbedInHostWorkingBench
        {
            get { return true; }
        }

        public void RefreshDisplay()
        {
            FolderResources.RootControl.RefreshDisplay();
            //显示默认的提示信息
            if (_mainWindow != null)
            {
                _mainWindow.ShowInfo(NoteText);
            }
        }

        public void BindToRootControl()
        {
            FolderResources.RootControl.DataObject = this;
        }


        public void RefreshMe()
        {
            FolderResources.RootControl.UpdateDataObjectInMemory();
        }

        public void SetRootControlDataAccessObj(IDataAccess accessObj)
        {
            FolderResources.RootControl.accessObj = accessObj as FolderAccess;
        }

        private IMainWindowFunction _mainWindow = null;
        public IMainWindowFunction MainWindow
        {
            get
            {
                return _mainWindow;
            }
            set
            {
                _mainWindow = value;
            }
        }
    }
}
