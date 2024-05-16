using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float alphaSpeed = 10;
    private bool isShow = true;
    private UnityAction hideCallBack;
    protected UnityAction updateActions;

    private void Awake()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    /// <summary>
    /// ��Ҫ���� ��ʼ�� ��ť�¼������ȵ�����
    /// </summary>
    protected abstract void Init();

    public virtual void ShowMe()
    {
        isShow = true;
        canvasGroup.alpha = 0;
    }

    public virtual void HideMe(UnityAction callBack)
    {
        isShow = false;
        canvasGroup.alpha = 1;
        //��¼ ����� �������ɹ����ִ�еĺ���
        hideCallBack = callBack;
    }

    // Update is called once per frame
    void Update()
    {
        updateActions?.Invoke();
        //����
        if (isShow && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1)
                canvasGroup.alpha = 1;
        }
        //����
        else if (!isShow && canvasGroup.alpha != 0)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
                canvasGroup.alpha = 0;
            //Ӧ���ù����� ɾ���Լ�
            hideCallBack?.Invoke();
        }
    }
}
