using DetailTextNode;
using FolderNode;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary.NodeChange
{
    public class DetailTextToFolderChanger:INodeChanger
    {
        public string ChangeToNodeType
        {
            get { return "Folder"; }
        }

        public InterfaceLibrary.NodeDataObject ChangeTo(InterfaceLibrary.NodeDataObject sourceObject, String EFConnectionString)
        {
            DetailTextInfo detailInfo = sourceObject.DataItem as DetailTextInfo;
            NodeDataObject dataObject = NodeFactory.CreateDataInfoNode("Folder", EFConnectionString);
            FolderInfo folderInfo = dataObject.DataItem as FolderInfo;

            folderInfo.Path = detailInfo.Path;
            folderInfo.Text = detailInfo.Text;
            folderInfo.RTFText = detailInfo.RTFText;
            //在数据库中删除记录
            sourceObject.AccessObject.DeleteDataInfoObject(detailInfo);
            //在数据库中创建对象
            dataObject.AccessObject.Create(folderInfo);
            return dataObject;
        }
    }
}
