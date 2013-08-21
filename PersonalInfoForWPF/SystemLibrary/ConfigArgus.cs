using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemLibrary
{
    /// <summary>
    /// 封装系统的一些设置参数
    /// </summary>
    [Serializable]
    public class ConfigArgus
    {
        
       
        private double treeNodeDefaultFontSize=15;
        /// <summary>
        /// 树节点的字体大小，在主窗体init中设置
        /// </summary>
        public double TreeNodeDefaultFontSize
        {
            get { return treeNodeDefaultFontSize; }
            set { treeNodeDefaultFontSize = value; }
        }
       
        private  double richTextEditorDefaultFontSize = 20;
        /// <summary>
        /// 文本编辑器的默认字体大小，在SuperWPFRichTextBox的Init()中设置 
        /// </summary>
        public double RichTextEditorDefaultFontSize
        {
            get
            {
                return richTextEditorDefaultFontSize;
            }
            set
            {
                richTextEditorDefaultFontSize = value;
            }


        }

        //private String lastVisitNodePath = "";
        ///// <summary>
        ///// 程序退出时最后访问的节点路径
        ///// </summary>
        //public String LastVisitNodePath
        //{
        //    get
        //    {
        //        return lastVisitNodePath;
        //    }
        //    set
        //    {
        //        lastVisitNodePath = value;
        //    }
        //}

        //private String _dbFileName = "infocenter.sdf";
        ///// <summary>
        ///// 数据库文件名
        ///// </summary>
        //public String DBFileName
        //{
        //    get
        //    {
        //        return _dbFileName;
        //    }
        //    set
        //    {
        //        _dbFileName = value;
        //    }
        //}

        private List<DatabaseInfo> _infos = new List<DatabaseInfo>();
        /// <summary>
        /// 代表上次程序退出时所打开的数据库的集合
        /// </summary>
        public List<DatabaseInfo> DbInfos
        {
            get
            {
                return _infos;
            }
            set
            {
                _infos = value;
            }
        }
        /// <summary>
        /// 代表上次程序退出时激活的数据库在DbInfos集合中的索引
        /// </summary>
        public int ActiveDBIndex { get; set; }

    }
}
