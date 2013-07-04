using DetailTextNode;
using FolderNode;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary.NodeChange
{
    /// <summary>
    /// 从文件夹型节点转换为详细信息型节点
    /// </summary>
    public class FolderToDetailTextChanger:INodeChanger
    {
        public string ChangeToNodeType
        {
            get { return "DetailText"; }
        }

        public InterfaceLibrary.NodeDataObject ChangeTo(InterfaceLibrary.NodeDataObject sourceObject)
        {
            FolderInfo folderInfo = sourceObject.DataItem as FolderInfo;
            NodeDataObject dataObject = NodeFactory.CreateDataInfoNode("DetailText");
            DetailTextInfo detailInfo = dataObject.DataItem as DetailTextInfo;

            detailInfo.Path = sourceObject.DataItem.Path;
            detailInfo.Text = folderInfo.Text;
            detailInfo.RTFText = folderInfo.RTFText;
            //在数据库中删除记录
            sourceObject.AccessObject.DeleteDataInfoObject(folderInfo);
            //在数据库中创建对象
            dataObject.AccessObject.Create(detailInfo);
            return dataObject;
        }
    }
}
