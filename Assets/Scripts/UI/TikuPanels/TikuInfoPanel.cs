using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TikuInfoPanel : MonoBehaviour
{
    public ScrollRect sr;
    public RectTransform tip;
    public Text sentence;//�������ʾ����

    public Button btnQuit;//�ص�ѡ�����
    public Button btnTry;//����
    public Button btnRestart;//���¿�ʼ
    public Button btnNext;//��һ��

    private bool istry;
    private int timuIndex = 0;
    private List<QuestionItem> questions = new List<QuestionItem>();

    // Start is called before the first frame update
    void Start()
    {
        //��Ҫ��ȡ��������Ŀ������

        //���������ʾ��Ŀ
        List<GameTree> timu = TikuMgr.Instance.tikuDic[TikuMgr.Instance.curTiku];
        for (int i = 0; i < timu.Count; ++i)
        {
            GameObject item = Instantiate(Resources.Load<GameObject>("Question"));
            item.transform.SetParent(sr.content.transform, false);
            QuestionItem question = item.GetComponent<QuestionItem>();
            questions.Add(question);
            question.InitInfo("��ɱ" + (i + 1), "����", timu[i], i);
        }

        //��ʾ��һ��������
        TikuMgr.Instance.ShowChessBoard(timu[0].size);
        //������setquestion
        TikuMgr.Instance.GetCurrentBoard().SetQuestion(timu[0]);

        btnQuit.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("UserScene");
            UIMgr.Instance.ShowPanel<MainPanel>();
            UIMgr.Instance.ShowPanel<GoPanel>();
            UIMgr.Instance.ShowPanel<TikuEntrancesPanel>();
        });

        btnTry.onClick.AddListener(() =>
        {
            //���µ�ʱ��Ϊ��ɫ ����Ϊ��ɫ
            istry = !istry;
            if(istry==false)
            {
                btnTry.image.color = Color.white; 
            }
            else
            {
                btnTry.image.color = Color.yellow;
            }
            TikuMgr.Instance.GetCurrentBoard().ChangeTryCase(istry);
            HideTip();
        });

        btnRestart.onClick.AddListener(() =>
        {
            HideTip();
            TikuMgr.Instance.GetCurrentBoard().ResetBoard();
        });

        btnNext.onClick.AddListener(() =>
        {
            if (timuIndex + 1 < timu.Count) timuIndex++;
            else return;

            HideTip();
            TikuMgr.Instance.ShowChessBoard(timu[timuIndex].size);
            //������setquestion
            TikuMgr.Instance.GetCurrentBoard().SetQuestion(timu[timuIndex]);
            TikuMgr.Instance.GetCurrentBoard().ResetBoard();
        });
    }

    public void ShowTip(bool success)
    {
        //��ʾ����
        if(success)
        {
            questions[timuIndex].SetComplete(true);//���������
            sentence.text = "����һ���Ǹ��֣�һ���־Ͳ�ͬ����";
        }
        else
        {
            sentence.text = "û��ô����";
            StartCoroutine("DelayHide", 1.5f);
        }
        tip.offsetMax = new Vector2(tip.offsetMax.x, 0);
    }

    public void HideTip()
    {
        tip.offsetMax = new Vector2(tip.offsetMax.x, 600);
    }

    public void SetIndex(int i)
    {
        timuIndex = i;
    }

    public void ResetTry()
    {
        istry = false;
        btnTry.image.color = Color.white;
    }

    IEnumerator DelayHide(float time)
    {
        yield return new WaitForSeconds(time);
        HideTip();
    }

    private void OnDestroy()
    {
        //��Ҫ����������Ŀ������

        TikuMgr.Instance.curboardSize = 0;
    }
}
