using FieldHelper;
using GameEvent;
using GameSystem;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HeroCharacter : Character, IPointerClickHandler
{
    [Header("Field and Movement Info")]
    public bool isFieldEnter = false;
    public bool onRandomMove = false;
    public bool isWaitingBuilding = false;
    private float randomMoveRadius = 2f;
    private float randomMoveTime;
    private float randomMoveTime_Max = 10f;
    private float randomMoveTime_Min = 3f;
    [Header("Scanning Info")]
    public EnemyCharacter soonTargetter = null;
    private float scanDelay = 0.1f;
    private bool isScanning = false;
    [Header("Click Process Info")]
    protected bool onClickProcess;

    [Header("Game Event Info")]
    public EventCallAnimation eventCallAnimation = null;
    public GameObject attackPrefab;
    public GameEventFilter attackEvent = null;
    UnityEvent eventListener = null;

    public override void ReCycle()
    {
        base.ReCycle();
        onRandomMove = false;
        isFieldEnter = false;
        isScanning = false;
        soonTargetter = null;
        characterCostume.CostumeEquip_Process();

        if (attackEvent != null)
        {
            eventListener = new UnityEvent();
            attackEvent.RegisterListener(gameObject, eventListener);
            eventCallAnimation.callPrefab = attackPrefab;
        }
    }

    public override void CustomUpdate()
    {
        if(isDead)
        {
            return;
        }

        //StartCoroutine(RandomMoveLocation(myField));
        //StartCoroutine(ObjectScan(scanDelay));
        AttackRangeScan();
        StatCalculate();
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

    private IEnumerator RandomMoveLocation(FieldMap.Field field)
    {
        if (onRandomMove)
        {
            yield break;
        }

        onRandomMove = true;
        randomMoveTime = Random.Range(randomMoveTime_Min, randomMoveTime_Max);

        yield return new WaitForSeconds(randomMoveTime);

        Vector3 boxSize = FieldManager.instance.fieldActivitys[(int)field].fieldSize;
        // 오버랩 박스 내에서 무작위 위치 생성
        Vector3 randomPositionWithinBox = new Vector3(
            Random.Range(-boxSize.x / 2, boxSize.x / 2),
            Random.Range(-boxSize.y / 2, boxSize.y / 2),
            Random.Range(-boxSize.z / 2, boxSize.z / 2)
        );

        // 현재 위치에 대해 상대적인 위치를 적용하여 이동
        FieldActivity controlField = FieldManager.instance.fieldActivitys[(int)field];
        targetLocation = controlField.getTransform.position + randomPositionWithinBox;

        onRandomMove = false;  // 이동 종료
    }

    public override IEnumerator ObjectScan(float scanDelay)
    {
        if (isStopScanning || isScanning)
        {
            yield break;
        }

        isScanning = true;

        ///주변 적 탐지가 아닌 필드 내 몬스터의 정보를 가져오는 방식
        FieldActivity field = FieldManager.instance.fieldActivitys[(int)currentField];
        List<Collider> detectedColls = new List<Collider>();
        List<Collider> detectedList = new List<Collider>(); //타겟으로 잡을 수 있는 상태의 몬스터들을 담음
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (EnemyCharacter enemy in field.monsters)
        {
            detectedColls.Add(enemy.myCollider);
        }

        foreach (Collider col in detectedColls)
        {
            if (col == null || col == myCollider)
            {
                continue;
            }

            for (int i = 0; i < field.monsters.Count; i++)
            {
                if (col == field.monsters[i].myCollider)
                {
                    if (field.monsters[i].isUntargetted == false)
                    {
                        detectedList.Add(field.monsters[i].myCollider);
                    }
                }
            }
        }

        foreach (Collider col in detectedList)
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
            if (nearestTarget != targetUnit)
            {
                bool isTargeting = false;

                EnemyCharacter target = null;

                foreach (EnemyCharacter enemy in field.monsters)
                {
                    if (enemy.myCollider.transform == nearestTarget)
                    {
                        target = enemy;
                    }
                }

                //기존 타겟이 아예 없는 경우 -> 새로 잡은 타겟이 기존 타겟이 되며 어태커 리스트에 자신을 추가
                //새로 잡은 타겟이 기존에 잡은 타겟과 같은 경우 -> 아무것도 할 필요 없음
                //새로 잡은 타겟이 기존 타겟과 다른 경우->기존 타겟 어태커 리스트에서 자신을 지움 이후 새 타겟의 어태커 리스트에 자신을 추가
                if (soonTargetter == null && target != null)
                {
                    soonTargetter = target;

                    if (soonTargetter.soonAttacker.Count < soonTargetter.soonAttackerLimit)
                    {
                        soonTargetter.soonAttacker.Add(this);
                        isTargeting = true;
                    }
                }
                else if (soonTargetter == target && target != null)
                {
                    soonTargetter = target;
                    if (soonTargetter.soonAttacker.Count < soonTargetter.soonAttackerLimit)
                    {
                        isTargeting = true;
                    }
                }
                else if (soonTargetter != target && target != null)
                {
                    soonTargetter.soonAttacker.Remove(this);

                    soonTargetter = target;

                    if (soonTargetter.soonAttacker.Count < soonTargetter.soonAttackerLimit)
                    {
                        soonTargetter.soonAttacker.Add(this);
                        isTargeting = true;
                    }
                }

                if (isTargeting)
                    targetUnit = soonTargetter.myObject;

            }
        }
        else
        {
            targetUnit = null;
        }


        yield return new WaitForSeconds(scanDelay);
        isScanning = false;
    }

    private void AttackRangeScan()
    {
        if(targetUnit == null)
        {
            isReadyToAttack = false;
            return;
        }

        float distance = Vector3.Distance(attackRangeCenter.position, targetUnit.position);

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
        UpdateMovementStatus();
        UpdateAttackStatus();
        UpdateLerpSpeed();
    }

    public override void AnimationUpdate()
    {
        UpdateMovementAnimation();
        UpdateAttackAnimation();
    }


    private void UpdateMovementStatus()
    {
        if (targetUnit != null)
        {
            SetTargetPosition(targetUnit.position);
            if(isFieldEnter) //필드 진입 시 더 이상 필드 중앙으로 이동할 필요 X
            {
                isFieldEnter = false;
                targetField = currentField;
            }

            if(!targetUnit.gameObject.activeSelf)
            {
                targetUnit = null;
            }
        }                                                      
        else if (targetField != currentField)
        { 
            if(!isMove) //UpdateLerpSpeed가 이 메서드보다 늦게 실행되어 onTargetFieldPos가 true가 되어 움직이지 못하는 현상 방지
            {
                onTargetFieldPos = false;
            }

            if (!onTargetFieldPos)
            {
                onTargetFieldPos = true;

                Vector3 fieldPos = Vector3.zero;
                FieldActivity controlField = FieldManager.instance.fieldActivitys[(int)targetField];
                Vector3 boxSize = controlField.fieldSize;

                Vector3 randomPositionWithinBox = new Vector3(
                    Random.Range(-boxSize.x / 2, boxSize.x / 2),
                    Random.Range(-boxSize.y / 2, boxSize.y / 2),
                    Random.Range(-boxSize.z / 2, boxSize.z / 2)
                );

               
                fieldPos = controlField.getTransform.position + randomPositionWithinBox;

                SetTargetPosition(fieldPos);
            }
        }
        else if(targetBuilding != Building.BuildingType.NONE)
        {   
            if(!isMove)
            {
                onTargetBuildingPos = false;
            }

            if (!onTargetBuildingPos && !isWaitingBuilding)
            {
                onTargetBuildingPos = true;

                Building building = BuildingManager.Instance.buildings[(int)targetBuilding - 1];

                Vector3 endPos = Vector3.zero;                                          
                Vector3 interactionCenter = building.interactionCenter.position;
                Vector3 randomPositionWithinBox = new Vector3(
                    Random.Range(-building.interactionRange.x / 2, building.interactionRange.x / 2),
                    Random.Range(-building.interactionRange.y / 2, building.interactionRange.y / 2),
                    0
                );
                
                endPos = interactionCenter + randomPositionWithinBox;
                SetTargetPosition(endPos);
            }
        }
        else if (targetLocation != Vector3.zero)
        {
            SetTargetPosition(targetLocation);
        }
        else
        {
            //isMove = false;
        }

        if(targetField == currentField)
        {
            onTargetFieldPos = false;
        }
        
    }

    private void UpdateAttackStatus()
    {
        if (isReadyToAttack)
        {
            isMove = false;
        }

        if(currentField == FieldMap.Field.VILLAGE)
        {
            isStopScanning = true;
        }
        else
        {
            isStopScanning = false; 
        }
    }

    private void UpdateLerpSpeed()
    {
        if (aiPath.reachedDestination || aiPath.reachedEndOfPath || isWaitingBuilding)
        {
            isMove = false;
        }

        aiPath.canMove = isMove;
        aiPath.maxSpeed = stateController.moveSpeed;
    }
    private void SetTargetPosition(Vector3 targetPosition)                       
    {
        isMove = true;
        aiPath.destination = new Vector3(targetPosition.x, targetPosition.y, 0);
    }

    private void UpdateMovementAnimation()
    {
        if (isMove)
        {
            anim.SetBool(AnimatorParams.MOVE, true);
        }
        else
        {
            anim.SetBool(AnimatorParams.MOVE, false);
        }
        SetDirection();
    }

    private void SetDirection()
    {
        if (isMove)
        {
            if (aiPath.steeringTarget.x < myObject.position.x)
            {
                viewObject.rotation = Quaternion.Euler(0, 180, 0); // Left
            }
            else
            {
                viewObject.rotation = Quaternion.Euler(0, 0, 0); // Right
            }
        }

        if(isReadyToAttack && targetUnit != null)
        {
            if (targetUnit.position.x < myObject.position.x)
            {
                viewObject.rotation = Quaternion.Euler(0, 180, 0); // Left
            }
            else
            {
                viewObject.rotation = Quaternion.Euler(0, 0, 0); // Right
            }
        }
    }

    private void UpdateAttackAnimation()
    {
        anim.SetBool(AnimatorParams.DEVILATTACK_1, isReadyToAttack);
    }

    public override IEnumerator Death()
    {
        if(!anim.GetBool(AnimatorParams.DEATH))
        {
            anim.SetBool(AnimatorParams.DEATH, true);
        }

        myCollider.enabled = false;
        isReadyToMove = false;
        
        yield return new WaitForSeconds(0.1f);

        myCollider.enabled = true;
        Disappear();
    }

    /// <summary>
    /// 캐릭터 클릭 시 추적
    /// </summary>
    public IEnumerator ProcessClick(Vector3 clickPosition)
    {
        Ray ray = FieldManager.instance.cameraController.mainCamera.ScreenPointToRay(clickPosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == myCollider)
            {
                FieldManager.instance.cameraController.trackingTarget = this;
            }
        }

        yield return 0;
    }

    
}
