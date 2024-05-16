using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ClientAsyncNet : BaseManager<ClientAsyncNet>
{
    private Socket socket;

    private byte[] cacheBytes = new byte[1024*1024];
    private int cacheNum = 0;

    public Queue<BaseMsg> receiveQueue = new Queue<BaseMsg>();

    private int SEND_HEART_MSG_TIME = 10;
    private HeartMsg hearMsg = new HeartMsg();

    public ClientAsyncNet()
    {
        ThreadPool.QueueUserWorkItem(SendHeartMsg);
    }

    public void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
            return;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = ipPoint;
        args.Completed += (lambdaSocket, lambdaArgs) =>
        {
            if (lambdaArgs.SocketError == SocketError.Success)
            {
                Debug.Log("���ӷ������ɹ� �ҵĵ�ַ��" 
                    + IPAddress.Parse(((IPEndPoint)this.socket.LocalEndPoint).Address.ToString()) 
                    + ":" + ((IPEndPoint)this.socket.LocalEndPoint).Port.ToString());

                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(cacheBytes, 0, cacheBytes.Length);
                receiveArgs.Completed += ReceiveCallBack;//��Ϣ����
                //��ʼ�첽���գ����յ���Ϣ���ִ��Completedί��
                (lambdaSocket as Socket).ReceiveAsync(receiveArgs);
            }
            else
            {
                Debug.Log("����ʧ��" + lambdaArgs.SocketError);
            }
        };

        socket.ConnectAsync(args);//����
    }

    //����Ϣ��ɺ������Ϣ����Ļص�����
    private void ReceiveCallBack(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            HandleReceiveMsg(args.BytesTransferred);
            //����ȥ����Ϣ ��������Ϣ��buffer
            //cacheNum ����������ʼλ�ÿ�ʼ��ƫ����
            //���õĻ������Ĵ�С
            args.SetBuffer(cacheNum, args.Buffer.Length - cacheNum);
            //�����첽����Ϣ
            if (this.socket != null && this.socket.Connected)
                socket.ReceiveAsync(args);
            else
                Close();
        }
        else
        {
            Debug.Log("������Ϣ����" + args.SocketError);
            Close();
        }
    }

    private void SendHeartMsg(object obj)
    {
        while(true)
        {
            if (this.socket != null && this.socket.Connected)
            {
                Send(hearMsg);
                Thread.Sleep(SEND_HEART_MSG_TIME*1000);
            }
        } 
    }

    public void Send(BaseMsg msg)
    {
        if (this.socket != null && this.socket.Connected)//���Լ���Ϊfalse?
        {
            if(msg is TalkMsg)
            {
                //����Ϣ��
                Debug.Log("��������Ϣ��");
            }
            else if(msg is HeartMsg)
            {
                Debug.Log("����������");
            }
            
            byte[] bytes = msg.Writing();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.Completed += (socket, args) =>
            {
                if (args.SocketError != SocketError.Success)
                {
                    Debug.Log("������Ϣʧ��" + args.SocketError);
                    Close();
                }
            };
            this.socket.SendAsync(args);
        }
        else
        {
            Close();
        }
    }

    //�ͻ��˷���ر������API
    public void Close()
    {
        if (socket != null)
        {
            QuitMsg msg = new QuitMsg();
            socket.Send(msg.Writing());//���͸������� �ر�������Ϣ
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
            socket = null;
        }
    }

    private void HandleReceiveMsg(int receiveNum)
    {
        int msgID = 0;
        int msgLength = 0;
        int nowIndex = 0;

        cacheNum += receiveNum;

        //while �ᴦ����������
        //ÿһ��ѭ���������һ����Ϣ
        while (true)
        {
            //ÿ�ν���������Ϊ-1 �Ǳ�����һ�ν��������� Ӱ����һ�ε��ж�
            msgLength = -1;
            if (cacheNum - nowIndex >= 8)
            {
                //������Ϣ��ID
                msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
                //��������
                msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
            }

            if (cacheNum - nowIndex >= msgLength && msgLength != -1)
            {
                //������Ϣ�� �ͻ����ǲ����յ�quit��heart��Ϣ��
                BaseMsg baseMsg = null;
                switch (msgID)
                {
                    case 1004:
                        baseMsg = new BoardMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1005:
                        baseMsg = new TalkMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1007:
                        baseMsg=new MatchResultMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1011:
                        baseMsg = new RegisterResultMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1013:
                        baseMsg = new LoginResultMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1015:
                        baseMsg = new UserInfoMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1016:
                        baseMsg = new UpdateUserInfoMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1018:
                        baseMsg = new StopStepMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                    case 1019:
                        baseMsg = new GameResultMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
                        break;
                }
                if (baseMsg != null)
                {
                    ThreadPool.QueueUserWorkItem(MsgHandle, baseMsg);
                    //receiveQueue.Enqueue(baseMsg);//ѹ����ն���
                }
                    
                nowIndex += msgLength;
                if (nowIndex == cacheNum)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else
            {
                if (msgLength != -1)
                    nowIndex -= 8;
                //���ǰ�ʣ��û�н������ֽ��������� �Ƶ�ǰ���� ���ڻ����´μ�������
                Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                cacheNum = cacheNum - nowIndex;//cacheNum����ʣ��û���������
                break;
            }
        }
    }

    private void MsgHandle(object obj)
    {
        switch (obj)
        {
            case BoardMsg msg:
                BoardMsg boardMsg = obj as BoardMsg;
                GameDataMgr.Instance.SetChessPos(boardMsg.x, boardMsg.y);
                break;
            case MatchResultMsg msg:
                if (msg.result==true)
                {
                    GameDataMgr.Instance.isWhite = msg.isWhite;
                    Debug.Log("ƥ��ɹ�����ʼ��ı���");
                    OnlinegameChessBoardMgr.Instance.readytoLoad = true;
                    Debug.Log("ƥ��ɹ�����ʼ��ı���");
                }
                break;
            case TalkMsg msg:
                UIMgr.Instance.chatPanel.UpdateText(msg.sentence, msg.username);
                break;
            case RegisterResultMsg msg:
                UIMgr.Instance.GetPanel<RegisterPanel>().TryRegister(msg.result);
                break;
            case LoginResultMsg msg:
                UIMgr.Instance.GetPanel<LoginPanel>().LoginIn(msg.result);
                break;
            case UserInfoMsg msg:
                if(msg.userType==0)
                {
                    //�洢���Լ�����Ϣ���ڴ���
                    GameDataMgr.Instance.SaveLocalUserInfo(msg.userInfo);
                }
                else if(msg.userType==1)
                {
                    //�洢������Ϣ���ڴ���
                    GameDataMgr.Instance.SaveOpponentUserInfo(msg.userInfo);            
                }
                break;
            case UpdateUserInfoMsg msg:
                GameDataMgr.Instance.SaveLocalUserInfo(msg.userInfo);
                break;
            case GameResultMsg msg:
                Debug.Log("�յ��������");
                GameDataMgr.Instance.gameResult = msg.result;
                break;
            case StopStepMsg msg:
                GameDataMgr.Instance.turn = !GameDataMgr.Instance.turn;
                OnlinechessBoard board = OnlinegameChessBoardMgr.Instance.GetCurrentBoard();
                board.opponentStopStep = true;
                break;
        }
    }
}
