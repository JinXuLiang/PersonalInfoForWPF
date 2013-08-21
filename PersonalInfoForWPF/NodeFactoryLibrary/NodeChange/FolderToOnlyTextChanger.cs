using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary.NodeChange
{
    public class FolderToOnlyTextChanger:INodeChanger
    {
        public string ChangeToNodeType
        {
            get { return "OnlyText"; }
        }

        public InterfaceLibrary.NodeDataObject ChangeTo(InterfaceLibrary.NodeDataObject sourceObject, String EFConnectionString)
        {
            NodeDataObject dataObject = NodeFactory.CreateDataInfoNode("OnlyText", EFConnectionString);
            dataObject.DataItem.Path = sourceObject.DataItem.Path;
            //在数据库中删除记录
            sourceObject.AccessObject.DeleteDataInfoObject(sourceObject.DataItem);
            return dataObject;   
        }
    }
}
