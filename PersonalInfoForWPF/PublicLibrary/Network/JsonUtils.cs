using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace PublicLibrary.Network
{
    /// <summary>
    /// 提供Json相关的一些功能
    /// </summary>
    public class JsonUtils
    {
        
       

        /// <summary>
        ///  将一个对象序列化为Json字串，它在内部使用DataContractJsonSerializer
        ///  它不转换DateTime对象（序列化结果为"\/Date(1294499956278+0800)\/"），可用于.NET平台上的对象序列化
        /// </summary>

        public static string ToJson<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
           

            return jsonString;

        }

        /// <summary>
        ///  将一个对象序列化为Json字串，它在内部使用DataContractJsonSerializer
        /// 它转换DateTime对象的序列化结果为"2011-01-09 01:00:56"形式，可用于向非.NET平台的应用传送对象序列化
        /// </summary>

        public static string ToJson_CovertDateTimeToGeneralFormat<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            //替换Json的Date字符串
            string p = @"\\/Date\((\d+)\+\d+\)\\/";

            MatchEvaluator matchEvaluator = new MatchEvaluator(ConvertJsonDateToDateString);

            Regex reg = new Regex(p);

            jsonString = reg.Replace(jsonString, matchEvaluator);

            return jsonString;

        }


        /// <summary>

        ///从Json字串中反序列化出对象
        ///它直接把"\/Date(1294499956278+0800)\/"形式的字串转换为DateTime对象，可用于.NET平台上的对象序列化
        /// </summary>

        public static T FromJson<T>(string jsonString) 
        {
                     
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

            T obj = (T)ser.ReadObject(ms);

            return obj;

        }

        /// <summary>

        /// 从Json字串中反序列化出对象
        ///它会把字串中将"yyyy-MM-dd HH:mm:ss"格式的字符串转为"\/Date(1294499956278+0800)\/"格式
        ///以便顺利地反序列化出DateTime对象
        /// </summary>

        public static T FromJson_CovertDateTimeFromGeneralFormat<T>(string jsonString)
        {
            //将"yyyy-MM-dd HH:mm:ss"格式的字符串转为"\/Date(1294499956278+0800)\/"格式

            string p = @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}";

            MatchEvaluator matchEvaluator = new MatchEvaluator(ConvertDateStringToJsonDate);

            Regex reg = new Regex(p);

            jsonString = reg.Replace(jsonString, matchEvaluator);

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

            T obj = (T)ser.ReadObject(ms);

            return obj;

        }

          /// <summary>

     /// 将Json序列化的时间由/Date(1294499956278+0800)转为字符串

    /// </summary>

     private static string ConvertJsonDateToDateString(Match m)

     {

        string result = string.Empty;

         DateTime dt = new DateTime(1970,1,1);

        dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));

         dt = dt.ToLocalTime();

         result = dt.ToString("yyyy-MM-dd HH:mm:ss");

         return result;

     }

         /// <summary>

       /// 将时间字符串转为Json时间

    /// </summary>

     private static string ConvertDateStringToJsonDate(Match m)

     {

         string result = string.Empty;

         DateTime dt = DateTime.Parse(m.Groups[0].Value);

        dt = dt.ToUniversalTime();

        TimeSpan ts = dt - DateTime.Parse("1970-01-01");

        result = string.Format("\\/Date({0}+0800)\\/",ts.TotalMilliseconds);

       return result;

    }
    }
}
