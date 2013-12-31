using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DetailTextNode
{
    public class DetailTextInfo:IDataInfo
    {

        private string NoteText = "就绪";
        public string Path
        {
            get;
            set;
        }

        public string Text { get; set; }

        public string RTFText { get; set; }

        public int ID { get; set; }

        private DateTime _modifyTime = DateTime.Now;
        public DateTime ModifyTime
        {
            get
            {
                return _modifyTime;
            }
            set
            {
                _modifyTime = value;
            }
        }

        public string NodeType
        {
            get { return "DetailText"; }
        }

        public ImageSource NormalIcon
        {
            get { return DetailTextResources.normalIcon; }
        }

        public ImageSource SelectedIcon
        {
            get { return DetailTextResources.selectedIcon; }
        }

        public System.Windows.Controls.Control RootControl
        {
            get {
               return DetailTextResources.RootControl; 
            }
        }

        public bool ShouldEmbedInHostWorkingBench
        {
            get { return true;  }
        }

        public void RefreshDisplay()
        {
            DetailTextResources.RootControl.RefreshDisplay();
            //显示默认的提示信息
            if (_mainWindow != null)
            {
                _mainWindow.ShowInfo(NoteText);
            }
        }

        public void BindToRootControl()
        {
            DetailTextResources.RootControl.DataObject = this;
        }

        public bool HasBeenLoadFromStorage
        {
            get;
            set;
        }


        public void RefreshMe()
        {
            DetailTextResources.RootControl.UpdateDataObjectInMemory();
        }
        /// <summary>
        /// 指定节点面板的数据存取对象，通过它将数据库保存到底层存储机构中
        /// </summary>
        /// <param name="accessObj"></param>
        public void SetRootControlDataAccessObj(IDataAccess accessObj)
        {
            DetailTextResources.RootControl.accessObj = accessObj as DetailTextAccess;
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
