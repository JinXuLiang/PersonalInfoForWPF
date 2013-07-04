using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersonalInfoForWPF
{
    /// <summary>
    /// 封装一个Stack，保存用户访问节点的历史记录
    /// </summary>
    class NodesStack
    {
        /// <summary>
        /// 最多保存50个历史记录
        /// </summary>
        private const int MAX_SIZE = 50;
        /// <summary>
        /// 用于己访问过的节点路径
        /// </summary>
        private List<String> VisitedNodes = new List<string>();

        public void Push(String NodePath)
        {
            if (String.IsNullOrEmpty(NodePath))
            {
                return;
            }
            //不加入重复的节点
            if (VisitedNodes.Count > 0 && VisitedNodes[0] == NodePath)
            {
                return;
            }
            //超过最大容量，移除最后10条
            if (VisitedNodes.Count >= MAX_SIZE)
            {
                VisitedNodes.RemoveRange(VisitedNodes.Count - 10, 10);
            }
           
            VisitedNodes.Insert(0, NodePath);
        }

        public String Pop()
        {
            if (VisitedNodes.Count == 0)
            {
                return null;
            }
            String top = VisitedNodes[0];
            VisitedNodes.RemoveAt(0);
            return top;
        }

        public bool IsEmpty()
        {
            return VisitedNodes.Count == 0;
        }

    }
}
