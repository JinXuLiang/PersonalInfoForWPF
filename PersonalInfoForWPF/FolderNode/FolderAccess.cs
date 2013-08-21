using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAccessLayer;
using DataAccessLayer.Folder;
using System.Collections.ObjectModel;

namespace FolderNode
{
    public class FolderAccess:IDataAccess
    {
        private FolderRepository repository = null;
        //public FolderAccess()
        //{
        //    repository = new FolderRepository(DALConfig.EFConnectString);
        //}

        public FolderAccess(String EFConnectionString)
        {
            repository = new FolderRepository(EFConnectionString);
        }
        /// <summary>
        /// 新建一个Folder记录，不包容任何文件
        /// </summary>
        /// <param name="dataInfoObject"></param>
        /// <returns></returns>
        public int Create(IDataInfo dataInfoObject)
        {
            if (dataInfoObject == null || (dataInfoObject as FolderInfo) == null)
            {
                return 0;
            }
            FolderDB dbobj = FolderHelper.changeToFolderDB(dataInfoObject as FolderInfo);
            int result = repository.AddFolderDB(dbobj);
            //将数据库生成的ID值传回
            dataInfoObject.ID = dbobj.ID;
            return 0;
        }

        public int DeleteDataInfoObjectOfNodeAndItsChildren(string nodePath)
        {
            if (String.IsNullOrEmpty(nodePath))
            {
                return 0;
            }
            return repository.DeleteFolderDBAndItsChildByPath(nodePath);
        }

        public int DeleteDataInfoObject(IDataInfo dataInfoObject)
        {
            if (dataInfoObject == null)
            {
                return 0;
            }
            return repository.DeleteFolderDB(dataInfoObject.Path);
        }

        public int UpdateDataInfoObject(IDataInfo dataInfoObject)
        {
            FolderInfo obj = dataInfoObject as FolderInfo;
            if (String.IsNullOrEmpty(obj.Text)==false && obj.Text.Length > DALConfig.MaxTextFieldSize)
            {
                obj.Text = obj.Text.Substring(0, DALConfig.MaxTextFieldSize);
            }
            if (dataInfoObject == null || obj == null)
            {
                return 0;
            }
            bool isNew = false;
            FolderDB dbobj = repository.GetFolderDBWithoutFileInfosByPath(obj.Path);
            if (dbobj == null)
            {
                dbobj = new FolderDB();
                isNew = true;

            }
            dbobj.ModifyTime = obj.ModifyTime;
            dbobj.Text = obj.Text;
            dbobj.Path = obj.Path;
            dbobj.RTFText = (String.IsNullOrEmpty(obj.RTFText)) ? null : Encoding.UTF8.GetBytes(obj.RTFText);
            if (isNew)
            {
                return repository.AddFolderDB(dbobj);
            }
            else
            {

                return repository.UpdateFolderDB(dbobj);


            }
        }

        public IDataInfo GetDataInfoObjectByPath(string nodePath)
        {
            FolderDB dbobj = repository.GetFolderDBWithoutFileInfosByPath(nodePath);
            ObservableCollection<DBFileInfo> files = repository.GetFileInfosOfFolderDB(nodePath);
           
                FolderInfo folderInfo = FolderHelper.changeToFolderInfo(dbobj);
                if (folderInfo != null)
                {
                    folderInfo.AttachFiles = files;
                }
           
            return folderInfo;
        }

        public void UpdateNodePath(string oldPath, string newPath)
        {
            repository.UpdateNodePaths(oldPath, newPath);
        }
        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="fileContent"></param>
        public void AddFile(string nodePath,DBFileInfo fileInfo, byte[] fileContent)
        {
            DiskFile file=DBFileInfo.toDiskFile(fileInfo, fileContent);
            repository.AddFileOfFolderDB(nodePath, file);
            //将ID传回
            fileInfo.ID = file.ID;
        }

        public void DeleteFilesOfFolderDB(String folderDBPath,List<int>fileIDs)
        {
            repository.DeleteFilesOfFolderDB(folderDBPath, fileIDs);
        }
        /// <summary>
        /// 按照fileID从数据库中提取文件的二进制数据
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        public byte[] getFileContent(int fileID)
        {
            return repository.getFileContent(fileID);
        }
    }
}
