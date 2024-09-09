using GameEvent;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class EnemyCharacter : Character
{
    [Header("RandomMove_Info")]
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

    public override void Update()
    {
        RandomMoveLocation();
        StartCoroutine(ObjectScan(scanDelay));
        StatusUpdate();
        AnimationUpdate();

        if (attackEvent != null)
        {
            eventListener = new UnityEvent();
            attackEvent.RegisterListener(gameObject, eventListener);
            eventCallAnimation.callPrefab = attackPrefab;
        }
    }
    private void RandomMoveLocation()
    {
        
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
            aiPath.destination = targetUnit.position;
        }
        else
        {
            //Ÿ���� ������ ���� ���
            if (targetLocation != Vector3.zero)
            {
                isMove = true;
                aiPath.destination = targetLocation;
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
        else
        {
            isMove = true;
        }

        if (isMove)
        {
            aiPath.canMove = true;

            //�����̴� �� �������� �����ϰų� �ִ� ��ο� ������ ���
            if (aiPath.reachedDestination || aiPath.reachedEndOfPath)
            {
                isMove = false;
            }
        }
        else
        {
            aiPath.canMove = false;
        }

        //����
        aiPath.speed = stateController.moveSpeed;
    }

    public override void AnimationUpdate()
    {
        if (isMove)
        {
            if (anim.GetBool(AnimatorParams.MOVE) == false)
            {
                anim.SetBool(AnimatorParams.MOVE, true);
            }

            if (aiPath.steeringTarget.x < myObject.position.x) //����
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
        isReadyToMove = false;

        FieldActivity field = FieldManager.instance.fields[(int)myField];
        field.monsters.Remove(this);

        yield return new WaitForSeconds(0.2f);

        myCollider.enabled = true;
        ItemDrop();
        Disappear();
    }

    private void ItemDrop()
    {
        for (int i = 0; i < dropItemCount; i++)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/FieldObject/DropItem");
            DropItem dropItem = prefab.GetComponent<DropItem>();

            dropItem.moneyType = GameMoney.GameMoneyType.RUBY;
            dropItem.dropCount = 1;

            Vector3 dropPos = GetRandomPositionInBox(myObject.position, dropRange);

            GameObject itemObject = PoolManager.instance.Spawn(dropItem.gameObject, dropPos, Vector3.one, Quaternion.identity, true, myObject.parent);
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
