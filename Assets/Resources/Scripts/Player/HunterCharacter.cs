using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class HunterCharacter : Character, IPointerClickHandler
{
    public bool isFieldEnter = false;
    [Header("RandomMove_Info")]
    private float randomMoveRadius = 2f; //�� ��ŭ�� �Ÿ����� ���� �̵�
    private float randomMoveTime; //�� �ð� ���� Ÿ���� ������ ������ ���� �Ÿ� �� ��ġ�� ���� �̵�
    private float randomMoveTime_Max = 5f; 
    private float randomMoveTime_Min = 1f;
    private float randomMoveTimer = 0f;
    [Header("ScanTime_Info")]
    private float scanDelay = 0.1f; //��ĵ�� ���۵��ϴ� �ð�
    private bool isScanning = false; //��ĵ �ڷ�ƾ�� ���������� üũ�ϴ� ����
    protected bool onClickProcess; //���� Ŭ�� ���� üũ (���� Ŭ�� ������)


    public override void Update()
    {
        if(isDisable)
        {
            return;
        }

        if (!isScanning)
        {
            StartCoroutine(ObjectScan(scanDelay));
        }
        //RandomMoveLocation();
        StatusUpdate();
        AnimatonUpdate(); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!onClickProcess)
        {
            StartCoroutine(ProcessClick(eventData.position));
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

                Vector3 targetPos = myObject.position + randomLocation;
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
    public override IEnumerator ObjectScan(float scanDelay)
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
        aiPath.speed = (float)playStatus.MoveSpeed;
    }

    public override void AnimatonUpdate()
    {
        if(isMove)
        {
            if (anim.GetBool("Move") == false)
            {
                anim.SetBool("Move", true);
            }

            if(aiPath.steeringTarget.x < myObject.position.x) //����
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
        anim.SetBool("Death", true);
        return base.Death();
    }

    /// <summary>
    /// ĳ���� Ŭ�� �� ����
    /// </summary>
    public IEnumerator ProcessClick(Vector3 clickPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(clickPosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == myCollider)
            {
                FieldManager.instance.cameraDrag.trackingTarget = this;
                Debug.Log("���� ���� Ȱ��ȭ");
            }
        }

        yield return 0;
    }

}
