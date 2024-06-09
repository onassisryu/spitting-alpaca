using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public static class SerializationHelper
{
    public static string SerializeToString<T>(T obj)
    {
        if (obj == null)
            return null;

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            binaryFormatter.Serialize(memoryStream, obj);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    public static T DeserializeFromString<T>(string str)
    {
        if (string.IsNullOrEmpty(str))
            return default(T);

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(str)))
        {
            return (T)binaryFormatter.Deserialize(memoryStream);
        }
    }

    public static T DeepCopy<T>(T obj)
    {
        return DeserializeFromString<T>(SerializeToString(obj));
    }
}
