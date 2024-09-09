using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameSystem;
/// <summary>
/// �� ���� �󿡼� �÷����߿� ������� �ؽ�Ʈ�� (UI �ƴ�)
/// </summary>
public class FloatingText : MonoBehaviour
{
    public new Transform transform;
    public TextMeshPro text;
    void Awake()
    {
        transform = GetComponent<Transform>();  
    }

    public virtual void OnEnable()
    {
        StartCoroutine(Text_Animation());
    }
    
    public virtual IEnumerator Text_Animation()
    {
        yield return 0;
    }

    public virtual void Disappeer()
    {
        PoolManager.instance.Release(gameObject);
    }
}
