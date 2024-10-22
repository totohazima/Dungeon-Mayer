using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldHelper;
using StatusHelper;
using Util;
using Unity.Entities.UniversalDelegates;

public class FieldActivity : MonoBehaviour, ICustomUpdateMono
{
    public Transform getTransform;
    public Transform gizmosPoint;
    public FieldSpawner mySpawner;

    [Header("FieldInfo")]
    [SerializeField] private FieldMap.Field fieldName;
    public FieldMap.Field FieldName
    {
        get { return fieldName; }
        private set { fieldName = value; }
    }
    public LayerMask scanLayer;
    public Vector3 fieldSize = new Vector3(1f, 1f, 1f);
    public Vector3 spawnSize = new Vector3(1f, 1f, 1f);
    [HideInInspector] public int maxBossPoint = 100;
    [Range(0, 100)] public int bossPoint = 0;

    [Header("Bool")]
    public bool isBossSpawned = false;
    private bool isHeroScanning = false;
    private bool isEnemyScanning = false;
    
    [Header("List")]
    public List<HeroCharacter> inCharacters = new List<HeroCharacter>();
    public List<EnemyCharacter> monsters = new List<EnemyCharacter>();
    public List<EnemyCharacter> bosses = new List<EnemyCharacter>();

    void Awake()
    {
        getTransform = GetComponent<Transform>();
        mySpawner = GetComponentInChildren<FieldSpawner>();
    }
    void OnEnable()
    {
       CustomUpdateManager.customUpdateMonos.Add(this);
    }
    void OnDisable()
    {
        CustomUpdateManager.customUpdateMonos.Remove(this);
    }

    public void CustomUpdate()
    {
        StartCoroutine(ScanCharacter(0.1f));
        StartCoroutine(ScanEnemy(0.1f));
        BossSpawn();
    }

    protected IEnumerator ScanEnemy(float scanDelay)
    {
        if (isEnemyScanning)
        {
            yield break;
        }

        isEnemyScanning = true;

        foreach(HeroCharacter hero in inCharacters)
        {
            EnemyCharacter nearMonster = null;
            float shortDistance = Mathf.Infinity;

            if(hero.isStopScanning)
            {
                hero.soonTargetter = null;
                hero.targetUnit = null;
                continue;
            }

            if (!isBossSpawned)
            {
                foreach (EnemyCharacter enemy in monsters)
                {
                    if (enemy.soonAttacker.Count >= enemy.soonAttackerLimit && enemy.soonAttackerLimit != -1)
                    {
                        continue;
                    }

                    //타겟으로 잡힌 몬스터는 미리 거리정보를 넣어줌
                    if (hero.targetUnit != null)
                    {
                        shortDistance = Vector3.Distance(hero.myObject.position, hero.targetUnit.position);
                    }

                    float dis = Vector3.Distance(hero.myObject.position, enemy.myObject.position);
                    if (dis < shortDistance)
                    {
                        shortDistance = dis;
                        nearMonster = enemy;
                    }
                }
            }
            else if(isBossSpawned)
            {
                foreach (EnemyCharacter enemy in bosses)
                {
                    if (enemy.soonAttacker.Count >= enemy.soonAttackerLimit && enemy.soonAttackerLimit != -1)
                    {
                        continue;
                    }

                    //타겟으로 잡힌 몬스터는 미리 거리정보를 넣어줌
                    if (hero.targetUnit != null)
                    {
                        shortDistance = Vector3.Distance(hero.myObject.position, hero.targetUnit.position);
                        nearMonster = enemy;
                    }

                    float dis = Vector3.Distance(hero.myObject.position, enemy.myObject.position);
                    if (dis < shortDistance)
                    {
                        shortDistance = dis;
                        nearMonster = enemy;
                    }
                }
            }


            if (nearMonster != null)
            {
                if (isBossSpawned)
                {
                    foreach (EnemyCharacter enemy in bosses)
                    {
                        if (enemy.soonAttacker.Contains(hero))
                        {
                            enemy.soonAttacker.Remove(hero);
                        }
                    }
                }
                else
                {
                    foreach (EnemyCharacter enemy in monsters)
                    {
                        if (enemy.soonAttacker.Contains(hero))
                        {
                            enemy.soonAttacker.Remove(hero);
                        }
                    }

                }

                if (hero.soonTargetter != null)
                {
                    if (hero.soonTargetter != nearMonster)
                    {   
                        hero.soonTargetter = nearMonster;
                        hero.soonTargetter.soonAttacker.Add(hero);
                        hero.targetUnit = hero.soonTargetter.myObject;
                    }
                    else
                    {
                        if(!nearMonster.soonAttacker.Contains(hero))
                        {
                            nearMonster.soonAttacker.Add(hero);
                        }
                        hero.targetUnit = hero.soonTargetter.myObject;
                    }
                }
                else
                {
                    hero.soonTargetter = nearMonster;
                    hero.soonTargetter.soonAttacker.Add(hero);
                    hero.targetUnit = hero.soonTargetter.myObject;
                }
            }
        }

        yield return new WaitForSeconds(scanDelay);
        isEnemyScanning = false;
    }

    protected IEnumerator ScanCharacter(float scanDelay)
    {
        if(isHeroScanning)
        {
            yield break;
        }

        isHeroScanning = true;

        inCharacters.Clear();

        Collider[] hitColliders = Physics.OverlapBox(getTransform.position, fieldSize / 2, Quaternion.identity, scanLayer);

        // 겹친 콜라이더에 대해 처리
        foreach (Collider hitCollider in hitColliders)
        {
            foreach(HeroCharacter hero in FieldManager.instance.heroList)
            {
                if(hitCollider == hero.myCollider)
                {
                    inCharacters.Add(hero);
                    CharacterFieldCalc(hero);
                }
            }
        }

        yield return new WaitForSeconds(scanDelay);
        isHeroScanning = false;
    }

    protected void BossSpawn()
    {
        if (bossPoint < maxBossPoint || isBossSpawned)
        {
            return;      
        }

        Debug.Log("보스 소환");
        bossPoint = 0;

        //카메라 이동 연출
        CameraController_InGame camera = FieldManager.instance.cameraController;
        camera.subCameraUsable.AddCoroutine(camera.subCameraUsable.CameraBossTracking(camera.transform.position, getTransform.position, 40f, this));

        //보스 소환 연출
        mySpawner.BossSpawn();

        //스캔된 캐릭터들의 타겟 지우기 and 재스캔
        foreach(HeroCharacter hero in inCharacters)
        {
            hero.targetUnit = null;
        }
        isEnemyScanning = false;
    }

    protected void CharacterFieldCalc(HeroCharacter character)
    {
        if(character.currentField != fieldName)
        {
            character.currentField = fieldName;
            character.isFieldEnter = true;

        }
    }


#if UNITY_EDITOR
    int segments = 100;
    bool drawWhenSelected = true;

    void OnDrawGizmosSelected()
    {
        if (drawWhenSelected)
        {
            for (float i = -0.02f; i <= 0.02f; i += 0.02f)
            {
                //탐지 시야
                Gizmos.color = Color.red;
                Vector3 offset = new Vector3(i, i, i);

                // 로컬 회전 설정
                Matrix4x4 originalMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(gizmosPoint.position + offset, gizmosPoint.rotation, Vector2.one);

                Gizmos.DrawWireCube(Vector2.zero, fieldSize);

                // 매트릭스 원래대로 돌리기
                Gizmos.matrix = originalMatrix;
            }

            //필드 내 스폰 범위
            Gizmos.color = Color.cyan;

            // 로컬 회전 설정
            Matrix4x4 spawnMatrix = Matrix4x4.TRS(gizmosPoint.position, gizmosPoint.rotation, Vector2.one);
            Gizmos.matrix = spawnMatrix;

            Gizmos.DrawWireCube(Vector2.zero, spawnSize);

            // 매트릭스 원래대로 돌리기
            Gizmos.matrix = spawnMatrix;
        }
    }
#endif

}
