using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;

namespace publicLibrary.Serializer
{
    /// <summary>
    /// 使用二进制或SOAP格式对一个对象进行序列化或反序列化
    /// </summary>
    public class DeepSerializer
    {
        /// <summary>
        /// 将一个对象串行化到指定的文件中(SOAP格式）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        public static void SoapSerialize(Object obj, String filename)
        {
            if (string.IsNullOrEmpty(filename.Trim()))
            {
                throw new Exception("请指定要串行化的文件名,在DeepSerializer.SoapSerialize() 中");
            }
            using (FileStream stream = File.Create(filename))
            {
                SoapFormatter formatter = new SoapFormatter();
                formatter.Serialize(stream, obj);
            }
        }

        //指定一个SOAP文件名，从其内容中重建对象
        public static Object SoapDeserialize(String filename)
        {


            FileStream stream = null;
            if (string.IsNullOrEmpty(filename.Trim()))
            {
                throw new Exception("请指定要串行化的文件名,在DeepSerializer.SoapDeserialize() 中");
            }
            SoapFormatter formater = new SoapFormatter();
            Object obj = null;

            using (stream = System.IO.File.OpenRead(filename))
            {
                obj = formater.Deserialize(stream);
                return obj;
            }
        }


        //将一个对象串行化到指定的文件中(Binary格式）
        public static void BinarySerialize(object obj, String filename)
        {
            if (obj == null)
                return;

            if (String.IsNullOrEmpty(filename.Trim()))
            {
                throw new Exception("请指定要串行化的文件名,在DeepSerializer.BinarySerialize() 中");
            }


            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = File.Create(filename))
            {
                formatter.Serialize(stream, obj);
            }


        }


        //指定一个二进制文件名，从其内容中重建对象
        public static Object BinaryDeserialize(String filename)
        {
            if (string.IsNullOrEmpty(filename.Trim()))
            {
                throw new Exception("请指定要串行化的文件名,在DeepSerializer.BinaryDeserialize() 中");
            }
            using (FileStream stream = File.OpenRead(filename))
            {
                BinaryFormatter formater = new BinaryFormatter();
                return formater.Deserialize(stream);
            }



        }


    }
}
