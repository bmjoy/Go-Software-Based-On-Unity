using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TextMeshPro���������
public class AutoExpandLineExtension : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Text txt=this.GetComponent<Text>();
        // ����������
        GameObject dividerParent = new GameObject("Divider");
        RectTransform dividerRect = dividerParent.AddComponent<RectTransform>();
        dividerRect.SetParent(this.transform, false);

        // ���ø������λ�ú�ê�����ı�������ͬ
        dividerRect.position = txt.rectTransform.position;
        dividerRect.anchorMin = new Vector2(0, 0.5f);
        dividerRect.anchorMax = new Vector2(0, 0.5f);
        dividerRect.pivot = new Vector2(0, 0.5f);

        // ���Image�����������ɫ
        Image dividerImage = dividerParent.AddComponent<Image>();
        dividerImage.color = Color.white;

        // ���ø�����Ĵ�С���ı�����Ĵ�С��ͬ
        dividerRect.sizeDelta = new Vector2(txt.fontSize * txt.text.Length, 1);

        // ���ø������λ�����ı�������·�
        dividerRect.anchoredPosition = new Vector2(0, -txt.fontSize + 5);
    }
}
