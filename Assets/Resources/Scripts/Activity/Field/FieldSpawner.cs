using FieldHelper;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;
using UnityEngine;

public class FieldSpawner : MonoBehaviour, ICustomUpdateMono
{
    public FieldActivity fieldActivity;
    public bool isReadyFieldAllSpawn = false; //true일 경우 리스폰
    public int maxSpawnUnitCount; //유닛 수 제한
    [SerializeField] private float reSpawnReadyTime;
    private float reSpawnReadyTimer;

    [SerializeField] private List<EnemyCharacter> monster = new List<EnemyCharacter>();
    private EnemyCharacter boss;

    void Awake()
    {
        GameObject unitPrefab = Resources.Load<GameObject>("Prefabs/Enemys/mn_000");
        EnemyCharacter mn = unitPrefab.GetComponent<EnemyCharacter>();
        monster.Add(mn);

        unitPrefab = Resources.Load<GameObject>("Prefabs/Enemys/mn_002");
        mn = unitPrefab.GetComponent<EnemyCharacter>();
        monster.Add(mn);


        GameObject boosPrefab = Resources.Load<GameObject>("Prefabs/Enemys/mn_000_Boss");
        boss = boosPrefab.GetComponent<EnemyCharacter>();
    }
    private void OnEnable()
    {
        CustomUpdateManager.customUpdateMonos.Add(this);   
    }
    public void CustomUpdate()
    {
        if(isReadyFieldAllSpawn)
        {
            MonsterSpawn(maxSpawnUnitCount);
            isReadyFieldAllSpawn = false;
        }
        else if(fieldActivity.monsters.Count < maxSpawnUnitCount && reSpawnReadyTimer >= reSpawnReadyTime)
        {
            MonsterSpawn(1);
            reSpawnReadyTimer = 0f;
        }

        reSpawnReadyTimer += Time.deltaTime;
    }
    private void OnDisable()
    {
        CustomUpdateManager.customUpdateMonos.Remove(this);
    }
    public void MonsterSpawn(int count)
    {
        List<EnemyCharacter> prefabs = new List<EnemyCharacter>();
        List<Vector3> pos = new List<Vector3>();

        for(int i = 0; i < count; i++)
        {
            int num = Random.Range(0, monster.Count);
            prefabs.Add(monster[num]);
        }

        pos = SpawnPointSet(count);
        UnitSpawn(prefabs, count, pos);

    }
    public void HeroSpawn(int count)
    {
        List<Vector3> pos = new List<Vector3>();
        pos = SpawnPointSet(count);

        for (int i = 0; i < pos.Count; i++)
        {
            GameObject heroPrefab = Resources.Load<GameObject>("Prefabs/Player/Hero");

            HeroCharacter character = heroPrefab.GetComponent<HeroCharacter>();
            CharacterInfoTable.Data data = GameManager.instance.gameDataBase.characterInfoTable.table[i];

            character.code = data.characterCode;
            character.characterName = data.characterName;
            character.jobClass = data.characterClass;
            character.characterCostume.dressCostume_Code = data.characterCostumeCode;

            GameObject hero = PoolManager.instance.Spawn(character.gameObject, pos[i], Vector3.one, Quaternion.identity, true, FieldManager.instance.spawnPool);
            TestAlgorithm algorithm = hero.GetComponent<TestAlgorithm>();
            if (i <= 4)
            {
                algorithm.targetField = FieldMap.Field.DESERT;
            }
            else
            {
                algorithm.targetField = FieldMap.Field.SNOW;
            }

            HeroCharacter heroCharacter = hero.GetComponent<HeroCharacter>();
            FieldManager.instance.heroList.Add(heroCharacter);
        }
    }

    protected void UnitSpawn(List<EnemyCharacter> prefab, int count, List<Vector3> spawnPos)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject monster = PoolManager.instance.Spawn(prefab[i].gameObject, spawnPos[i], Vector3.one, Quaternion.identity, true, FieldManager.instance.spawnPool);

            EnemyCharacter monsterCharacter = monster.GetComponent<EnemyCharacter>();
            monsterCharacter.currentField = fieldActivity.FieldName;
            monsterCharacter.targetField = fieldActivity.FieldName;
            monsterCharacter.soonAttackerLimit = 1;
            fieldActivity.monsters.Add(monsterCharacter);
        }
    }

    //보스를 스폰 시키고 보스 등장 연출
    public void BossSpawn()
    {
        fieldActivity.isBossSpawned = true;

        GameObject monster = PoolManager.instance.Spawn(boss.gameObject, fieldActivity.getTransform.position, Vector3.one, Quaternion.identity, true, FieldManager.instance.spawnPool);

        EnemyCharacter monsterCharacter = monster.GetComponent<EnemyCharacter>();
        monsterCharacter.currentField = fieldActivity.FieldName;
        monsterCharacter.targetField = fieldActivity.FieldName;
        monsterCharacter.soonAttackerLimit = -1;
        fieldActivity.bosses.Add(monsterCharacter);
        monsterCharacter.enabled = false;

        StartCoroutine(BossSpawnAnimation(monsterCharacter, 1.5f));
    }

    protected List<Vector3> SpawnPointSet(int count)
    {
        //매번 새로운 시드로 설정
        Random.InitState(System.DateTime.Now.Millisecond);

        List<Vector3> pos = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            Vector3 boxSize = fieldActivity.spawnSize;
            // 오버랩 박스 내에서 무작위 위치 생성
            Vector3 randomPositionWithinBox = new Vector3(
                Random.Range(-boxSize.x / 2, boxSize.x / 2),
                Random.Range(-boxSize.y / 2, boxSize.y / 2),
                0
            );

            Vector3 targetPos = fieldActivity.gizmosPoint.position + randomPositionWithinBox;
            pos.Add(targetPos);
        }
        return pos;
    }

    protected IEnumerator BossSpawnAnimation(EnemyCharacter boss, float fadeDuration)
    {
        foreach (SpriteRenderer sprite in boss.spriteGroup.spriteRenderers)
        {
            StartCoroutine(FadeSprite(sprite, fadeDuration, 0f, 1f)); // 페이드 인: 알파 0에서 1로
        }

        yield return new WaitForSeconds(fadeDuration); // 모든 페이드가 끝날 때까지 대기

        boss.enabled = true;
    }

    private IEnumerator FadeSprite(SpriteRenderer spriteRenderer, float duration, float startAlpha, float endAlpha)
    {
        // 초기 색상을 가져옴
        Color color = spriteRenderer.color;
        float elapsed = 0f;

        // duration 동안 페이드 효과
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 알파값을 점진적으로 변화
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            spriteRenderer.color = color;

            yield return null; // 다음 프레임까지 대기
        }

        // 최종 알파값 적용 (마지막 프레임에서 정확히 맞추기 위해)
        color.a = endAlpha;
        spriteRenderer.color = color;
    }


}
