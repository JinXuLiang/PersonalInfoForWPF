using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceLibrary
{
    /// <summary>
    /// 用于定义主窗体应该实现的功能，这些功能将被各个节点所调用
    /// 比如，某节点所对应的可视化界面中，可能有代码调用主窗体的ShowInfo功能在主窗体上显示特定的信息
    /// </summary>
    public interface IMainWindowFunction
    {
        /// <summary>
        /// 当节点需要在主窗体上显示一些信息时，调用此方法
        /// </summary>
        /// <param name="info"></param>
        void ShowInfo(String info);
    }
}
