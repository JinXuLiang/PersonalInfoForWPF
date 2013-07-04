using DetailTextNode;
using FolderNode;
using InterfaceLibrary;
using OnlyTextNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary
{
    /// <summary>
    /// 依据节点类型，创建相应的默认对象
    /// </summary>
    public class NodeFactory
    {
        /// <summary>
        /// 依据节点类型创建"空白"的数据信息对象，其HasBeenLoadFromStorage属性为false
        /// （无需数据存储的节点，如OnlyText例此，其HasBeenLoadFromStorage始终为true）
        /// </summary>
        /// <param name="NodeType"></param>
        /// <returns></returns>
        public static NodeDataObject CreateDataInfoNode(String NodeType)
        {
            NodeDataObject nodeDataObject = new NodeDataObject();
            if (NodeType == "OnlyText")
            {
                nodeDataObject.DataItem = new OnlyTextInfo();
                nodeDataObject.AccessObject = null;
            }
            if (NodeType == "Folder")
            {
                nodeDataObject.DataItem = new FolderInfo();
                //设置数据未装入标记
                nodeDataObject.DataItem.HasBeenLoadFromStorage = false;
                nodeDataObject.AccessObject = new FolderAccess();
            }
            if (NodeType == "DetailText")
            {
                nodeDataObject.DataItem=new DetailTextInfo();
                //设置数据未装入标记
                nodeDataObject.DataItem.HasBeenLoadFromStorage = false;
                nodeDataObject.AccessObject=new DetailTextAccess();
            }
            return nodeDataObject;
        }
    }
}
