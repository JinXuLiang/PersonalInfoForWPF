using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace DataAccessLayer
{
    /// <summary>
    /// 此类负责完成在不同数据库中转移记录的功能
    /// </summary>
    public class NodeMoveBetweenDBManager
    {
        ///// <summary>
        ///// 源数据库文件名
        ///// </summary>
        //private String _SourceDBFileWithPath = "";
        ///// <summary>
        ///// 目标数据库文件名
        ///// </summary>
        //private String _TargetDBFileWithPath = "";

        private InfocenterEntities SourceDbContext = null;
        private InfocenterEntities TargetDbContext = null;
        public NodeMoveBetweenDBManager(String SourceDBFileWithPath, String TargetDBFileWithPath)
        {
            //_SourceDBFileWithPath = SourceDBFileWithPath;
            SourceDbContext = new InfocenterEntities(DALConfig.getEFConnectionString(SourceDBFileWithPath));
            //_TargetDBFileWithPath = TargetDBFileWithPath;
            TargetDbContext = new InfocenterEntities(DALConfig.getEFConnectionString(TargetDBFileWithPath));
        }
        /// <summary>
        /// 将源数据库中的数据记录移到另一个数据库
        /// </summary>
        /// <param name="SourceRootNodePath"></param>
        /// <param name="RootNodeType"></param>
        /// <param name="TargetRootNodePath"></param>
        public void MoveNodeBetweenDB(String SourceRootNodePath, String TargetRootNodePath)
        {


            int slashIndex = SourceRootNodePath.LastIndexOf("/", SourceRootNodePath.Length - 2);
            //源节点文本，即在树中显示的文本
            String SourceRootNodeText = SourceRootNodePath.Substring(slashIndex + 1);

            //处理DetailText节点
            var sourceDetailNodes = from node in SourceDbContext.DetailTextDBs.AsNoTracking()
                                    where node.Path.StartsWith(SourceRootNodePath)
                                    select node;
            foreach (var detailNode in sourceDetailNodes)
            {
                //源节点路径去掉开头的“/”之后，拼接到目标路径之后

                detailNode.Path = TargetRootNodePath + detailNode.Path.Replace(SourceRootNodePath, SourceRootNodeText);

                TargetDbContext.DetailTextDBs.Add(detailNode);

            }

            //处理Folder节点，提取其相关联的所有文件
            var sourceFolderNodes = from node in SourceDbContext.FolderDBs.Include("DiskFiles").AsNoTracking()
                                    where node.Path.StartsWith(SourceRootNodePath)
                                    select node;
            foreach (var folderNode in sourceFolderNodes)
            {
                //源路径去掉开头的“/”之后，拼接到目标路径之后
                folderNode.Path = TargetRootNodePath + folderNode.Path.Replace(SourceRootNodePath, SourceRootNodeText);
                TargetDbContext.FolderDBs.Add(folderNode);
            }


            //提交更改

            TargetDbContext.SaveChanges();

            TargetDbContext.Dispose();

            //在源数据库中删除相关详细信息节点记录
            SourceDbContext.Database.ExecuteSqlCommand("Delete from DetailTextDB where Path like {0}", SourceRootNodePath + "%");
            //在源数据库中删除所有文件夹节点相关的记录，涉及三个表，为简单起见，使用EF完成。
            var query = from folder in SourceDbContext.FolderDBs
                        where folder.Path.StartsWith(SourceRootNodePath)
                        select folder;
            foreach (var folder in query)
            {

                List<DiskFile> files = folder.DiskFiles.ToList();

                foreach (var file in files)
                {
                    SourceDbContext.Entry(file).State = EntityState.Deleted;
                }

                SourceDbContext.FolderDBs.Remove(folder);
            }
            SourceDbContext.SaveChanges();
            SourceDbContext.Dispose();



        }
    }
}
