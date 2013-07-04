using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary.NodeChange
{
    class DetailTextToOnlyTextChanger:INodeChanger
    {
        public string ChangeToNodeType
        {
            get { return "OnlyText"; }
        }

        public NodeDataObject ChangeTo(NodeDataObject sourceObject)
        {
            NodeDataObject dataObject = NodeFactory.CreateDataInfoNode("OnlyText");
            dataObject.DataItem.Path = sourceObject.DataItem.Path;
            //在数据库中删除记录
            sourceObject.AccessObject.DeleteDataInfoObject(sourceObject.DataItem);
            return dataObject;   
        }
    }
}
