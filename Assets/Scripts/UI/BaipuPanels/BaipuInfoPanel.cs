using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaipuInfoPanel : MonoBehaviour
{
    public ScrollRect sv;
    List<BaipuItem> list = new List<BaipuItem>();//����������ٵ�0����Ҳ���ǳ�ʼ����

    //ÿ��һ���壬��Ҫ���BaipuItem����BaipuchessBoard�е���
    public void AddItem(GameObject item)
    {
        item.transform.SetParent(sv.content.transform, false);
        list.Add(item.GetComponent<BaipuItem>());
        //ÿһ�θ��¶��Ƶ�������
        sv.verticalNormalizedPosition = 0f;
    }

    //�ṩ�ⲿɾ���Ľӿڣ�ɾ����start���������
    public void RemoveAfter(int start)
    {
        for(int i= list.Count-1; i>=start; i--)
        {
            GameObject.Destroy(list[i].gameObject);
        }
        list.RemoveRange(start, list.Count - start);
    }

    public BaipuItem GetItem(int i)
    {
        return list[i];
    }

    private void OnDestroy()
    {
        BaiPuMgr.Instance.curboardsize = 0;
    }
}
