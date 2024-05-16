using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

//ʶ�����׵������޸��߼� ����Ҫ��ʷ��¼ Ҳû��Χ���߼�
public class RecognizechessBoard : MonoBehaviour
{
    private Transform LeftTop;
    private Transform RightBottom;

    Vector3 LTPos;
    Vector3 RBPos;

    public int size;

    int[,] board;
    Renderer[,] chesses;
    bool[,] transparent;

    float halfGridWidth = 1;
    float halfGridHeight = 1;
    int rowi = 0, coli = 0;

    public bool turn = false;//ʲô��ɫ����
    bool canFall = false;
    public bool deletemode = false;

    ChessManual chessmanual = new ChessManual();//���������Ϣ

    private bool canPlay = true;

    void Awake()
    {
        board = new int[size, size];
        transparent = new bool[size, size];
        chesses = new Renderer[size, size];

        Application.targetFrameRate = 20;
        chessmanual.setTime(System.DateTime.Now.ToString("f"));
        chessmanual.setSize(size);
        chessmanual.setRule("chinese");
        if (size != 19) chessmanual.setTiemu(6.5f);
        else chessmanual.setTiemu(7.5f);
        chessmanual.setBlackName("�ڷ�����");
        chessmanual.setBlackRank("15k");
        chessmanual.setWhiteName("�׷�����");
        chessmanual.setWhiteRank("15k");
    }

    // Start is called before the first frame update
    void Start()
    {
        Transform cb = this.transform;
        LeftTop = cb.Find("LeftTop");
        RightBottom = cb.Find("RightBottom");

        Transform row;
        for (int i = 0; i < size; ++i)
        {
            row = cb.Find($"row{i + 1}");
            for (int j = 0; j < size; ++j)
            {
                chesses[i, j] = row.Find($"Chess{i + 1}_{j + 1}").gameObject.GetComponent<Renderer>();
                chesses[i, j].material.color = new Color(chesses[i, j].material.color.r, chesses[i, j].material.color.g, chesses[i, j].material.color.b, 0);//һ��ʼ��͸������ʾ,Color�ǽṹ�壬��Ӱ��GC
            }
        }

        if (RecognizeImageMgr.Instance.cm != null)
        {
            InitChessManual(RecognizeImageMgr.Instance.cm);
            RecognizeImageMgr.Instance.cm = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIMgr.Instance.ShowPanel<RecognizePausePanel>();
            canPlay = false;
        }

        if (!canPlay) return;

        LTPos = Camera.main.WorldToScreenPoint(LeftTop.transform.position);
        RBPos = Camera.main.WorldToScreenPoint(RightBottom.transform.position);

        halfGridWidth = (RBPos.x - LTPos.x) / (size * 2 - 2);
        halfGridHeight = (LTPos.y - RBPos.y) / (size * 2 - 2);

        int colt = (int)((Input.mousePosition.x - LTPos.x) / halfGridWidth);
        int rowt = (int)((Input.mousePosition.y - RBPos.y) / halfGridHeight);
        coli = (colt + 1) / 2;
        rowi = (rowt + 1) / 2;
        if (colt >= 0 && rowt >= 0 && rowi >= 0 && rowi < size && coli >= 0 && coli < size)
        {
            if (board[rowi, coli] == 0)
            {
                transparent[rowi, coli] = true;
                canFall = true;
            }
            else if (board[rowi,coli]!=0 && deletemode)
            {
                canFall = true;
            }
            else
            {
                canFall = false;
            }
        }
        else
        {
            canFall = false;
        }

        if (canFall && Input.GetMouseButtonUp(0))
        {
            if(!deletemode)
            {
                board[rowi, coli] = turn ? 1 : 2;
            }
            else
            {
                board[rowi, coli] = 0;
            }
        }
        DrawBoard();
    }

    private void DrawBoard()
    {
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (board[i, j] == 0)
                {
                    chesses[i, j].material.color = new Color(chesses[i, j].material.color.r, chesses[i, j].material.color.g, chesses[i, j].material.color.b, 0f);
                    if (transparent[i, j] == true)
                    {
                        if (turn)
                            chesses[i, j].material.color = new Color(1, 1, 1, 0.5f);
                        else if (!turn)
                            chesses[i, j].material.color = new Color(0, 0, 0, 0.5f);
                        transparent[i, j] = false;
                    }
                }
                if (board[i, j] == 1)
                {
                    chesses[i, j].material.color = Color.white;
                }
                else if (board[i, j] == 2)
                {
                    chesses[i, j].material.color = Color.black;
                }
            }
        }
    }

    public void InitChessManual(ChessManual cm)
    {
        chessmanual = cm;
        if (chessmanual.getSize() != size)
        {
            print("���̴�С��ƥ��");
            return;
        }

        //��ȡ�µ����׼�¼������й���Ϣ
        chessmanual.setTime(System.DateTime.Now.ToString("f"));
        if (size != 19) chessmanual.setTiemu(6.5f);
        else chessmanual.setTiemu(7.5f);
        chessmanual.setBlackName(cm.getBlackName());
        chessmanual.setBlackRank(cm.getBlackRank());
        chessmanual.setWhiteName(cm.getWhiteName());
        chessmanual.setWhiteRank(cm.getWhiteRank());

        ClearBoard();
        //ʶ����������� ֻ�ǵ�������������
        List<Stone> stones = chessmanual.getPlayList();
        int col, row;
        foreach(Stone stone in stones)
        {
            row = stone.getY();
            col = stone.getX();
            board[row, col] = stone.getColor() ? 1 : 2;
        }
        DrawBoard();
    }

    public void SaveChessManual(string path)
    {
        //���֮ǰ��chessmanual stones
        chessmanual.getPlayList().Clear();

        //����board����stone
        int step = 0;
        Stone stone = new Stone();
        for(int i=0;i<size;i++)
        {
            for(int j=0;j<size;j++)
            {
                stone.setStep(step);
                stone.setY(i);
                stone.setX(j);
                if (board[i,j]==1)
                {
                    stone.setColor(true);
                    chessmanual.AddStone(stone);
                }
                else if (board[i,j]==2)
                {
                    stone.setColor(false);
                    chessmanual.AddStone(stone);
                }
            }
        }
        File.WriteAllText(path, ParseSgfHelper.Chessmanual2Sgf(chessmanual));
    }

    public void ClearBoard()
    {
        Array.Clear(board, 0, board.Length);
    }

    public void DelayCanPlay()
    {
        StartCoroutine("DelayPlay");
    }

    private IEnumerator DelayPlay()
    {
        yield return null;
        canPlay = true;
    }
}

