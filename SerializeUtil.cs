using SD.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotCover
{
    public static class SerializeUtil
    {
        public static TModel Deserialize<TModel>(string dataFileName) where TModel:class
        {
            if (!File.Exists(dataFileName))
            {
                return default(TModel);
            }

            using (var fs = new FileStream(dataFileName, FileMode.Open, FileAccess.Read))
            {
                var bf = new BinaryFormatter();
                try
                {
                    var model = bf.Deserialize(fs) as TModel;
                    return model;
                }
                catch (Exception ex)
                {
                }
            }
            
            return default(TModel);
        }

        /// <summary>
        /// 序列化对象到本地
        /// </summary>
        /// <param name="dataFileName"></param>
        /// <param name="graph"></param>
        public static void Serialize(string dataFileName, object graph)
        {
            var directoryName = new FileInfo(dataFileName).Directory.FullName;
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            try
            {
                SerializeToLocal(dataFileName, graph);
            }
            catch (Exception ex)
            {
            }
        }

        private static void SerializeToLocal(string dataFileName, object graph)
        {
            using (var fs = new FileStream(dataFileName, FileMode.OpenOrCreate))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, graph);
            }
        }
    }
}
