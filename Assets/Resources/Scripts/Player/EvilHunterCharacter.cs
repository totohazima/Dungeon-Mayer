using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilHunterCharacter : Character
{
    [Header("RandomMove_Info")]
    private float randomMoveRadius = 5f; //�� ��ŭ�� �Ÿ����� ���� �̵�
    private float randomMoveTime = 3f; //�� �ð� ���� Ÿ���� ������ ������ ���� �Ÿ� �� ��ġ�� ���� �̵�
    private float randomMoveTimer = 0f;
    [Header("ScanTime_Info")]
    private float scanDealay = 0.1f; //��ĵ�� ���۵��ϴ� �ð�
    private float scantimer = 0;

    public override void Update()
    {
        scantimer += Time.deltaTime;
        if (scantimer > scanDealay)
        {
            ObjectScan();
        }
        RandomMoveLocation();
        StatusUpdate(); 
    }

    private void RandomMoveLocation()
    {
        if(targetUnit == null)
        {
            randomMoveTimer += Time.deltaTime;
            if(randomMoveTimer > randomMoveTime)
            {
                Vector3 randomLocation = Random.insideUnitCircle * randomMoveRadius;
                randomLocation.z = 0;

                Vector3 targetPos = getTransform.position + randomLocation;
                targetLocation = targetPos;

                randomMoveTimer = 0f;
            }
        }
        else
        {
            randomMoveTimer = 0f;
            targetLocation = Vector3.zero;
        }
    }
    public override void ObjectScan()
    {
        Collider[] detectedColls = Physics.OverlapSphere(getTransform.position, (float)playStatus.viewRange, 1 << 6);
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (Collider col in detectedColls)
        {
            if (col == null)
            {
                continue;
            }

            Transform target = col.transform;
            float dis = Vector3.Distance(getTransform.position, target.position);

            if(dis < shortestDistance)
            {
                shortestDistance = dis;
                nearestTarget = target;
            }
        }

        if(nearestTarget != null)
        {
            targetUnit = nearestTarget;

        }
        else
        {
            targetUnit = null;
        }

        scantimer = 0f;
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
        }
        else
        {
            aiPath.canMove = false;
        }

        if (isDead)
        {
            StartCoroutine(Death());
        }

        //����
        aiPath.maxSpeed = (float)playStatus.MoveSpeed;
    }

    public override void MoveAnimator(Vector3 dir)
    {
        
    }
    public override IEnumerator Death()
    {
        anim.SetBool("Dead", true);
        return base.Death();
    }
}
