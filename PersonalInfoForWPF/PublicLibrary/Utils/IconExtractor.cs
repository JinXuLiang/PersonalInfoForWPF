using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;


namespace PublicLibrary.Utils
{
    public class IconExtractor
    {
        /// <summary>
        /// 根据文件名提取出此文件所对应的图标
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="iconSize"></param>
        /// <returns></returns>
        public static Icon GetFileIcon(string fileName, IconSize iconSize)
        {
            Icon icon = null;
            try
            {
                SHFILEINFO psfi = new SHFILEINFO();
                Win32.SHGetFileInfo(fileName, 0, ref psfi, (uint)Marshal.SizeOf(psfi), (uint)(0x100 | ((iconSize == IconSize.Small) ? 1 : 0)));
                icon = Icon.FromHandle(psfi.hIcon);
            }
            catch
            {
            }
            return icon;
        }

        private static string GetTypeName(string fileName)
        {
            SHFILEINFO psfi = new SHFILEINFO();
            Win32.SHGetFileInfo(fileName, 0x80, ref psfi, (uint)Marshal.SizeOf(psfi), 0x400);
            return psfi.szTypeName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0;
            public const uint SHGFI_SMALLICON = 1;

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref IconExtractor.SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        }
    }

    /// <summary>
    /// 文件图标的大小类型
    /// </summary>
    public enum IconSize
    {
        Small,
        Large
    }
}
