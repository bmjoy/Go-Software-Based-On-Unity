using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;

public class BaipuchessBoard : MonoBehaviour
{
    private Transform LeftTop;
    private Transform RightBottom;

    Vector3 LTPos;
    Vector3 RBPos;

    public int size;

    int[,] board;
    List<int[,]> board_history = new List<int[,]>();//������ʷ״̬�����ڲ������ٸ���
    Renderer[,] chesses;
    bool[,] transparent;

    float halfGridWidth = 1;
    float halfGridHeight = 1;
    int rowi = 0, coli = 0;
    [HideInInspector]
    public bool isWhite = false;
    [HideInInspector]
    public bool turn = true;//�Ƿ�������
    bool canFall = false;
    bool[,] visited;
    List<Tuple<int, int>> eatenChesses = new List<Tuple<int, int>>();
    List<Tuple<int, int>> tempCheeses = new List<Tuple<int, int>>();
    Tuple<int, int> jie = new Tuple<int, int>(-1, -1);
    bool prevJie = false;

    ChessManual chessmanual = new ChessManual();//���������Ϣ
    int curstep = 0;//��ǰ���˵ڼ���
    int curshow = 0;//��ǰ��ʾ�����Ĳ�������
    List<Stone> stones = new List<Stone>();//��¼������ӣ�����getplaylist����

    public BaipuInfoPanel BaipuInfo;//�漰���ĸ������Զ���

    private bool canPlay = true;

    TextStep textStep = null;

    void Awake()
    {
        board = new int[size, size];
        transparent = new bool[size, size];
        visited = new bool[size, size];
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
        board_history.Add((int[,])board.Clone());
        if(BaiPuMgr.Instance.cm!=null)
        {
            InitChessManual(BaiPuMgr.Instance.cm);
            BaiPuMgr.Instance.cm = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIMgr.Instance.ShowPanel<BaipuPausePanel>();
            canPlay = false;
        }

        if (!canPlay) return;

        LTPos = Camera.main.WorldToScreenPoint(LeftTop.transform.position);
        RBPos = Camera.main.WorldToScreenPoint(RightBottom.transform.position);

        halfGridWidth = (RBPos.x - LTPos.x) / (size * 2 - 2);
        halfGridHeight = (LTPos.y - RBPos.y) / (size * 2 - 2);

        //ȷ��������λ�õ�������coli,������rowi
        int colt = (int)((Input.mousePosition.x - LTPos.x) / halfGridWidth);
        int rowt = (int)((Input.mousePosition.y - RBPos.y) / halfGridHeight);
        coli = (colt + 1) / 2;
        rowi = (rowt + 1) / 2;
        if (colt >= 0 && rowt >= 0 && rowi >= 0 && rowi < size && coli >= 0 && coli < size)//�ڹ涨��Χ�ڲ�������
        {
            if (board[rowi, coli] == 0)
            {
                transparent[rowi, coli] = true;
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
            if(FallChess(rowi, coli, isWhite ? 1 : 2))
            {
                MusicMgr.Instance.PlaySound("����", false);
                //�ж��Ƿ����ʷ��ֿ�ʼ���壬������Ҫ����
                if (curshow != curstep && BaipuItem.isClick == true)
                {
                    BaipuInfo.RemoveAfter(curshow);//����UI
                    board_history.RemoveRange(curshow + 1, board_history.Count - curshow - 1);//�Ƴ���ʷ���
                    stones.RemoveRange(curshow, stones.Count - curshow);
                    
                    curstep = curshow;
                    BaipuItem.isClick = false;
                }

                ++curstep;
                curshow = curstep;
                Stone newstone = new Stone(isWhite, coli, rowi, curstep);
                stones.Add(newstone);
                ShowTextObj(curshow, !isWhite, chesses[rowi, coli].transform.position - new Vector3(0, 0, 0.2f));

                board_history.Add(board);//�������µ���ֵ���ʷ���
                board = (int[,])board.Clone();//ע��Ҫ�����������һ�����޸�ͬһ���ڴ�����
                
                //�������
                GameObject item = Instantiate(Resources.Load<GameObject>("BaipuItem"));
                item.GetComponent<BaipuItem>().InitInfo(newstone);
                BaipuInfo.AddItem(item);

                if(turn)
                {
                    isWhite = !isWhite;
                }    
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
                        if (isWhite)
                            chesses[i, j].material.color = new Color(1, 1, 1, 0.5f);
                        else if (!isWhite)
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

    protected void ShowTextObj(int _step, bool _whitecolor, Vector3 _position)
    {
        if (textStep == null || textStep.gameObject.activeSelf == false)
            textStep = PoolMgr.Instance.GetObj("Text/3dStep").GetComponent<TextStep>();
        textStep.InitText(_step, _whitecolor, _position, size == 19 ? 0.8f : (size == 13 ? 0.9f : 1f));
    }

    public void ShowTextObj(Stone s)
    {
        if (textStep == null || textStep.gameObject.activeSelf == false)
            textStep = PoolMgr.Instance.GetObj("Text/3dStep").GetComponent<TextStep>();
        textStep.InitText(s.getStep(), !s.getColor(), chesses[s.getY(), s.getX()].transform.position - new Vector3(0, 0, 0.2f), size == 19 ? 0.8f : (size == 13 ? 0.9f : 1f));
    }

    public void SetBoard(int i)
    {
        curshow = i;
        board = (int[,])board_history[i].Clone();//һ��Ҫ�����������޸���ʷ���
        DrawBoard();
        if(i==0 && textStep!=null)
            PoolMgr.Instance.PushObj("Text/3dStep", textStep.gameObject);
    }

    public void SetLatestBoard()
    {
        curshow = board_history.Count - 1;
        board = (int[,])board_history[curshow].Clone();
        DrawBoard();
    }

    //�������� �Լ� ������ϢUI����������
    public void ResetBoard()
    {
        BaipuInfo.RemoveAfter(0);
        board_history.RemoveRange(1, board_history.Count - 1);
        curstep = 0;
        BaipuItem.isClick = false;

        SetBoard(0);
    }

    public void InitChessManual(ChessManual cm)
    {
        chessmanual = cm;
        ResetBoard();
        if (chessmanual.getSize()!= size)
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

        stones = chessmanual.getPlayList();
        int x,y;
        foreach (Stone stone in stones)
        {
            x = stone.getX();
            y = stone.getY();
            FallChess(y, x, stone.getColor() ? 1 : 2);//����
            board_history.Add(board);
            board = (int[,])board.Clone();
            ++curstep;
            curshow = curstep;
            GameObject item = Instantiate(Resources.Load<GameObject>("BaipuItem"));
            item.GetComponent<BaipuItem>().InitInfo(stone);
            BaipuInfo.AddItem(item); 
        }
        DrawBoard();
        ShowTextObj(stones[stones.Count-1]);
    }

    public void SaveChessManual(string path)
    {
        chessmanual.setPlayList(stones);
        File.WriteAllText(path, ParseSgfHelper.Chessmanual2Sgf(chessmanual));
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

    #region Χ�屾�����Ϸ�߼�
    //�ж��Ƿ�����
    bool hasAir(int i, int j, int type)
    {
        if (board[i, j] == 0) return true;//Ϊ������
        if (board[i, j] != type) return false;//��Ϊ�����෴����
        //ͬɫ��������

        visited[i, j] = true;
        tempCheeses.Add(new Tuple<int, int>(i, j));
        if (j < size-1 && !visited[i, j + 1] && hasAir(i, j + 1, type))
        {
            return true;
        }
        if (i > 0 && !visited[i - 1, j] && hasAir(i - 1, j, type))
        {
            return true;
        }
        if (j > 0 && !visited[i, j - 1] && hasAir(i, j - 1, type))
        {
            return true;
        }
        if (i < size-1 && !visited[i + 1, j] && hasAir(i + 1, j, type))
        {
            return true;
        }
        return false;
    }

    void ResetVisited()
    {
        for (int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                visited[i, j] = false;
    }

    void eatChesses(out int cnt, out int[] firstEatenChess)
    {
        cnt = 0;
        firstEatenChess = new int[2] { -1, -1 };//��¼��һ�����Ե��ӵ�λ��
        foreach (var item in eatenChesses)
        {
            cnt++;
            board[item.Item1, item.Item2] = 0;
            if (cnt == 1)
            {
                firstEatenChess[0] = item.Item1;
                firstEatenChess[1] = item.Item2;
            }
        }
        eatenChesses.Clear();
    }

    bool FallChess(int i, int j, int type)
    {
        board[i, j] = type;//ֱ�����ӣ����ж��Լ�����������Լ��Է������������������ǽ������Ȼỹԭ

        bool self_hasAir = hasAir(i, j, type);
        tempCheeses.Clear();
        ResetVisited();
        int opposite_type = (type == 1 ? 2 : 1);
        bool other_hasAir = true;//�ж� �Ƿ�������������Ķ����嶼���� һ����һ��û�о���Ϊfalse
        bool playmusic = true;
        int eatcount = 0;

        if (j < size-1 && !visited[i, j + 1] && board[i, j + 1] == opposite_type)
        {
            if (hasAir(i, j + 1, opposite_type) == false)//û��
            {
                other_hasAir = false;
                foreach (var item in tempCheeses)
                {
                    eatenChesses.Add(item);
                }
            }
            tempCheeses.Clear();//һ��Ҫд���棬��hasAir��Ҫ��
            ResetVisited();//visited����Ҳһ��,��Ϊ�ҵ���ȱ����������꣬�����ҵ��пս�����
        }

        if (i > 0 && !visited[i - 1, j] && board[i - 1, j] == opposite_type)
        {
            if (hasAir(i - 1, j, opposite_type) == false)
            {
                other_hasAir = false;
                foreach (var item in tempCheeses)
                {
                    eatenChesses.Add(item);
                }
            }
            tempCheeses.Clear();
            ResetVisited();
        }

        if (j > 0 && !visited[i, j - 1] && board[i, j - 1] == opposite_type)
        {
            if (hasAir(i, j - 1, opposite_type) == false)
            {
                other_hasAir = false;
                foreach (var item in tempCheeses)
                {
                    eatenChesses.Add(item);
                }
            }
            tempCheeses.Clear();
            ResetVisited();
        }

        if (i < size-1 && !visited[i + 1, j] && board[i + 1, j] == opposite_type)
        {
            if (hasAir(i + 1, j, opposite_type) == false)
            {
                other_hasAir = false;
                foreach (var item in tempCheeses)
                {
                    eatenChesses.Add(item);
                }
            }
            tempCheeses.Clear();
            ResetVisited();
        }

        eatChesses(out eatcount, out int[] eatenChess);
        if (eatcount == 1)
        {
            //�жϵ�ǰ�Ե��ӵ��ǲ����ϴν����ڵ�λ��
            if (prevJie == true && eatenChess[0] == jie.Item1 && eatenChess[1] == jie.Item2)
            {
                //�ǣ������£����һ�ԭ���Ե���
                board[i, j] = 0;//������
                board[eatenChess[0], eatenChess[1]] = opposite_type;//��ԭ���Ե���
                playmusic = false;
                print("ͬһ���ٴ�ٲ���ѭ��!");
            }
            else
            {
                //���Ǵ�ͬһ���٣��������������ӣ�Ҳ�п��ܴ������һ����
                jie = new Tuple<int, int>(i, j);//ֻ��1�ӣ������ǽ٣���¼λ��
                prevJie = true;
            }
        }
        else
        {
            //û���ӣ����˴���һ�ӵ����������� û���
            prevJie = false;
        }

        if (self_hasAir == false && other_hasAir == true)//������! ��ԭ��
        {
            board[i, j] = 0;
            playmusic = false;
            print("������Ŷ!");
        }

        if (playmusic == true)
        {
            return true;
        }
        return false;
    }
    #endregion
}
