using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterfaceLibrary
{
    /// <summary>
    /// 代表挂在每棵树上的数据对象
    /// </summary>
    public class NodeDataObject
    {
        /// <summary>
        /// 封装数据信息的对象
        /// </summary>
        public IDataInfo DataItem { get; set; }
        
        /// <summary>
        /// 提供CRUD功能的数据存取对象
        /// </summary>
        public IDataAccess AccessObject { get; set; }
    }
}
