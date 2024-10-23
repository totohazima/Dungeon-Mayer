using FieldHelper;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TestCheat : EditorWindow
{
    public FieldMethod fieldMethod;
    private bool showFieldCheat = true;
    public struct FieldMethod
    {
        public FieldMap.Field controllField;
        [Range(0, 100)] public int bossPoint;
    }

    [MenuItem("Window/GameCheatEditor")]
    public static void ShowWindow()
    {
        GetWindow<TestCheat>("Game Cheat Editor");
    }

    private void OnGUI()
    {
        // 가운데 정렬을 위한 스타일 생성
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.fontStyle = FontStyle.Bold; // 강조를 위해 굵게 설정
        centeredStyle.fontSize = 15;
        // 최상단에 고정된 제목 추가
        GUILayout.Label("Game Cheat Editor", centeredStyle);
        GUILayout.Space(10); // 제목과 GUI 사이에 여백 추가


        showFieldCheat = EditorGUILayout.Foldout(showFieldCheat, "필드 치트");
        if (showFieldCheat)
        {
            FieldCheatGUI();
        }
    }

    private void FieldCheatGUI()
    {
        //GUILayout.Label("Field Settings", EditorStyles.boldLabel);

        // Field controllField 선택하기
        fieldMethod.controllField = (FieldMap.Field)EditorGUILayout.EnumPopup("치트 적용 필드", fieldMethod.controllField);

        if (FieldManager.instance != null)
        {
            FieldActivity field = FieldManager.instance.fields[(int)fieldMethod.controllField];
            fieldMethod.bossPoint = (int)EditorGUILayout.Slider("보스 포인트", field.bossPoint, 0, 100);
        }

        if (GUILayout.Button("몬스터 최대 스폰"))
        {
            maxMonsterSpawn();
        }

        if (GUILayout.Button("보스 스폰"))
        {
            BossSpawn();
        }
    }

    private void maxMonsterSpawn()
    {
        if (FieldManager.instance == null)
        {
            Debug.LogError("FieldManager.instance is Null");
            return;
        }
        FieldActivity field = FieldManager.instance.fields[(int)fieldMethod.controllField];

        int enoughCount = field.mySpawner.maxSpawnUnitCount - field.monsters.Count;
        field.mySpawner.MonsterSpawn(enoughCount);
    }

    private void BossSpawn()
    {
        if (FieldManager.instance == null)
        {
            Debug.LogError("FieldManager.instance is Null");
            return;
        }
        FieldActivity field = FieldManager.instance.fields[(int)fieldMethod.controllField];

        field.BossSpawn(field.maxBossPoint);
    }
}
