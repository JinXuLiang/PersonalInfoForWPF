using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

using System.ComponentModel;
using PublicLibrary.Utils;
using System.Windows.Input;
using InterfaceLibrary;
using NodeFactoryLibrary;
using DataAccessLayer.MainTree;
using System.IO;
using System.Threading;

namespace WPFSuperTreeView
{
    /// <summary>
    /// 一个支持图标的TreeViewItem
    /// 它实现了IComarable接口，因此可以相互比较和判等
    /// </summary>
    public class TreeViewIconsItem : TreeViewItem, IComparable<TreeViewIconsItem>, INotifyPropertyChanged
    {
        /// <summary>
        /// 默认字体大小
        /// </summary>
        public static double TreeNodeDefaultFontSize = 15;

        private double _fontSize = 15;
        /// <summary>
        /// 设置本节点的字体大小
        /// </summary>
        public double MyNodeFontSize
        {
            get { return _fontSize; }
            set
            {
              
                _fontSize = value;
                textBlock.FontSize = value;
                edtText.FontSize = value;
            }
        }
        private bool _Strikethough = false;
        /// <summary>
        /// 给节点文本加上或移除删除线
        /// </summary>
        public bool Strikethrough
        {
            get
            {
                return _Strikethough;
            }
            set
            {
                _Strikethough = value;
                if (_Strikethough == true)
                {
                    textBlock.TextDecorations = TextDecorations.Strikethrough;
                    
                }
                else
                {
                    textBlock.TextDecorations = null;
                }
            }
        }
        /// <summary>
        /// 保存本节点的前景色，以便序列化到XML中
        /// 之所以设置其属性，是因为如果直接使用ForeGround，则当此节点处于选中状态时，Foreground总为白色
        /// </summary>
        private Brush _myForeground = Brushes.Black;
        public Brush MyForeground
        {
            get
            {
                return _myForeground;
            }
            set
            {
                _myForeground = value;
                Foreground = value;
            }
        }

        private SuperTreeView belongToTreeView = null;

        /// <summary>
        /// 本对象的ID，其值为创建时的时间值
        /// </summary>
        private long id;

        private String _path = "";
        /// <summary>
        /// 本节点的路径，其格式为：/root/child/.../child/，注意最后有一个“/”
        /// 另外，在各种影响路径的节点操作中，一定要注意及时更新此节点路径
        /// </summary>
        public String Path
        {
            get
            {
                
                return _path;
            }
            set
            {
                _path = value;
                //同步更新数据对象的路径
                if (NodeData.DataItem != null)
                {
                    NodeData.DataItem.Path = value;
                }
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Path"));
            }
        }

        private NodeDataObject _dataObject = null;
        /// <summary>
        /// 代表本节点所关联的数据节点
        /// </summary>
        public NodeDataObject NodeData
        {
            get
            {
                return _dataObject;
            }
            set
            {
                _dataObject = value;
            }
        }

        private StackPanel headerContainer = null;
        /// <summary>
        /// 用于显示节点文本的控件
        /// </summary>
        private TextBlock textBlock;
        /// <summary>
        /// 用于显示图标的控件
        /// </summary>
        private Image _iconImageControl;

        /// <summary>
        /// 用于编辑的文本框
        /// </summary>
        private TextBox edtText;
        /// <summary>
        /// 是否EndEdit()方法己被调用过？
        /// 之所以设定此标记，是因为LostFocus和KeyDown都会调用EndEdit(),用此标记就可以保证相应方法只调用一次。
        /// </summary>
        private bool EndEditHasBeenHandled = false;

        public TreeViewIconsItem(SuperTreeView tree, NodeDataObject dataObject)
        {
            belongToTreeView = tree;
            _dataObject = dataObject;

            //以下处理UI控件
            headerContainer = new StackPanel();


            //  设置StackPanel中的内容水平排列
            headerContainer.Orientation = Orientation.Horizontal;
            //将Header设置为StackPanel
            Header = headerContainer;
            //设置正常状态下的显示控件
            _iconImageControl = new Image();
            _iconImageControl.VerticalAlignment = VerticalAlignment.Center;
            _iconImageControl.Margin = new Thickness(0, 0, 4, 0);
            _iconImageControl.Source = _icon;
            //  向StackPanel对象中添加一个图标对象
            headerContainer.Children.Add(_iconImageControl);

            //  创建用于添加文本信息的TextBlock对象
            textBlock = new TextBlock();
            textBlock.FontSize = TreeNodeDefaultFontSize;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            //  向StackPanel对象中添加文本信息
            headerContainer.Children.Add(textBlock);

            //实例化文本框控件，准备用于编辑
            edtText = new TextBox();
            edtText.FontSize = TreeNodeDefaultFontSize;
            edtText.MinWidth = 100;
            //对事件进行响应
            edtText.KeyDown += edtText_KeyDown;
            edtText.LostFocus += edtText_LostFocus;
            //由于默认情况下WPF的TextBox的Cut存在BUG：即完成复制工作但却不删除选中的文字，因此不得不用自己的代码取代默认的命令响应代码
            CommandBinding bind = new CommandBinding();
            bind.Command = ApplicationCommands.Cut;
            bind.Executed += bind_Executed;
            bind.CanExecute += bind_CanExecute;
            edtText.CommandBindings.Add(bind);
            //使用当前时间作为本Item的ID
            id = TimerUtils.getHighPrecisionCurrentTime();
        }

        void bind_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(edtText.SelectedText))
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        void bind_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Copy似乎没有BUG
            edtText.Copy();
            //将当前选中的文本清空
            edtText.SelectedText = "";
        }

        void edtText_LostFocus(object sender, RoutedEventArgs e)
        {
           
            EndEdit();
        }


        void edtText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter )
            {
                EndEdit();
            }
        }

        private ImageSource _icon;
        /// <summary>
        /// 用于设置或获得节点中的图标对象
        /// </summary>
        public ImageSource Icon
        {
            set
            {
                _icon = value;
                _iconImageControl.Source = _icon;
                _iconImageControl.Width = 16;
                _iconImageControl.Height = 16;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Icon"));
            }
            get
            {
                return _icon;
            }
        }

        /// <summary>
        /// 设置或获得节点中的文本信息
        /// </summary>
        public string HeaderText
        {
            set
            {
                textBlock.Text = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("HeaderText"));
            }
            get
            {
                return textBlock.Text;
            }
        }

        /// <summary>
        /// 按id进行大小比较
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(TreeViewIconsItem other)
        {

            return id.CompareTo(other.id);
        }

        


        /// <summary>
        /// 此事件用于通知数据绑定控件可以刷新显示
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 进入编辑状态
        /// </summary>
        public void BeginEdit()
        {
            belongToTreeView.IsInEditMode = true;
            headerContainer.Children.RemoveAt(1);

            //将原节点文本复制到文本框中，并全选
            edtText.Text = textBlock.Text;
            edtText.SelectAll();
            headerContainer.Children.Add(edtText);
            //关键：以下这些代码说明：只有当一个控件可视时，才能Focus
            //因此，使用了以下Hack手段等其变为可视的 
            if (!edtText.IsVisible)
            {
                DependencyPropertyChangedEventHandler deferredFocus = null;
                deferredFocus = delegate
                {
                    edtText.Focus();
                    edtText.IsVisibleChanged -= deferredFocus;
                };
                edtText.IsVisibleChanged += deferredFocus;
            }
            else
            {
                edtText.Focus();
            }
            EndEditHasBeenHandled = false;


        }
        /// <summary>
        /// 结束编辑并自动更新内存和数据库中的相关记录
        /// </summary>
        public void EndEdit()
        {
            if (EndEditHasBeenHandled == true)
                return;
            String OldValue = textBlock.Text;
            String NewValue = edtText.Text;
            String oldPath = Path;

            //如果用户没有输入
            if (String.IsNullOrEmpty(NewValue))
            {
               
                headerContainer.Children.RemoveAt(1);
                headerContainer.Children.Add(textBlock);
                belongToTreeView.IsInEditMode = false;
                EndEditHasBeenHandled = true;
                return;
            }
            //如果用户没有输入
            if (NewValue.IndexOf("/")!=-1)
            {
                MessageBox.Show("由于“/”被作为树节点路径的分隔符，因此节点文本中不能使用“/”");
                headerContainer.Children.RemoveAt(1);
                headerContainer.Children.Add(textBlock);
                belongToTreeView.IsInEditMode = false;
                EndEditHasBeenHandled = true;
                GetTreeViewScrollerAndScrollToLeftEnd();
                return;
            }
            //如果用户没有修改，则还原
            if (OldValue == NewValue)
            {
                headerContainer.Children.RemoveAt(1);
                headerContainer.Children.Add(textBlock);
                belongToTreeView.IsInEditMode = false;
                EndEditHasBeenHandled = true;
                GetTreeViewScrollerAndScrollToLeftEnd();
                return;
            }
            
            //生成新路径
            String newPath = Path.Replace("/" + OldValue + "/", "/" + NewValue + "/");
            if (belongToTreeView.IsNodeExisted(newPath))
            {
                MessageBox.Show("己经存在相同的路径的节点！");
                headerContainer.Children.RemoveAt(1);
                headerContainer.Children.Add(textBlock);
                belongToTreeView.IsInEditMode = false;
                EndEditHasBeenHandled = true;
                GetTreeViewScrollerAndScrollToLeftEnd();
                return;
            }

            
            textBlock.Text = NewValue;
            headerContainer.Children.RemoveAt(1);
            headerContainer.Children.Add(textBlock);

            GetTreeViewScrollerAndScrollToLeftEnd();

            belongToTreeView.IsInEditMode = false;
            EndEditHasBeenHandled = true;
            //更新本节点的路径
            Path = newPath;
            NodeData.DataItem.Path = newPath;

            //更新所有相关节点路径
            belongToTreeView.nodesManager.UpdateNodePath(oldPath, newPath);

            //更新数据库中相关子节点的路径
            if (belongToTreeView.TreeNodePathManager!= null)
            {
                belongToTreeView.TreeNodePathManager.UpdateNodePath(oldPath, newPath);
            }
            

            //更新数据库中树
            belongToTreeView.SaveToDB();
            //String treeXml = belongToTreeView.saveToXmlString();

            //(new MainTreeRepository()).SaveTree(treeXml);
            ////在独立的线程中完成数据更新任务
            //Thread thread = new Thread(() =>
            //{
            //    (new MainTreeRepository()).SaveTree(treeXml);
            //});
            //thread.Start();
            
        }
        /// <summary>
        /// 依据控件模板的层次关系，获取本节点所属的TreeView的ScrollViewer
        /// 然后，将其滚动到最左部，这样，当结束编辑时，会自动地显示本节点的开头
        /// </summary>
        private void GetTreeViewScrollerAndScrollToLeftEnd()
        {
            int count = VisualTreeHelper.GetChildrenCount(belongToTreeView);
            DependencyObject RootBorder = VisualTreeHelper.GetChild(belongToTreeView, 0);
            DependencyObject contentProvider = VisualTreeHelper.GetChild(RootBorder, 0);
            DependencyObject treeView = VisualTreeHelper.GetChild(contentProvider, 0);
            DependencyObject Border = VisualTreeHelper.GetChild(treeView, 0);

            ScrollViewer scrollViewer = VisualTreeHelper.GetChild(Border, 0) as ScrollViewer;
            scrollViewer.ScrollToLeftEnd();
        }
    }
}
