using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GameDataMgr : BaseManager<GameDataMgr>
{
    //��¼���
    private LoginData loginData;
    private UserInfo localUserInfo;
    private UserInfo opponentUserInfo;

    //�����ս���
    public bool isWhite;//�����ս�У����ػ������ִʲô��
    public bool turn = false;//�غ� true ������ false �����£�  ���� ��ʼ��Ϊtrue�������У�ͨ������������� ����Ĭ������Ϊ������
    private int[] chessPos = new int[2] { -1, -1 };

    public int eatenCount1;//�׷�������
    public int eatenCount2;//�ڷ�������

    public GameResult gameResult = null;

    //��ȡ���һ���û��ĵ�¼��Ϣ
    public LoginData GetLoginData()
    {
        if (loginData != null) return loginData;
        loginData = JsonMgr.Instance.LoadData<LoginData>("LoginData");
        return loginData;
    }

    //��ȡĳ���û��ĵ�¼��Ϣ
    public LoginData GetLoginData(string fileName)
    {
        LoginData data = JsonMgr.Instance.LoadData<LoginData>(fileName);
        return data;
    }

    public void SaveLoginData()
    {
        if (loginData == null) return;
        JsonMgr.Instance.SaveData(loginData, "LoginData");
    }

    public void SaveLoginData(LoginData data)
    {
        loginData = data;
        JsonMgr.Instance.SaveData(loginData, "LoginData");
    }

    public void ClearLoginData()
    {
        loginData.rememberPw = false;
    }

    public void NotifyServerUpdateUserInfo()
    {
        UserInfoMsg msg=new UserInfoMsg();
        msg.userInfo = localUserInfo;
        ClientAsyncNet.Instance.Send(msg);
    }

    public void SaveLocalUserInfo(UserInfo data)
    {
        localUserInfo = data;
    }

    public UserInfo GetLocalUserInfo()
    {
        return localUserInfo;
    }

    public void SaveOpponentUserInfo(UserInfo data)
    {
        opponentUserInfo = data;
    }

    public UserInfo GetOpponentUserInfo()
    {
        return opponentUserInfo;
    }

    public void SetChessPos(int x, int y)
    {
        chessPos[0] = x;
        chessPos[1] = y;
    }

    public int[] GetChessPos()
    {
        int[] t = new int[2] { chessPos[0], chessPos[1] };
        chessPos[0] = -1;
        chessPos[1] = -1;
        return t;
    }

    public void Reset()
    {
        eatenCount1 = 0;
        eatenCount2 = 0;

        gameResult = null;
        opponentUserInfo = null;
        chessPos = new int[] { -1, -1 };
    }
}
