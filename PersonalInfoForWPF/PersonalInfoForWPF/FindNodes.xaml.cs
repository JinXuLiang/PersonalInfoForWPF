using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFSuperTreeView;

namespace PersonalInfoForWPF
{
    /// <summary>
    /// Interaction logic for FindNodes.xaml
    /// </summary>
    public partial class FindNodes : Window
    {
        private SuperTreeView _tree;
        private String EFConnectionString;

        public FindNodes()
        {
            InitializeComponent();
           
        }

        public void SetTree(SuperTreeView tree)
        {
            _tree = tree;
            //绑定显示数据
            dgNodes.ItemsSource = _tree.Nodes;
            //获取集合视图
            nodesCollectionView = CollectionViewSource.GetDefaultView(dgNodes.ItemsSource) as ListCollectionView;
            txtSearch.Focus();
            EFConnectionString = _tree.EFConnectionString;
        }

        /// <summary>
        /// 用于实现搜索的集合视图对象
        /// </summary>
        private ListCollectionView nodesCollectionView = null;

        public bool ShouldExit
        {
            get;
            set;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ShouldExit)
            {
                this.Hide();
                e.Cancel = true;
            }
           
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (dgNodes.SelectedItem != null)
            {
                //先保存原有的节点数据
                if (_tree.SelectedItem != null)
                {
                    _tree.SelectedItem.NodeData.DataItem.RefreshMe();
                    //当前节点如果是OnlyText时，其AccessObj==null，此时不应该更新底层存储
                    if (_tree.SelectedItem.NodeData.AccessObject != null)
                    {
                        try
                        {
                            _tree.SelectedItem.NodeData.AccessObject.UpdateDataInfoObject(_tree.SelectedItem.NodeData.DataItem);
                        }
                        catch (Exception ex)
                        {

                            Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.ToString()); }));
                        }
                       
                    }
                       

                }
                //显示新数据
                TreeViewIconsItem node = dgNodes.SelectedItem as TreeViewIconsItem;
                _tree.ShowNode(node.Path);
                //如果当前节点不可视，则滚动显示它！
                node.BringIntoView();
            }
        }

        private void SearchDB()
        {
            String FindWhat=txtSearch.Text.Trim();
            //如果搜索字串为空，则重置表格为所有的节点记录
            if(String.IsNullOrEmpty(FindWhat))
            {
                nodesCollectionView.Filter = null;
                return;
            }
            NodeDataSearchRepository repository = new NodeDataSearchRepository();
            List<string> result = repository.SearchDataNodeText(FindWhat,EFConnectionString);
            nodesCollectionView.Filter = (item) => result.IndexOf((item as TreeViewIconsItem).NodeData.DataItem.Path) != -1;

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (rdoTree.IsChecked.Value)
            {
                //搜索本节点路径记录
                nodesCollectionView.Filter = (item) => (item as TreeViewIconsItem).NodeData.DataItem.Path.IndexOf(txtSearch.Text) != -1;
            }
            else
            {
                //搜索数据库
                SearchDB();
            }
            txtSearch.SelectAll();
 
        }

        private void rdoTree_Checked(object sender, RoutedEventArgs e)
        {
            if(tbInfo!=null)
            tbInfo.Text = "查找节点路径：";
        }

        private void rdoDB_Checked(object sender, RoutedEventArgs e)
        {
            if (tbInfo != null)
            tbInfo.Text = "查找节点数据：";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rdoDB.IsChecked.Value)
            {
                return;
            }
            //搜索本节点路径记录
            nodesCollectionView.Filter = (item) => (item as TreeViewIconsItem).NodeData.DataItem.Path.IndexOf(txtSearch.Text) != -1;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            nodesCollectionView.MoveCurrentToFirst();
            if(dgNodes.SelectedItem!=null)
            //确保选择项可见
            dgNodes.ScrollIntoView(dgNodes.SelectedItem);
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (nodesCollectionView.IsCurrentBeforeFirst == false)
                nodesCollectionView.MoveCurrentToPrevious();
            else
                nodesCollectionView.MoveCurrentToFirst();

            if (dgNodes.SelectedItem != null)
            dgNodes.ScrollIntoView(dgNodes.SelectedItem);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (nodesCollectionView.IsCurrentAfterLast == false)
                nodesCollectionView.MoveCurrentToNext();
            else
                nodesCollectionView.MoveCurrentToLast();
            if (dgNodes.SelectedItem != null)
            dgNodes.ScrollIntoView(dgNodes.SelectedItem);
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            nodesCollectionView.MoveCurrentToLast();
            if (dgNodes.SelectedItem != null)
            dgNodes.ScrollIntoView(dgNodes.SelectedItem);
        }
       
    }
}
