using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using InterfaceLibrary;
using NodeFactoryLibrary;
using DataAccessLayer.MainTree;
using System.Threading.Tasks;


namespace WPFSuperTreeView
{
    /// <summary>
    /// 此用户控件内部组合了一个TreeView，向外界提供各种方便的树操作功能
    /// 在需要时，外界可以通过InnerTreeView属性访问内部的TreeView控件
    /// </summary>
    public partial class SuperTreeView : UserControl
    {
        public SuperTreeView()
        {
            InitializeComponent();

        }

        public SuperTreeView(String EFConnectionString)
        {
            InitializeComponent();
            this.EFConnectionString = EFConnectionString;
        }
        /// <summary>
        /// 用于在数据库中更新相关路径
        /// </summary>
        private NodePathManager _nodePathManager = null;
        public NodePathManager TreeNodePathManager
        {
            get
            {
                return _nodePathManager;
            }
            set
            {
                _nodePathManager = value;
            }
        }
        /// <summary>
        /// 用于存储树节点的数据库连接字段，只有设置此属性，才能获得从数据库中存取数据的能力
        /// </summary>
        private String _EFConnectionString = "";
        public String EFConnectionString
        {
            get
            {
                return _EFConnectionString;
            }
            set
            {
                _EFConnectionString = value;
                _nodePathManager = new NodePathManager(_EFConnectionString);
                _repository = new MainTreeRepository(_EFConnectionString);
            }
        }
        /// <summary>
        /// 用于存取数据库
        /// </summary>
        private MainTreeRepository _repository = null;

        /// <summary>
        /// 清除所有节点,未清除数据库
        /// </summary>
        public void ClearAllNodes()
        {
            tree.Items.Clear();
            nodesManager.nodes.Clear();
            ShouldRaiseSelectedItemChangedEvent = true;

        }

        /// <summary>
        /// 按照路径显示节点
        /// </summary>
        /// <param name="NodePath"></param>
        /// <returns></returns>
        public TreeViewIconsItem ShowNode(String NodePath)
        {

            if (String.IsNullOrEmpty(NodePath))
            {
                return null;
            }
            //要显示的节点就是当前节点
            TreeViewIconsItem curSelectedNode = tree.SelectedItem as TreeViewIconsItem;
            if (curSelectedNode != null && curSelectedNode.Path == NodePath)
            {
                return null;
            }
            //查找节点
            TreeViewIconsItem node = Nodes.FirstOrDefault(n => n.Path == NodePath);
            if (node != null)
            {
                ShouldRaiseSelectedItemChangedEvent = true;
                node.IsSelected = true;
                node.IsExpanded = true;
                ExpandToNode(node);
                node.Focus();
                return node;
            }

            return null;

        }

        /// <summary>
        /// 当树处于编辑状态时，应该禁用所有针对它的命令
        /// </summary>
        public bool IsInEditMode { get; set; }
        /// <summary>
        /// 已装入的节点的数量
        /// </summary>
        public int NodeCount
        {
            get { return _nodesManager.nodes.Count; }
        }

        private TreeViewNodesManager _nodesManager = new TreeViewNodesManager();
        /// <summary>
        /// 提供树节点集合的管理工作
        /// </summary>
        public TreeViewNodesManager nodesManager
        {
            get
            {
                return _nodesManager;
            }
        }

        /// <summary>
        /// 在需要时，外界可以通过此属性访问内部的TreeView控件
        /// </summary>
        public TreeView InnerTreeView
        {
            get
            {
                return tree;
            }
        }
        /// <summary>
        /// 直接展开显示指定的节点
        /// </summary>
        /// <param name="item"></param>
        public void ExpandToNode(TreeViewItem item)
        {
            item.IsExpanded = true;

            if (item.Parent is TreeViewItem)
                ExpandToNode(item.Parent as TreeViewItem);
        }
        /// <summary>
        /// 本TreeView所有节点对象的集合，可用于数据绑定
        /// </summary>
        public ObservableCollection<TreeViewIconsItem> Nodes
        {
            get
            {
                return nodesManager.nodes;
            }
        }

        #region "节点的添加"


        private Boolean _autoSelectNewNode = true;
        /// <summary>
        /// 是否自动选中新加的节点，默认值为True
        /// </summary>
        public Boolean AutoSelectNewNode
        {
            get { return _autoSelectNewNode; }
            set { _autoSelectNewNode = value; }
        }
        /// <summary>
        /// 判断相同路径的节点是否己经存在
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public bool IsNodeExisted(String nodePath)
        {
            TreeViewIconsItem item = Nodes.FirstOrDefault(p => p.Path == nodePath);
            return item != null;
        }

        /// <summary>
        /// 添加节点
        /// 成功返回新节点，不成功，返回null
        /// 新节点自动展开且选中
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="nodeText"></param>
        /// <param name="nodeIcon"></param>
        /// <returns></returns>
        private TreeViewIconsItem addNode(AddNodeCategory addType, TreeViewIconsItem selectedNode, String nodeText, NodeDataObject nodeDataObject)
        {
            if (selectedNode != null)
            {
                selectedNode.IsExpanded = true;
            }
            ShouldRaiseSelectedItemChangedEvent = true;

            TreeViewIconsItem treeNode = new TreeViewIconsItem(this, nodeDataObject);
            treeNode.HeaderText = nodeText;
            treeNode.Icon = nodeDataObject.DataItem.NormalIcon;
            //自动展开
            treeNode.IsExpanded = true;
            switch (addType)
            {
                case AddNodeCategory.AddRoot:
                    treeNode.Path = "/" + nodeText + "/";
                    nodeDataObject.DataItem.Path = treeNode.Path;
                    tree.Items.Add(treeNode);
                    nodesManager.nodes.Add(treeNode);
                    break;
                case AddNodeCategory.AddChild:
                    if (selectedNode != null)
                    {
                        treeNode.Path = selectedNode.Path + nodeText + "/";
                        nodeDataObject.DataItem.Path = treeNode.Path;
                        selectedNode.Items.Add(treeNode);
                        nodesManager.nodes.Add(treeNode);
                    }
                    else
                    {
                        return null;
                    }
                    break;
                case AddNodeCategory.AddSibling:
                    if (selectedNode != null && selectedNode.Parent != null)
                    {
                        //如果选中节点是顶层节点
                        if (selectedNode.Parent == tree)
                        {
                            treeNode.Path = "/" + nodeText + "/";
                            nodeDataObject.DataItem.Path = treeNode.Path;
                            tree.Items.Add(treeNode);
                        }
                        else
                        {
                            //不是顶层节点
                            TreeViewIconsItem parent = selectedNode.Parent as TreeViewIconsItem;
                            treeNode.Path = parent.Path + nodeText + "/";
                            nodeDataObject.DataItem.Path = treeNode.Path;
                            parent.Items.Add(treeNode);
                        }
                        nodesManager.nodes.Add(treeNode);
                    }
                    else  //当前没有选中节点或选中节点没有父亲
                    {
                        return null;
                    }

                    break;
                default:
                    break;
            }
            if (_autoSelectNewNode)
            {
                treeNode.IsSelected = true;
            }

            //更新树的存储
            String treeXml = saveToXmlString();
            if (_repository != null)
            {
                _repository.SaveTree(treeXml);
            }


            return treeNode;

        }
        /// <summary>
        /// 添加根节点，如果参数中有为null或空串的，返回null
        /// </summary>
        /// <param name="nodeText"></param>
        /// <param name="nodeIcon"></param>
        /// <returns></returns>
        public TreeViewIconsItem AddRoot(String nodeText, NodeDataObject nodeDataObject)
        {
            if (String.IsNullOrEmpty(nodeText))
            {
                return null;
            }
            return addNode(AddNodeCategory.AddRoot, null, nodeText, nodeDataObject);
        }
        /// <summary>
        /// 添加子节点，如果参数中有为null或空串的，或者当前TreeView中没有选中的节点，返回null
        /// </summary>
        /// <param name="nodeText"></param>
        /// <param name="nodeIcon"></param>
        /// <returns></returns>
        public TreeViewIconsItem AddChild(String nodeText, NodeDataObject nodeDataObject)
        {
            if (String.IsNullOrEmpty(nodeText) || tree.SelectedItem == null)
            {
                return null;
            }
            return addNode(AddNodeCategory.AddChild, tree.SelectedItem as TreeViewIconsItem, nodeText, nodeDataObject);
        }
        /// <summary>
        /// 添加兄弟节点，如果参数中有为null或空串的，或者当前TreeView中没有选中的节点，返回null
        /// </summary>
        /// <param name="nodeText"></param>
        /// <param name="nodeIcon"></param>
        /// <returns></returns>
        public TreeViewIconsItem AddSibling(String nodeText, NodeDataObject nodeDataObject)
        {
            if (String.IsNullOrEmpty(nodeText) || tree.SelectedItem == null)
            {
                return null;
            }
            return addNode(AddNodeCategory.AddSibling, tree.SelectedItem as TreeViewIconsItem, nodeText, nodeDataObject);
        }

        #endregion

        #region "节点删除"
        /// <summary>
        /// 删除指定的节点,同时更新内存中的节点数据及数据库中的记录
        /// </summary>
        /// <param name="selectedNode"></param>
        public void DeleteNode(TreeViewIconsItem selectedNode)
        {

            if (selectedNode == null || selectedNode.Parent == null)
            {
                return;
            }
            ShouldRaiseSelectedItemChangedEvent = true;
            if (selectedNode.Parent == tree)
            {
                tree.Items.Remove(selectedNode);
            }
            else
            {
                (selectedNode.Parent as TreeViewIconsItem).Items.Remove(selectedNode);
            }
            //更新内存中的节点集合
            nodesManager.DeleteNode(selectedNode);
            //更新数据库
            if (_nodePathManager != null)
            {
                _nodePathManager.DeleteDataInfoObjectOfNodeAndItsChildren(selectedNode.Path);
            }


        }
        #endregion

        #region "节点遍历"
        /// <summary>
        /// 查找选中节点在本层节点集合中的索引，找不到返回-1
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeIndex(TreeViewIconsItem node)
        {
            int index = -1;
            TreeViewIconsItem tempNode = null;
            ItemCollection nodes = null;
            if (node.Parent != null)
            {
                if (node.Parent == tree)
                {
                    //是一级节点，在TreeView的子节点集合中查找
                    nodes = tree.Items;
                }
                else
                {
                    //在父节点的子节点集合中查找
                    nodes = (node.Parent as TreeViewItem).Items;
                }
                //开始查找
                for (int i = 0; i < nodes.Count; i++)
                {
                    tempNode = nodes[i] as TreeViewIconsItem;
                    if (tempNode.CompareTo(node) == 0)
                    {
                        index = i;
                        break;
                    }
                }

            }
            return index;
        }

        /// <summary>
        /// 获取节点的父亲
        /// 如果是顶层结点，返回TreeView，否则，会得到一个TreeViewIconsItem
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ItemsControl GetParent(TreeViewItem node)
        {
            if (node == null)
            {
                return null;
            }
            if (node.Parent == tree)
            {
                return tree as ItemsControl;
            }
            else
            {
                return node.Parent as TreeViewIconsItem;
            }
        }
        /// <summary>
        /// 判断某节点是否是顶层结点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Boolean IsFirstLevelNode(TreeViewItem node)
        {
            if (node == null)
            {
                throw new NullReferenceException("参数node==null");
            }
            if (node.Parent == tree)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取前一个兄弟节点
        /// 如果己经是第一个了，返回null
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public TreeViewIconsItem GetPrevNode(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return null;
            }
            int index = GetNodeIndex(node);
            if (index == 0)
            {
                return null;
            }

            if (IsFirstLevelNode(node))
            {
                return tree.Items[index - 1] as TreeViewIconsItem;
            }
            else
            {
                return (node.Parent as TreeViewIconsItem).Items[index - 1] as TreeViewIconsItem;

            }

        }
        /// <summary>
        /// 获取后一个兄弟节点
        /// 如果己经是最后一个了，返回null
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public TreeViewIconsItem GetNextNode(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return null;
            }
            int nodeCount = GetParent(node).Items.Count;


            int index = GetNodeIndex(node);
            if (index == nodeCount - 1)
            {
                return null;
            }

            if (IsFirstLevelNode(node))
            {
                return tree.Items[index + 1] as TreeViewIconsItem;
            }
            else
            {
                return (node.Parent as TreeViewIconsItem).Items[index + 1] as TreeViewIconsItem;

            }

        }
        /// <summary>
        /// 获取指定节点的所有孩子
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<TreeViewIconsItem> GetChildren(TreeViewIconsItem node)
        {

            if (node == null)
            {
                return null;
            }
            List<TreeViewIconsItem> children = new List<TreeViewIconsItem>();
            TreeViewIconsItem tempNode = null;
            foreach (var item in node.Items)
            {
                tempNode = item as TreeViewIconsItem;
                if (tempNode != null)
                    children.Add(tempNode);
            }
            return children;
        }

        #endregion

        #region "节点移动及NodeMove事件"
        /// <summary>
        /// 用于确定是否激发树的SelectedItemChanged事件通知外界
        /// </summary>
        private bool ShouldRaiseSelectedItemChangedEvent = true;
        /// <summary>
        /// 节点移动事件
        /// </summary>
        public event EventHandler<NodeMoveEventArgs> NodeMove;
        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="node"></param>
        public void MoveUp(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return;
            }
            int index = GetNodeIndex(node);
            //最前一个无法上移
            if (index == 0)
            {
                return;
            }
            //上移时，不激发树的SelectedItemChanged事件
            ShouldRaiseSelectedItemChangedEvent = false;
            //先删除自己，再在新位置重新插入
            tree.BeginInit();
            ItemsControl parent = GetParent(node);
            parent.Items.RemoveAt(index);
            parent.Items.Insert(index - 1, node);
            node.IsSelected = true;
            tree.EndInit();

            ShouldRaiseSelectedItemChangedEvent = true;

        }
        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="node"></param>
        public void MoveDown(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return;
            }
            //共有多少个兄弟节点
            ItemsControl parent = GetParent(node);
            int nodeCount = parent.Items.Count;
            //获取我的索引
            int index = GetNodeIndex(node);
            //最后一个无法下移
            if (index == nodeCount - 1)
            {
                return;
            }
            //下移时，不激发树的SelectedItemChanged事件
            ShouldRaiseSelectedItemChangedEvent = false;
            //先删除自己，再在新位置重新插入
            tree.BeginInit();
            parent.Items.RemoveAt(index);
            parent.Items.Insert(index + 1, node);
            node.IsSelected = true;

            tree.EndInit();
            ShouldRaiseSelectedItemChangedEvent = true;

        }
        /// <summary>
        /// 左移（即升级）
        /// 1.已是顶级节点则不动
        /// 2.成为父节点的兄弟
        /// </summary>
        /// <param name="node"></param>
        public void MoveLeft(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return;
            }
            //己经是顶级节点
            if (IsFirstLevelNode(node))
            {
                return;
            }
            //左移时，不激发树的SelectedItemChanged事件
            ShouldRaiseSelectedItemChangedEvent = false;
            //记录下当前路径
            String oldPath = node.Path;
            //查找父节点
            TreeViewIconsItem parent = GetParent(node) as TreeViewIconsItem;
            int parentIndex = GetNodeIndex(parent);
            //查找爷爷节点
            ItemsControl grandfather = GetParent(parent);
            String newPath = "";
            //获取新路径
            if (grandfather == tree)
            {
                newPath = "/" + node.HeaderText + "/";
            }
            else
            {
                newPath = (grandfather as TreeViewIconsItem).Path + node.HeaderText + "/";
            }
            if (IsNodeExisted(newPath))
            {
                MessageBox.Show("己经存在相同路径的节点，不允许升级");
                return;
            }
            tree.BeginInit();
            //先移除自己
            parent.Items.Remove(node);
            //插入成为父亲的兄弟节点
            grandfather.Items.Insert(parentIndex + 1, node);

            node.Path = newPath;
            node.NodeData.DataItem.Path = newPath;
            //更新所有内存相关子节点集合的路径
            nodesManager.UpdateNodePath(oldPath, newPath);
            if (_nodePathManager != null)
            {
                //更新数据库中相关子节点的路径
                _nodePathManager.UpdateNodePath(oldPath, newPath);

            }

            node.IsSelected = true;

            tree.EndInit();
            ShouldRaiseSelectedItemChangedEvent = true;
            //激发事件
            if (NodeMove != null)
            {
                NodeMoveEventArgs e = new NodeMoveEventArgs
                {
                    MoveType = NodeMoveType.NodeMoveLeft,
                    Node = node,
                    PrevPath = oldPath
                };
                NodeMove(node, e);
            }
        }
        /// <summary>
        /// 右移（即降级）
        /// 1.无兄弟节点则不动
        /// 2.如有兄弟节点,则成为上一个兄弟节点的子节点
        /// 3.如果本身是所有兄弟节点中的第一个,则成为下一个兄弟的子节点
        /// </summary>
        /// <param name="node"></param>
        public void MoveRight(TreeViewIconsItem node)
        {
            if (node == null)
            {
                return;
            }
            //右移时，不激发树的SelectedItemChanged事件
            ShouldRaiseSelectedItemChangedEvent = false;
            //记录下当前路径
            String oldPath = node.Path;

            ItemsControl parent = GetParent(node);
            //查找兄弟节点
            TreeViewIconsItem prevBrother = GetPrevNode(node);
            TreeViewIconsItem nextBrother = GetNextNode(node);
            //是独子
            if (prevBrother == null && nextBrother == null)
            {
                return;
            }
            //获取新路径
            String newPath = "";

            if (prevBrother != null)
            {

                newPath = prevBrother.Path + node.HeaderText + "/";

            }
            else
            {

                newPath = nextBrother.Path + node.HeaderText + "/";

            }
            if (IsNodeExisted(newPath))
            {
                MessageBox.Show("已经存在相同路径的节点，不允许降级。");
                return;
            }


            tree.BeginInit();
            //删除自己
            parent.Items.Remove(node);
            //插入
            if (prevBrother != null)
            {
                prevBrother.Items.Add(node);
                node.Path = newPath;
                node.NodeData.DataItem.Path = newPath;
                prevBrother.IsExpanded = true;
            }
            else
            {
                nextBrother.Items.Add(node);
                node.Path = newPath;
                node.NodeData.DataItem.Path = newPath;
                nextBrother.IsExpanded = true;
            }



            //更新所有内存相关子节点集合的路径
            nodesManager.UpdateNodePath(oldPath, newPath);
            if (_nodePathManager != null)
            {
                //更新数据库中相关子节点的路径
                _nodePathManager.UpdateNodePath(oldPath, newPath);
            }


            node.IsSelected = true;
            tree.EndInit();
            ShouldRaiseSelectedItemChangedEvent = true;
            //激发事件
            if (NodeMove != null)
            {
                NodeMoveEventArgs e = new NodeMoveEventArgs
                {
                    MoveType = NodeMoveType.NodeMoveRight,
                    Node = node,
                    PrevPath = oldPath
                };
                NodeMove(node, e);
            }
        }

        /// <summary>
        /// 剪切一个节点
        /// </summary>
        /// <param name="nodeToBeCut"></param>
        public TreeViewIconsItem CutNode(TreeViewIconsItem nodeToBeCut)
        {
            if (nodeToBeCut == null)
            {
                return null;
            }
            ShouldRaiseSelectedItemChangedEvent = true;
            ItemsControl parent = GetParent(nodeToBeCut);
            parent.Items.Remove(nodeToBeCut);
            return nodeToBeCut;
        }
        /// <summary>
        /// 粘贴节点并自动更新相关的内存及数据库中的记录
        /// 要求被剪切的节点必须是“独立”的（其Parent属性==null)
        /// </summary>
        /// <param name="nodeToBeCut">被剪切的节点</param>
        /// <param name="attachToNode">将接收被剪切节点的那个节点</param>
        public void PasteNode(TreeViewIconsItem nodeToBeCut, TreeViewIconsItem attachToNode)
        {
            PasteNode(nodeToBeCut, attachToNode, false);
        }
        /// <summary>
        /// 粘贴节点并自动更新相关的节点内存记录,但不更新底层数据库
        /// </summary>
        /// <param name="nodeToBeCut"></param>
        /// <param name="attachToNode"></param>
        public void PasteNodeCrossDB(TreeViewIconsItem nodeToBeCut, TreeViewIconsItem attachToNode)
        {
            PasteNode(nodeToBeCut, attachToNode, true);
        }

        /// <summary>
        /// 粘贴节点并自动更新相关的内存,IsCrossDB表明是否是跨数据库粘贴
        /// </summary>
        /// <param name="nodeToBeCut"></param>
        /// <param name="attachToNode"></param>
        /// <param name="IsCrossDB">IsCrossDB表明是否是跨数据库粘贴</param>
        private void PasteNode(TreeViewIconsItem nodeToBeCut, TreeViewIconsItem attachToNode, bool IsCrossDB)
        {
            if (nodeToBeCut == null || nodeToBeCut.Parent != null)
            {
                return;
            }
            ShouldRaiseSelectedItemChangedEvent = true;
            String oldPath = nodeToBeCut.Path;
            String newPath = "";
            //如果attachToNode为null，则将在根节点添加子树
            if (attachToNode == null)
            {
                //newPath = "/" + nodeToBeCut.HeaderText + "/";
                newPath = "/";
            }
            else
            {
                //当前己经选中了节点，被粘贴的节点成为其子树
                newPath = attachToNode.Path + nodeToBeCut.HeaderText + "/";
            }

            tree.BeginInit();

            if (!IsCrossDB)
            {
                if (attachToNode != null)
                {
                    //被粘贴的节点成为当前选中节点的子树
                    attachToNode.Items.Add(nodeToBeCut);
                }
                else
                {
                    //在顶层放置被粘贴的节点
                    tree.Items.Add(nodeToBeCut);
                }

                //更新所有内存相关子节点集合的路径
                nodesManager.UpdateNodePath(oldPath, newPath);
                if (_nodePathManager != null)
                {
                    //更新数据库中相关子节点的路径
                    _nodePathManager.UpdateNodePath(oldPath, newPath);
                }

                tree.EndInit();
                //激发事件
                if (NodeMove != null)
                {
                    NodeMoveEventArgs e = new NodeMoveEventArgs
                    {
                        MoveType = NodeMoveType.NodePaste,
                        Node = nodeToBeCut,
                        PrevPath = oldPath
                    };
                    NodeMove(nodeToBeCut, e);
                }
            }
            else
            {
                //跨数据库的粘贴
               
                if (attachToNode != null)
                {
                    //被粘贴的节点成为当前选中节点的子树
                    attachToNode.Items.Add(nodeToBeCut); 
                    updateChildNodePath(attachToNode.Path, nodeToBeCut);
                }
                else
                {
                    //在顶层放置被粘贴的节点
                    tree.Items.Add(nodeToBeCut);
                    updateChildNodePath("/", nodeToBeCut);
                }
               

                tree.EndInit();

            }
        }
        /// <summary>
        /// 为跨数据库粘贴的树节点更新其路径信息
        /// </summary>
        /// <param name="newRootPath"></param>
        /// <param name="nodeAttachedToNewParent"></param>
        private void updateChildNodePath(String newRootPath, TreeViewIconsItem nodeAttachedToNewParent)
        {

            if (nodeAttachedToNewParent != null)
            {
                String oldChildTreeRootPath = nodeAttachedToNewParent.Path;
                nodeAttachedToNewParent.Path = newRootPath + nodeAttachedToNewParent.HeaderText + "/";
                nodesManager.nodes.Add(nodeAttachedToNewParent);
                foreach (var child in nodeAttachedToNewParent.Items)
                {
                    updateChildNodePath(nodeAttachedToNewParent.Path, (child as TreeViewIconsItem));
                }
            }
        }

        #endregion

        #region "SelectedItem和SelectedItemChanged事件"

        private void InnerTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (SelectedItemChanged != null && ShouldRaiseSelectedItemChangedEvent)
            {
                SelectedItemChanged(sender, e);
            }


        }
        /// <summary>
        /// 用于将内部TreeView的SelectedItemChanged事件导出到UserControl外部以方便使用
        /// </summary>
        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;

        /// <summary>
        /// 当前选中的节点
        /// </summary>
        public TreeViewIconsItem SelectedItem
        {
            get
            {
                return tree.SelectedItem as TreeViewIconsItem;
            }

        }
        #endregion

        #region "XML序列化支持"
        // <summary>
        /// 从XElement中创建TreeViewIconsItem对象，不理会其子节点
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private TreeViewIconsItem CreateTreeViewNodeFromXElement(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            NodeDataObject nodeDataObject = NodeFactory.CreateDataInfoNode(element.Attribute("NodeType").Value, EFConnectionString);
            TreeViewIconsItem node = new TreeViewIconsItem(this, nodeDataObject);
            node.HeaderText = element.Attribute("Title").Value;
            if (element.Attribute("Foreground") != null)
            {
                String color = element.Attribute("Foreground").Value;

                node.MyForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
            else
            {
                //默认为黑色
                node.Foreground = Brushes.Black;
            }

            if (element.Attribute("FontWeight") != null)
            {
                String FontWeight = element.Attribute("FontWeight").Value;
                if (FontWeight != "Normal")
                {
                    node.FontWeight = FontWeights.ExtraBold;
                }
                else
                {
                    node.FontWeight = FontWeights.Normal;
                }

            }
            node.Icon = nodeDataObject.DataItem.NormalIcon;
            return node;
        }

        private XElement CreateXElementFromTreeViewIconsItem(TreeViewIconsItem node)
        {
            XElement element = new XElement("TreeNode");
            element.SetAttributeValue("Title", node.HeaderText);
            element.SetAttributeValue("NodeType", node.NodeData.DataItem.NodeType);
            element.SetAttributeValue("FontWeight", node.FontWeight);
            if (node.Foreground != Brushes.White)
            {
                element.SetAttributeValue("Foreground", node.MyForeground);
            }


            return element;
        }

        private void CreateXmlTreeFromTreeViewIconsItem(TreeViewIconsItem rootNode, XElement element)
        {
            XElement tempElement = null;

            foreach (var item in rootNode.Items)
            {
                //加入所有子节点
                tempElement = CreateXElementFromTreeViewIconsItem(item as TreeViewIconsItem);
                element.Add(tempElement);
                CreateXmlTreeFromTreeViewIconsItem(item as TreeViewIconsItem, tempElement);
            }

        }
        /// <summary>
        /// 提取TreeView中的所有节点，创建XElement对象树
        /// </summary>
        /// <returns></returns>
        private XElement CreateXmlTree()
        {
            XElement rootElement = new XElement("Root");
            XElement firstLevelNode = null;
            foreach (var item in tree.Items)
            {
                firstLevelNode = CreateXElementFromTreeViewIconsItem(item as TreeViewIconsItem);
                CreateXmlTreeFromTreeViewIconsItem(item as TreeViewIconsItem, firstLevelNode);
                rootElement.Add(firstLevelNode);

            }

            return rootElement;
        }
        /// <summary>
        /// 将整个TreeView中的所有节点转换为XML字串
        /// </summary>
        /// <returns></returns>
        public String saveToXmlString()
        {
            XElement tree = CreateXmlTree();
            using (MemoryStream mem = new MemoryStream())
            {
                tree.Save(mem);
                return Encoding.UTF8.GetString(mem.ToArray());
            }

        }
        /// <summary>
        /// 将树保存到底层数据库中
        /// </summary>
        public void SaveToDB()
        {
            if (_repository != null)
            {
                _repository.SaveTree(saveToXmlString());
            }
        }
        /// <summary>
        /// 将整个TreeView中的所有节点转换为XML字串并保存到文件中
        /// </summary>
        /// <param name="FileName"></param>
        public void saveToXMLFile(String FileName)
        {
            XElement tree = CreateXmlTree();
            tree.Save(FileName);
        }
        /// <summary>
        /// 将element的所有子节点添加为rootNode的子节点，递归进行
        /// </summary>
        /// <param name="element"></param>
        /// <param name="rootNode"></param>
        /// <param name="nodeIcon"></param>
        private void AddXmlNodeToTree(XElement element, TreeViewIconsItem rootNode)
        {
            if (element == null)
            {
                return;
            }

            TreeViewIconsItem tempNode = null;
            foreach (var item in element.Elements())
            {
                tempNode = CreateTreeViewNodeFromXElement(item);
                tempNode.Path = rootNode.Path + tempNode.HeaderText + "/";
                tempNode.NodeData.DataItem.Path = tempNode.Path;
                rootNode.Items.Add(tempNode);
                nodesManager.nodes.Add(tempNode);
                //递归处理树节点
                AddXmlNodeToTree(item, tempNode);
            }

        }
        /// <summary>
        /// 以rootElement为根节点，创建整个子树
        /// </summary>
        /// <param name="rootElement"></param>
        /// <param name="nodeIcon"></param>
        /// <returns></returns>
        private TreeViewIconsItem CreateTree(XElement rootElement)
        {



            TreeViewIconsItem root = CreateTreeViewNodeFromXElement(rootElement);
            root.Path = "/" + root.HeaderText + "/";
            root.NodeData.DataItem.Path = root.Path;
            nodesManager.nodes.Add(root);
            AddXmlNodeToTree(rootElement, root);
            return root;

        }
        /// <summary>
        /// 从Xml文件中装入树
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="nodeIcon"></param>
        public void LoadFromXmlFile(String FileName)
        {
            if (File.Exists(FileName) == false)
            {
                return;
            }
            tree.Items.Clear();
            nodesManager.nodes.Clear();
            String xml = File.ReadAllText(FileName);
            XElement xmlTree = XElement.Parse(xml);
            //XElement xmlTree = XElement.Load(FileName);

            foreach (var item in xmlTree.Elements())
            {

                TreeViewIconsItem root = CreateTree(item);
                tree.Items.Add(root);
            }

        }

        /// <summary>
        /// 从Xml字串中装入树
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="nodeIcon"></param>
        public void LoadFromXmlString(String xml)
        {
            if (String.IsNullOrEmpty(xml))
            {
                return;
            }
            //除去字串前面可能有的BOM，即字符65279，XElement解析时会报告错误 
            int index = xml.IndexOf("<");
            if (index != 0)
            {
                xml = xml.Substring(index);
            }
            XElement xmlTree = XElement.Parse(xml);
            foreach (var item in xmlTree.Elements())
            {

                TreeViewIconsItem root = CreateTree(item);
                tree.Items.Add(root);
            }
        }
        /// <summary>
        /// 从数据库中装载树
        /// </summary>
        public String LoadTreeXMLFromDB()
        {

            return _repository.GetTreeFromDB();
        }
        #endregion
    }
}
