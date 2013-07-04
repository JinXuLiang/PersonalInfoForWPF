using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DataAccessLayer.Folder
{
    /// <summary>
    /// 代表保存于数据库中的文件信息
    /// 之所以设计它是为了避免将文件内容全部加载到内存所带来的内存浪费
    /// 此对象支持WPF数据绑定
    /// 此对象重写了equals方法和getHashCode()方法，因此，支持判等操作，判断依据是FilePath属性
    /// 即FilePath属性相等的两个DBFileInfo对象认为是相等的。
    /// </summary>
    public class DBFileInfo : INotifyPropertyChanged
    {
        public int ID { get; set; }
        private String _filePath = "";
        public String FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }
        private long _fileSize = 0;
        public long FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                _fileSize = value;
                OnPropertyChanged("FileSize");
            }
        }
        private DateTime _time;
        public DateTime AddTime
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                OnPropertyChanged("AddTime");
            }
        }
        /// <summary>
        /// 将DBFileInfo对象转换为EF可以直接保存的DiskFile对象
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public static DiskFile toDiskFile(DBFileInfo fileInfo, byte[] fileContent)
        {
            if (fileInfo != null)
            {
                DiskFile file = new DiskFile()
                {
                    ID = fileInfo.ID,
                    FileSize = fileInfo.FileSize,
                    FilePath = fileInfo.FilePath,
                    AddTime = fileInfo.AddTime,
                    FileContent = fileContent
                };
                return file;

            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public override bool Equals(object obj)
        {
            if ((obj is DBFileInfo) == false)
                return false;

            return _filePath==(obj as DBFileInfo).FilePath;
        }

        public override int GetHashCode()
        {
            return _filePath.GetHashCode();
        }
    }
}
