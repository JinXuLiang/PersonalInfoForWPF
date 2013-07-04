using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary.NodeChange
{
    public class OnlyTextToFolderChanger:INodeChanger
    {
        public string ChangeToNodeType
        {
            get { return "Folder"; }
        }

        public InterfaceLibrary.NodeDataObject ChangeTo(InterfaceLibrary.NodeDataObject sourceObject)
        {
            NodeDataObject dataObject = NodeFactory.CreateDataInfoNode("Folder");
            dataObject.DataItem.Path = sourceObject.DataItem.Path;
            //在数据库中创建对象
            dataObject.AccessObject.Create(dataObject.DataItem);
            return dataObject;
        }
    }
}
