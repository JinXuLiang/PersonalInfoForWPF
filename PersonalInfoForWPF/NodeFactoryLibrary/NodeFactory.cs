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
        /// 用于引用主窗体，节点可以使用此引用调用主窗体所提供的功能（比如显示信息）
        /// </summary>
        public static IMainWindowFunction _mainWindow = null;
        /// <summary>
        /// 依据节点类型创建"空白"的数据信息对象，其HasBeenLoadFromStorage属性为false
        /// （无需数据存储的节点，如OnlyText例此，其HasBeenLoadFromStorage始终为true）
        /// </summary>
        /// <param name="NodeType"></param>
        /// <returns></returns>
        public static NodeDataObject CreateDataInfoNode(String NodeType,String EFConnectionString)
        {
            NodeDataObject nodeDataObject = new NodeDataObject();
            if (NodeType == "OnlyText")
            {
                nodeDataObject.DataItem = new OnlyTextInfo() { MainWindow=_mainWindow };
                nodeDataObject.AccessObject = null;
            }
            if (NodeType == "Folder")
            {
                FolderInfo info = new FolderInfo() { MainWindow = _mainWindow };
                FolderAccess access = new FolderAccess(EFConnectionString) ;
                info.SetRootControlDataAccessObj(access);

                nodeDataObject.DataItem =info ;
                //设置数据未装入标记
                nodeDataObject.DataItem.HasBeenLoadFromStorage = false;

                nodeDataObject.AccessObject =access ;
                
            }
            if (NodeType == "DetailText")
            {
                DetailTextInfo info = new DetailTextInfo() { MainWindow = _mainWindow }; 
                DetailTextAccess accessObj= new DetailTextAccess(EFConnectionString);

                info.SetRootControlDataAccessObj(accessObj);
                nodeDataObject.DataItem=info;
                //设置数据未装入标记
                nodeDataObject.DataItem.HasBeenLoadFromStorage = false;
               
                nodeDataObject.AccessObject = accessObj;

              
            }
            return nodeDataObject;
        }
        /// <summary>
        /// 依据数据库连接字串（EntityFramework格式）创建合适的IDataAcess对象
        /// </summary>
        /// <param name="NodeType"></param>
        /// <param name="EFConnectionString"></param>
        /// <returns></returns>
        public static IDataAccess CreateNodeAccessObject(String NodeType,String EFConnectionString)
        {
            NodeDataObject nodeDataObject = new NodeDataObject();
            if (NodeType == "OnlyText")
            {
                return null;
            }
            if (NodeType == "Folder")
            {
                return new FolderAccess(EFConnectionString);
            }
            if (NodeType == "DetailText")
            {
                return new DetailTextAccess(EFConnectionString);
            }
            return null;
        }
    }
}
