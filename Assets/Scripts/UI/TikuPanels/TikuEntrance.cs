using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//��ⰴť��ڽű�
public class TikuEntrance : MonoBehaviour
{
    public Button btn;
    public Text bottomText;
    public Text topText;
    private string LibraryName;
    private string LibraryPath;

    public void InitEntrance(string librarypath)
    {
        LibraryPath = librarypath;
        LibraryName= Path.GetFileName(librarypath);
    }

    // Start is called before the first frame update
    void Start()
    {
        bottomText.text = LibraryName;

        if (!TikuMgr.Instance.tikuDic.ContainsKey(LibraryName))
        {
            TikuMgr.Instance.tikuDic.Add(LibraryName, new List<GameTree>());
        }

        btn.onClick.AddListener(() =>
        {
            TikuMgr.Instance.curTiku = LibraryName;
            if(TikuMgr.Instance.tikuDic[LibraryName].Count != 0)//�ٴν���
            {
                //donothing
            }
            else
            {
                //��������
                string[] files = Directory.GetFiles(LibraryPath, "*.sgf");
                for (int i = 0; i < files.Length; ++i)
                {
                    TikuMgr.Instance.tikuDic[LibraryName].Add(new GameTree(ParseSgfHelper.GetContent(files[i])));
                }
            }
            UIMgr.Instance.HideAll();
            //���볡��
            SceneManager.LoadScene("TikuGameScene");
        });
    }
}
