using PublicLibrary.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PublicLibrary.Network
{
    /// <summary>
    /// 封装发送HTTP请求与接收响应的相应功能
    /// </summary>
    public class HttpWorker
    {
        /// <summary>
        /// 向指定的网址Post一个字符串，使用UTF8编码。
        /// 其中ContentType可以使用HttpContentType类中定义的常量
        /// 返回服务端发回的信息。
        /// 如果出错，本方法会抛出一个Exception异常对象，将服务端返回的信息作为此对象的Message
        /// 同时，其innerException属性引用.NET基类库所使用的原始WebException对象。
        /// </summary>
        /// <param name="WebUrl"></param>
        /// <param name="ContentType"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static String PostString(String WebUrl,String ContentType,String Content)
        {
            
            try
            {
                Uri uri = new Uri(WebUrl);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "Post";
                request.Timeout = 8000;
                request.ContentType = ContentType;

                byte[] requestData =StringUtils.getBytesUsingUTF8(Content);
                request.ContentLength = requestData.Length;
                request.AllowWriteStreamBuffering = false;
                //将要发送的数据写入到流中
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestData, 0, requestData.Length);
                requestStream.Close();
                //上传并读取响应
                HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
                byte[] responseData = getResponseData(response);

                if (responseData != null && responseData.Length > 0)
                {
                    return StringUtils.getStringUsingUTF8(responseData);
                }
            }
            catch (WebException ex)
            {
                WebResponse response = ex.Response;
                if (response != null)
                {
                    
                    byte[] responseData = getResponseData(response);
                    response.Close();
                    throw new Exception(StringUtils.getStringUsingUTF8(responseData), ex);
                }


            }
            catch (Exception e)
            {

                throw e;
            }
           
            return "";
        }

        /// <summary>
        /// 从Web服务器发回的响应流中读取数据，放入字节数组中
        /// 读取完毕之后，自动关闭数据流
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static byte[] getResponseData(WebResponse response)
        {
            byte[] buffer = new byte[8192];//数据缓冲区
            MemoryStream ms = new MemoryStream();
            Stream responsStream = response.GetResponseStream();
            //从将ResponseStream中的数据读入到MemeoryStream中
            int readBytes = 0;
            do
            {
                readBytes = responsStream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, readBytes);
            } while (readBytes > 0);
            responsStream.Close();
            return ms.ToArray();

        }
    }
}
