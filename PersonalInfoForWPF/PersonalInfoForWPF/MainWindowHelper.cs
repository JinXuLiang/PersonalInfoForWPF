using DataAccessLayer.MainTree;
using InterfaceLibrary;
using NodeFactoryLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using WPFSuperTreeView;

namespace PersonalInfoForWPF
{
    /// <summary>
    /// 用于放置一些从MainWindows中移出的代码，以减少主窗体的代码量
    /// </summary>
    public partial class MainWindow
    {
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

        /// <summary>
        /// 将树结构保存到数据库中
        /// </summary>
        public void SaveTreeToDB()
        {
            String treeXml = treeView1.saveToXmlString();
            MainTreeRepository.SaveTree(treeXml);

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


        #region "更换节点类型"


        /// <summary>
        /// 从纯文本型转换为详细信息型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToDetailText(object sender, ExecutedRoutedEventArgs e)
        {

            ChangeNodeType("DetailText");


        }

        private void ChangeNodeType(String ToType)
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

            NodeDataObject detailTextNodeDataObject = NodeTypeChanger.GetNodeChanger(FromType, ToType).ChangeTo(nodeDataObject);

            if (detailTextNodeDataObject != null)
            {
                treeView1.SelectedItem.NodeData = detailTextNodeDataObject;
                ChangeNodeIcon(treeView1.SelectedItem, detailTextNodeDataObject.DataItem.SelectedIcon);
                LoadDataAndShowInUI(treeView1.SelectedItem);
                SaveTreeToDB();
            }
        }

        private void ToOnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeNodeType("OnlyText");
        }
        private void ToFolder(object sender, ExecutedRoutedEventArgs e)
        {
            ChangeNodeType("Folder");
        }

        #endregion

    }
}
