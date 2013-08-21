using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFSuperTreeView
{
    /// <summary>
    /// 定义节点移动类型
    /// </summary>
    public enum NodeMoveType
    {
        NodeMoveUp,//节点上移
        NodeMoveDown,//节点下移
        NodeMoveLeft,//节点左移,即升级
        NodeMoveRight,//节点右移,即降级
        NodePaste   //节点粘贴
    }
}
