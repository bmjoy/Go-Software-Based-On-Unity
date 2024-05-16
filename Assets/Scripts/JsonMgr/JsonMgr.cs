using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum JsonType
{
    JsonUtility,
    LitJson
}


//�����Ǹ��ݲ�ͬ���ļ���ʽ��ȡ�ַ���Ϣ��ͨ������ʵ�����л��ͷ����л�
/// <summary>
/// Json���ݹ����� ��Ҫ���ڽ��� Json�����л��洢��Ӳ�� �� �����л���Ӳ���ж�ȡ���ڴ���
/// </summary>
public class JsonMgr
{
    private static JsonMgr instance = new JsonMgr();
    public static JsonMgr Instance => instance;

    private JsonMgr() { }

    public void SaveData(object data,string fileName,JsonType type=JsonType.LitJson)
    {
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        string jsonStr = "";
        switch(type)
        {
            case JsonType.JsonUtility:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
        }
        File.WriteAllText(path, jsonStr);
    }

    public T LoadData<T>(string fileName,JsonType type=JsonType.LitJson) where T : new()
    {
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //Ĭ���ļ���
        if(!File.Exists(path)) { path = Application.persistentDataPath + "/" + fileName + ".json"; }
        //��д�ļ���
        if (!File.Exists(path)) return new T();
        //Ĭ�϶���

        string jsonStr = File.ReadAllText(path);
        T data = default(T);
        switch(type)
        {
            case JsonType.JsonUtility:
                data=JsonUtility.FromJson<T>(jsonStr); break;
            case JsonType.LitJson:
                data=JsonMapper.ToObject<T>(jsonStr); break;
        }
        return data;
    }
}
