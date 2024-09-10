using GameEvent;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
        isFieldEnter = false;
        isScanning = false;

        targetField = null;
        targetUnit = null;
        targetLocation = Vector3.zero;

        StopAllCoroutines();
    }
    public override void Update()
    {
        if(isDisable)
        {
            return;
        }

        StartCoroutine(ObjectScan(scanDelay));
        //StartCoroutine(RandomMoveLocation());
        StatCalculate();
        StatusUpdate();
        AnimationUpdate();
        
        if(attackEvent != null)
        {
            eventListener = new UnityEvent();
            attackEvent.RegisterListener(gameObject, eventListener);
            eventCallAnimation.callPrefab = attackPrefab;
        }
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
                isReadyToAttack = false;
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
        aiPath.speed = stateController.moveSpeed;
    }

    public override void AnimationUpdate()
    {
        ///�̵� �ִϸ��̼�
        if (isMove)  
        {
            if (!anim.GetBool(AnimatorParams.MOVE))
            {
                anim.SetBool(AnimatorParams.MOVE, true);
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
            if (anim.GetBool(AnimatorParams.MOVE))
            {
                anim.SetBool(AnimatorParams.MOVE, false);
            }
        }

        ///���� �ִϸ��̼�
        if (isReadyToAttack) 
        {
            if(!anim.GetBool(AnimatorParams.DEVILATTACK_1))
            {
                anim.SetBool(AnimatorParams.DEVILATTACK_1, true);
            }
        }
        else
        {
            if (anim.GetBool(AnimatorParams.DEVILATTACK_1))
            {
                anim.SetBool(AnimatorParams.DEVILATTACK_1, false);
            }
        }
    }

    public override IEnumerator Death()
    {
        if(!anim.GetBool(AnimatorParams.DEATH))
        {
            anim.SetBool(AnimatorParams.DEATH, true);
        }

        myCollider.enabled = false;
        isReadyToMove = false;

        yield return new WaitForSeconds(0.2f);

        myCollider.enabled = true;

        //PoolManager.instance.Release(gameObject);
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
