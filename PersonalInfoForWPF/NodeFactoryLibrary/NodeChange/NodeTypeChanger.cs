using NodeFactoryLibrary.NodeChange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeFactoryLibrary
{
    /// <summary>
    /// 完成节点间类型转换的工作
    /// </summary>
    public class NodeTypeChanger
    {
        private static SortedDictionary<String, List<INodeChanger>> NodeTypeChangeDictionaries;

        static NodeTypeChanger()
        {
            NodeTypeChangeDictionaries = new SortedDictionary<string, List<INodeChanger>>();
            //For OnlyText
            List<INodeChanger> TypeChangers = new List<INodeChanger>();
            TypeChangers.Add(new OnlyTextToDetailTextChanger());
            TypeChangers.Add(new OnlyTextToFolderChanger());
            NodeTypeChangeDictionaries.Add("OnlyText", TypeChangers);

            //For DetailText
            TypeChangers = new List<INodeChanger>();
            TypeChangers.Add(new DetailTextToOnlyTextChanger());
            TypeChangers.Add(new DetailTextToFolderChanger());
            NodeTypeChangeDictionaries.Add("DetailText", TypeChangers);

            //For Folder
            TypeChangers = new List<INodeChanger>();
            TypeChangers.Add(new FolderToOnlyTextChanger());
            TypeChangers.Add(new FolderToDetailTextChanger());
            NodeTypeChangeDictionaries.Add("Folder", TypeChangers);

        }
        /// <summary>
        /// 某种类型的节点能否转换为另一种类型的节点
        /// </summary>
        /// <param name="FromNodeType"></param>
        /// <param name="ToNodeType"></param>
        /// <returns></returns>
        public static bool CanChangeTo(String FromNodeType, String ToNodeType)
        {
            bool result = false;
            //自己转换自己，肯定能成！
            if (FromNodeType == ToNodeType)
            {
                return true;
            }
            //查找本类型的类型转换器
            List<INodeChanger> TypeChangers = NodeTypeChangeDictionaries[FromNodeType];
            if (TypeChangers != null)
            {
                //如果找到了，则类型转换器不为null
                result = TypeChangers.FirstOrDefault(p => p.ChangeToNodeType == ToNodeType) == null ? false : true;
            }
            return result;
        }
        /// <summary>
        /// 尝试获取类型转换器，不成功，返回null
        /// </summary>
        /// <param name="FromNodeType"></param>
        /// <param name="ToNodeType"></param>
        /// <returns></returns>
        public static INodeChanger GetNodeChanger(String FromNodeType, String ToNodeType)
        {
            if (CanChangeTo(FromNodeType, ToNodeType))
            {
                //查找本类型的类型转换器
                List<INodeChanger> TypeChangers = NodeTypeChangeDictionaries[FromNodeType];
                if (TypeChangers != null)
                {
                    return TypeChangers.FirstOrDefault(p => p.ChangeToNodeType == ToNodeType);
                }
            }
            return null;
        }
    }
}
