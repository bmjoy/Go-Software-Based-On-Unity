using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public struct Stone
{
    private bool isWhite;//trueΪ�� falseΪ��
    private int x;//��
    private int y;//��
    private int step;

    public Stone(bool color, int x, int y, int step)
    {
        this.isWhite = color;
        this.x = x;
        this.y = y;
        this.step = step;
    }

    public Stone(bool color, int x, int y)
    {
        this.isWhite = color;
        this.x = x;
        this.y = y;
        this.step = 0;//�ṹ����븳ֵ
    }

    public static bool operator ==(Stone a,Stone b)
    {
        if(a.x==b.x && a.y==b.y && a.isWhite ==b.isWhite && a.step==b.step)
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(Stone a, Stone b)
    {
        if (a.x == b.x && a.y == b.y && a.isWhite == b.isWhite && a.step == b.step)
        {
            return false;
        }
        return true;
    }

    public bool getColor()
    {
        return isWhite;
    }

    public void setColor(bool color)
    {
        this.isWhite = color;
    }

    public int getX()
    {
        return this.x;
    }

    public void setX(int x)
    {
        this.x = x;
    }

    public int getY()
    {
        return this.y;
    }

    public void setY(int y)
    {
        this.y = y;
    }

    public int getStep()
    {
        return step;
    }

    public void setStep(int i)
    {
        this.step = i;
    }
}

//�洢һ��������Ҫ �����е���Ϣת��Ϊsgf�ı���ʽ
public class ChessManual
{
    private static string regexPattern = @"(?<key>[A-Z]{1,2})(\[(?<value>.*?)\]){1,}";
    
    private string gameName;//������
    private string time;//����ʱ��
    private float tiemu;//��Ŀ
    private string result;//���
    private string place;//�ص�
    private string rule;//��������

    public const short sgfVersion = 4;//sgf��ʽ�淶�汾
    public short gameType = 1;//��Ϸ���� Ĭ��1 Χ��
    private string creator;//sgf�ļ�������
    private string playerBlack;//�ڷ�
    private string blackRank;//�ڷ�ˮƽ
    private string playerWhite;//�׷�
    private string whiteRank;//�׷�ˮƽ

    private int boardsize = 19;
    private List<Stone> playList=new List<Stone>();//����˳�򣬶��ڶԾ֣�������Ψһ�ģ������߷���

    public ChessManual() { }
    public ChessManual(string gameName, string time, string place, float tiemu, string result, string playerBlack,
            string blackRank, string playerWhite, string whiteRank)
    {
        this.gameName = gameName;
        this.time = time;
        this.place = place;
        this.tiemu = tiemu;
        this.result = result;
        this.playerBlack = playerBlack;
        this.blackRank = blackRank;
        this.playerWhite = playerWhite;
        this.whiteRank = whiteRank;
    }

    /// <summary>
    /// ������Ϸsgf�ļ�
    /// </summary>
    /// <param name="sgfContent">����ȫ������</param>
    public ChessManual(string sgfContent)
    {
        int step = 0;
        MatchCollection tokens = Regex.Matches(sgfContent, regexPattern);
        foreach (Match token in tokens)
        {
            switch (token.Groups["key"].Value)
            {
                case "B"://��������һ��
                    string bpos = token.Groups["value"].Value;
                    int bcol = bpos[0] - 97, brow = bpos[1] - 97;
                    playList.Add(new Stone(false, bcol, brow, ++step));
                    break;
                case "W"://��������һ��
                    string wpos = token.Groups["value"].Value;
                    int wcol = wpos[0] - 97, wrow = wpos[1] - 97;
                    playList.Add(new Stone(true, wcol, wrow, ++step));
                    break;
                case "GN"://������
                    gameName = token.Groups["value"].Value;
                    break;
                case "DT"://����ʱ��
                    time = token.Groups["value"].Value;
                    break;
                case "FF"://sgf�ļ���ʽ�淶�汾
                    break;
                case "GM"://��Ϸ���� 1ΪΧ��
                    gameType = short.Parse(token.Groups["value"].Value);
                    break;
                case "AN"://�Ծ�ע����
                    break;
                case "PC"://�����ص�
                    place = token.Groups["value"].Value;
                    break;
                case "US"://��sgf�ļ�������
                    creator = token.Groups["value"].Value;
                    break;
                case "SZ"://���̳ߴ�
                    int size = int.Parse(token.Groups["value"].Value);
                    boardsize = size;
                    break;
                case "TM"://ʱ������
                    break;
                case "PB"://�ڷ��������
                    playerBlack = token.Groups["value"].Value;
                    break;
                case "PW"://�׷��������
                    playerWhite = token.Groups["value"].Value;
                    break;
                case "WR"://�׷�ˮƽ ..d ..k ..p
                    whiteRank = token.Groups["value"].Value;
                    break;
                case "BR"://�ڷ�ˮƽ
                    blackRank = token.Groups["value"].Value;
                    break;
                case "KM"://��Ŀ�� 5.5 0 -10
                    tiemu = float.Parse(token.Groups["value"].Value);
                    break;
                case "AB"://������֮ǰ���������ϵĺ���
                    break;
                case "AW"://������֮ǰ���������ϵİ���
                    break;
                case "RU"://Χ�����  Japanese, Chinese, AGA, GOE etc.
                    rule = token.Groups["value"].Value;
                    break;
                case "HA"://������
                    break;
                case "CA"://�ַ�������
                    break;
                case "RE"://�������
                    result = token.Groups["value"].Value;
                    break;
                case "PL"://����Χ����Ŀ ��ʾ�������ķ�
                    break;
            }
        }
    }

    public string getGN()
    {
        return gameName;
    }

    public void setGN(string gameName)
    {
        this.gameName = gameName;
    }

    public string getTime()
    {
        return time;
    }

    public void setTime(string time)
    {
        this.time = time;
    }

    public string getPlace()
    {
        return place;
    }

    public void setPlace(string place)
    {
        this.place = place;
    }

    public float getTiemu()
    {
        return tiemu;
    }

    public void setTiemu(float tiemu)
    {
        this.tiemu = tiemu;
    }

    public string getResult()
    {
        return result;
    }

    public void setResult(string result)
    {
        this.result = result;
    }

    public string getBlackName()
    {
        return playerBlack;
    }

    public void setBlackName(string playerBlack)
    {
        this.playerBlack = playerBlack;
    }

    public string getBlackRank()
    {
        return blackRank;
    }

    public void setBlackRank(string blackRank)
    {
        this.blackRank = blackRank;
    }

    public string getWhiteName()
    {
        return playerWhite;
    }

    public void setWhiteName(string playerWhite)
    {
        this.playerWhite = playerWhite;
    }

    public string getWhiteRank()
    {
        return whiteRank;
    }

    public void setWhiteRank(string whiteRank)
    {
        this.whiteRank = whiteRank;
    }

    public void AddStone(Stone point)
    {
        playList.Add(point);
    }

    public List<Stone> getPlayList()
    {
        return playList;
    }

    public void setPlayList(List<Stone> playList)
    {
        this.playList = playList;
    }

    public void setCreator(string name)
    {
        this.creator = name;
    }

    public string getCreator()
    {
        return creator;
    }

    public int getSize()
    {
        return boardsize;
    }

    public void setSize(int size)
    {
        this.boardsize = size;
    }

    public short getGameType()
    {
        return gameType;
    }

    public void setGameType(short type=1)
    {
        this.gameType = type;
    }

    public void setRule(string rule)
    {
        this.rule = rule;
    }

    public string getRule()
    {
        return this.rule;
    }
}
