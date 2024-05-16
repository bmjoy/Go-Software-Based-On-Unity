using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitPanel : BasePanel
{
    public Text txt;

    protected override void Init()
    {
        updateActions += () =>
        {
            if (OnlinegameChessBoardMgr.Instance.readytoLoad == true)
            {
                OnlinegameChessBoardMgr.Instance.readytoLoad = false;
                //ƥ��ɹ����س���
                StartCoroutine("LoadSceneAndDoSomething");
            }
        };

        StartCoroutine(AutoTxt());
    }

    IEnumerator AutoTxt()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            txt.text = "����ƥ������൱�Ķ���";
            yield return new WaitForSecondsRealtime(0.5f);
            txt.text = "����ƥ������൱�Ķ���.";
            yield return new WaitForSecondsRealtime(0.5f);
            txt.text = "����ƥ������൱�Ķ���..";
            yield return new WaitForSecondsRealtime(0.5f);
            txt.text = "����ƥ������൱�Ķ���...";
        }
    }

    private IEnumerator LoadSceneAndDoSomething()
    {
        yield return SceneManager.LoadSceneAsync("OnlineGameScene" + OnlinegameChessBoardMgr.Instance.boardsize);
        // �ڳ���������Ϻ�ִ�в���
        OnlinegameChessBoardMgr.Instance.GetCurrentBoard();
        //����������� �����Լ�
        UIMgr.Instance.HideAll();
    }
}
