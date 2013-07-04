using DetailTextNode;
using FolderNode;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary
{
    /// <summary>
    /// 封装对节点路径变更的管理
    /// 之所以创建此类，是因为各种节点类型的数据可能分布于不同的表，
    /// 或者是其他的存储方式，当节点路径改变时，应该通知所有的数据存取对象更新路径信息
    /// 本应用主要有3种方式影响路径：
    /// 1 修改节点文本
    /// 2 节点升级、降级或粘贴
    /// 3 节点被删除
    /// </summary>
    public class NodePathManager
    {
        /// <summary>
        /// 在此加入当前所有节点类型的数据存取对象
        /// </summary>
        private static List<IDataAccess> DataAccessList = new List<IDataAccess>
        {
            new DetailTextAccess(),new FolderAccess()
        };

        public static void UpdateNodePath(String oldPath,String newPath)
        {
            foreach (var item in DataAccessList)
            {
                item.UpdateNodePath(oldPath, newPath);
            }
        }

        public static void DeleteDataInfoObjectOfNodeAndItsChildren(String nodePath)
        {
            foreach (var item in DataAccessList)
            {
                item.DeleteDataInfoObjectOfNodeAndItsChildren(nodePath);
            }
        }
        
    }
}
