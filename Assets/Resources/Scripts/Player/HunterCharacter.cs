using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HunterCharacter : Character
{
    [Header("RandomMove_Info")]
    private float randomMoveRadius = 2f; //�� ��ŭ�� �Ÿ����� ���� �̵�
    private float randomMoveTime; //�� �ð� ���� Ÿ���� ������ ������ ���� �Ÿ� �� ��ġ�� ���� �̵�
    private float randomMoveTime_Max = 5f; 
    private float randomMoveTime_Min = 1f;
    private float randomMoveTimer = 0f;
    [Header("ScanTime_Info")]
    private float scanDelay = 0.1f; //��ĵ�� ���۵��ϴ� �ð�
    private float scantimer = 0;

    public override void Update()
    {
        if(isDisable)
        {
            return;
        }

        scantimer += Time.deltaTime;
        if (scantimer > scanDelay)
        {
            ObjectScan();
        }
        RandomMoveLocation();
        StatusUpdate();
        AnimatonUpdate();

        // PC ȯ�濡�� ���콺 ���� Ŭ�� ����
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ProcessClick(Input.mousePosition));
        }

        // ����� ȯ�濡�� ��ġ ����
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                StartCoroutine(ProcessClick(touch.position));
            }
        }
    }

    private void RandomMoveLocation()
    {
        if(randomMoveTime == 0f)
        {
            randomMoveTime = Random.Range(randomMoveTime_Min, randomMoveTime_Max);
        }

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
                randomMoveTime = Random.Range(randomMoveTime_Min, randomMoveTime_Max);
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
            if (col == null || col == myCollider)
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

        if (isDead)
        {
            StartCoroutine(Death());
        }

        //����
        aiPath.maxSpeed = (float)playStatus.MoveSpeed;
    }

    public override void AnimatonUpdate()
    {
        if(isMove)
        {
            anim.SetBool("Run", true);

            if(aiPath.steeringTarget.x < getTransform.position.x)
            {
                viewSprite.flipX = true;
            }
            else
            {
                viewSprite.flipX = false;
            }
        }
        else
        {
            anim.SetBool("Run", false);
        }
    }
    public override IEnumerator Death()
    {
        anim.SetBool("Dead", true);
        return base.Death();
    }

    /// <summary>
    /// ĳ���� Ŭ�� �� ����
    /// </summary>
    public IEnumerator ProcessClick(Vector3 clickPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(clickPosition);
        RaycastHit hit;

        FieldActivity.instance.cameraDrag.isCameraMove = false;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == myCollider)
            {
                FieldActivity.instance.cameraDrag.trackingTarget = this;
                Debug.Log("���� ���� Ȱ��ȭ");
            }
            else
            {
                FieldActivity.instance.cameraDrag.trackingTarget = null;
                Debug.Log("���� ���� �� Ȱ��ȭ");
            }
        }
        else
        {
            FieldActivity.instance.cameraDrag.trackingTarget = null;
            Debug.Log("���� ���� �� Ȱ��ȭ");
        }

        //�����̸� ���� ������ CameraDrag ��ũ��Ʈ���� ī�޶� ����������
        yield return new WaitForSeconds(0.5f);

        FieldActivity.instance.cameraDrag.isCameraMove = true;
    }
  
}
