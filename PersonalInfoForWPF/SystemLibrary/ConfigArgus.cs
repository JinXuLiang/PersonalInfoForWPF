using System;
using System.Collections.Generic;

namespace SystemLibrary
{
    /// <summary>
    /// 封装系统的一些设置参数
    /// </summary>
    [Serializable]
    public class ConfigArgus
    {
        /// <summary>
        /// 代表本软件的最新版本，每次更改之后，都应该更新此版本号
        /// </summary>
        public const String version = "2.1.0.2";

        private double treeNodeDefaultFontSize = 15;
        /// <summary>
        /// 树节点的字体大小，在主窗体init中设置
        /// </summary>
        public double TreeNodeDefaultFontSize
        {
            get { return treeNodeDefaultFontSize; }
            set { treeNodeDefaultFontSize = value; }
        }

        [NonSerialized]
        private bool _IsArgumentsValueChanged = false;
        /// <summary>
        /// 判断一下用户是否通过“系统设置”窗口更改了设置，如果没有更改，则不重启,本属性不参与序列化
        /// </summary>
        /// <returns></returns>
        public bool IsArgumentsValueChanged
        {
            get
            {
                return _IsArgumentsValueChanged;
            }
            set
            {
                _IsArgumentsValueChanged = value;
            }
        }

        private double richTextEditorDefaultFontSize = 20;
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
