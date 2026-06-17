using System.IO;
using LitJson;
using UnityEngine;


public enum SerializeType
{
    JsonUtility,
    LitJson
}

public enum SerializePath
{
    Persistent,
    Streamling
}


/// <summary>
/// 【 Json管理器 】
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> ObjToJson </c>：将对象数据序列化成 Json </description>
/// </item>
/// <item>
/// <description><c> JsonToObj </c>：将Json反序列化为对象 </description>
/// </item>
/// <item>
/// <description><c> 外部给Json命就好，不要加斜杠,外部需要给Json加后缀</c>：</description>
/// </item>
/// </list>
/// </remarks>
public class JsonManager
{
    private static JsonManager _instance = new JsonManager();

    public static JsonManager Instance
    {
        get { return _instance; }
    }

    private JsonManager()
    {
    }

    public void ObjToJson(System.Object obj, string fileName, SerializeType type = SerializeType.LitJson, SerializePath path = SerializePath.Persistent)
    {
        string dataPath = JudgePath(path);

        switch (type)
        {
            case SerializeType.LitJson:
                string dataJsonMapper = JsonMapper.ToJson(obj);
                File.WriteAllText(dataPath + "/" + fileName, dataJsonMapper);
                break;
            case SerializeType.JsonUtility:
                string dataJsonUtility = JsonUtility.ToJson(obj);
                File.WriteAllText(dataPath + "/" + fileName, dataJsonUtility);
                break;
        }
    }

    public T JsonToObj<T>(string fileName, SerializeType type = SerializeType.LitJson, SerializePath path = SerializePath.Persistent)
    {
        string dataPath = JudgePath(path);

        if (!File.Exists(dataPath + "/" + fileName))
        {
            Debug.Log("json file doesn't exist");
            return default(T);
        }

        string json = File.ReadAllText(dataPath + "/" + fileName);

        T obj = default(T);
        switch (type)
        {
            case SerializeType.LitJson:
                obj = JsonMapper.ToObject<T>(json);
                break;
            case SerializeType.JsonUtility:
                obj = JsonUtility.FromJson<T>(json);
                break;
        }

        return obj;
    }

    public string JudgePath(SerializePath path)
    {
        string dataPath = null;

        switch (path)
        {
            case SerializePath.Persistent:
                dataPath = Application.persistentDataPath;
                break;
            case SerializePath.Streamling:
                dataPath = Application.streamingAssetsPath;
                break;
        }

        if (dataPath == null)
        {
            Debug.Log("dataPath is null");
        }

        return dataPath;
    }
}