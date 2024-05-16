using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    public Button btnLogin;
    public Button btnRegister;
    public Toggle togRemPass;
    public InputField txtAccount;
    public InputField txtPassword;
    public Text txtTip;

    private bool canClickLogin = true;
    //�л��û����ܣ���Ҫ������������ĸ�
    private bool tempTip = false;
    private bool saveLoginData = false;
    private bool changeAccountInfo = false;
    private AccountInfo accountInfo=new AccountInfo();
    private bool showPanels = false;
    private LoginData loginData; 

    protected override void Init()
    {     
        txtTip.text = "";

        //��ȡ���ش洢������ �������
        loginData = GameDataMgr.Instance.GetLoginData();
        togRemPass.isOn = loginData.rememberPw;
        txtAccount.text = loginData.account;
        if (loginData.rememberPw == true)
            txtPassword.text = loginData.password;

        btnLogin.onClick.AddListener(() =>
        {
            if (canClickLogin == false) return;
            canClickLogin = false;
            //�ж��˻����볤���Ƿ���ȷ
            if (txtAccount.text.Length <= 20 && txtPassword.text.Length <= 20 && txtAccount.text.Length >= 5 && txtPassword.text.Length >= 5)
            {
                //�����������¼��Ϣ �ж��˺���Ϣ�Ƿ���ȷ
                LoginMsg msg = new LoginMsg();
                msg.accountInfo = new AccountInfo(txtAccount.text, txtPassword.text);
                ClientAsyncNet.Instance.Send(msg);
            }
            else
            {
                txtTip.text = "�˺Ż��������";
                canClickLogin = true;
            }
        });

        btnRegister.onClick.AddListener(() =>
        {
            //��ʾע�����
            UIMgr.Instance.ShowPanel<RegisterPanel>();
            UIMgr.Instance.HidePanel<LoginPanel>();
        });

        txtAccount.onValueChanged.AddListener((str) =>
        {
            txtTip.text = "";
        });

        txtPassword.onValueChanged.AddListener((str) =>
        {
            txtTip.text = "";
        });

        togRemPass.onValueChanged.AddListener((ison) =>
        {
            loginData.rememberPw = ison;
            GameDataMgr.Instance.SaveLoginData();
        });

        updateActions += () =>
        {
            if (tempTip)
            {
                txtTip.text = "�˺Ż��������";
                tempTip = false;
            }
            if (saveLoginData == true)
            {
                //���ؼ�¼��Ϣ�������¼
                loginData.account = txtAccount.text;
                loginData.password = txtPassword.text;
                GameDataMgr.Instance.SaveLoginData();
                saveLoginData = false;
            }
            if(changeAccountInfo)
            {
                txtAccount.text = accountInfo.account;
                txtPassword.text=accountInfo.password;
                changeAccountInfo = false;
            }
            if(showPanels)
            {
                UIMgr.Instance.ShowPanel<MainPanel>();
                UIMgr.Instance.ShowPanel<PlayPanel>();
                UIMgr.Instance.HidePanel<LoginPanel>(false);
                showPanels = false;
            }
        };
    }

    //���ݷ�����������Ϣ�� bool �ж��Ƿ��½�ɹ�
    public void LoginIn(bool can)
    {
        if (can)
        {
            showPanels = true;
            saveLoginData = true;
        }
        else
        {
            tempTip = true;
        }
        canClickLogin = true;
    }

    public void ChangeInfo(string _account,string _password)
    {
        changeAccountInfo = true;
        accountInfo.account = _account;
        accountInfo.password = _password;
    }
}
