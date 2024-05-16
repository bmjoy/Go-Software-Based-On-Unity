using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QipuchessBoard : MonoBehaviour
{
    private Transform LeftTop;
    private Transform RightBottom;

    Vector3 LTPos;
    Vector3 RBPos;

    public int size;
    float halfGridWidth = 1;
    float halfGridHeight = 1;
    int rowi = 0, coli = 0;

    int[,] board;
    bool[,] transparent;
    List<int[,]> board_history = new List<int[,]>();//������ʷ״̬���������̿��ٸ���
    List<Stone> move_history = new List<Stone>();//ȫ������ ����λ����ʷ״̬�����ڲ������ٸ���
    Renderer[,] chesses;

    bool[,] visited;
    List<Tuple<int, int>> eatenChesses = new List<Tuple<int, int>>();
    List<Tuple<int, int>> tempCheeses = new List<Tuple<int, int>>();
    Tuple<int, int> jie = new Tuple<int, int>(-1, -1);

    public bool canplay = true;
    public bool isTry = false;
    bool canFall = true;
    bool prevJie = false;
    bool isWhite = false;
    bool initialColor;
    bool showWinRatePanel = false;

    int step;

    ChessManual chessmanual;
    int curindex = 0;//ָ��ǰ��ʾ������board_history�±꣬0Ϊ��
    int tryindex = 0;//����ʱpredict_board_history���±꣬0Ϊ��һ��

    private string filepath;
    List<MoveInfo> moveinfos = new List<MoveInfo>();//AIԤ��Ĳ�
    List<GameObject> textPercentObjs = new List<GameObject>();//ʤ���ı�����
    List<GameObject> textStepObjs = new List<GameObject>();//�����ı�����

    List<Stone> stones = new List<Stone>();//��¼AI������
    //�ڶ���board_history Ҳ��Ϊ�˻��˺�ǰ�� ͬʱҲҪɾ��������λ�õ�����
    List<int[,]> predict_board_history = new List<int[,]>();

    TextStep textStep = null;

    // Start is called before the first frame update
    void Start()
    {
        board = new int[size, size];
        chesses = new Renderer[size, size];
        visited = new bool[size, size];
        transparent = new bool[size, size];

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

        step = curindex;

        //��öԾ�
        chessmanual = QipuChessBoardMgr.Instance.GetChessManual();
        List<Stone> playList = chessmanual.getPlayList();//����
        board_history.Add((int[,])board.Clone());//��ӿ�����
        if (playList.Count > 0)
            isWhite = playList[0].getColor();//��һ������ɫ
        else
            isWhite = false;//����ʱΪ�� ��ȷ��Ӧ�ÿ�PL

        initialColor = isWhite;
        foreach (Stone stone in playList)
        {
            FallChess(stone.getY(), stone.getX(), stone.getColor() ? 1 : 2);
            move_history.Add(stone);
            int[,] t = (int[,])board.Clone();//���
            board_history.Add(t);
        }
        board = board_history[curindex];//�����˻ָ�����ʼ״̬

        filepath = QipuChessBoardMgr.Instance.GetFilePath();

        //Katago��Ϣ����
        KatagoHelper.katago.LoadSgf(filepath, 1);
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIMgr.Instance.ShowPanel<QipuPausePanel>();
            canplay = false;
            return;
        }

        if (!canplay) return;
        showWinRatePanel = false;

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

        if (colt >= 0 && rowt >= 0 && rowi >= 0 && rowi < size && coli >= 0 && coli < size)//�ڹ涨��Χ�ڲ�������
        {
            if (board[rowi, coli] == 0)//ֻ�пձ�־�²������� ��ʾ��͸��
            {
                if(isTry)
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

        if (isTry) DrawTransparentChesses();//����͸������Ӧ�÷���ǰ��

        if (canFall)
        {
            if (isTry == false)
            {
                isWhite = curindex % 2 == 1 ? !initialColor : initialColor;

                //��ͣ��ɫ���� ��ʾ��ʾUI
                foreach (MoveInfo moveinfo in moveinfos)
                {
                    if (moveinfo.row == rowi && moveinfo.col == coli)
                    {
                        UIMgr.Instance.ShowPanel<XuanDianPanel>().InitText(isWhite, moveinfo.order + 1, moveinfo.winrate);
                        showWinRatePanel = true;
                        break;
                    }
                }
            }

            //���������
            if (Input.GetMouseButtonUp(0) && isTry==false)
            {
                bool clear = false;//��¼�Ƿ�������ɫ����
                foreach (MoveInfo moveinfo in moveinfos)
                {
                    //����Ƿ�㵽��ɫ����������ʾ�����·�
                    if (moveinfo.row == rowi && moveinfo.col == coli)
                    {
                        board = (int[,])board_history[curindex].Clone();//��õ�ǰ����״̬

                        //��������״̬ дǰ��
                        QipuChessBoardMgr.Instance.GetQipuBtnPanel().btnTry.onClick.Invoke();

                        step = curindex;

                        foreach (Tuple<int, int> tuple in moveinfo.predict_moves)
                        {
                            FallChess(tuple.Item2, tuple.Item1, isWhite ? 1 : 2);//tuple��һ������
                            predict_board_history.Add((int[,])board.Clone());
                            stones.Add(new Stone(isWhite, tuple.Item1, tuple.Item2, ++step));//��¼���ӣ�����������ʾ�ͻ���
                            isWhite = !isWhite;
                        }

                        ClearWinrateTextObjs();
                        //��ʾ����
                        DrawBoard(board);
                        tryindex = moveinfo.predict_moves.Count;//���µ�ǰ��ʾ���±� moveinfo.predict_moves.Count + һ��ʼ��

                        //��ʾ ���� ����
                        GenerateStepTextObjs();
                        clear = true;

                        //ȥ��֮ǰisTryΪfalseʱ��ʾ�Ĳ�������
                        PoolMgr.Instance.PushObj("Text/3dStep", textStep.gameObject);
                        //�ҵ��˳�
                        break;
                    }
                }
                if(clear) moveinfos.Clear();//�㵽��ɫ�������AIԤ�����Ϣ �������Ҫ��
            }
            else if (Input.GetMouseButtonUp(0) && isTry == true)
            {
                board = (int[,])predict_board_history[tryindex].Clone();

                if (FallChess(rowi, coli, isWhite ? 1 : 2))
                {
                    //�Ƴ�
                    if(tryindex < predict_board_history.Count - 1)
                    {
                        print($"predict_board:{tryindex + 1} {predict_board_history.Count - tryindex - 1}");
                        print($"stones:{tryindex + 1} {stones.Count - tryindex}");
                        predict_board_history.RemoveRange(tryindex + 1, predict_board_history.Count - tryindex - 1);
                        stones.RemoveRange(tryindex, stones.Count - tryindex);//ע����������������Ĳ��Countʼ����1
                    }
                    //���
                    predict_board_history.Add((int[,])board.Clone());
                    step = curindex + tryindex + 1;
                    stones.Add(new Stone(isWhite, coli, rowi, step));

                    Play(1);
                    GenerateStepTextObjs();//����Play��
                }
            }
        }

        if (showWinRatePanel == false) UIMgr.Instance.HidePanel<XuanDianPanel>();
    }

    //��ʷ����play
    public void Play(int delta)
    {
        if (delta == 0) return;
        
        if(isTry==false)
        {
            if (delta > 0)
            {
                if (curindex >= board_history.Count - 1) return;
                curindex = (curindex + delta) < board_history.Count ? curindex + delta : board_history.Count - 1;
                board = (int[,])board_history[curindex].Clone();
                DrawBoard(board_history[curindex]);
                MusicMgr.Instance.PlaySound("����", false);
            }
            else if (delta < 0)
            {
                if (curindex <= 0) return;
                curindex = (curindex + delta) >= 0 ? curindex + delta : 0;
                board = (int[,])board_history[curindex].Clone();
                DrawBoard(board_history[curindex]);
            }

            if (curindex == 0)
            {
                PoolMgr.Instance.PushObj("Text/3dStep", textStep.gameObject);
                return;
            }
            
            GenerateStepTextObj();
        }
        else if(isTry==true)
        {
            if (delta > 0)
            {
                if (tryindex >= predict_board_history.Count - 1) return;
                tryindex = (tryindex + delta) < predict_board_history.Count ? tryindex + delta : predict_board_history.Count - 1;
                board = (int[,])predict_board_history[tryindex].Clone();
                DrawBoard(predict_board_history[tryindex]);
                isWhite = delta % 2 == 1 ? !isWhite : isWhite;//����������ɫ

                GenerateStepTextObjs();
                MusicMgr.Instance.PlaySound("����", false);
            }
            else if (delta < 0)
            {
                if (tryindex <= 0) return;
                tryindex = (tryindex + delta) >= 0 ? tryindex + delta : 0;
                board = (int[,])predict_board_history[tryindex].Clone();
                DrawBoard(predict_board_history[tryindex]);
                isWhite = delta % 2 == -1 ? !isWhite : isWhite;

                GenerateStepTextObjs();
            }
        }
    }

    public IEnumerator AutoPlay()
    {
        for(int i=curindex;i<board_history.Count;i++)
        {
            yield return new WaitForSecondsRealtime(1f);
            Play(1);
        }
    }

    public void ResetBoard()
    {
        curindex = 0;
        isWhite = initialColor;
        DrawBoard(board_history[curindex]);
    }

    public void SetBoard(int index)
    {
        curindex = index;
        DrawBoard(board_history[curindex]);
    }

    public void CleanBoard()
    {
        predict_board_history.Clear();
        stones.Clear();
        tryindex = 0;

        moveinfos.Clear();

        DrawBoard(board_history[curindex]);
    }

    public void ClearWinrateTextObjs()
    {
        if (textPercentObjs.Count == 0) return;
        for (int i = textPercentObjs.Count-1; i>=0; i--)
        {
            PoolMgr.Instance.PushObj("Text/3dPercent", textPercentObjs[i]);
        }
        textPercentObjs.Clear();
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

    public void GenerateStepTextObj()
    {
        //��������������ʾ
        if (textStep == null || textStep.gameObject.activeSelf == false)
            textStep = PoolMgr.Instance.GetObj("Text/3dStep").GetComponent<TextStep>();
        Stone stone = move_history[curindex - 1];
        if (size == 19)
            textStep.InitText(stone.getStep(), !stone.getColor(), chesses[stone.getY(), stone.getX()].transform.position - new Vector3(0, 0, 0.2f), 0.8f);
        else if(size==13)
            textStep.InitText(stone.getStep(), !stone.getColor(), chesses[stone.getY(), stone.getX()].transform.position - new Vector3(0, 0, 0.2f), 0.9f);
        else if(size==9)
            textStep.InitText(stone.getStep(), !stone.getColor(), chesses[stone.getY(), stone.getX()].transform.position - new Vector3(0, 0, 0.2f), 1f);
    }

    public void GenerateStepTextObjs()
    {
        ClearStepTextObjs();//���

        int[,] tboard = (int[,])predict_board_history[tryindex].Clone();
        int[,] stepboard = new int[size,size];
        int col, row, step;
        Stone stone;
        //���α���
        for (int i=0; i<=tryindex-1; i++)
        {
            stone = stones[i];
            row = stone.getY();
            col = stone.getX();
            step = stone.getStep();
            if (tboard[row, col] == 0) continue;

            stepboard[row, col] = step;//���²��� �����п��ܱ��Ե�Ȼ���²�������
        }

        for (int i = 0; i <= tryindex - 1; i++)
        {
            stone = stones[i];
            row = stone.getY();
            col = stone.getX();
            step = stone.getStep();
            if (tboard[row, col] == 0) continue;

            if (stepboard[row,col]==step)
            {
                GameObject obj = PoolMgr.Instance.GetObj("Text/3dStep");
                textStepObjs.Add(obj);
                TextStep script = obj.GetComponent<TextStep>();
                if (size == 19)
                    script.InitText(step, !stone.getColor(), chesses[row, col].transform.position - new Vector3(0, 0, 0.2f), 0.8f);
                else if(size==13)
                    script.InitText(step, !stone.getColor(), chesses[row, col].transform.position - new Vector3(0, 0, 0.2f), 0.9f);
                else if(size==9)
                    script.InitText(step, !stone.getColor(), chesses[row, col].transform.position - new Vector3(0, 0, 0.2f), 1f);
            }
        }
    }

    public void AIAnalyze()
    {
        board = (int[,])board_history[curindex].Clone();
        canplay = false;
        KatagoHelper.katago.LoadSgf(filepath, curindex + 1);
        Task.Run(async () =>
        {
            moveinfos = await KatagoHelper.katago.Analyze(6);//�����AIԤ����·�
            moveinfos.Sort();
        }).ContinueWith(task =>
        {
            float value = .8f;
            
            //��ʾ��ɫ�����Լ��ŵ�ʤ��
            foreach (MoveInfo moveinfo in moveinfos)
            {
                chesses[moveinfo.row, moveinfo.col].material.color = new Color(0.45f, 1, 0.35f, value);
                GameObject obj = PoolMgr.Instance.GetObj("Text/3dPercent");
                textPercentObjs.Add(obj);
                TextPercent script = obj.GetComponent<TextPercent>();
                if (size == 19)
                    script.InitText(moveinfo.winrate, chesses[moveinfo.row, moveinfo.col].transform.position, 0.8f);
                else if(size==13)
                    script.InitText(moveinfo.winrate, chesses[moveinfo.row, moveinfo.col].transform.position, 0.9f);
                else if(size==9)
                    script.InitText(moveinfo.winrate, chesses[moveinfo.row, moveinfo.col].transform.position, 1f);

                value = value - 0.4f / (moveinfos.Count - 1);
            };

            //�Ҳ�RecommendMovesPanel������
            QipuChessBoardMgr.Instance.GetRecommendMovesPanel().Clear();
            bool twhite = curindex % 2 == 1 ? !initialColor : initialColor;//��ǰ��Ӧ��ʲô��ɫ
            foreach (MoveInfo moveinfo in moveinfos)
            {
                RecommendMovesItem item = Instantiate(Resources.Load<GameObject>("RecommendMovesItem")).GetComponent<RecommendMovesItem>();
                item.InitInfo(moveinfo, twhite);
                QipuChessBoardMgr.Instance.GetRecommendMovesPanel().AddItem(item);
            }
            canplay = true;
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void InitPredictBoard()
    {
        predict_board_history.Clear();
        predict_board_history.Add((int[,])board_history[curindex].Clone());//��ʼ��predict_board_history��0����Ϊԭʼ����
    }

    public void ShowPredictMoves(MoveInfo moveinfo)
    {
        if (moveinfos.Count != 0) moveinfos.Clear();

        InitPredictBoard();
        stones.Clear();

        //��������״̬ дǰ��
        isTry = true;
        QipuChessBoardMgr.Instance.GetQipuBtnPanel().EnterTryCondition();

        isWhite = curindex % 2 == 1 ? !initialColor : initialColor;
        board = (int[,])board_history[curindex].Clone();//��õ�ǰ����״̬
        step = curindex;

        foreach (Tuple<int, int> tuple in moveinfo.predict_moves)
        {
            FallChess(tuple.Item2, tuple.Item1, isWhite ? 1 : 2);//tuple��һ������
            predict_board_history.Add((int[,])board.Clone());
            stones.Add(new Stone(isWhite, tuple.Item1, tuple.Item2, ++step));//��¼���ӣ�����������ʾ�ͻ���
            isWhite = !isWhite;
        }

        ClearWinrateTextObjs();
        //��ʾ����
        DrawBoard(board);
        tryindex = moveinfo.predict_moves.Count;//���µ�ǰ��ʾ���±� moveinfo.predict_moves.Count + һ��ʼ��1��

        //��ʾ ���� ����
        GenerateStepTextObjs();

        //����еĻ���ȥ��֮ǰisTryΪfalseʱ��ʾ�Ĳ�������
        PoolMgr.Instance.PushObj("Text/3dStep", textStep.gameObject);
    }

    private void OnDestroy()
    {
        QipuChessBoardMgr.Instance.Destory();
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

    private void DrawTransparentChesses()
    {
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                if (board[i,j]==0)
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
        }
    }


    #region Χ���߼�
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