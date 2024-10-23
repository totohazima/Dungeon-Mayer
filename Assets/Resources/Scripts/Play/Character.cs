using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using StatusHelper;
using FieldHelper;
using GameSystem;
using Pathfinding;
using System;
using Util;
using Unity.Mathematics;
using UnityEngine;

public class Character : FieldObject, ICustomUpdateMono
{
    public WhiteFlash whiteFlash;
    public StateController stateController;
    public PopupController popupController;
    public UICharacterCostume characterCostume;
    public SpriteGroup spriteGroup;
    public Transform popupCenter; //팝업 문구 중심
    public Transform dropCenter; //아이템 드랍 중심
    public Transform attackRangeCenter; //공격 사거리 중심
    public bool isInvincible; //true일 경우 무적
    public bool isUntargetted; //true일 경우 타겟으로 잡히지 않음
    public bool isStopScanning; //true일 경우 스캔 정지
    public bool isReadyToMove; //true일 경우 움직임
    public bool isReadyToAttack; //true일 경우 공격 가능
    public bool isMove;
    public bool isAttacking;
    public bool isDead;

    [Header("StatusInfo")]
    public FieldMap.Field currentField;
    public string code;
    public string characterName;
    public string jobClass;
    public PlayStatus playStatus;
    public Desire_PlayStatus playStatus_Desire;
    public KDA_PlayStatus playStatus_KDA;
    public Animator anim;
    public AIPath aiPath;
    public Vector3 dropRange = new Vector3(1f, 1f, 1f);

    [Header("TargetInfo")]
    public FieldMap.Field targetField; //내가 가야 할 필드
    public Transform targetUnit;  //타겟으로 잡힌 유닛
    public Vector3 targetLocation;  //이동해야 할 좌표(위치)
    public Building.BuildingType targetBuilding; //내가 가야 할 건물
    protected bool onTargetFieldPos = false; //필드 내 랜덤위치를 구할 시 true로
    protected bool onTargetBuildingPos = false; //건물 도착 랜덤위치 구할 시 true로

    public void OnEnable()
    {
        CustomUpdateManager.customUpdateMonos.Add(this);
        playStatus.CurHealth = playStatus.MaxHealth;
        ReCycle();
        isReadyToMove = true;
    }
    public void OnDisable()
    {
        CustomUpdateManager.customUpdateMonos.Remove(this);   
    }
    public virtual void ReCycle()
    {
        isReadyToAttack = false;
        isAttacking = false;
        isReadyToMove = false;
        isMove = false;
        isDead = false;

        aiPath.destination = myObject.position;
        targetUnit = null;
        targetLocation = Vector3.zero;
        targetBuilding = Building.BuildingType.NONE;

        StopAllCoroutines();
    }
    public virtual void CustomUpdate()
    {       
    }

    //유닛 이동
    public virtual void MoveUnit(Vector3 dir)
    {   
    }

    /// <summary>
    /// 현재 스테이터스(상태, 스탯) 매 업데이트마다 설정
    /// </summary>
    public virtual void StatusUpdate()
    {
    }
    public virtual void AnimationUpdate()
    {
    }

    /// <summary>
    /// 공격이 들어오는지 체크하는 함수
    /// </summary>
    public virtual void OnHit(double damage, Character attacker)
    {
        DamageCalc(damage, attacker);
    }

    /// <summary>
    /// 공격이 들어온 경우 어느정도의 대미지가 들어오는지 계산하는 함수
    /// </summary>
    public virtual void DamageCalc(double damage, Character attacker)
    {
        if (isInvincible)
        {
            damage = 0f;
        }

        playStatus.CurHealth -= damage;


        if (playStatus.CurHealth <= 0)
        {
            if (attacker.gameObject.layer == LayerMask.NameToLayer(Layers.Player))
            {
                List<GameMoney.GameMoneyType> types = new List<GameMoney.GameMoneyType>();
                List<int> counts = new List<int>();

                types.Add(GameMoney.GameMoneyType.GOLD);
                int point = UnityEngine.Random.Range(1, 4);
                counts.Add(point);
                attacker.popupController.ItemPopup(types, counts);
            }

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
    /// ViewRange 내에 오브젝트 탐지
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
            //탐지 시야
            Gizmos.color = Color.cyan;
            DrawHollowCircle(myObject.position, (float)playStatus.viewRange, segments);

            if (attackRangeCenter != null)
            {
                //공격 사거리
                Gizmos.color = Color.red;
                DrawHollowCircle(attackRangeCenter.position, (float)playStatus.attackRange, segments);
            }

            if (dropCenter != null)
            {
                //아이템 드랍 범위
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(dropCenter.position, dropRange);
            }
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
