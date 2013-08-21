using DataAccessLayer;
using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DetailTextNode
{
    /// <summary>
    /// 完成DetailText的CRUD
    /// </summary>
    public class DetailTextRepository
    {
        /// <summary>
        /// 用于创建数据库连接的连接字符串（Entity framework格式）
        /// </summary>
        private String EFConnectionString = "";

        //public DetailTextRepository()
        //{
        //    EFConnectionString = DALConfig.EFConnectString;
        //}

        public DetailTextRepository(String EFConnectionString)
        {
            this.EFConnectionString = EFConnectionString;
        }
       

        /// <summary>
        /// 向数据库中加入一条新记录
        /// </summary>
        /// <param name="dataInfoObject"></param>
        /// <returns></returns>
        public int Create(DetailTextDB dbObj)
        {
            if (dbObj == null)
            {
                return -1;
            }
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                context.DetailTextDBs.Add(dbObj);
               
                return context.SaveChanges();
            }
        }

        public void DeleteAll()
        {
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                context.Database.ExecuteSqlCommand("delete from DetailTextDB ");
            }
        }

        public int Delete(int id)
        {
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                return context.Database.ExecuteSqlCommand("delete from DetailTextDB where ID={0}", id);
            }
        }

        public int Update(DetailTextDB dbObj)
        {
            if (dbObj == null)
            {
                return 0;
            }
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                DetailTextDB oldObj = context.DetailTextDBs.FirstOrDefault(o => o.ID == dbObj.ID);
                if (oldObj != null)
                {
                    //最大的字段长度为3000
                    if (dbObj.Text.Length >DALConfig.MaxTextFieldSize)
                    {
                        dbObj.Text.Substring(0, DALConfig.MaxTextFieldSize);
                    }
                    //更新数据库
                    oldObj.ModifyTime = dbObj.ModifyTime;
                    oldObj.Path = dbObj.Path;
                    oldObj.RTFText = dbObj.RTFText;
                    oldObj.Text = dbObj.Text;
                    return context.SaveChanges();
                }
                return 0;
            }

        }
        /// <summary>
        /// 按照路径提取DetailTextDB对象
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public DetailTextDB GetDataInfoObjectByPath(string nodePath)
        {
            if (String.IsNullOrEmpty(nodePath))
            {
                return null;
            }
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                DetailTextDB dbobj = context.DetailTextDBs.FirstOrDefault(o => o.Path == nodePath);
                if (dbobj != null)
                {
                    return dbobj;
                }

            }
            return null;
        }
        /// <summary>
        /// 更新节点的路径：查找所有路径以oldPath打头的记录，将其路径替换为以newPath打头
        /// 有效路径前后应该以“/”包围
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public void UpdateNodePaths(String oldPath, String newPath)
        {
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                var query = from item in context.DetailTextDBs
                            where item.Path.StartsWith(oldPath)
                            select item;
                foreach (var item in query)
                {
                    item.Path = item.Path.Replace(oldPath, newPath);
                }
                context.SaveChanges();
            }
               
        }
        /// <summary>
        /// 删除一个节点及其所有的子节点
        /// </summary>
        /// <param name="dbobj"></param>
        /// <returns></returns>
        public int DeleteNodeAndItsChild(String nodePath)
        {
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                return context.Database.ExecuteSqlCommand("Delete from DetailTextDB where Path like {0}", nodePath + "%");
            }

        }
    }
}
