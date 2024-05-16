using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMgr : MonoBehaviour
{
    //���ص�gameScene��Pool�����ϣ��л���������ͻ��´���һ��PoolMgr����

    private static PoolMgr instance;
    public static PoolMgr Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject("PoolMgr");
                instance = obj.AddComponent<PoolMgr>();
            }
            return instance;
        }
    }

    public Dictionary<string, List<GameObject>> poolDic = new Dictionary<string, List<GameObject>>();


    //name ��Դ·��
    public GameObject GetObj(string name)
    {
        //nameΪ��Դ·��
        GameObject obj = null;

        if (poolDic.ContainsKey(name) && poolDic[name].Count != 0)
        {
            int num = poolDic[name].Count;
            obj = poolDic[name][num - 1];
            poolDic[name].RemoveAt(num - 1);
        }
        else
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
        }

        obj.SetActive(true);
        return obj;
    }

    public void PushObj(string name, GameObject obj)
    {
        obj.SetActive(false);//��ʧЧ

        //����������poolDic�ķ���
        if (poolDic == null) return;

        if (poolDic.ContainsKey(name))
        {
            poolDic[name].Add(obj);
        }
        else
        {
            poolDic.Add(name, new List<GameObject>() { obj });
        }
    }

    public void Clear()
    {
        poolDic.Clear();//���Clear��APIֻ�����ֵ��е�Ԫ��Ϊnull������ʵ����Count���ǲ��䣬�ֵ�������Ԫ�أ����Ƕ�Ϊnull
        poolDic = null;//�����ø�null����ϰ��
    }

    private void OnDestroy()
    {
        Clear();
    }
}
