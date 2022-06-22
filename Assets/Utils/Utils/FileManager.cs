using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace _Scripts.Utils
{
    public static class FileManager
    {
        public static string GetFilePath(string fileName)
        {
            return fileName.Contains(global::UnityEngine.Application.persistentDataPath + "/") ? fileName : global::UnityEngine.Application.persistentDataPath + "/" + fileName;
        }
        public static bool HasDirectory(string dirName)
        {
            return System.IO.Directory.Exists(GetFilePath(dirName));
        }
        public static void CreateDirectory(string dirName)
        {
            if (HasDirectory(dirName))
                return;
            System.IO.Directory.CreateDirectory(GetFilePath(dirName));
        }
        public static void DeleteDirectory(string dirName)
        {
            if (!HasDirectory(dirName))
                return;
            System.IO.Directory.Delete(GetFilePath(dirName), true);
        }
        public static string LoadFile(string fileName)
        {
            return System.IO.File.ReadAllText(GetFilePath(fileName));
        }
        public static void SaveFile(string fileName, string data)
        {
            System.IO.File.WriteAllText(GetFilePath(fileName), data);
        }
        public static void CreateFile(string fileName)
        {
            FileStream file = File.Create(GetFilePath(global::UnityEngine.Application.persistentDataPath + "/" + fileName));
            file.Close();
        }
        public static void DeleteFile(string fileName)
        {
            System.IO.File.Delete(GetFilePath(fileName));
        }
        public static bool FileExists(string fileName)
        {
            return File.Exists(GetFilePath(fileName));
            /*try
            {
                LoadFile(fileName);
                return true;
            }
            catch (FileNotFoundException e)
            {
                return false;
            }*/
        }
        public static T LoadJsonFile<T>(string fileName)
        {
            return JsonConvert.DeserializeObject<T>(LoadFile(fileName));
        }
        public static void SaveJsonFile(string fileName, string json)
        {
            SaveFile(fileName, json);
        }

        public static void SaveObjectFile(string fileName, Object data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(GetFilePath(fileName));
            bf.Serialize(file, data);
            file.Close();
        }
        public static Object LoadObjectFile(string fileName)
        {
            if (File.Exists(global::UnityEngine.Application.persistentDataPath + fileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(GetFilePath(fileName), FileMode.Open);
                Object a = (Object)bf.Deserialize(file);
                file.Close();
                return a;
            }
            return null;
        }
        public static string SaveImageFile(string fileName, Texture2D image)
        {
            byte[] bytes = image.EncodeToPNG();
            var path = GetFilePath(fileName);
            File.WriteAllBytes(path, bytes);
            return path;
        }
        public static Texture2D LoadImageFile(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(GetFilePath(fileName));
            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(bytes);
            return t;
        }
    }
}