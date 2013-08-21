using InterfaceLibrary;
using OnlyTextNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary.NodeChange
{
    /// <summary>
    /// 将OnlyTextInfo对象转换为DetailTextObject
    /// </summary>
    class OnlyTextToDetailTextChanger:INodeChanger
    {
        public string ChangeToNodeType
        {
            get { return "DetailText"; }
        }

        public NodeDataObject ChangeTo(NodeDataObject sourceObject, String EFConnectionString)
        {
            NodeDataObject dataObject = NodeFactory.CreateDataInfoNode("DetailText", EFConnectionString);
            dataObject.DataItem.Path = sourceObject.DataItem.Path;
            //在数据库中创建对象
            dataObject.AccessObject.Create(dataObject.DataItem);
            return dataObject;
        }
    }
}
