using GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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

    public override void Update()
    {
        RandomMoveLocation();
        StartCoroutine(ObjectScan(scanDelay));
        StatusUpdate();
        AnimationUpdate();
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

            Collider[] detectedColls = Physics.OverlapSphere(myObject.position, (float)playStatus.viewRange, 1 << 6);
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

            }
            else
            {
                targetUnit = null;
            }

            isScanning = false;
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
        aiPath.speed = (float)playStatus.MoveSpeed;
    }

    public override void AnimationUpdate()
    {
        if (isMove)
        {
            if (anim.GetBool("Move") == false)
            {
                anim.SetBool("Move", true);
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
            if (anim.GetBool("Move") == true)
            {
                anim.SetBool("Move", false);
            }
        }
    }

    public override IEnumerator Death()
    {
        if (!anim.GetBool("Dead"))
        {
            anim.SetBool("Dead", true);
        }

        myCollider.enabled = false;
        attackTimer = 0f;
        isReadyToMove = false;

        yield return new WaitForSeconds(0.2f);

        myCollider.enabled = true;
        ItemDrop();
        Disappear();
    }

    private void ItemDrop()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/FieldObject/DropItem");
        DropItem dropItem = prefab.GetComponent<DropItem>();
        dropItem.moneyType = GameMoney.GameMoneyType.RUBY;
        dropItem.dropCount = 1;

        GameObject itemObject = PoolManager.instance.Spawn(dropItem.gameObject, myObject.position, Vector3.one, Quaternion.identity, true, myObject.parent);
    }

    private void Disappear()
    {
        PoolManager.instance.Release(gameObject);
    }
}
