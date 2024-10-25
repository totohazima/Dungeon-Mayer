using FieldHelper;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldManager : MonoBehaviour
{
    public static FieldManager instance;
    public AstarPath astarPath;
    public CameraController_InGame cameraController;
    public Transform spawnPool;
    public List<Transform> fieldList = new List<Transform>();
    [HideInInspector] public List<FieldActivity> fieldActivitys = new List<FieldActivity>();
    [HideInInspector] public List<FieldSpawner> fieldSpawners = new List<FieldSpawner>();

    [Range(0, 10)] public int spawnHeroCount = 1;
    public List<HeroCharacter> heroList = new List<HeroCharacter>();

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < fieldList.Count; i++)
        {
            if (fieldList[i].GetComponent<FieldActivity>() != null)
            {
                fieldActivitys.Add(fieldList[i].GetComponent<FieldActivity>());
            }
            if (fieldList[i].GetComponentInChildren<FieldSpawner>() != null)
            {
                fieldSpawners.Add(fieldList[i].GetComponentInChildren<FieldSpawner>());
            }
        }   
    }

    void Start()
    {
        HeroSpawn();
        AllFieldSpawn();
    }

    protected void HeroSpawn()
    {
        foreach(FieldActivity activity in fieldActivitys)
        {
            if(activity.FieldName == FieldMap.Field.VILLAGE)
            {
                activity.mySpawner.HeroSpawn(spawnHeroCount);
            }
        }
    }

    public void AllFieldSpawn()
    {
        foreach(FieldSpawner field in fieldSpawners)
        {
            field.isReadyFieldAllSpawn = true;
        }
    }


}
