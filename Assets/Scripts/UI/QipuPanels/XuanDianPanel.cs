using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class XuanDianPanel : BasePanel
{
    public TextMeshProUGUI txtXuandian;
    public TextMeshProUGUI txtWinrate;

    private Dictionary<int, string> dic = new Dictionary<int, string>()
    {
        {1,"һ"},{2,"��"},{3,"��"},{4,"��"},{5,"��"},{6,"��"},{7,"��"},{8,"��"},{9,"��"},{10,"ʮ"},{11,"ʮһ"},{12,"ʮ��"}
    };
    protected override void Init()
    {
        
    }

    public void InitText(bool iswhite, int order, double rate)
    {
        txtXuandian.text = (iswhite ? "��" : "��") + "����" + dic[order] + "ѡ��";
        txtWinrate.text = "ʤ�ʣ�" + rate + "%";
    }
}
