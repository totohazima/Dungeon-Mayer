using FieldHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCharacterAlgorithm_Step3 : MonoBehaviour
{
    public HeroCharacter character;
    public FieldMap.Field combatField;
    public float combatTime = 0f;
    [SerializeField] private float combatTimer = 0f;

    private void Start()
    {
        character = GetComponent<HeroCharacter>();
        
        StartCoroutine(GoCombatField(combatField));
    }
    private IEnumerator GoCombatField(FieldMap.Field combatField)
    {
        character.targetField = combatField;
        character.targetUnit = null;
        combatTimer = combatTime;

        while (true)
        {
            if (character.currentField == combatField)
            {
                StartCoroutine(CombatField());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator CombatField()
    {
        while (true)
        {
            if (character.isReadyToAttack)
            {
                combatTimer -= Time.deltaTime;
            }

            if (combatTimer <= 0f && !character.isReadyToAttack || FieldManager.instance.fieldActivitys[(int)combatField].monsters.Count == 0)
            {
                StartCoroutine(GoVillage());
                yield break;
            }

            yield return null;
        }
    }
    private IEnumerator GoVillage()
    {
        character.targetField = FieldMap.Field.VILLAGE;
        character.targetUnit = null;
        while(true)
        {
            character.isStopScanning = true;

            if(character.currentField == FieldMap.Field.VILLAGE)
            {
                character.targetBuilding = GameManager.instance.GetRandomEnumValue<Building.BuildingType>(1, Enum.GetValues(typeof(Building.BuildingType)).Length);
                StartCoroutine(GoBuilding());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator GoBuilding()
    {
        
        while (true)
        {
            if(character.targetBuilding == Building.BuildingType.NONE)
            {
                StartCoroutine(GoCombatField(combatField));
                yield break;
            }

            yield return null;
        }
    }
}
