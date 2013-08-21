using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccessLayer
{
    /// <summary>
    /// 
    /// 封装数据存取层的一些配置信息
    /// </summary>
    public class DALConfig
    {
        /// <summary>
        /// 最大数据库文件大小设置为不超过4G
        /// </summary>
        private static String connectStringTemplate = "metadata=res://*/InfoCenterModel.csdl|res://*/InfoCenterModel.ssdl|res://*/InfoCenterModel.msl;provider=System.Data.SqlServerCe.4.0;provider connection string=\"data source={0};Max Database Size=4000;\"";

        public static String getEFConnectionString(String dbFileName)
        {
            return String.Format(connectStringTemplate, dbFileName);
        }

        /// <summary>
        /// 由SQL Server每行限制为8060字节，因此，在此限制Text字段保存的最长字串为3000个字符（之所以是不能更大一些，是因为
        /// Path字段也有长度）
        /// </summary>
        public const int MaxTextFieldSize = 3000;
    }
}
