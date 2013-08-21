using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DetailTextNode
{
    /// <summary>
    /// 封装一些辅助功能
    /// </summary>
    class DetailTextHelper
    {
        public static DetailTextDB changeToDetailTextDB(DetailTextInfo obj)
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
          
            DetailTextDB dbObj = new DetailTextDB()
            {
                ModifyTime = obj.ModifyTime,
                Path = obj.Path,
                RTFText = String.IsNullOrEmpty(obj.RTFText) ? null : Encoding.UTF8.GetBytes(obj.RTFText),
                Text = String.IsNullOrEmpty(obj.Text)?"":obj.Text,
                ID = obj.ID
            };
            
            return dbObj;
        }

        public static DetailTextInfo changeToDetailTextInfo(DetailTextDB dbobj)
        {
            if (dbobj == null)
            {
                return null;
            }
            DetailTextInfo obj = new DetailTextInfo()
            {
                Text = String.IsNullOrEmpty(dbobj.Text)?"":dbobj.Text,
                RTFText = dbobj.RTFText==null?"":Encoding.UTF8.GetString(dbobj.RTFText),
                Path = dbobj.Path,
                ModifyTime = dbobj.ModifyTime,
                ID = dbobj.ID

            };
            return obj;
        }

    }
}
