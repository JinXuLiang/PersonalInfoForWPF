using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFSuperTreeView
{
    /// <summary>
    /// 用于封装对SuperTreeView内部节点集合的管理功能
    /// </summary>
    public class TreeViewNodesManager
    {
        private ObservableCollection<TreeViewIconsItem> _nodes = new ObservableCollection<TreeViewIconsItem>();
        /// <summary>
        /// 用于保存所有子节点的引用
        /// </summary>
        public ObservableCollection<TreeViewIconsItem> nodes
        {
            get
            {
                return _nodes;
            }
        }
        /// <summary>
        /// 删除节点时，在节点集合中删除它及所有子节点
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return;
            }
           
            var query = from n in nodes
                        where n.Path.StartsWith(node.Path)
                        select n;
            List<TreeViewIconsItem> nodesToBeDeletes = query.ToList() ;
            foreach (var n in nodesToBeDeletes)
            {
                nodes.Remove(n);
            }
        }
        /// <summary>
        /// 查找其路径以OldPath打头的所有记录，并且将其替换为newPath
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public void UpdateNodePath(String oldPath, String newPath)
        {
            if (String.IsNullOrEmpty(oldPath) || String.IsNullOrEmpty(newPath))
            {
                return;
            }
            var query = from n in _nodes
                        where n.Path.StartsWith(oldPath)
                        select n;
            foreach (var n in query)
            {
                n.Path = n.Path.Replace(oldPath, newPath);
                n.NodeData.DataItem.Path = n.Path;
            }
        }
    }
}
