using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextStep : MonoBehaviour
{
    public TextMeshPro textStep;

    //bool��Ҫ��ʲô��ɫ���� true��ɫ false��ɫ
    public void InitText(int _step, bool color, Vector3 position, float scale)
    {
        textStep.text = _step.ToString();
        textStep.color = color == true ? Color.white : Color.black;
        this.transform.position = position;
        this.transform.localScale = new Vector3(scale, scale, 1);
    }
}
