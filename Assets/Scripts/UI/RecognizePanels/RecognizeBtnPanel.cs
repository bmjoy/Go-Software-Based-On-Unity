using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecognizeBtnPanel : MonoBehaviour
{
    public Button btnDelete;
    public Button btnBlack;
    public Button btnWhite;
    public Button btnEmpty;
    public Button btnRecognize;
    public Button btnExport;

    private RecognizechessBoard board;

    void Start()
    {
        board = RecognizeImageMgr.Instance.GetCurrentBoard();
        btnDelete.onClick.AddListener(() =>
        {
            board.deletemode = true;
        });

        btnBlack.onClick.AddListener(() =>
        {
            board.deletemode = false;
            board.turn = false;
        });

        btnWhite.onClick.AddListener(() =>
        {
            board.deletemode = false;
            board.turn = true;
        });

        btnEmpty.onClick.AddListener(() =>
        {
            board.ClearBoard();
        });

        btnRecognize.onClick.AddListener(() =>
        {
            board.ClearBoard();

            string image_path = FileOperationByWin32.Get_OpeningImageFilePath();
            if (image_path == null || image_path == "") return;

            string output_sgf_path = Application.streamingAssetsPath;
            output_sgf_path += '/' + image_path.Split('\\').Last().Split('.').First() + ".sgf";
            Debug.Log(output_sgf_path);

            GoRecognitionHelper goRecognitionHelper = new GoRecognitionHelper();

            Task<bool> t1 = new Task<bool>(() =>
            {
                return goRecognitionHelper.StartRecognize(image_path, output_sgf_path);
            });
            t1.Start();

            if (t1.Result == true)//���������
            {
                if (File.Exists(output_sgf_path) == false) { Debug.Log("ʶ��ʧ��"); UIMgr.Instance.ShowPanel<TipPanel>().InitText("ͼƬʶ��ʧ��"); return; }
                ChessManual cm = ParseSgfHelper.Sgf2Chessmanual(output_sgf_path);
                board.InitChessManual(cm);
            }
            else
            {
                //��ʾʶ��ʧ����ʾ���
                UIMgr.Instance.ShowPanel<TipPanel>().InitText("ͼƬʶ��ʧ��");
                Debug.Log("ʶ��ʧ��");
            }
        });

        btnExport.onClick.AddListener(() =>
        {
            string path = FileOperationByWin32.Get_SavingSgfFilePath();
            if(path!=null && path!="")
                board.SaveChessManual(path);
        });
    }
}
