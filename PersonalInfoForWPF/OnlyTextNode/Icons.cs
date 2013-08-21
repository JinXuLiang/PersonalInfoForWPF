using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace OnlyTextNode
{
    class Icons
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
                    _normal = ImageUtils.GetBitmapSourceFromImageFileName("pack://application:,,,/OnlyTextNode;component/Images/OnlyText.ico", UriKind.Absolute);

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
                    _selected = ImageUtils.GetBitmapSourceFromImageFileName("pack://application:,,,/OnlyTextNode;component/Images/SelectedOnlyText.ico", UriKind.Absolute);

                }
                return _selected;
            }
            set
            {
                _selected = value;
            }

        }
    }
}
