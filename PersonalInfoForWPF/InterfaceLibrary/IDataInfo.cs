using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace InterfaceLibrary
{
    /// <summary>
    /// 代表每种节点类型必须封装的基本数据信息
    /// </summary>
    public interface IDataInfo
    {
        /// <summary>
        /// 本节点数据信息是否己经从底层存储机构中提取
        /// </summary>
        bool HasBeenLoadFromStorage { get; set; }
        int ID { get; set; }
        String Path { get; set; }
        /// <summary>
        /// 创建或修改的时间
        /// </summary>
        DateTime ModifyTime{get;set;}
        /// <summary>
        /// 节点类型
        /// </summary>
        String NodeType{get;}
        /// <summary>
        /// 正常情况下的图标
        /// </summary>
        ImageSource NormalIcon { get;  }
        /// <summary>
        /// 选中状态下的图标
        /// </summary>
        ImageSource SelectedIcon { get;  }
        /// <summary>
        /// 本节点UI界面的最顶层控件，通常为ContentControl或ItemsControl
        /// </summary>
        Control RootControl { get;  }
        /// <summary>
        /// 是否应该将自己的界面嵌入到主程序的UI界面中（有些节点可能采用弹出独立窗口的方式）
        /// </summary>
        bool ShouldEmbedInHostWorkingBench { get; }

      
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        void RefreshDisplay();
        /// <summary>
        /// 将数据对象与UI绑定
        /// </summary>
        void BindToRootControl();
        /// <summary>
        /// 强制刷新数据对象（通常是使用用户界面上控件的当前值刷新本对象的字段值）
        /// </summary>
        void RefreshMe();
       
    }
}
