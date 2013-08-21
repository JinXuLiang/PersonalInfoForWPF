using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicLibrary.Network
{
    /// <summary>
    /// 封装一些HTTP请求的ContentType常量
    /// </summary>
    public static class HttpContentType
    {
        public const String Form_UrlEncoded = "application/x-www-form-urlencoded";
        public const String Json = "text/json; charset=utf-8";
        public const String Xml = "text/xml; charset=utf-8";
        public const String Html = "text/xml; charset=utf-8";
    }
}
