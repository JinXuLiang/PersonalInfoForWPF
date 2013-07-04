using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace DataAccessLayer.Folder
{
    /// <summary>
    /// 完成文件夹型节点的增删改查功能
    /// </summary>
    public class FolderRepository
    {
        /// <summary>
        /// 更新节点的路径：查找所有路径以oldPath打头的记录，将其路径替换为以newPath打头
        /// 有效路径前后应该以“/”包围
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public void UpdateNodePaths(String oldPath, String newPath)
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                var query = from item in context.FolderDBs
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
        /// 向数据库中添加一条记录
        /// </summary>
        /// <param name="FolderObj"></param>
        /// <returns></returns>
        public int AddFolderDB(FolderDB FolderObj)
        {
            if (FolderObj == null)
            {
                return 0;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                context.FolderDBs.Add(FolderObj);
                return context.SaveChanges();
            }
        }
        /// <summary>
        /// 更新文件夹节点的信息
        /// </summary>
        /// <param name="folder"></param>
        public int UpdateFolderDB(FolderDB folder)
        {
            if (folder == null)
            {
                return 0;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                FolderDB folderToModify = context.FolderDBs.FirstOrDefault(p => p.ID == folder.ID);
                if (folderToModify != null)
                {
                    folderToModify.ModifyTime = folder.ModifyTime;
                    folderToModify.Text = folder.Text;
                    folderToModify.RTFText = folder.RTFText;
                    return context.SaveChanges();

                }
            }
            return 0;

        }

        #region "提取信息"

        /// <summary>
        /// 提取所有的文件夹型节点（包括其相关联的文件）
        /// 尽量少用，这将导致占用大量内存
        /// </summary>
        /// <returns></returns>
        public List<FolderDB> GetAllFolderDBWithItsDiskFile()
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                var query = from folder in context.FolderDBs.Include("DiskFiles")
                            select folder;
                return query.ToList();
            }
        }

        /// <summary>
        /// 提取所有的文件夹型节点（不包括其相关联的文件）
        ///
        /// </summary>
        /// <returns></returns>
        public List<FolderDB> GetAllFolderDBWithoutItsDiskFile()
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                var query = from folder in context.FolderDBs
                            select folder;
                return query.ToList();
            }
        }

        /// <summary>
        /// 按照路径提取文件夹对象(仅提取第一个匹配的），找不到，返回null
        /// 获取的文件夹对象包容其所有的关联文件
        /// Note:尽量少用，因此方法会加载所有文件内容，占用大量内存
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderDB GetFolderDBWithFilesByPath(String path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                return context.FolderDBs.Include("DiskFiles").FirstOrDefault(p => p.Path == path);
            }
        }
        /// <summary>
        /// 依据路径提取文件夹节点的数据，不包括其包容的文件信息，
        /// 此方法应该与GetFileInfosOfFolderDB()结合起来，向外界返回真正有用的信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderDB GetFolderDBWithoutFileInfosByPath(String path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                return context.FolderDBs.FirstOrDefault(p => p.Path == path);
            }
        }
        /// <summary>
        /// 按照Path提取相应文件夹节点所有的文件信息(是FileInfo对象）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ObservableCollection<DBFileInfo> GetFileInfosOfFolderDB(String path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }
            ObservableCollection<DBFileInfo> fileInfos = new ObservableCollection<DBFileInfo>();
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                var query = from folder in context.FolderDBs
                            where folder.Path == path
                            select folder;
                FolderDB folderObj = query.FirstOrDefault();
                if (folderObj != null)
                {
                    foreach (var file in folderObj.DiskFiles)
                    {
                        fileInfos.Add(new DBFileInfo()
                        {
                            AddTime = file.AddTime.Value,
                            FilePath = file.FilePath,
                            FileSize = file.FileSize.Value,
                            ID = file.ID
                        });

                    }

                }
                return fileInfos;
            }
        }
        #endregion

        #region "添加文件"

        /// <summary>
        /// 将一个文件加入到文件夹节点的文件集合中
        /// </summary>
        /// <param name="folderDBPath"></param>
        /// <param name="file"></param>
        public int AddFileOfFolderDB(String folderDBPath, DiskFile file)
        {
            if (string.IsNullOrEmpty(folderDBPath) || file == null)
            {
                return 0;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                FolderDB folder = context.FolderDBs.FirstOrDefault(p => p.Path == folderDBPath);
                if (folder == null)
                {
                    return 0;
                }
                folder.DiskFiles.Add(file);
                return context.SaveChanges();
            }
            
        }
        /// <summary>
        /// 将多个文件追加到文件夹节点中
        /// 如果文件夹节点找不到，或者files为空集合，什么也不干，返回
        /// </summary>
        /// <param name="FolderDBPath"></param>
        /// <param name="files"></param>
        public void AddFilesOfFolderDB(String FolderDBPath, List<DiskFile> files)
        {
            if (string.IsNullOrEmpty(FolderDBPath) || files == null || files.Count == 0)
            {
                return;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                FolderDB folder = context.FolderDBs.FirstOrDefault(p => p.Path == FolderDBPath);
                if (folder == null)
                {
                    return;
                }
                foreach (var file in files)
                {
                    folder.DiskFiles.Add(file);
                }
                context.SaveChanges();
            }
        }
        #endregion

        #region "删除操作"

        /// <summary>
        /// 按照路径删除FolderDB记录，如果它还有子记录，一并删除
        /// </summary>
        /// <param name="path"></param>
        public int DeleteFolderDBAndItsChildByPath(String path)
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                var query = from folder in context.FolderDBs
                            where folder.Path.StartsWith(path)
                            select folder;
                foreach (var folder in query)
                {
                   
                    List<DiskFile> files = folder.DiskFiles.ToList();

                    foreach (var file in files)
                    {
                        context.Entry(file).State = EntityState.Deleted;
                    }

                    context.FolderDBs.Remove(folder);
                }
                return context.SaveChanges();
            }

        }
        /// <summary>
        /// 接照路径删除指定的文件夹节点记录，其子节点数据不动（因为在更换节点
        /// 类型时，需要删除自己，但子节点是不能删除的）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int DeleteFolderDB(String path)
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                var query = from folder in context.FolderDBs
                            where folder.Path == path
                            select folder;
                foreach (var folder in query)
                {
                    List<DiskFile> files = folder.DiskFiles.ToList();

                    foreach (var file in files)
                    {
                        context.Entry(file).State = EntityState.Deleted;
                    }

                    context.FolderDBs.Remove(folder);
                }
                return context.SaveChanges();
            }

        }

        /// <summary>
        /// 删除所有文件夹节点记录
        /// </summary>
        public void DeleteAllFolderRecords()
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                List<FolderDB> folders = context.FolderDBs.ToList();
                foreach (var folder in folders)
                {
                    context.FolderDBs.Remove(folder);
                }
                context.SaveChanges();
            }

        }
        /// <summary>
        /// 删除某个文件夹节点中的指定文件
        /// </summary>
        /// <param name="folderDBPath"></param>
        /// <param name="fileID"></param>
        public void DeleteFileOfFolderDB(String folderDBPath, int fileID)
        {
            List<int> fileIds = new List<int>();
            fileIds.Add(fileID);
            DeleteFilesOfFolderDB(folderDBPath, fileIds);

        }

        /// <summary>
        /// 从文件夹节点所关联的文件集合中删除指定的文件
        /// </summary>
        /// <param name="folderDBPath"></param>
        /// <param name="fileIDs"></param>
        public void DeleteFilesOfFolderDB(String folderDBPath, List<int> fileIDs)
        {
            if (string.IsNullOrEmpty(folderDBPath) || fileIDs == null || fileIDs.Count == 0)
            {
                return;
            }
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                FolderDB folder = context.FolderDBs.FirstOrDefault(p => p.Path == folderDBPath);
                if (folder == null)
                {
                    return;
                }

                foreach (var file in folder.DiskFiles.ToList())
                {
                    if (fileIDs.IndexOf(file.ID) != -1)
                    {
                        //保证删除FolderAndFile记录
                        folder.DiskFiles.Remove(file);
                        //确保DiskFile删除
                        context.Entry(file).State = EntityState.Deleted;
                    }


                }
                context.SaveChanges();
            }

        }

        #endregion
        /// <summary>
        /// 按照文件ID从数据库中提取文件内容
        /// 找不到返回null
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public byte[] getFileContent(int fileID)
        {
            using (InfocenterEntities context = new InfocenterEntities(DALConfig.ConnectString))
            {
                DiskFile file = context.DiskFiles.FirstOrDefault(f => f.ID == fileID);
                if (file != null)
                {
                    return file.FileContent;
                }
            }
            return null;
        }


    }
}
