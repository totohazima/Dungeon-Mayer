using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using StatusHelper;
using FieldHelper;
using GameSystem;
using Pathfinding;
using System;
using Util;
using static UnityEngine.GraphicsBuffer;

public class Character : FieldObject
{
    public WhiteFlash whiteFlash;
    public StateController stateController;
    public UICharacterCostume characterCostume;
    public bool isInvincible; //true�� ��� ����
    public bool isDisable; //true�� ��� ����
    public bool isReadyToMove; //true�� ��� ������
    public bool isReadyToAttack; //true�� ��� ���� ����
    public bool isMove;
    public bool isAttacking;
    public bool isDead;
    [Header("StatusInfo")]
    //public StatusInfo statusInfo;
    public FieldMap.Field myField;
    public PlayStatus playStatus;
    public DesirePlayStatus desirePlayStatus;
    public Animator anim;
    public AILerp aiLerp;
    public Vector3 dropRange = new Vector3(1f, 1f, 1f);
    [Header("TargetInfo")]
    public Transform targetField; //���� ���� �� �ʵ�
    public Transform targetUnit;  //Ÿ������ ���� ����
    public Vector3 targetLocation;  //�̵��ؾ� �� ��ǥ(��ġ)


    public void OnEnable()
    {
        playStatus.CurHealth = playStatus.MaxHealth;
        ReCycle();
        isReadyToMove = true;
    }
    public virtual void ReCycle()
    {
        isReadyToAttack = false;
        isAttacking = false;
        isReadyToMove = false;
        isMove = false;
        isDead = false;

        targetField = null;
        targetUnit = null;
        targetLocation = Vector3.zero;

        StopAllCoroutines();
    }
    public virtual void Update()
    {       
    }

    //���� �̵�
    public virtual void MoveUnit(Vector3 dir)
    {   
    }

    /// <summary>
    /// ���� �������ͽ�(����, ����) �� ������Ʈ���� ����
    /// </summary>
    public virtual void StatusUpdate()
    {
    }
    public virtual void AnimationUpdate()
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

    public virtual void StatCalculate()
    {
        if(stateController == null)
        {
            return;
        }
        
        stateController.SetMoveSpeedPercent(playStatus.moveSpeedPercent);
        stateController.SetAttackSpeedPercent(playStatus.attackSpeedPercent);
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
        yield return 0;
    }

    public virtual void Disappear()
    {
        PoolManager.instance.Release(gameObject);
    }
    public void Push(Vector3 vector, bool ignoreKinematic = false)
    {
        rigid.isKinematic = ignoreKinematic;
        rigid.AddForce(vector, ForceMode.Impulse);

        rigid.isKinematic = true;
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

            //������ ��� ����
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(myObject.position, dropRange);
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
