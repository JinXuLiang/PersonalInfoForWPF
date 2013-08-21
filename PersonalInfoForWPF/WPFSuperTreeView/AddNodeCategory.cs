using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFSuperTreeView
{
    /// <summary>
    /// 添加节点的几种情况
    /// </summary>
    public enum AddNodeCategory
    {
        AddRoot,//根节点
        AddChild,//子节点
        AddSibling//兄弟节点
    }
}
