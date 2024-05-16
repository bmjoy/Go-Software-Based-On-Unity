using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ParseSgfHelper
{
    public static string alphabet = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

    // �õ��ļ�����
    public static string GetContent(string path)
    {
        return File.ReadAllText(path);
    }

    public static void SaveContent(string despath, string content)
    {
        File.WriteAllText(despath, content);
    }

    public static void SaveContent(string despath, ChessManual cm)
    {
        File.WriteAllText(despath, Chessmanual2Sgf(cm));
    }

    /*
     * ������������ʽ�С� \ ������ ? ������ * ������ ^ ������ $ ������ + ���������������������� | ������ { ������ [ �����ַ��Ѿ�����һ���������壬
     * �����Ҫ�����ǵ�ԭʼ���壬��Ӧ�ö�������ת�壬����ϣ �����ַ�����������һ���� \ ������ô������ʽӦ����ôд�� \\+ ��
     */
    //�������û��
    public static List<string> GetStrContainData(string str, string start, string end)
    {
        List<string> result = new List<string>();
        string regex = Regex.Escape(start) + "(.*?)" + Regex.Escape(end);
        Regex pattern = new Regex(regex);
        MatchCollection matches = pattern.Matches(str);
        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            if (!key.Contains(start) && !key.Contains(end))
            {
                result.Add(key);
            }
        }
        return result;
    }

    public static string Chessmanual2Sgf(ChessManual chessmanual)
    {
        StringBuilder builder = new StringBuilder(2000);
        builder.Append("(;");
        builder.Append(@$"FF[{ChessManual.sgfVersion}]");//sgf�淶
        builder.Append($"GM[{chessmanual.gameType}]");//��Ϸ����
        builder.Append($"US[{chessmanual.getCreator()}]");//������
        builder.Append($"GN[{chessmanual.getGN()}]");//������
        builder.Append($"RU[{chessmanual.getRule()}]");//��������
        builder.Append($"SZ[{chessmanual.getSize()}]");//���̴�С
        builder.Append($"PB[{chessmanual.getBlackName()}]");//�ڷ�����
        builder.Append($"BR[{chessmanual.getBlackRank()}]");//�ڷ���λ
        builder.Append($"PW[{chessmanual.getWhiteName()}]");//�׷�����
        builder.Append($"WR[{chessmanual.getWhiteRank()}]");//�׷���λ
        builder.Append($"RE[{chessmanual.getResult()}]");//��ֽ��
        builder.Append($"KM[{chessmanual.getTiemu()}]");//��ú�����Ŀ��
        builder.Append($"DT[{chessmanual.getTime()}]");//�������

        byte[] btNumber = new byte[1];
        ASCIIEncoding asciiEncoding = new ASCIIEncoding();
        //����
        List<Stone> list = chessmanual.getPlayList();
        foreach(Stone stone in list)
        {
            btNumber[0] = (byte)(stone.getX() + 97);
            string col = asciiEncoding.GetString(btNumber);
            btNumber[0] = (byte)(stone.getY() + 97);
            string row = asciiEncoding.GetString(btNumber);
            builder.Append(string.Format(";{0}[{1}{2}]", stone.getColor() ? 'W' : 'B', col, row));
        }
        builder.Append(")");
        return builder.ToString();
    }

    public static ChessManual Sgf2Chessmanual(string path)
    {
        string sgfcontent = GetContent(path);
        ChessManual chessManual = new ChessManual(sgfcontent);
        return chessManual;
    }
}
