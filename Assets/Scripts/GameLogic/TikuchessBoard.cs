using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikuchessBoard : MonoBehaviour
{
    private Transform LeftTop;
    private Transform RightBottom;

    Vector3 LTPos;
    Vector3 RBPos;

    //[IntOptions(9, 13, 19)]
    public int size;//Ū�����Ի�������!!! ��ʱ��֪��ԭ��

    int[,] board;
    List<int[,]> board_history = new List<int[,]>();//������ʷ״̬�����ڲ������ٸ��£�0�±�Ϊ��ʼ����
    Renderer[,] chesses;
    bool[,] transparent;

    float halfGridWidth = 1;
    float halfGridHeight = 1;
    int rowi = 0, coli = 0;
    [HideInInspector]
    public bool isWhite = false;
    bool canFall = false;
    bool[,] visited;
    List<Tuple<int, int>> eatenChesses = new List<Tuple<int, int>>();
    List<Tuple<int, int>> tempCheeses = new List<Tuple<int, int>>();
    Tuple<int, int> jie = new Tuple<int, int>(-1, -1);
    bool prevJie = false;

    private List<Stone> placeStones;
    List<List<Stone>> answers;
    GameNode root;
    GameNode curnode;
    private List<List<int[,]>> correctBoards = new List<List<int[,]>>();
    private int[,] tboard;//��¼��һ������״̬
    bool isTry = false;//�Ƿ�������
    private List<Stone> stones = new List<Stone>();//��¼Ҫ��ʾ�Ĳ���
    private int curstep = 0;
    List<GameObject> textStepObjs = new List<GameObject>();//�����ı�����

    bool waitPlay = false;//�ȴ��Զ�����
    bool canPlay = true;

    private void OnEnable()
    {
        //��������SetQuestionִ�� ���еĻ���Э�̴���setquestion
        if (board == null) board = new int[size, size];
        if (transparent == null) transparent = new bool[size, size];
        if (visited == null) visited = new bool[size, size];
        
        waitPlay = false;
        isTry = false;
        TikuMgr.Instance.GetTikuInfoPanel().ResetTry();

        canPlay = true;
    }

    private void OnDisable()
    {
        canPlay = false;
        prevJie = false;

        //ʧ����ζ�Ż��⻻�˲�ͬ��С�����̣�����
        Array.Clear(board, 0, board.Length);
        Array.Clear(transparent, 0, transparent.Length);
        Array.Clear(visited, 0, visited.Length);
        stones.Clear();
        board_history.Clear();
        tboard = null;
        correctBoards.Clear();
        root = null;
        curnode = null;
        waitPlay = false; 
    }

    void Awake()
    {
        chesses = new Renderer[size, size];

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
                chesses[i, j].material.color = new Color(chesses[i, j].material.color.r, chesses[i, j].material.color.g, chesses[i, j].material.color.b, 0);
            }
        }
    }

    void Update()
    {
        if (!canPlay) return;
        if (waitPlay) return;

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
            if (FallChess(rowi, coli, isWhite ? 1 : 2))
            {
                MusicMgr.Instance.PlaySound("����", false);
                if (isTry == true)//����
                {
                    board_history.Add(board);
                    board = (int[,])board.Clone();
                    isWhite = !isWhite;
                }
                else//����
                {
                    bool canfind = false;//�Ƿ��ں������ҵ��ò�
                    foreach(GameNode node in curnode.childrens)
                    {
                        if(coli==node.x && rowi==node.y)
                        {
                            curnode = node;
                            canfind = true;
                            break;
                        }
                    }

                    if(canfind==false)//�������
                    {
                        StartCoroutine("BackBoard");
                        TikuMgr.Instance.GetTikuInfoPanel().ShowTip(false);
                    }
                    else if(canfind==true)//�ҵ�
                    {
                        //���벽��UI
                        stones.Add(new Stone(curnode.isWhite, coli, rowi, ++curstep));
                        //Generate
                        GenerateStepTextObjs();

                        if(curnode.childrens.Count==0)//ΪҶ�ӽڵ㣬����ɹ�
                        {
                            TikuMgr.Instance.GetTikuInfoPanel().ShowTip(true);
                            canPlay = false;//����ɹ����������ӣ�����ˢ��
                            tboard = (int[,])board.Clone();//��¼��������״̬
                        }
                        else//��û���꣬�Զ���һ���������
                        {
                            GameNode node = curnode.childrens[0];//ѡ������ߵ�
                            StartCoroutine("PlayChessLater", node);
                        }
                    }
                }
            }
        }

        DrawBoard(board);
    }

    //�����Է��֣�setquestion�����awake��start��ִ�У��������ִ��˳��
    public void SetQuestion(GameTree t)
    {
        canPlay = false;//��ʼ��ʱ�������ֶ�����

        //��Ŀ�л�ʱ����С���䣬����ͬһ���̣�Ҫ�������
        stones.Clear();
        ClearStepTextObjs();

        this.placeStones = t.placeStones;
        this.answers = t.answers;

        GameNode node = t.GetRoot();//��ø����
        root = node;
        curnode = root;

        Array.Clear(board, 0, board.Length);

        int x, y;
        string color;
        foreach (Stone s in placeStones)
        {
            x = s.getX();
            y = s.getY();
            FallChess(y, x, s.getColor() ? 1 : 2);
        }
        board_history.Clear();
        board_history.Add((int[,])board.Clone());
        tboard = (int[,])board.Clone();

        curstep = placeStones.Count;

        //��ÿ����ȷ���ת��Ϊ��Ӧ������״̬����
        for (int i=0;i<answers.Count; i++)
        {
            correctBoards.Add(new List<int[,]>());
            for(int j = 0; j < answers[i].Count; j++)
            {
                FallChess(answers[i][j].getY(), answers[i][j].getX(), answers[i][j].getColor() ? 1 : 2);
                correctBoards[i].Add((int[,])board.Clone());
            }
            board = (int[,])board_history[0].Clone();//����ʼ״̬
        }
        DrawBoard(board);
        canPlay = true;
    }

    //�������̣���������
    public void ResetBoard()
    {
        isWhite = answers[0][0].getColor() ? true : false;//������Ӧ��ִʲô��ɫ���� ����1����ȷ��⣬�����������1��
        board_history.RemoveRange(1, board_history.Count - 1);
        stones.Clear();
        curstep = placeStones.Count;
        ClearStepTextObjs();
        tboard = board_history[0];
        curnode = root;
        SetBoard(0);
        canPlay = true;
    }

    public void SetBoard(int i)
    {
        board = (int[,])board_history[i].Clone();//һ��Ҫ�����������޸���ʷ���
        DrawBoard(board);
    }

    public void ChangeTryCase(bool v)
    {
        isTry = v;
        ResetBoard();
    }

    private IEnumerator PlayChessLater(GameNode node)
    {
        waitPlay = true;
        yield return new WaitForSeconds(0.5f);
        FallChess(node.y, node.x, node.isWhite ? 1 : 2);
        MusicMgr.Instance.PlaySound("����", false);
        curnode = node;
        tboard = (int[,])board.Clone();//��¼��������״̬
        //Add
        stones.Add(new Stone(node.isWhite, node.x, node.y, ++curstep));
        //Generate
        GenerateStepTextObjs();

        if (node.childrens.Count==0)//����ɹ�
        {
            TikuMgr.Instance.GetTikuInfoPanel().ShowTip(true);
            DrawBoard(board);
            canPlay = false;
        }
        waitPlay = false;
    }

    //���˵���һ��
    private IEnumerator BackBoard()
    {
        waitPlay = true;
        yield return new WaitForSeconds(1f);
        board = (int[,])tboard.Clone();//�ص��ϴε�״̬
        DrawBoard(board);
        waitPlay = false;
    }

    public void ClearStepTextObjs()
    {
        if (textStepObjs.Count == 0) return;
        for (int i = textStepObjs.Count - 1; i >= 0; i--)
        {
            PoolMgr.Instance.PushObj("Text/3dStep", textStepObjs[i]);
        }
        textStepObjs.Clear();
    }

    public void GenerateStepTextObjs()
    {
        ClearStepTextObjs();

        int[,] stepboard = new int[size, size];
        int col, row, step;
        for(int i=0;i<stones.Count;i++)
        {
            col = stones[i].getX();
            row = stones[i].getY();
            step = stones[i].getStep();

            stepboard[row, col] = step;
        }

        for (int i = 0; i < stones.Count; i++)
        {
            col = stones[i].getX();
            row = stones[i].getY();
            step = stones[i].getStep();

            if (board[row, col] == 0) continue;
            if (stepboard[row,col] == step)
            {
                GameObject obj = PoolMgr.Instance.GetObj("Text/3dStep");
                textStepObjs.Add(obj);
                TextStep script = obj.GetComponent<TextStep>();
                if (size == 19)
                    script.InitText(step, !stones[i].getColor(), chesses[row, col].transform.position - new Vector3(0, 0, 0.2f), 0.8f);
                else if (size == 13)
                    script.InitText(step, !stones[i].getColor(), chesses[row, col].transform.position - new Vector3(0, 0, 0.2f), 0.9f);
                else if (size == 9)
                    script.InitText(step, !stones[i].getColor(), chesses[row, col].transform.position - new Vector3(0, 0, 0.2f), 1f);
            }
        }
    }

    private void DrawBoard(int[,] print)
    {
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (print[i, j] == 0)
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
                if (print[i, j] == 1)
                {
                    chesses[i, j].material.color = Color.white;
                }
                else if (print[i, j] == 2)
                {
                    chesses[i, j].material.color = Color.black;
                }
            }
        }
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
        Array.Clear(visited, 0, visited.Length);
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
        if (board[i, j] != 0) return false;//������
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
