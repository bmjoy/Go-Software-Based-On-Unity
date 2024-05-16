using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class OnlinePlayerPanel : MonoBehaviour
{
    //1Ϊ�� ���� 2Ϊ�� ����
    public Text txtPlayerName1;
    public Text txtPlayerName2;
    public Text txtRankScore1;
    public Text txtRankScore2;

    public Text txtEatenChesses1;//��ʾ������������UI
    public Text txtEatenChesses2;

    public Text txtTime1;//չʾ�ĵ���ʱUI
    public Text txtTime2;
    public readonly float duration = 7200f;//��ʱ�趨Ϊ�̶�ֵ����λΪ�룬Ĭ��ֵΪ7200s
    public readonly float looptime = 30f;//����ʱ����������ʱ��
    public readonly int loopturns = 5;//��������
    private float currentTime1;
    private float currentLoopTime1;
    private int currentLoopTurns1;
    private bool loopStart1 = false;
    private float currentTime2;
    private float currentLoopTime2;
    private int currentLoopTurns2;
    private bool loopStart2 = false;

    OnlinechessBoard chessboard;

    [HideInInspector]
    public AudioSource audioSource = null;

    private void Awake()
    {
        UIMgr.Instance.chatPanel = GameObject.Find("OnlinePlayerPanel").GetComponent<ChatPanel>();
        chessboard = OnlinegameChessBoardMgr.Instance.GetCurrentBoard();
    }

    void Start()
    {
        UserInfo localInfo = GameDataMgr.Instance.GetLocalUserInfo();
        UserInfo opponentInfo = GameDataMgr.Instance.GetOpponentUserInfo();
        if (GameDataMgr.Instance.isWhite == false)
        {
            txtPlayerName2.text = localInfo.userName;
            txtRankScore2.text = localInfo.rankScore.ToString();
            txtPlayerName1.text = opponentInfo.userName;
            txtRankScore1.text = opponentInfo.rankScore.ToString();
        }
        else
        {
            txtPlayerName1.text = localInfo.userName;
            txtRankScore1.text = localInfo.rankScore.ToString();
            txtPlayerName2.text = opponentInfo.userName;
            txtRankScore2.text = opponentInfo.rankScore.ToString();
        }

        currentTime1 = duration;
        currentLoopTime1 = looptime;
        currentLoopTurns1 = loopturns;
        currentTime2 = duration;
        currentLoopTime2 = looptime;
        currentLoopTurns2 = loopturns;

        ChangeTimeUI(txtTime1, currentTime1);
        ChangeTimeUI(txtTime2, currentTime2);
    }

    void Update()
    {
        //Update��ı�ʱ�䣬ʱ��UI�ı仯���е㲻һ�£�����ȱ��
        if (chessboard.isgamefinish) return;

        bool turn = GameDataMgr.Instance.turn;
        if(turn)//��������
        {
            if(currentTime1 <=0)//˼��ʱ�������� ��ʾ��ʼ����
            {
                if(loopStart1==false && SettingMgr.Instance.isDumiaoOn==true)
                {
                    MusicMgr.Instance.PlaySound("��ʼ����", false);
                    loopStart1 = true;
                }
                
                //��ʾ����ʱ
                if(currentLoopTime1<= 5 && SettingMgr.Instance.isCountdownOn)
                {
                    audioSource = MusicMgr.Instance.PlaySound("5�뵹��ʱ", false);
                }

                if(currentLoopTime1<=0)//����һ�ζ���
                {
                    if(currentLoopTurns1 > 0)
                    {
                        currentLoopTurns1--;
                        currentLoopTime1 = looptime;
                    }
                    else //��ʱ�и� ����������ͱ������ �����û����� �������ᷢ��Ϣ���¿ͻ�������
                    {
                        GameTimeOutMsg msg = new GameTimeOutMsg();
                        msg.iswhitewin = !GameDataMgr.Instance.isWhite;
                        ClientAsyncNet.Instance.Send(msg);
                        MusicMgr.Instance.PlaySound("��ʱ�и�",false);
                    }
                }
                currentLoopTime1 -= Time.deltaTime;
                ChangeTimeUI(txtTime1, currentLoopTime1);
            }
            else
            {
                currentTime1 -= Time.deltaTime;
                ChangeTimeUI(txtTime1, currentTime1);
            }
        }
        else if(!turn)//��������
        {
            if (currentTime2 <= 0)//˼��ʱ�������� ��ʾ��ʼ����
            {
                if (loopStart2 == false && SettingMgr.Instance.isDumiaoOn == true)
                {
                    MusicMgr.Instance.PlaySound("��ʼ����", false);
                    loopStart2 = true;
                }

                //��ʾ����ʱ
                if (currentLoopTime2 <= 5 && SettingMgr.Instance.isCountdownOn)
                {
                    audioSource = MusicMgr.Instance.PlaySound("5�뵹��ʱ", false);
                }

                if (currentLoopTime2 <= 0)//����һ�ζ���
                {
                    if (currentLoopTurns2 > 0)
                    {
                        currentLoopTurns2--;
                        currentLoopTime2 = looptime;
                    }
                    else //��ʱ�и� ����������ͱ������ �����û����� �������ᷢ��Ϣ���¿ͻ�������
                    {
                        GameTimeOutMsg msg = new GameTimeOutMsg();
                        msg.iswhitewin = !GameDataMgr.Instance.isWhite;
                        ClientAsyncNet.Instance.Send(msg);
                        MusicMgr.Instance.PlaySound("��ʱ�и�", false);
                    }
                }
                currentLoopTime2 -= Time.deltaTime;
                ChangeTimeUI(txtTime2, currentLoopTime2);
            }
            else
            {
                currentTime2 -= Time.deltaTime;
                ChangeTimeUI(txtTime2, currentTime2);
            }
        }

        txtEatenChesses1.text = GameDataMgr.Instance.eatenCount1.ToString();
        txtEatenChesses2.text = GameDataMgr.Instance.eatenCount2.ToString();
    }

    private void ChangeTimeUI(Text txtTime,float time)
    {
        int t1 = (int)time / 3600;
        if (t1 < 10)
        {
            txtTime.text = "0" + t1 + ":";
        }
        else
        {
            txtTime.text = t1 + ":";
        }

        int t2 = (int)time % 3600 / 60;
        if (t2 < 10)
        {
            txtTime.text += "0" + t2 + ":";
        }
        else
        {
            txtTime.text += t2 + ":";
        }

        int t3 = (int)time % 60;
        if (t3 < 10)
        {
            txtTime.text += "0" + t3;
        }
        else
        {
            txtTime.text += t3;
        }
    }

    void OnDestroy()
    {
        GameDataMgr.Instance.Reset();
        UIMgr.Instance.onlinePlayerPanel = null;
    }
}
