using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFSuperTreeView;

namespace PersonalInfoForWPF.BackAndForward
{
    /// <summary>
    /// 在内部保存所有己经访问过的节点路径，从而允许“Back”和“forward”，就象浏览器一样
    /// </summary>
    public class VisitedNodesManager
    {
        private SuperTreeView _treeView = null;

        public VisitedNodesManager(SuperTreeView treeView)
        {
            _treeView = treeView;
        }

        private NodesStack BackStack = new NodesStack();
        private NodesStack ForwardStack = new NodesStack();

        private bool ShouldAddToStack = true;

        public void AddHistoryRecord(String NodePath)
        {
            if (ShouldAddToStack)
            {
                BackStack.Push(NodePath);
            }
            else//用户是点击“Back”或“Forward”引发的节点切换，则本次操作不保存
            {
                ShouldAddToStack = true;  //下次可以了记录了
            }
            
        }

        public bool CanGoBack()
        {
            return !BackStack.IsEmpty();
        }

        public void GoBack()
        {
            String nodePath = BackStack.Pop();
            if (!String.IsNullOrEmpty(nodePath))
            {
                ForwardStack.Push(_treeView.SelectedItem.Path);
                ShouldAddToStack = false;
                _treeView.ShowNode(nodePath);
                
            }

        }
        public void GoForward()
        {
            String nodePath = ForwardStack.Pop();
            if (!String.IsNullOrEmpty(nodePath))
            {
                BackStack.Push(_treeView.SelectedItem.Path);
                ShouldAddToStack = false;
                _treeView.ShowNode(nodePath);
               
            }
        }
        public bool CanGoForward()
        {
            return !ForwardStack.IsEmpty();
        }
    }
}
