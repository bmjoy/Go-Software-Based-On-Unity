using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LocalchessBoard : MonoBehaviour
{
    //ê��λ�ã����ڼ����������
    private Transform LeftTop;
    private Transform RightBottom;

    //ê������Ļ�ϵ�ӳ��λ��
    Vector3 LTPos;
    Vector3 RBPos;

    public int size;//�༭���Ｔ���������̳ߴ�

    int[,] board;//��¼�����������
    Renderer[,] chesses;//size��size����������
    bool[,] transparent;//͸���ȣ���board�ֿ�
    List<int[,]> board_history = new List<int[,]>();

    float halfGridWidth = 1; //���������ȵ�һ��
    float halfGridHeight = 1; //��������߶ȵ�һ��
    int rowi = 0, coli = 0;
    bool isWhite = false;//���壬��������
    bool canFall = false;//�������£��Ƿ�������
    bool[,] visited;//���ʼ�������
    List<Tuple<int, int>> eatenChesses = new List<Tuple<int, int>>();
    List<Tuple<int, int>> tempCheeses = new List<Tuple<int, int>>();
    Tuple<int, int> jie = new Tuple<int, int>(-1, -1);//��¼���µ�һ���ٵ�λ�ã�����н��ǻ��кܶ����
    bool prevJie = false;//�ϴ����ӿ����ǽ���

    //��ּ�¼
    ChessManual chessmanual = new ChessManual();
    List<Stone> stones = new List<Stone>();
    int step = 0;

    private bool canplay = true;
    private bool prevStopStep = false;

    Katago katago = KatagoHelper.katago;

    TextStep textStep = null;

    void Awake()
    {
        board = new int[size, size];
        chesses = new Renderer[size, size];
        transparent = new bool[size, size];
        visited = new bool[size, size];
        board_history.Add((int[,])board.Clone());

        chessmanual.setTime(System.DateTime.Now.ToString("f"));
        if (size != 19) chessmanual.setTiemu(6.5f);
        else chessmanual.setTiemu(7.5f);
        chessmanual.setBlackName("�ڷ�����");
        chessmanual.setBlackRank("15k");
        chessmanual.setWhiteName("�׷�����");
        chessmanual.setWhiteRank("15k");
        chessmanual.setRule("chinese");
        chessmanual.setSize(size);
    }

    void Start()
    {
        Transform cb = this.transform;
        LeftTop = cb.Find("LeftTop");
        RightBottom = cb.Find("RightBottom");

        Transform row;
        for (int i=0;i<size;++i)
        {
            row = cb.Find($"row{i + 1}");
            for (int j=0;j<size;++j)
            {
                chesses[i, j] = row.Find($"Chess{i + 1}_{j + 1}").gameObject.GetComponent<Renderer>();
                chesses[i, j].material.color = new Color(chesses[i, j].material.color.r, chesses[i, j].material.color.g, chesses[i, j].material.color.b, 0);//һ��ʼ��͸������ʾ,Color�ǽṹ�壬��Ӱ��GC
            }
        }

        Task.Run(() =>
        {
            if (size != 19)
            {
                katago.SetKomi(6.5f);
            }
            else
            {
                katago.SetKomi(7.5f);
            }
            katago.ChangeBoardSize(size);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!canplay) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIMgr.Instance.ShowPanel<LocalPausePanel>();
            canplay = false;
            return;
        }
        if(Input.GetMouseButtonUp(1))
        {
            HuiQi();
        }

        ///
        /// ê��λ�ú������Ȼ��ֱܷ���Ӱ�죬���Է���update��Ż��취����Ļ�ֱ��ʸı�ʱ���ı���Щֵ����ʱ����֪����ô��
        ///
        //����ê��λ��
        LTPos = Camera.main.WorldToScreenPoint(LeftTop.transform.position);
        RBPos = Camera.main.WorldToScreenPoint(RightBottom.transform.position);

        //����������
        halfGridWidth = (RBPos.x - LTPos.x) / (size * 2 - 2);//������Ը�����*2
        halfGridHeight = (LTPos.y - RBPos.y) / (size * 2 - 2);

        //ȷ��������λ�õ�������coli,������rowi
        int colt = (int)((Input.mousePosition.x - LTPos.x) / halfGridWidth);
        int rowt = (int)((Input.mousePosition.y - RBPos.y) / halfGridHeight);
        coli = (colt + 1) / 2;
        rowi = (rowt + 1) / 2;
        if(colt>=0 && rowt>=0 && rowi >= 0 && rowi < size && coli >= 0 && coli < size)//�ڹ涨��Χ�ڲ�������
        { 
            if(board[rowi, coli] == 0)//ֻ�пձ�־�²������� ��ʾ��͸��
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
            canFall=false;
        }

        if(canFall && Input.GetMouseButtonUp(0))
        {
            if(FallChess(rowi, coli, isWhite ? 1 : 2))
            {
                board_history.Add((int[,])board.Clone());
                step++;
                stones.Add(new Stone(isWhite, coli, rowi, step));
                ShowTextObj(step, !isWhite, chesses[rowi, coli].transform.position - new Vector3(0, 0, 0.2f));
                isWhite = !isWhite;
                prevStopStep = false;
            }
        }

        DrawBoard();
    }

    public void DelayCanPlay()
    {
        StartCoroutine("DelayPlay");
    }

    private IEnumerator DelayPlay()
    {
        yield return null;
        canplay = true;
    }

    public void SetBoard(int i)
    {
        board = (int[,])board_history[i].Clone();
        board_history.RemoveRange(i + 1, board_history.Count - i - 1);
        stones.RemoveRange(i, stones.Count - i);
        step = i;
        DelayCanPlay();
        DrawBoard();
    }

    //����
    public void HuiQi()
    {
        if (step == 0)
        {
            DelayCanPlay();
            return;
        }
        else
        {
            isWhite = !isWhite;
            SetBoard(step - 1);
            if (step > 0)
                ShowTextObj(step, !stones[step - 1].getColor(), chesses[stones[step - 1].getY(), stones[step - 1].getX()].transform.position - new Vector3(0, 0, 0.2f));
            else
                PoolMgr.Instance.PushObj("Text/3dStep", textStep.gameObject);
        }
    }

    //���¿�ʼ
    public void ResetBoard()
    {
        stones.Clear();
        isWhite = false;
        step = 0;
        PoolMgr.Instance.PushObj("Text/3dStep", textStep.gameObject);
        SetBoard(0);
    }

    public void StopOneStep()
    {
        if(prevStopStep)
        {
            //˫����̶�ͣһ�֣�����Ŀ������ʾ�������
            canplay = false;
            ParseSgfHelper.SaveContent(Application.streamingAssetsPath + "/LocalMatchPath/match.sgf", chessmanual);

            katago.LoadSgf(Application.streamingAssetsPath + "/LocalMatchPath/match.sgf");

            GameResult result = katago.Get_Final_Score(); 
            bool winnerIsWhite = result.winner == 'W' ? true : false;
            GameResultPanel gpanel = UIMgr.Instance.ShowPanel<GameResultPanel>();
            gpanel.action = () =>
            {
                UIMgr.Instance.ShowPanel<LocalPausePanel>();
            };
            gpanel.ChangeTxtInfo(winnerIsWhite == true ? "�׷�ʤ" : "�ڷ�ʤ", (winnerIsWhite == true ? "�׷�����" : "�ڷ�����") + result.reason + "Ŀ");

            chessmanual.setResult(winnerIsWhite == true ? "W" : "B" + "+" + result.reason);
            print(winnerIsWhite == true ? "W" : "B" + "+" + result.reason);
            ParseSgfHelper.SaveContent(Application.streamingAssetsPath + "/LocalMatchPath/match.sgf", chessmanual);
        }
        else
        {
            prevStopStep = true;
            isWhite = !isWhite;
            DelayCanPlay();
        }
    }

    protected void ShowTextObj(int _step, bool _whitecolor, Vector3 _position)
    {
        if (textStep == null || textStep.gameObject.activeSelf == false)
            textStep = PoolMgr.Instance.GetObj("Text/3dStep").GetComponent<TextStep>();
        textStep.InitText(_step, _whitecolor, _position, size == 19 ? 0.8f : (size == 13 ? 0.9f : 1f));
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

    #region Χ����Ϸ�߼�
    //�ж��Ƿ�����
    bool hasAir(int i,int j,int type)
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
        for(int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                visited[i, j] = false;
    }

    void eatChesses(out int cnt, out int[] firstEatenChess)
    {
        cnt = 0;
        firstEatenChess = new int[2] { -1, -1 };//��¼��һ�����Ե��ӵ�λ��
        foreach(var item in eatenChesses)
        {
            cnt++;
            board[item.Item1, item.Item2] = 0;
            if(cnt==1)
            {
                firstEatenChess[0] = item.Item1;
                firstEatenChess[1] = item.Item2;
            }
        }
        eatenChesses.Clear();
    }

    bool FallChess(int i,int j,int type)
    {
        board[i, j] = type;//ֱ�����ӣ����ж��Լ�����������Լ��Է������������������ǽ������Ȼỹԭ

        bool self_hasAir = hasAir(i, j, type);
        tempCheeses.Clear();
        ResetVisited();
        int opposite_type = (type == 1 ? 2 : 1);
        bool other_hasAir = true;//�ж� �Ƿ�������������Ķ����嶼���� һ����һ��û�о���Ϊfalse
        bool playmusic = true;
        int eatcount = 0;

        if (j<size-1 && !visited[i,j+1] && board[i,j+1] == opposite_type)
        {
            if (hasAir(i, j + 1, opposite_type) == false)//û��
            {
                other_hasAir = false;
                foreach(var item in tempCheeses)
                {
                    eatenChesses.Add(item);
                }
            }
            tempCheeses.Clear();//һ��Ҫд���棬��hasAir��Ҫ��
            ResetVisited();//visited����Ҳһ��,��Ϊ�ҵ���ȱ����������꣬�����ҵ��пս�����
        }

        if (i>0 && !visited[i-1,j] && board[i-1, j] == opposite_type)
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

        if (j>0 && !visited[i,j-1] && board[i,j-1] == opposite_type)
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

        if (i<size-1 && !visited[i+1,j] && board[i+1,j] == opposite_type)
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

        eatChesses(out eatcount,out int[] eatenChess);
        if (eatcount==1)
        {
            //�жϵ�ǰ�Ե��ӵ��ǲ����ϴν����ڵ�λ��
            if (prevJie==true && eatenChess[0]==jie.Item1 && eatenChess[1]==jie.Item2)
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
        
        if (self_hasAir==false && other_hasAir==true)//������! ��ԭ��
        {
            board[i, j] = 0;
            playmusic = false;
            print("������Ŷ!");
        }

        if(playmusic==true)
        {
            chessmanual.AddStone(new Stone(isWhite, j, i, step+1));
            MusicMgr.Instance.PlaySound("����", false);
            return true;
        }
        return false;
    }
    #endregion
}