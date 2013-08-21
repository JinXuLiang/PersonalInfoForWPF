using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemLibrary
{
    /// <summary>
    /// 代表当前打开的数据库信息,支持按照DatabaseFilePath属性值进行的判等操作
    /// </summary>
    [Serializable]
    public class DatabaseInfo
    {
        /// <summary>
        ///  程序退出时本数据库最后访问的节点路径
        /// </summary>
        public String LastVisitNodePath { get; set; }

        /// <summary>
        /// 代表本数据库文件的完整路径
        /// </summary>
        public String DatabaseFilePath { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null || (obj is DatabaseInfo==false))
            {
                return false;
            }

            return DatabaseFilePath==(obj as DatabaseInfo).DatabaseFilePath;
        }

        public override int GetHashCode()
        {
            return DatabaseFilePath.GetHashCode();
        }
    }
}
