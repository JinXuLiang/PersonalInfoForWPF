using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceLibrary
{
    /// <summary>
    /// 用于封装数据存取功能
    /// </summary>
    public interface IDataAccess
    {
        /// <summary>
        /// 为数据对象在数据库中创建相应的记录
        /// </summary>
        /// <param name="dataObject"></param>
        int Create(IDataInfo dataInfoObject);
        /// <summary>
        /// 按照节点路径删除相关联的数据对象（包括其子对象所关联的也一并删除)
        /// </summary>
        /// <param name="id"></param>
        int DeleteDataInfoObjectOfNodeAndItsChildren(String nodePath);
        /// <summary>
        /// 删除指定的数据对象（但保留其所有子对象的数据,主要用于节点类型转换）
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        int DeleteDataInfoObject(IDataInfo dataInfoObject);
        /// <summary>
        /// 更新数据对象
        /// </summary>
        /// <param name="dataObject"></param>
        int UpdateDataInfoObject(IDataInfo dataInfoObject);
        /// <summary>
        /// 依据路径名提取数据对象，如果成功，数据对象的HasBeenLoadFromStorage属性应该为True
        /// 如果找不到，返回null
        /// </summary>
        /// <param name="nodePath"></param>
        IDataInfo GetDataInfoObjectByPath(String nodePath);
        /// <summary>
        /// 更新节点的路径（多由于树节点文本被更改而导致）
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        void UpdateNodePath(String oldPath, String newPath);
    }
}
