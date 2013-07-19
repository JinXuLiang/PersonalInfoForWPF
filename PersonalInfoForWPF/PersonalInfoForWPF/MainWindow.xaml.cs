using DataAccessLayer;
using DataAccessLayer.MainTree;
using InterfaceLibrary;
using NodeFactoryLibrary;
using PersonalInfoForWPF.BackAndForward;
using publicLibrary.Serializer;
using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using WPFUserControlLibrary;

namespace PersonalInfoForWPF
{
  

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            Init();

        }

        /// <summary>
        /// 用于实现节点历史访问记录功能
        /// </summary>
        private VisitedNodesManager visitedNodesManager = null;

        /// <summary>
        /// 完成系统初始化功能
        /// </summary>
        private void Init()
        {
            //绑定显示数据源
            findNodesWindow = new FindNodes(treeView1);
            ConfigArgus argu = null;
            if (File.Exists(SystemConfig.ConfigFileName))
            {
                tbInfo.Text = "正在加载配置文件……";
                //定位到上次访问的节点
                try
                {
                    argu = DeepSerializer.BinaryDeserialize(SystemConfig.ConfigFileName) as ConfigArgus;
                }
                catch (Exception)
                {
                    argu = null;
                }

                if (argu != null)
                {
                    SystemConfig.configArgus = argu;
                    //设置树节点的默认字体大小
                    TreeViewIconsItem.TreeNodeDefaultFontSize = argu.TreeNodeDefaultFontSize;
                }

            }

            if (string.IsNullOrEmpty(SystemConfig.configArgus.DBFileName))
            {
                SystemConfig.configArgus.DBFileName = "infocenter.sdf";
            }
            //创建连接字符串
            DALConfig.ConnectString = DALConfig.getConnectionString(SystemConfig.configArgus.DBFileName);

            this.Title = "个人资料管理中心-" + SystemConfig.configArgus.DBFileName;
            this.Cursor = Cursors.AppStarting;
            //profiler发现，GetTreeFromDB()需要花费大量的时间，因此，将其移到独立的线程中去完成
            tbInfo.Text = "从数据库中装载数据……";
            Task tsk = new Task(() =>
            {
                String treeXML = MainTreeRepository.GetTreeFromDB();
                Action afterFetchTreeXML = () =>
                {
                    treeView1.LoadFromXmlString(treeXML);
                    if (argu != null)
                    {
                        treeView1.ShowNode(argu.LastVisitNodePath);
                    }

                    visitedNodesManager = new VisitedNodesManager(treeView1);

                    MenuItem mnuChangeTextColor = treeView1.ContextMenu.Items[treeView1.ContextMenu.Items.Count - 1] as MenuItem;

                    ColorBrushList brushList = new ColorBrushList(mnuChangeTextColor);
                    brushList.BrushChanged += brushList_BrushChanged;
                    tbInfo.Text = "就绪。";
                    Cursor = null;
                    
                };
                Dispatcher.BeginInvoke(afterFetchTreeXML);
            });

            tsk.Start();

        }

        void brushList_BrushChanged(object sender, BrushChangeEventArgs e)
        {
            if (treeView1.SelectedItem != null)
            {
                treeView1.SelectedItem.MyForeground = e.selectedBrush;
                SaveTreeToDB();
            }
        }

        private FindNodes findNodesWindow = null;

        /// <summary>
        /// 剪切的节点
        /// </summary>
        private TreeViewIconsItem cutNode = null;
        /// <summary>
        /// 保存上次粘贴的文本
        /// </summary>
        private String LastPasteNodeText = "";

        #region "用户点击树中的不同节点"

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
            if (visitedNodesManager != null && e.OldValue!=null)
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
                NodeUIContainer.Content = dataInfoObject.DataItem.RootControl;
            }
            //显示最新的数据
            dataInfoObject.DataItem.BindToRootControl();
            dataInfoObject.DataItem.RefreshDisplay();

        }

        #endregion

        #region "添加节点"

        /// <summary>
        /// 依据要添加节点的种类和节点文本，生成其路径，可用于检测是否己存在同名节点
        /// </summary>
        /// <param name="category"></param>
        /// <param name="newNodeText"></param>
        /// <returns></returns>
        private String getNewNodePath(AddNodeCategory category, String newNodeText)
        {
            String selectNodePath = (treeView1.SelectedItem as TreeViewIconsItem) == null ? "" : (treeView1.SelectedItem as TreeViewIconsItem).Path;
            String newNodePath = "";
            switch (category)
            {
                case AddNodeCategory.AddRoot:
                    newNodePath = "/" + newNodeText + "/";
                    break;
                case AddNodeCategory.AddChild:
                    if (selectNodePath != "")
                    {
                        newNodePath = selectNodePath + newNodeText + "/";
                    }

                    break;
                case AddNodeCategory.AddSibling:
                    if (selectNodePath != "")
                    {
                        String selectNodeText = (treeView1.SelectedItem as TreeViewIconsItem).HeaderText;
                        newNodePath = selectNodePath.Replace("/" + selectNodeText + "/", "/" + newNodeText + "/");
                    }

                    break;
            }
            return newNodePath;
        }
        /// <summary>
        /// 依据节点类型，生成节点默认的文本（不包括后面的数字）
        /// </summary>
        /// <param name="NodeType"></param>
        /// <returns></returns>
        private String getDefaultNodeText(String NodeType)
        {
            String defaultNodeText = "";
            switch (NodeType)
            {
                case "DetailText":
                    defaultNodeText = "新详细信息";
                    break;
                case "OnlyText":
                    defaultNodeText = "新纯文本";
                    break;
                case "Folder":
                    defaultNodeText = "新文件夹";
                    break;
            }
            return defaultNodeText;
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="category">指要新建儿子、兄弟还是根节点</param>
        /// <param name="nodeType">指节点的类型是：DetailText\OnlyText等 </param>
        private void AddNode(AddNodeCategory category, String nodeType)
        {
            //为新节点生成默认文本
            String NodeText = getDefaultNodeText(nodeType) + (treeView1.NodeCount + 1);
            //尝试从剪贴板中提取文本
            String textFromClipboard = StringUtils.getFirstLineOfString(Clipboard.GetText());
            if (String.IsNullOrEmpty(textFromClipboard) == false && textFromClipboard != LastPasteNodeText && textFromClipboard.IndexOf("/") == -1)
            {
                //检测一下从剪贴板中获取的文本是否有效（即不会导致重名的节点出现）
                String newNodeText = textFromClipboard;
                bool nodeExisted = treeView1.IsNodeExisted(getNewNodePath(category, newNodeText));
                //如果不存在同名的路径
                if (nodeExisted == false)
                {
                    NodeText = newNodeText;
                    LastPasteNodeText = NodeText;
                }
            }
            //如果还有重复路径的，则循环使用随机数，务必保证路径不会相同
            while (treeView1.IsNodeExisted(getNewNodePath(category, NodeText)))
            {
                NodeText = getDefaultNodeText(nodeType) + new Random().Next();
            }

            //创建默认的节点数据对象
            NodeDataObject dataobject = NodeFactory.CreateDataInfoNode(nodeType);

            TreeViewIconsItem newNode = null;
            //在树中添加节点
            switch (category)
            {
                case AddNodeCategory.AddRoot:
                    newNode = treeView1.AddRoot(NodeText, dataobject);
                    break;
                case AddNodeCategory.AddChild:
                    newNode = treeView1.AddChild(NodeText, dataobject);
                    break;
                case AddNodeCategory.AddSibling:
                    newNode = treeView1.AddSibling(NodeText, dataobject);
                    break;
                default:
                    break;
            }
            if (newNode == null)
            {
                return;
            }
            //新节点，默认不是粗体的
            newNode.FontWeight = FontWeights.Normal;
            //在数据库中创建记录
            if (dataobject.AccessObject != null)
            {
                dataobject.AccessObject.Create(dataobject.DataItem);
            }
            //保存树结构
            SaveTreeToDB();


            //自动进入编辑状态

            newNode.BeginEdit();


        }

        private void AddChild_OnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddChild, "OnlyText");
        }

        private void AddChild_DetailText(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddChild, "DetailText");
        }

        private void AddSibling_OnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddSibling, "OnlyText");
        }

        private void AddSibling_DetailText(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddSibling, "DetailText");
        }

        private void AddRoot_OnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddRoot, "OnlyText");
        }

        private void AddRoot_DetailText(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddRoot, "DetailText");
        }

        #endregion

        private void DeleteNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            treeView1.DeleteNode(treeView1.SelectedItem as TreeViewIconsItem);
            //保存树结构
            SaveTreeToDB();
            
        }

        #region "节点移动"

        private void OnNodeMove(NodeMoveType moveType)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            TreeViewIconsItem selectedNode = treeView1.SelectedItem as TreeViewIconsItem;
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
            //SaveTreeToFile();
        }

        private void MoveLeft(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveLeft);
        }

        private void MoveRight(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveRight);
        }

        private void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveUp);
        }

        private void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            OnNodeMove(NodeMoveType.NodeMoveDown);
        }

        #endregion

        private void RenameNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            TreeViewIconsItem selectedNode = treeView1.SelectedItem as TreeViewIconsItem;
            if (selectedNode != null)
            {
                Console.WriteLine(FocusManager.GetFocusedElement(selectedNode));

                selectedNode.BeginEdit();
            }

        }

        private void CutNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            cutNode = treeView1.SelectedItem as TreeViewIconsItem;
            treeView1.CutNode(cutNode);
        }

        private void PasteNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            if (cutNode == null)
            {
                return;
            }
            TreeViewIconsItem selectedNode = treeView1.SelectedItem as TreeViewIconsItem;
            String newPath = selectedNode.Path + cutNode.HeaderText + "/";
            if (treeView1.IsNodeExisted(newPath))
            {
                MessageBox.Show("在此处粘贴将导致两个节点拥有相同的路径，因此，请在其他地方粘贴");
                return;
            }
            treeView1.PasteNode(cutNode, selectedNode);
            OnNodeMove(NodeMoveType.NodePaste);
            cutNode = null;
            treeView1.SelectedItem.IsExpanded = true;
        }

        private void CopyNodeTextExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CopyNodeText();
        }
        /// <summary>
        /// 复制节点文本
        /// </summary>
        private void CopyNodeText()
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
                catch (System.Runtime.InteropServices.ExternalException ex)
                {
                    MessageBox.Show("打开剪贴板失败,可能有其他程序正在使用。请稍候重试操作。");
                }
             

            }
        }

        private void ExpandAllNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            if (treeView1.SelectedItem != null)
            {
                treeView1.SelectedItem.ExpandSubtree();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cutNode != null)
            {
                e.Cancel = true;
                MessageBox.Show("有未粘贴的节点。请先粘贴节点后再退出");
                return;
            }
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();


            }
            findNodesWindow.ShouldExit = true;
            findNodesWindow.Close();

            if (treeView1.SelectedItem != null)
            {
                SystemConfig.configArgus.LastVisitNodePath = treeView1.SelectedItem.Path;
                IDataAccess accessobj=treeView1.SelectedItem.NodeData.AccessObject;
                IDataInfo infoObj = treeView1.SelectedItem.NodeData.DataItem;
                if (accessobj != null)
                {
                    try
                    {
                        accessobj.UpdateDataInfoObject(infoObj);
                    }
                    catch (Exception ex)
                    {

                        Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.ToString()); }));
                    }
                   
                }
            }

            DeepSerializer.BinarySerialize(SystemConfig.configArgus, SystemConfig.ConfigFileName);
        }

        private void ShowFindNodesWindow(object sender, ExecutedRoutedEventArgs e)
        {
            findNodesWindow.Show();
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


        private void treeView1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) && (e.Key == Key.C))
            {
                CopyNodeText();
            }
        }

        private void ShowConfigWin(object sender, ExecutedRoutedEventArgs e)
        {
            ConfigWin win = new ConfigWin();
            bool? result = win.ShowDialog();
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
                SystemConfig.configArgus.LastVisitNodePath = treeView1.SelectedItem.Path;
            }
            if (result.Value)
            {
                DeepSerializer.BinarySerialize(SystemConfig.configArgus, SystemConfig.ConfigFileName);
                //清除树的所有节点
                treeView1.ClearAllNodes();
                //重新初始化
                Init();
            }
        }


        private void ExitApplication(object sender, ExecutedRoutedEventArgs e)
        {
            findNodesWindow.ShouldExit = true;
            Close();
        }
      
        private void AddChild_Folder(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddChild, "Folder");
        }

        private void AddSibling_Folder(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddSibling, "Folder");
        }

        private void AddRoot_Folder(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.IsInEditMode)
            {
                treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddRoot, "Folder");
        }

        private void GoBack(object sender, ExecutedRoutedEventArgs e)
        {

            visitedNodesManager.GoBack();

        }

       

        private void GoForward(object sender, ExecutedRoutedEventArgs e)
        {

            visitedNodesManager.GoForward();

        }

       

        private void ToggleNodeTextBold(object sender, ExecutedRoutedEventArgs e)
        {
            if (treeView1.SelectedItem != null)
            {
                if (treeView1.SelectedItem.FontWeight != FontWeights.ExtraBold)
                    treeView1.SelectedItem.FontWeight = FontWeights.ExtraBold;
                else
                    treeView1.SelectedItem.FontWeight = FontWeights.Normal;
                SaveTreeToDB();
            }
        }


        #region "数据库相关的操作"

        /// <summary>
        /// 更换数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeDB(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            openFileDialog.RestoreDirectory = true;

            openFileDialog.Filter = "sqlserver ce 4.0数据库（*.sdf）|*.sdf";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show("切换数据库后，必须重新启动程序");
                SystemConfig.configArgus.DBFileName = openFileDialog.FileName;
                findNodesWindow.ShouldExit = true;
                Close();
                Process.Start(Assembly.GetEntryAssembly().Location);
            }
        }
        private void CopyDB(object sender, ExecutedRoutedEventArgs e)
        {
            String DBTemplateFileName = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\infocenter.sdf";
            if (File.Exists(DBTemplateFileName) == false)
            {
                MessageBox.Show("数据库模板文件：infocenter.sdf未找到");
                return;
            }

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            saveFileDialog.RestoreDirectory = true;

            saveFileDialog.Title = "创建新资料库";
            DateTime now = DateTime.Now;
            String fileDateString = now.Year + "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute;
            saveFileDialog.FileName = "infoCenter_" + fileDateString + ".sdf";
            saveFileDialog.Filter = "sqlserver ce 4.0数据库（*.sdf）|*.sdf";
            saveFileDialog.DefaultExt = "sdf";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (SystemConfig.configArgus.DBFileName == saveFileDialog.FileName)
                {
                    MessageBox.Show("不能选择正在使用的资料库文件名");
                    return;
                }
                File.Copy(DBTemplateFileName, saveFileDialog.FileName, true);
                MessageBox.Show("切换数据库后，必须重新启动程序");
                SystemConfig.configArgus.DBFileName = saveFileDialog.FileName;
                findNodesWindow.ShouldExit = true;
                Close();
                Process.Start(Assembly.GetEntryAssembly().Location);

            }
        }
        #endregion
       




    }
}
