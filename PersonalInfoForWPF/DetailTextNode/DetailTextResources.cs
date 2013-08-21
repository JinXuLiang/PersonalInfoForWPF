using InterfaceLibrary;
using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace DetailTextNode
{
    class DetailTextResources
    {
        private static ImageSource _normal = null;
        /// <summary>
        /// 正常状态下的图标
        /// </summary>
        public static ImageSource normalIcon
        {
            get
            {
                if (_normal == null)
                {
                    _normal = ImageUtils.GetBitmapSourceFromImageFileName("pack://application:,,,/DetailTextNode;component/Images/Detail.ico", UriKind.Absolute);

                }
                return _normal;
            }
            set
            {
                _normal = value;
            }

        }

        private static ImageSource _selected = null;
        /// <summary>
        /// 处于选中状态的图标
        /// </summary>
        public static ImageSource selectedIcon
        {
            get
            {
                if (_selected == null)
                {
                    _selected = ImageUtils.GetBitmapSourceFromImageFileName("pack://application:,,,/DetailTextNode;component/Images/SelectedDetail.ico", UriKind.Absolute);

                }
                return _selected;
            }
            set
            {
                _selected = value;
            }

        }
        /// <summary>
        /// 实现复用的UI组件
        /// </summary>
       public static DetailTextPanel RootControl = new DetailTextPanel();
       
    }
}
