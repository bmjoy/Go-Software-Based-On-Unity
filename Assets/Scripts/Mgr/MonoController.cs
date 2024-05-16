using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoController : MonoBehaviour
{
    public event UnityAction updateEvent;
    public event UnityAction destoryEvent;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (updateEvent != null)
            updateEvent.Invoke();
    }

    //���ⲿ�ṩ ���֡�����¼��ĺ���
    public void AddUpdateListener(UnityAction fun)
    {
        updateEvent += fun;
    }

    //�ṩ���ⲿ �����Ƴ�֡�����¼�����
    public void RemoveUpdateListener(UnityAction fun)
    {
        updateEvent -= fun;
    }

    private void OnDestroy()
    {
        destoryEvent?.Invoke();
    }

    public void AddDestoryListener(UnityAction fun)
    {
        destoryEvent += fun;
    }

    public void RemoveDestoryListener(UnityAction fun)
    {
        destoryEvent -= fun;
    }
}
