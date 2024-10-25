using FieldHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAlgorithm : MonoBehaviour
{
    public HeroCharacter character;
    public FieldMap.Field targetField;
    public float combatTime = 0f;
    [SerializeField] private float combatTimer = 0f;
    private bool afterBuildingUse = false;

    private void Awake()
    {
        character = GetComponent<HeroCharacter>();

        Recycle();
    }

    private void Update()
    {
        CombatTimeUpdate();  
        
        StartCombatField();
        StartVillage();
    }

    private void CombatTimeUpdate()
    {
        if (!character.isReadyToAttack)
        {
            return; 
        }

        combatTimer -= Time.deltaTime;
    }

    private void StartCombatField()
    {
        if (combatTimer <= 0f || character.currentField == targetField)
        {
            return;    
        }

        character.targetField = targetField;
    }

    private void StartVillage()
    {
        if(combatTimer > 0f || character.isReadyToAttack)
        {
            return;
        }

        FieldActivity field = FieldManager.instance.fieldActivitys[(int)character.currentField];
        bool isReturn = false;

        foreach (EnemyCharacter enemy in field.monsters)
        {
            if(enemy.targetUnit == character.myObject)
            {
                isReturn = true;
            }
        }

        if (!isReturn)
        {
            character.targetField = FieldMap.Field.VILLAGE;
            character.isStopScanning = true;

            if (character.currentField == FieldMap.Field.VILLAGE)
            {
                StartCoroutine(StartBuilding());
            }
        }
    }

    private IEnumerator StartBuilding()
    {
        if (character.targetBuilding == Building.BuildingType.NONE && !afterBuildingUse)
        {
            afterBuildingUse = true;
            character.targetBuilding = GameManager.instance.GetRandomEnumValue<Building.BuildingType>(1, Enum.GetValues(typeof(Building.BuildingType)).Length - 1);
        }

        while (character.targetBuilding != Building.BuildingType.NONE)
        {
            yield return null;
        }

        Recycle();
    }

    private void Recycle()
    {
        combatTimer = combatTime;
        afterBuildingUse = false;
    }
}
