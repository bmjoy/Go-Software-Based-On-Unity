using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    public Button btnCancel;
    public Button btnSure;
    public InputField txtAccount;
    public InputField txtPassword;
    public InputField txtName;
    public Text txtAccountTip;
    public Text txtPasswordTip;
    public Text txtNameTip;

    private string tempstr="";//ע�ⲻ��ʼ����Ϊnull�ģ�=""��ζ��str������һ��ʵ�ʴ��ڵĿ��ַ�������
    private bool bAcc,bPas,bName;
    private bool canClickSure = true;
    private bool saveLoginData = false;
    private bool showPanels = false;
    protected override void Init()
    {
        txtAccountTip.text = "";
        txtPasswordTip.text = "";
        txtNameTip.text = "";

        btnCancel.onClick.AddListener(() =>
        {
            UIMgr.Instance.ShowPanel<LoginPanel>();
            UIMgr.Instance.HidePanel<RegisterPanel>();
        });

        btnSure.onClick.AddListener(() =>
        {
            if (canClickSure == false) return;
            if( bAcc && bPas && bName)
            {
                //����ע����Ϣ
                RegisterMsg msg = new RegisterMsg();
                msg.accountInfo = new AccountInfo(txtAccount.text, txtPassword.text);
                msg.userName = txtName.text;
                ClientAsyncNet.Instance.Send(msg);
                print("����ע����");
            }
            canClickSure = false;
        });

        txtAccount.onEndEdit.AddListener((str) =>
        {
            if (str.Length > 20)
            {
                txtAccountTip.text = "�˺ų���20��������";
                bAcc = false;
            }
            else if(str.Length<5)
            {
                txtAccountTip.text = "�˺ų��ȹ���";
                bAcc = false;
            }
            else
            {
                txtAccountTip.text = "";
                bAcc = true;
            }
        });

        txtPassword.onEndEdit.AddListener((str) =>
        {
            if (str.Length > 20)
            {
                txtPasswordTip.text = "���볬��20��������";
                bPas = false;
            }
            else if(str.Length <5)
            {
                txtPasswordTip.text = "���볤�ȹ���";
                bPas = false;
            }
            else
            {
                txtPasswordTip.text = "";
                bPas = true;
            }   
        });

        txtName.onEndEdit.AddListener((str) =>
        {
            if (str.Length > 10)
            {
                bName = false;
                txtNameTip.text = "�û�������10��������";
            }
            else if(str.Length != 0)
            {
                bName = true;
                txtNameTip.text = "";
            } 
        });

        updateActions += () =>
        {
            if (tempstr != "")
            {
                txtNameTip.text = "�û����Ѵ���";
                tempstr = "";
            }
            if(saveLoginData==true)
            {
                //���ؼ�¼��Ϣ�������¼
                LoginData data = new LoginData();
                data.account = txtAccount.text;
                data.password = txtPassword.text;
                data.rememberPw = false;
                GameDataMgr.Instance.SaveLoginData(data);
                saveLoginData = false;
            }
            if(showPanels)
            {
                //�������
                UIMgr.Instance.ShowPanel<LoginPanel>();
                UIMgr.Instance.GetPanel<LoginPanel>().ChangeInfo(txtAccount.text, txtPassword.text);
                UIMgr.Instance.HidePanel<RegisterPanel>();
                showPanels = false;
            }
        };
    }

    //����ģ��������
    public void TryRegister(bool can)
    {
        //���Ϊtrue��ע��ɹ��������¼���
        if (can==true)
        {
            saveLoginData = true;
            showPanels = true;
        }
        //���Ϊfalse �û����������޸���ʾ
        else
        {
            tempstr = "a";
        }
        canClickSure = true;
    }
}
