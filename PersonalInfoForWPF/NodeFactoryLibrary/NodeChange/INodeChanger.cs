using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary
{
    /// <summary>
    /// 节点类型转换对象，它负责从某种类型转换到另一种类型，是一对一的转换
    /// </summary>
    public interface INodeChanger
    {
        String ChangeToNodeType{get;}
        NodeDataObject ChangeTo(NodeDataObject sourceObject);
    }
}
