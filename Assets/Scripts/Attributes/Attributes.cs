using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��Ÿ�������
public class IntOptionsAttribute : PropertyAttribute
{
    public int[] Options;

    public IntOptionsAttribute(params int[] options)
    {
        this.Options = options;
    }
}