using DataAccessLayer;
using DataAccessLayer.MainTree;
using InterfaceLibrary;
using NodeFactoryLibrary;
using PersonalInfoForWPF.BackAndForward;
using publicLibrary.Serializer;
using System;
using System.Collections.Generic;
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
using SystemLibrary;
using WPFSuperTreeView;

namespace PersonalInfoForWPF
{
    /// <summary>
    /// Interaction logic for DBInfoTab.xaml
    /// </summary>
    public partial class DBInfoTab : Grid
    {
        public DBInfoTab(DatabaseInfo infoObject)
        {
            InitializeComponent();
            dbInfoObject = infoObject;
            visitedNodesManager = new VisitedNodesManager(treeView1);
        }
        /// <summary>
        /// 用于实现节点历史访问记录功能
        /// </summary>
        public VisitedNodesManager visitedNodesManager = null;
        /// <summary>
        /// 相关联的数据库信息对象，序列化后将保存于配置文件中
        /// </summary>
        public DatabaseInfo dbInfoObject { get; set; }
        /// <summary>
        /// 是否己从数据库中装入
        /// </summary>
        public bool HasBeenLoadedFromDB { get; set; }
       

        #region "更改图标"


        private void ChangeNodeIcon(TreeViewIconsItem node, ImageSource newIcon)
        {
            if (node == null || newIcon == null)
            {
                return;
            }
            node.Icon = newIcon;
        }
        /// <summary>
        /// 当用户点击树节点时，切换节点图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChangedSelectedNodeIconWhenClick(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //将老节点的图标改为默认的文件夹图标
            TreeViewIconsItem oldNode = e.OldValue as TreeViewIconsItem;
            if (oldNode != null)
            {
                ChangeNodeIcon(oldNode, oldNode.NodeData.DataItem.NormalIcon);
            }
            //设置新节点图标
            TreeViewIconsItem selectedNode = e.NewValue as TreeViewIconsItem;
            if (selectedNode != null)
            {
                ChangeNodeIcon(selectedNode, selectedNode.NodeData.DataItem.SelectedIcon);
            }
        }
        #endregion

        public void ChangeNodeType(String ToType)
        {
            if (treeView1.SelectedItem != null)
            {
                treeView1.SelectedItem.EndEdit();
            }
            String FromType = treeView1.SelectedItem.NodeData.DataItem.NodeType;

            NodeDataObject nodeDataObject = treeView1.SelectedItem.NodeData;
            if (nodeDataObject.DataItem.NodeType == ToType)
            {
                MessageBox.Show(String.Format("本节点己经是{0}型的了。", ToType));
                return;
            }
            if (NodeTypeChanger.CanChangeTo(FromType, ToType) == false)
            {
                MessageBox.Show(string.Format("不能完成从{0}到{1}类型的转换。", FromType, ToType));
                return;
            }

            NodeDataObject detailTextNodeDataObject = NodeTypeChanger.GetNodeChanger(FromType, ToType).ChangeTo(nodeDataObject,DALConfig.getEFConnectionString(dbInfoObject.DatabaseFilePath));

            if (detailTextNodeDataObject != null)
            {
                treeView1.SelectedItem.NodeData = detailTextNodeDataObject;
                ChangeNodeIcon(treeView1.SelectedItem, detailTextNodeDataObject.DataItem.SelectedIcon);
                LoadDataAndShowInUI(treeView1.SelectedItem);
                SaveTreeToDB();
            }
        }
        /// <summary>
        /// 将树结构保存到数据库中
        /// </summary>
        public void SaveTreeToDB()
        {
            String treeXml = treeView1.saveToXmlString();
            
            (new MainTreeRepository(DALConfig.getEFConnectionString(dbInfoObject.DatabaseFilePath))).SaveTree(treeXml);

        }

        #region "用户点击树中的不同节点"

        /// <summary>
        /// 当切换数据库选项卡时，必须手动刷新界面
        /// </summary>
        public void RefreshDisplay()
        {
            if (treeView1.SelectedItem != null)
            {
                 LoadDataAndShowInUI(treeView1.SelectedItem);
            }
            else
            {
                //没有选中任何节点，则显示空白的窗体
                NodeUIContainer.Content = null;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            
            TreeViewIconsItem newSelectedNode = e.NewValue as TreeViewIconsItem;
            if (treeView1.IsInEditMode)
            {
                if (newSelectedNode != null)
                {
                    newSelectedNode.IsSelected = false;
                }
                return;
            }

            //更换节点图标
            ChangedSelectedNodeIconWhenClick(sender, e);

            if (newSelectedNode != null)
            {
                LoadDataAndShowInUI(newSelectedNode);
            }
            else
            {
                //没有选中任何节点，则显示空白的窗体
                NodeUIContainer.Content = null;
            }
            if (visitedNodesManager != null && e.OldValue != null)
            {
                visitedNodesManager.AddHistoryRecord((e.OldValue as TreeViewIconsItem).Path);
            }


        }
        /// <summary>
        /// 为新节点提取数据并显示
        /// </summary>
        /// <param name="newSelectedNode"></param>
        private void LoadDataAndShowInUI(TreeViewIconsItem newSelectedNode)
        {
            if (newSelectedNode.NodeData.DataItem.NodeType == "OnlyText")
            {
                NodeUIContainer.Content = null;
                return;
            }
            NodeDataObject dataInfoObject = newSelectedNode.NodeData;
            //正确地设置可视化界面所关联的数据存取对象
            dataInfoObject.DataItem.SetRootControlDataAccessObj(dataInfoObject.AccessObject);

            //检查一下数据是否己被装入
            if (!dataInfoObject.DataItem.HasBeenLoadFromStorage)
            {
                //装入数据
                IDataInfo dataObj = dataInfoObject.AccessObject.GetDataInfoObjectByPath(newSelectedNode.Path);
               
                if (dataObj != null)
                {
                    //将己装入数据的对象挂到节点上
                    newSelectedNode.NodeData.DataItem = dataObj;
                }
            }

            if (dataInfoObject.DataItem.ShouldEmbedInHostWorkingBench)
            {
                if (dataInfoObject.DataItem.RootControl.Parent != null)
                {
                    (dataInfoObject.DataItem.RootControl.Parent as ContentControl).Content=null;
                }
                NodeUIContainer.Content = dataInfoObject.DataItem.RootControl;
            }
            //显示最新的数据
            dataInfoObject.DataItem.BindToRootControl();
            dataInfoObject.DataItem.RefreshDisplay();

        }

        /// <summary>
        /// 如果当前有选中的节点，则显示快捷菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (treeView1.SelectedItem == null)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 当用户在TreeView上右击鼠标时，设置当前选中的节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement currentElement = treeView1.InputHitTest(e.GetPosition(treeView1));
            TreeViewIconsItem node = GetNodeUnderMouseCursor(currentElement as DependencyObject);
            if (node != null)
            {

                node.IsSelected = true;
            }
            else
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 获取当前鼠标指针下的TreeView节点
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private TreeViewIconsItem GetNodeUnderMouseCursor(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }
            DependencyObject parent = LogicalTreeHelper.GetParent(element);
            while (parent != null && (parent as TreeViewIconsItem) == null)
            {
                parent = LogicalTreeHelper.GetParent(parent);

            }
            if (parent == null)
            {
                return null;
            }
            return parent as TreeViewIconsItem;
        }

        private void treeView1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) && (e.Key == Key.C))
            {
                CopyNodeText();
            }
        }
        /// <summary>
        /// 复制节点文本
        /// </summary>
        public void CopyNodeText()
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            if (treeView1.SelectedItem != null)
            {
                String str = treeView1.SelectedItem.HeaderText;

                try
                {
                    System.Windows.Forms.Clipboard.SetText(str);
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    MessageBox.Show("打开剪贴板失败,可能有其他程序正在使用。请稍候重试操作。");
                }


            }
        }

        #endregion

        #region "节点移动"
        /// <summary>
        /// 用于代表当前节点移动的类型
        /// </summary>
        // private NodeMoveType curNodeMove=NodeMoveType.NodeNotMove ;

        public void OnNodeMove(NodeMoveType moveType)
        {
            // curNodeMove = moveType;

            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            TreeViewIconsItem selectedNode = treeView1.SelectedItem;
            //还原默认节点图标
            ChangeNodeIcon(treeView1.SelectedItem, treeView1.SelectedItem.NodeData.DataItem.NormalIcon);
            switch (moveType)
            {
                case NodeMoveType.NodeMoveUp:
                    treeView1.MoveUp(selectedNode);

                    break;
                case NodeMoveType.NodeMoveDown:
                    treeView1.MoveDown(selectedNode);
                    break;
                case NodeMoveType.NodeMoveLeft:
                    treeView1.MoveLeft(selectedNode);
                    break;
                case NodeMoveType.NodeMoveRight:
                    treeView1.MoveRight(selectedNode);
                    break;
                case NodeMoveType.NodePaste:
                    //todo:paste!
                    break;
                default:
                    break;
            }
            SaveTreeToDB();
            //更新节点图标
            ChangeNodeIcon(treeView1.SelectedItem, treeView1.SelectedItem.NodeData.DataItem.SelectedIcon);

            //SaveTreeToFile();
        }

        public void MoveLeft(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveLeft);
        }

        public void MoveRight(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveRight);
        }

        public void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveUp);
        }

        public void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveDown);
        }

        #endregion
    }
}
