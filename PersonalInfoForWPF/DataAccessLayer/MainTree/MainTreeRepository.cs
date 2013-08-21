using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataAccessLayer.MainTree
{
    /// <summary>
    /// 完成对于树的CRUD
    /// </summary>
    public class MainTreeRepository
    {
         /// <summary>
        /// 用于创建数据库连接的连接字符串（Entity framework格式）
        /// </summary>
        private String EFConnectionString = "";

        //public MainTreeRepository()
        //{
        //    EFConnectionString = DALConfig.EFConnectString;
        //}

        public MainTreeRepository(String EFConnectionString)
        {
            this.EFConnectionString = EFConnectionString;
        }
        /// <summary>
        /// 从数据库中提取树，如果找不到，则自动创建一棵空树保存到数据库中
        /// </summary>
        /// <returns></returns>
        public String GetTreeFromDB()
        {
            String TreeXml = "";
            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
                Tree treeObj = context.Trees.FirstOrDefault();
                if (treeObj != null)
                {
                    TreeXml = Encoding.UTF8.GetString(treeObj.maintree);
                }
                else
                {
                    //创建一棵空树
                    XElement root = new XElement("root");
                    TreeXml = root.ToString();
                    treeObj = new Tree();
                    treeObj.maintree = Encoding.UTF8.GetBytes(TreeXml);
                    context.Trees.Add(treeObj);
                    context.SaveChanges();
                }

            }
            return TreeXml;
        }

        public void SaveTree(String TreeXml)
        {
            if (String.IsNullOrEmpty(TreeXml))
            {
                //创建一棵空树
                XElement root = new XElement("root");
                TreeXml = root.ToString();
            }


            using (InfocenterEntities context = new InfocenterEntities(EFConnectionString))
            {
               Tree treeObj = context.Trees.FirstOrDefault();
               if (treeObj != null)
               {
                   //更新树
                   treeObj.maintree = Encoding.UTF8.GetBytes(TreeXml);
                   context.SaveChanges();
               }
               else
               {
                   //正常情况下应该不会走到这个分支，但仍然写在这里，逻辑就完整了。
                   treeObj = new Tree();
                   treeObj.maintree = Encoding.UTF8.GetBytes(TreeXml);
                   context.Trees.Add(treeObj);
                   context.SaveChanges();
               }
              
            }
        }

    }
}
