using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderNode
{
    /// <summary>
    /// 封装一些辅助功能
    /// </summary>
    public class FolderHelper
    {
        /// <summary>
        /// 转换为FolderDB对象，不理会附属文件集合
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static FolderDB changeToFolderDB(FolderInfo obj)
        {
            if (obj == null)
            {
                return null;
            }
            //注意数据库中nvarchar最大允许4000个字符
            if (obj.Text != null && obj.Text.Length > DALConfig.MaxTextFieldSize)
            {
                obj.Text = obj.Text.Substring(0, DALConfig.MaxTextFieldSize);
            }

            FolderDB dbObj = new FolderDB()
            {
                ModifyTime = obj.ModifyTime,
                Path = obj.Path,
                RTFText = String.IsNullOrEmpty(obj.RTFText) ? null : Encoding.UTF8.GetBytes(obj.RTFText),
                Text = String.IsNullOrEmpty(obj.Text) ? "" : obj.Text,
                ID = obj.ID
            };

            return dbObj;
        }
        /// <summary>
        /// 转换为FolderInfo，不理会附属文件集合
        /// </summary>
        /// <param name="dbobj"></param>
        /// <returns></returns>
        public static FolderInfo changeToFolderInfo(FolderDB dbobj)
        {
            if (dbobj == null)
            {
                return null;
            }
            FolderInfo obj = new FolderInfo()
            {
                Text = String.IsNullOrEmpty(dbobj.Text) ? "" : dbobj.Text,
                RTFText = dbobj.RTFText == null ? "" : Encoding.UTF8.GetString(dbobj.RTFText),
                Path = dbobj.Path,
                ModifyTime = dbobj.ModifyTime.Value,
                ID = dbobj.ID

            };
            return obj;
        }

    }
}
