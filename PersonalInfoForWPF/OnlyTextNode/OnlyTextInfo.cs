using InterfaceLibrary;
using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;


namespace OnlyTextNode
{
    public class OnlyTextInfo : IDataInfo
    {
        private const String NoteText = "就绪";
        private DateTime _CreateOrModifyTime = DateTime.Now;
        public DateTime ModifyTime
        {
            get
            {
                return _CreateOrModifyTime;
            }
            set
            {
                _CreateOrModifyTime = value;
            }
        }

        public ImageSource NormalIcon
        {
            get
            {
                return Icons.normalIcon;
            }

        }

        public ImageSource SelectedIcon
        {
            get
            {
                return Icons.selectedIcon;
            }

        }

        public Control RootControl
        {
            get
            {
                return null;
            }

        }

        public bool ShouldEmbedInHostWorkingBench
        {
            get
            {
                return false;
            }

        }


        public string NodeType
        {
            get { return "OnlyText"; }
        }






        public void RefreshDisplay()
        {
            //显示默认的提示信息
            if (MainWindow != null)
            {
                MainWindow.ShowInfo(NoteText);
            }
            return;
        }

        public void BindToRootControl()
        {
            return;
        }




        public string Path
        {
            get;

            set;

        }

        public int ID
        {
            get;
            set;
        }

        public bool HasBeenLoadFromStorage
        {
            get
            {
                return true;
            }
            set
            {

            }
        }




        public void RefreshMe()
        {
            return;
        }


        public void SetRootControlDataAccessObj(IDataAccess accessObj)
        {
            return;
        }


        public IMainWindowFunction MainWindow
        {
            get;
            set;
        }
    }
}
