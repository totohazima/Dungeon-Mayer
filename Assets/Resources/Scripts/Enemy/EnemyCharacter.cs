using GameEvent;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using static Pathfinding.Examples.Interactable.TeleportAgentOnLinkAction;

public class EnemyCharacter : Character
{
    [Header("RandomMove_Info")]
    private bool onRandomMove = false;
    //private float randomMoveRadius = 2f; //�� ��ŭ�� �Ÿ����� ���� �̵�
    private float randomMoveTime; //�� �ð� ���� Ÿ���� ������ ������ ���� �Ÿ� �� ��ġ�� ���� �̵�
    private float randomMoveTime_Max = 5f;
    private float randomMoveTime_Min = 1f;
    [Header("ScanTime_Info")]
    private float scanDelay = 0.1f; //��ĵ�� ���۵��ϴ� �ð�
    private bool isScanning = false; //��ĵ �ڷ�ƾ�� ���������� üũ�ϴ� ����
    [Header("ItemDrop_Info")]
    public int dropItemCount = 1; //������ �� ��� ����
    [Header("GameEvent")]
    public EventCallAnimation eventCallAnimation = null;
    public GameObject attackPrefab;
    public GameEventFilter attackEvent = null;
    UnityEvent eventListener = null;

    public override void ReCycle()
    {
        isReadyToAttack = false;
        isAttacking = false;
        isReadyToMove = false;
        isMove = false;
        onRandomMove = false;
        isScanning = false;

        targetField = null;
        targetUnit = null;
        targetLocation = Vector3.zero;

        StopAllCoroutines();
    }
    public override void Update()
    {
        if(isDisable || isDead)
        {
            return;
        }

        StartCoroutine(RandomMoveLocation());
        StartCoroutine(ObjectScan(scanDelay));
        StatCalculate();
        StatusUpdate();
        AnimationUpdate();

        if (attackEvent != null)
        {
            eventListener = new UnityEvent();
            attackEvent.RegisterListener(gameObject, eventListener);
            eventCallAnimation.callPrefab = attackPrefab;
        }
    }
    private IEnumerator RandomMoveLocation()
    {
        if (!onRandomMove)
        {
            onRandomMove = true;
            randomMoveTime = Random.Range(randomMoveTime_Min, randomMoveTime_Max);

            yield return new WaitForSeconds(randomMoveTime);

            Vector3 boxSize = FieldManager.instance.fields[(int)myField].boxSize;
            // ������ �ڽ� ������ ������ ��ġ ����
            Vector3 randomPositionWithinBox = new Vector3(
                Random.Range(-boxSize.x / 2, boxSize.x / 2),
                Random.Range(-boxSize.y / 2, boxSize.y / 2),
                Random.Range(-boxSize.z / 2, boxSize.z / 2)
            );

            // ���� ��ġ�� ���� ������� ��ġ�� �����Ͽ� �̵�
            FieldActivity controlField = FieldManager.instance.fields[(int)myField];
            targetLocation = controlField.getTransform.position + randomPositionWithinBox;

            onRandomMove = false;  // �̵� ����

        }
    }
    public override IEnumerator ObjectScan(float scanDelay)
    {
        if (!isScanning)
        {
            isScanning = true;

            yield return new WaitForSeconds(scanDelay);

            Collider[] detectedColls = Physics.OverlapSphere(myObject.position, (float)playStatus.viewRange, 1 << 8);
            float shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;

            foreach (Collider col in detectedColls)
            {
                if (col == null || col == myCollider)
                {
                    continue;
                }

                Transform target = col.transform;
                float dis = Vector3.Distance(myObject.position, target.position);

                if (dis < shortestDistance)
                {
                    shortestDistance = dis;
                    nearestTarget = target;
                }
            }

            if (nearestTarget != null)
            {
                targetUnit = nearestTarget;
                AttackRangeScan();
            }
            else
            {
                targetUnit = null;
                isReadyToAttack = false;
            }

            isScanning = false;
        }
    }

    private void AttackRangeScan()
    {
        float distance = Vector3.Distance(myObject.position, targetUnit.position);

        if (distance <= playStatus.attackRange)
        {
            isReadyToAttack = true;
        }
        else
        {
            isReadyToAttack = false;
        }
    }

    public override void StatusUpdate()
    {
        //����
        if (targetUnit != null)
        {
            isMove = true;
            aiLerp.destination = targetUnit.position;
        }
        else
        {
            //Ÿ���� ������ ���� ���
            if (targetLocation != Vector3.zero)
            {
                isMove = true;
                aiLerp.destination = targetLocation;
            }
            else
            {
                isMove = false;
            }
        }

        ///���� ����
        if (isReadyToAttack)
        {
            isMove = false;
        }

        if (isMove)
        {
            aiLerp.canMove = true;

            //�����̴� �� �������� �����ϰų� �ִ� ��ο� ������ ���
            if (aiLerp.reachedDestination || aiLerp.reachedEndOfPath)
            {
                isMove = false;
            }
        }
        else
        {
            aiLerp.canMove = false;
        }

        //����
        aiLerp.speed = stateController.moveSpeed;
    }

    public override void AnimationUpdate()
    {
        if (isMove)
        {
            if (anim.GetBool(AnimatorParams.MOVE) == false)
            {
                anim.SetBool(AnimatorParams.MOVE, true);
            }

            if (aiLerp.steeringTarget.x < myObject.position.x) //����
            {
                viewObject.rotation = Quaternion.Euler(0, 180, 0);
            }
            else //������
            {
                viewObject.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else
        {
            if (anim.GetBool(AnimatorParams.MOVE) == true)
            {
                anim.SetBool(AnimatorParams.MOVE, false);
            }
        }

        ///���� �ִϸ��̼�
        if (isReadyToAttack)
        {
            if (!anim.GetBool(AnimatorParams.ATTACK_1))
            {
                anim.SetBool(AnimatorParams.ATTACK_1, true);
            }
        }
        else
        {
            if (anim.GetBool(AnimatorParams.ATTACK_1))
            {
                anim.SetBool(AnimatorParams.ATTACK_1, false);
            }
        }
    }

    public override IEnumerator Death()
    {
        if (!anim.GetBool(AnimatorParams.DEATH))
        {
            anim.SetBool(AnimatorParams.DEATH, true);
        }

        myCollider.enabled = false;

        FieldActivity field = FieldManager.instance.fields[(int)myField];
        field.monsters.Remove(this);

        yield return new WaitForSeconds(0.2f);

        myCollider.enabled = true;

        ItemDrop();
        ReCycle();
        Disappear();
    }

    protected void ItemDrop()
    {
        for (int i = 0; i < dropItemCount; i++)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/FieldObject/DropItem");
           
            GameObject itemObject = PoolManager.instance.Spawn(prefab, myObject.position, Vector3.one, Quaternion.identity, true, myObject.parent);

            DropItem dropItem = itemObject.GetComponent<DropItem>();

            dropItem.moneyType = GameMoney.GameMoneyType.RUBY;
            dropItem.dropCount = 1;

            Vector3 dropPos = GetRandomPositionInBox(myObject.position, dropRange);

            dropItem.Drop_Animation(myObject.position, dropPos);
        }
    }


    Vector3 GetRandomPositionInBox(Vector3 center, Vector3 range)
    {
        float randomX = Random.Range(center.x - range.x / 2f, center.x + range.x / 2f);
        float randomY = Random.Range(center.y - range.y / 2f, center.y + range.y / 2f);

        return new Vector3(randomX, randomY, center.z);
    }
    private void Disappear()
    {
        PoolManager.instance.Release(gameObject);
    }
}
