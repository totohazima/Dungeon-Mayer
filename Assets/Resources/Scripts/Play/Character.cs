using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using StatusHelper;
using FieldHelper;
using GameSystem;
using Pathfinding;
using System;

public class Character : FieldObject
{
    public WhiteFlash whiteFlash;
    public bool isInvincible; //true�� ��� ����
    public bool isDisable; //true�� ��� ����
    public bool isReadyToMove; //true�� ��� ������
    public bool isReadyToAttack; //true�� ��� ���� ����
    public bool isMove;
    public bool isDead;
    private float attackTimer;
    [Header("StatusInfo")]
    //public StatusInfo statusInfo;
    public FieldMap.Field myField;
    public PlayStatus playStatus;
    public DesirePlayStatus desirePlayStatus;
    public Animator anim;
    public AILerp aiPath;
    [Header("TargetInfo")]
    public Transform targetUnit;  //Ÿ������ ���� ����
    public Vector3 targetLocation;  //�̵��ؾ� �� ��ǥ(��ġ)


    public void OnEnable()
    {
        playStatus.CurHealth = playStatus.MaxHealth;
        isReadyToMove = true;
        isReadyToAttack = true;
    }

    public virtual void Update()
    {       
    }

    //���� �̵�
    public virtual void MoveUnit(Vector3 dir)
    {   
    }
    public virtual void AnimatonUpdate()
    {
    }

    /// <summary>
    /// ���� �������ͽ�(����, ����) �� ������Ʈ���� ����
    /// </summary>
    public virtual void StatusUpdate()
    {
    }
    /// <summary>
    /// ������ �������� üũ�ϴ� �Լ�
    /// </summary>
    public virtual void OnHit(double damage)
    {
        DamageCalc(damage);
    }

    /// <summary>
    /// ������ ���� ��� ��������� ������� �������� ����ϴ� �Լ�
    /// </summary>
    public virtual void DamageCalc(double damage)
    {
        if (isInvincible)
        {
            damage = 0f;
        }

        playStatus.CurHealth -= damage;


        if (playStatus.CurHealth <= 0)
        {
            isDead = true;
            StartCoroutine(Death());
        }
        else
        {
            if (whiteFlash != null)
            {
                whiteFlash.PlayFlash();
            }
        }
    }

    /// <summary>
    /// ViewRange ���� ������Ʈ Ž��
    /// </summary>
    public virtual IEnumerator ObjectScan(float scanDelay)
    {
        yield return null;
    }

    public virtual IEnumerator Death()
    {
        myCollider.enabled = false;
        attackTimer = 0f;
        isReadyToMove = false;
        isReadyToAttack = false;

        yield return new WaitForSeconds(1f);

        myCollider.enabled = true;
        PoolManager.instance.FalsedPrefab(gameObject, gameObject.name);
    }

#if UNITY_EDITOR
    int segments = 100;
    bool drawWhenSelected = true;

    void OnDrawGizmosSelected()
    {
        if (drawWhenSelected && myObject != null)
        {
            //Ž�� �þ�
            Gizmos.color = Color.cyan;
            DrawHollowCircle(myObject.position, (float)playStatus.viewRange, segments);

            //���� ��Ÿ�
            Gizmos.color = Color.red;
            DrawHollowCircle(myObject.position, (float)playStatus.attackRange, segments);
        }
    }

    void DrawHollowCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

        for (int i = 1; i <= segments; i++)
        {
            angle = i * Mathf.PI * 2f / segments;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
#endif
}
