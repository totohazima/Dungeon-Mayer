using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FieldSpawner : MonoBehaviour, ICustomUpdateMono
{
    public FieldActivity fieldActivity;
    public bool isSpawn = false; //true일 경우 리스폰
    public int spawnUnitCount; //유닛 수 제한
    public Transform spawnPointGroup;
    public List<Transform> spawnPoints = new List<Transform>();

    private GameObject unitPrefab;
    private EnemyCharacter monster;

    void Awake()
    {
        for(int i = 0; i < spawnPointGroup.childCount; i++)
        {
            spawnPoints.Add(spawnPointGroup.GetChild(i).transform);
        }

        unitPrefab = Resources.Load<GameObject>("Prefabs/Enemys/mn_000");
        monster = unitPrefab.GetComponent<EnemyCharacter>();
    }
    private void OnEnable()
    {
        CustomUpdateManager.customUpdateMonos.Add(this);   
    }
    public void CustomUpdate()
    {
        if(fieldActivity.monsters.Count < spawnUnitCount || isSpawn)
        {
            SpawnSetting();
            isSpawn = false;
        }
    }
    private void OnDisable()
    {
        CustomUpdateManager.customUpdateMonos.Remove(this);
    }

    protected void SpawnSetting()
    {
        EnemyCharacter prefabs = new EnemyCharacter();
        Vector3 pos = Vector3.zero;

        prefabs = monster;
        pos = SpawnPointSet();

        UnitSpawn(prefabs, pos);
    }
    protected void UnitSpawn(EnemyCharacter prefab,Vector3 spawnPos)
    {
        prefab.myField = fieldActivity.controlField;
        GameObject monster = PoolManager.instance.Spawn(prefab.gameObject, spawnPos, Vector3.one, Quaternion.identity, true, FieldManager.instance.spawnPool);
        monster.transform.position = spawnPos;

        EnemyCharacter monsterCharacter = monster.GetComponent<EnemyCharacter>();
        fieldActivity.monsters.Add(monsterCharacter);
    }

    protected Vector3 SpawnPointSet()
    {
        Vector3 pos = Vector3.zero;

        float[] chanceList = new float[spawnPoints.Count];

        for (int i = 0; i < chanceList.Length; i++)
        {
            chanceList[i] = 1f / chanceList.Length; // 각 스폰 포인트의 확률을 동일하게 설정
        }

        int index = GameManager.instance.Judgment(chanceList);

        Vector3 randPos = spawnPoints[index].position;
        pos = randPos;

        return pos;
    }

}
