using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class HunterCharacter : Character, IPointerClickHandler
{
    public bool isFieldEnter = false;
    [Header("RandomMove_Info")]
    public bool onRandomMove = false;
    private float randomMoveRadius = 2f; //�� ��ŭ�� �Ÿ����� ���� �̵�
    private float randomMoveTime; //�� �ð� ���� Ÿ���� ������ ������ ���� �Ÿ� �� ��ġ�� ���� �̵�
    private float randomMoveTime_Max = 10f; 
    private float randomMoveTime_Min = 3f;
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

        StartCoroutine(ObjectScan(scanDelay));
        StartCoroutine(RandomMoveLocation());
        StatusUpdate();
        AnimationUpdate(); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!onClickProcess)
        {
            StartCoroutine(ProcessClick(eventData.position));
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

            Collider[] detectedColls = Physics.OverlapSphere(myObject.position, (float)playStatus.viewRange, 1 << 7);
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


            yield return new WaitForSeconds(scanDelay);
            isScanning = false;
        }
    }
    private void AttackRangeScan()
    {
        float distance = Vector3.Distance(myObject.position, targetUnit.position);

        if(distance <= playStatus.attackRange)
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
        ///Ÿ�� ����
        if (targetUnit != null)
        {
            isMove = true;
            aiPath.destination = targetUnit.position;
        }
        else if (targetField != null)
        {
            isMove = true;
            aiPath.destination = targetField.position;
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
         
        ///�̵� ����
        if (isMove)
        {
            aiPath.canMove = true;

            if(targetField != null && myObject.position == targetField.position || targetUnit != null)
            {
                targetField = null;
            }

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
        ///�̵� �ִϸ��̼�
        if (isMove)  
        {
            if (!anim.GetBool("Move"))
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
            if (anim.GetBool("Move"))
            {
                anim.SetBool("Move", false);
            }
        }

        ///���� �ִϸ��̼�
        attackTimer += Time.deltaTime;
        if (isReadyToAttack && attackTimer >= playStatus.attackSpeed) 
        {
            if(!anim.GetBool("DevilAttack_01"))
            {
                anim.SetBool("DevilAttack_01", true);
                attackTimer = 0f;
            }
        }
        else
        {
            if (anim.GetBool("DevilAttack_01"))
            {
                anim.SetBool("DevilAttack_01", false);
            }
        }
    }

    public override IEnumerator Death(Character character)
    {
        anim.SetBool("Death", true);

        return base.Death(character);
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
