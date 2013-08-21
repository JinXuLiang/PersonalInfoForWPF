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
        /// 表示当前激活的数据库选项卡
        /// </summary>
        private DBInfoTab curDbInfoTab = null;
        /// <summary>
        /// 用于查找的窗口
        /// </summary>
        private FindNodes findNodesWindow = null;

      
        /// <summary>
        /// 完成系统初始化功能
        /// </summary>
        private void Init()
        {
            ConfigArgus argu = new ConfigArgus();
            //如果找到了配置文件
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
                findNodesWindow = new FindNodes();
                //用于保存有效的数据库文件对象
                List<DatabaseInfo> validDBInfos = new List<DatabaseInfo>();
                //创建数据库选项卡
                foreach (var dbinfo in argu.DbInfos)
                {
                    if (File.Exists(dbinfo.DatabaseFilePath))
                    {
                        DBInfoTab tab = new DBInfoTab(dbinfo);
                        DBtabContainer.Add(System.IO.Path.GetFileName(dbinfo.DatabaseFilePath), tab);
                        validDBInfos.Add(dbinfo);
                    }

                }
                bool allDBAreOk = (argu.DbInfos.Count == validDBInfos.Count);
                if (!allDBAreOk)
                {
                    argu.DbInfos = validDBInfos;
                }

                if (DBtabContainer.Items.Count != 0)
                {
                    //设置当前激活的卡片
                   
                    if (argu.ActiveDBIndex < DBtabContainer.Items.Count && allDBAreOk)
                    {
                        DBtabContainer.SelectedIndex = argu.ActiveDBIndex;
                    }
                    else
                    {
                        argu.ActiveDBIndex = 0;
                        DBtabContainer.SelectedIndex = 0;
                    }
                    curDbInfoTab = (DBtabContainer.Items[argu.ActiveDBIndex] as TabItem).Content as DBInfoTab;
                    LoadCurrentTabDataFromDB();
                }
                else
                {
                    tbInfo.Text = "未能找到上次打开的数据库，请从系统功能菜单中选择打开命令打开一个资料库";
                }
            }
            else
            {
                tbInfo.Text = "请从系统功能菜单中选择打开命令打开一个资料库";
            }

            //响应用户点击不同选项卡的操作
            DBtabContainer.SelectionChanged += DBtabContainer_SelectionChanged;
            //关闭选项卡时，激发此事件
            DBtabContainer.TabPageClosed += DBtabContainer_TabPageClosed;
        }

        void DBtabContainer_TabPageClosed(object sender, TabPageClosedEventArgs e)
        {
            //保存被关闭的选项卡的数据
            DBInfoTab closedTab = e.ClosedTabItem.Content as DBInfoTab;
            //从参数中移除本选项卡所对应的DbInfo对象
            int index = SystemConfig.configArgus.DbInfos.IndexOf(closedTab.dbInfoObject);
            if (index != -1)
            {
                SystemConfig.configArgus.DbInfos.RemoveAt(index);
            }
            SaveDbTabDataToDB(closedTab);

        }
        /// <summary>
        /// 将指定选项卡的数据保存到数据库中
        /// </summary>
        /// <param name="infoTab"></param>
        private void SaveDbTabDataToDB(DBInfoTab infoTab)
        {
            TreeViewIconsItem selectedItem = infoTab.treeView1.SelectedItem;
            DatabaseInfo info = infoTab.dbInfoObject;
            if (selectedItem != null)
            {
                info.LastVisitNodePath = selectedItem.Path;
                IDataAccess accessobj = selectedItem.NodeData.AccessObject;
                IDataInfo infoObj = selectedItem.NodeData.DataItem;
                if (accessobj != null)
                {
                    try
                    {
                        infoObj.RefreshMe();
                        accessobj.UpdateDataInfoObject(infoObj);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() => { MessageBox.Show(ex.ToString()); }));
                    }
                }
            }
        }
        /// <summary>
        /// 从数据库中为当前激活的选项卡装入信息
        /// </summary>
        /// <param name="argu"></param>
        private void LoadCurrentTabDataFromDB()
        {

            //绑定显示数据源
            if (findNodesWindow == null)
            {
                findNodesWindow = new FindNodes();
            }
            findNodesWindow.SetTree(curDbInfoTab.treeView1);
            //创建连接字符串
            String EFConnectString = DALConfig.getEFConnectionString(curDbInfoTab.dbInfoObject.DatabaseFilePath);

            this.Title = "个人资料管理中心-" + curDbInfoTab.dbInfoObject.DatabaseFilePath;
            this.Cursor = Cursors.AppStarting;
            //profiler发现，GetTreeFromDB()需要花费大量的时间，因此，将其移到独立的线程中去完成
            tbInfo.Text = "从数据库中装载数据……";
            Task tsk = new Task(() =>
            {
                curDbInfoTab.treeView1.EFConnectionString = EFConnectString;
                String treeXML = curDbInfoTab.treeView1.LoadTreeXMLFromDB();
                Action afterFetchTreeXML = () =>
                {
                    curDbInfoTab.treeView1.LoadFromXmlString(treeXML);
                    curDbInfoTab.treeView1.ShowNode(curDbInfoTab.dbInfoObject.LastVisitNodePath);
                    curDbInfoTab.visitedNodesManager = new VisitedNodesManager(curDbInfoTab.treeView1);

                    MenuItem mnuChangeTextColor = curDbInfoTab.treeView1.ContextMenu.Items[curDbInfoTab.treeView1.ContextMenu.Items.Count - 1] as MenuItem;

                    ColorBrushList brushList = new ColorBrushList(mnuChangeTextColor);
                    brushList.BrushChanged += brushList_BrushChanged;
                    tbInfo.Text = "就绪。";
                    Cursor = null;
                    //设置己从数据库中装入标记
                    curDbInfoTab.HasBeenLoadedFromDB = true;

                };
                Dispatcher.BeginInvoke(afterFetchTreeXML);
            });

            tsk.Start();
        }



        void brushList_BrushChanged(object sender, BrushChangeEventArgs e)
        {
            if (curDbInfoTab.treeView1.SelectedItem != null)
            {
                curDbInfoTab.treeView1.SelectedItem.MyForeground = e.selectedBrush;
                curDbInfoTab.SaveTreeToDB();
            }
        }



        /// <summary>
        /// 剪切的节点
        /// </summary>
        private TreeViewIconsItem cutNode = null;
        /// <summary>
        /// 保存上次粘贴的文本
        /// </summary>
        private String LastPasteNodeText = "";



        #region "添加节点"

        /// <summary>
        /// 依据要添加节点的种类和节点文本，生成其路径，可用于检测是否己存在同名节点
        /// </summary>
        /// <param name="category"></param>
        /// <param name="newNodeText"></param>
        /// <returns></returns>
        private String getNewNodePath(AddNodeCategory category, String newNodeText)
        {
            String selectNodePath = (curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem) == null ? "" : (curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem).Path;
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
                        String selectNodeText = (curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem).HeaderText;
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
            String NodeText = getDefaultNodeText(nodeType) + (curDbInfoTab.treeView1.NodeCount + 1);
            //尝试从剪贴板中提取文本
            String textFromClipboard = StringUtils.getFirstLineOfString(Clipboard.GetText());
            if (String.IsNullOrEmpty(textFromClipboard) == false && textFromClipboard != LastPasteNodeText && textFromClipboard.IndexOf("/") == -1)
            {
                //检测一下从剪贴板中获取的文本是否有效（即不会导致重名的节点出现）
                String newNodeText = textFromClipboard;
                bool nodeExisted = curDbInfoTab.treeView1.IsNodeExisted(getNewNodePath(category, newNodeText));
                //如果不存在同名的路径
                if (nodeExisted == false)
                {
                    NodeText = newNodeText;
                    LastPasteNodeText = NodeText;
                }
            }
            //如果还有重复路径的，则循环使用随机数，务必保证路径不会相同
            while (curDbInfoTab.treeView1.IsNodeExisted(getNewNodePath(category, NodeText)))
            {
                NodeText = getDefaultNodeText(nodeType) + new Random().Next();
            }

            //创建默认的节点数据对象
            NodeDataObject dataobject = NodeFactory.CreateDataInfoNode(nodeType, DALConfig.getEFConnectionString(curDbInfoTab.dbInfoObject.DatabaseFilePath));

            TreeViewIconsItem newNode = null;
            //在树中添加节点
            switch (category)
            {
                case AddNodeCategory.AddRoot:
                    newNode = curDbInfoTab.treeView1.AddRoot(NodeText, dataobject);
                    break;
                case AddNodeCategory.AddChild:
                    newNode = curDbInfoTab.treeView1.AddChild(NodeText, dataobject);
                    break;
                case AddNodeCategory.AddSibling:
                    newNode = curDbInfoTab.treeView1.AddSibling(NodeText, dataobject);
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
            curDbInfoTab.SaveTreeToDB();


            //自动进入编辑状态

            newNode.BeginEdit();


        }

        private void AddChild_OnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddChild, "OnlyText");
        }

        private void AddChild_DetailText(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddChild, "DetailText");
        }

        private void AddSibling_OnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddSibling, "OnlyText");
        }

        private void AddSibling_DetailText(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddSibling, "DetailText");
        }

        private void AddRoot_OnlyText(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddRoot, "OnlyText");
        }

        private void AddRoot_DetailText(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddRoot, "DetailText");
        }

        #endregion

        private void DeleteNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            curDbInfoTab.treeView1.DeleteNode(curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem);
            //保存树结构
            curDbInfoTab.SaveTreeToDB();

        }



        private void RenameNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            TreeViewIconsItem selectedNode = curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem;
            if (selectedNode != null)
            {
               selectedNode.BeginEdit();
            }

        }
        /// <summary>
        /// 当剪切节点时，记录下被剪切节点所在的数据库选项卡
        /// </summary>
        private DBInfoTab cutNodeSourceTab = null;
        private void CutNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            cutNode = curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem;
            curDbInfoTab.treeView1.CutNode(cutNode);
            cutNodeSourceTab = curDbInfoTab;
        }

        private void PasteNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            if (cutNode == null)
            {
                return;
            }
            TreeViewIconsItem selectedNode = curDbInfoTab.treeView1.SelectedItem as TreeViewIconsItem;
            String newPath = selectedNode.Path + cutNode.HeaderText + "/";
            if (curDbInfoTab.treeView1.IsNodeExisted(newPath))
            {
                MessageBox.Show("在此处粘贴将导致两个节点拥有相同的路径，因此，请在其他地方粘贴");
                return;
            }
            //在同一数据库中粘贴
            if (curDbInfoTab == cutNodeSourceTab)
            {
                curDbInfoTab.treeView1.PasteNode(cutNode, selectedNode);
                curDbInfoTab.OnNodeMove(NodeMoveType.NodePaste);
            }
            else
            { 
                //在不同的数据库中粘贴
                String sourcePath=cutNode.Path;
                String targetPath=selectedNode.Path;
                //将源树保存到数据库中
                cutNodeSourceTab.treeView1.SaveToDB();
                //将剪切的节点子树追加到当前节点
                curDbInfoTab.treeView1.PasteNodeCrossDB(cutNode, selectedNode);
                //保存目标树结构
                curDbInfoTab.treeView1.SaveToDB();
                //更新所有粘贴节点的数据存取对象
                String EFConnectionString = DALConfig.getEFConnectionString(curDbInfoTab.dbInfoObject.DatabaseFilePath);
                UpdateDataAcessObject(selectedNode, EFConnectionString);
               //更新数据库中内容
                NodeMoveBetweenDBManager nodeMoveManager = new NodeMoveBetweenDBManager(cutNodeSourceTab.dbInfoObject.DatabaseFilePath, curDbInfoTab.dbInfoObject.DatabaseFilePath);
                nodeMoveManager.MoveNodeBetweenDB(sourcePath,targetPath);
                


            }
            cutNode = null;
            curDbInfoTab.treeView1.SelectedItem.IsExpanded = true;


        }
        private void UpdateDataAcessObject(TreeViewIconsItem root,String EFConnectionString)
        {
            if (root == null)
            {
                return;
            }
            root.NodeData.AccessObject = NodeFactory.CreateNodeAccessObject(root.NodeData.DataItem.NodeType, EFConnectionString);
            foreach (var node in root.Items)
            {
                UpdateDataAcessObject(node as TreeViewIconsItem, EFConnectionString);
            }

        }
        private void CopyNodeTextExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.CopyNodeText();
        }

        private void ExpandAllNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            if (curDbInfoTab.treeView1.SelectedItem != null)
            {
                curDbInfoTab.treeView1.SelectedItem.ExpandSubtree();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (findNodesWindow != null)
            {
                findNodesWindow.ShouldExit = true;
                findNodesWindow.Close();
            }

            if (cutNode != null)
            {
                e.Cancel = true;
                MessageBox.Show("有未粘贴的节点。请先粘贴节点后再退出");
                return;
            }
            //用户没有打开任何数据库文件
            if (curDbInfoTab == null)
            {
                return;
            }
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            SaveDbTabDataToDB(curDbInfoTab);


            //更新配置文件内容
            DatabaseInfo info = null;
            TreeViewIconsItem selectedItem = null;
            SystemConfig.configArgus.DbInfos.Clear();
            foreach (var item in DBtabContainer.Items)
            {
                selectedItem = ((item as TabItem).Content as DBInfoTab).treeView1.SelectedItem;
                info = ((item as TabItem).Content as DBInfoTab).dbInfoObject;
              
                SystemConfig.configArgus.DbInfos.Add(info);
            }
            SystemConfig.configArgus.ActiveDBIndex = DBtabContainer.SelectedIndex;
            DeepSerializer.BinarySerialize(SystemConfig.configArgus, SystemConfig.ConfigFileName);
        }

        private void ShowFindNodesWindow(object sender, ExecutedRoutedEventArgs e)
        {
            findNodesWindow.Show();
        }


        private void ShowConfigWin(object sender, ExecutedRoutedEventArgs e)
        {
            ConfigWin win = new ConfigWin();
            bool? result = win.ShowDialog();
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
                // SystemConfig.configArgus.LastVisitNodePath = curDbInfoTab.treeView1.SelectedItem.Path;
            }
            if (result.Value)
            {
                DeepSerializer.BinarySerialize(SystemConfig.configArgus, SystemConfig.ConfigFileName);
                

                MessageBox.Show("参数修改，请重新启动程序");

                findNodesWindow.ShouldExit = true;
                Close();
                Process.Start(Assembly.GetEntryAssembly().Location);
            }
        }


        private void ExitApplication(object sender, ExecutedRoutedEventArgs e)
        {
            findNodesWindow.ShouldExit = true;
            Close();
        }

        private void AddChild_Folder(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddChild, "Folder");
        }

        private void AddSibling_Folder(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddSibling, "Folder");
        }

        private void AddRoot_Folder(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.IsInEditMode)
            {
                curDbInfoTab.treeView1.SelectedItem.EndEdit();
            }
            AddNode(AddNodeCategory.AddRoot, "Folder");
        }

        private void GoBack(object sender, ExecutedRoutedEventArgs e)
        {

            curDbInfoTab.visitedNodesManager.GoBack();

        }



        private void GoForward(object sender, ExecutedRoutedEventArgs e)
        {

            curDbInfoTab.visitedNodesManager.GoForward();

        }



        private void ToggleNodeTextBold(object sender, ExecutedRoutedEventArgs e)
        {
            if (curDbInfoTab.treeView1.SelectedItem != null)
            {
                if (curDbInfoTab.treeView1.SelectedItem.FontWeight != FontWeights.ExtraBold)
                    curDbInfoTab.treeView1.SelectedItem.FontWeight = FontWeights.ExtraBold;
                else
                    curDbInfoTab.treeView1.SelectedItem.FontWeight = FontWeights.Normal;
                curDbInfoTab.SaveTreeToDB();
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
                String DBFileName = openFileDialog.FileName;
                bool IsDBAlreadyOpen = false;
                foreach (var item in DBtabContainer.Items)
                {
                    if (((item as TabItem).Content as DBInfoTab).dbInfoObject.DatabaseFilePath == DBFileName)
                    {
                        IsDBAlreadyOpen = true;
                        break;
                    }
                }


                if (IsDBAlreadyOpen)
                {
                    MessageBox.Show("此资料库己被打开");
                    return;
                }
                AddNewDbInfoTabAndLoadData(DBFileName);
            }
        }
        /// <summary>
        /// 打开指定的数据库文件，创建新选项卡，装入数据，新创建的选项卡成为当前选项卡
        /// </summary>
        /// <param name="DBFileName"></param>
        private void AddNewDbInfoTabAndLoadData(String DBFileName)
        {
            DatabaseInfo dbInfo = new DatabaseInfo()
            {
                DatabaseFilePath = DBFileName,
                LastVisitNodePath = ""
            };

            SystemConfig.configArgus.DbInfos.Add(dbInfo);

            //添加选项卡
            DBInfoTab tab = new DBInfoTab(dbInfo);
            DBtabContainer.Add(System.IO.Path.GetFileName(dbInfo.DatabaseFilePath), tab);
            DBtabContainer.SelectedIndex = DBtabContainer.Items.Count - 1;
            SystemConfig.configArgus.ActiveDBIndex = DBtabContainer.Items.Count - 1;

            curDbInfoTab = tab;
            //Note: 新加选项卡，会激发DBtabContainer的SelectedIndexChanged事件，在事件响应代码DBtabContainer_SelectionChanged（）
            //中完成了从数据库中装载数据的工作，无需显示调用LoadCurrentTabDataFromDB();方法
        }
        private void CopyDB(object sender, ExecutedRoutedEventArgs e)
        {
            String currentDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            String DBTemplateFileName = "";
            if (File.Exists(currentDir + "\\infocenter.sdf"))
            {
                DBTemplateFileName = currentDir + "\\infocenter.sdf";
            }
            else
            {
                if (File.Exists(currentDir + "\\template\\infocenter.sdf"))
                {
                    DBTemplateFileName = currentDir + "\\template\\infocenter.sdf";
                }
                else
                {
                    MessageBox.Show("数据库模板文件：templateDB.sdf未找到");
                    return;
                }
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
                bool IsDBAlreadyOpen = false;
                foreach (var item in DBtabContainer.Items)
                {
                    if(((item as TabItem).Content as DBInfoTab).dbInfoObject.DatabaseFilePath==saveFileDialog.FileName){
                        IsDBAlreadyOpen=true;
                        break;
                    }
                }

                
                if (IsDBAlreadyOpen)
                {
                    MessageBox.Show("不能选择正在使用的资料库文件名");
                    return;
                }
                //复制数据库
                File.Copy(DBTemplateFileName, saveFileDialog.FileName, true);

                AddNewDbInfoTabAndLoadData(saveFileDialog.FileName);

            }
        }
        #endregion

        private void MoveLeft(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.MoveLeft(sender, e);
        }

        private void MoveRight(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.MoveRight(sender, e);
        }

        private void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.MoveUp(sender, e);
        }

        private void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            curDbInfoTab.MoveDown(sender, e);
        }

        private void DBtabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            //初始化时，此方法不做任何事
            if (DBtabContainer.SelectedIndex == -1)
            {
                return;
            }
            //当TabControl的TabItem中放置有ComboBox控件时，ComboBox的SelectionChanged事件会触发
            //TabControl的SelectionChanged事件，虽然可以通过在下层控件的事件响应过程中添加
            //e.Handled = true阻止这一过程，但还是在此屏蔽掉此事件的响应代码，以免有漏网之鱼
            if (e.AddedItems.Count == 1 && e.AddedItems[0].GetType().Name!="TabItem")
            {
                return;
            }

            curDbInfoTab = (DBtabContainer.Items[DBtabContainer.SelectedIndex] as TabItem).Content as DBInfoTab;
            this.Title = "个人资料管理中心-" + curDbInfoTab.dbInfoObject.DatabaseFilePath;
            if (curDbInfoTab.HasBeenLoadedFromDB == false)
            {
                //从数据库中装入数据
                LoadCurrentTabDataFromDB();
            }
            else
            {
                String EFConnectString = DALConfig.getEFConnectionString(curDbInfoTab.dbInfoObject.DatabaseFilePath);
                curDbInfoTab.treeView1.EFConnectionString = EFConnectString;
                findNodesWindow.SetTree(curDbInfoTab.treeView1);
                curDbInfoTab.RefreshDisplay();
            }

            

        }
    }
}
