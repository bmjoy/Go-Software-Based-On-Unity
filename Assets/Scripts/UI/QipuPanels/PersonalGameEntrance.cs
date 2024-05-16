using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//������ֵ����
public class PersonalGameEntrance : MonoBehaviour
{
    public Button btn;
    public Text topName;
    public Text bottomName;
    private int index;//��Ŀ�±�

    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            QipuChessBoardMgr.Instance.index = index;
            QipuChessBoardMgr.Instance.boardsize = QipuChessBoardMgr.Instance.qijuList[index].getSize();
            UIMgr.Instance.HideAll();
            //���볡��
            SceneManager.LoadScene("QipuScene");
        });
    }

    public void InitEntrance(int i)
    {
        index = i;
        //���ָ��»�û�� ���������⣬��Ϊ��ѭ����
        topName.text = QipuChessBoardMgr.Instance.qijuList[i].getBlackName();
        bottomName.text = QipuChessBoardMgr.Instance.qijuList[i].getWhiteName();
        
    }
}
