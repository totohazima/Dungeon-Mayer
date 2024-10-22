using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GDBA;
using UnityEditor;
using GameSystem;
using UnityEngine.U2D;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameDataBase gameDataBase;

    [Header("Manager")]
    public EffectManager effectManager;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        ScriptInit();

        Application.targetFrameRate = 60;
        //LoadingManager.instance.LoadScene("Stage");
    }

    private void ScriptInit()
    {
        gameDataBase.InitInstance();
        PoolManager.InitInstance();
    }

    /// <summary>
    /// 랜덤 기능 함수
    /// </summary>
    public int Judgment(float[] rando)
    {
        int count = rando.Length;
        float max = 0;
        for (int i = 0; i < count; i++)
            max += rando[i];

        float range = UnityEngine.Random.Range(0f, (float)max);
        //0.1, 0.2, 30, 40
        double chance = 0;
        for (int i = 0; i < count; i++)
        {
            chance += rando[i];
            if (range > chance)
                continue;

            return i;
        }

        return -1;
    }


    /// <summary>
    /// 열거형에서 랜덤으로 값을 가져옴
    /// </summary>
    /// <param name="minNumber">몇번부터 가져올건지 Ex)0번이 NONE일 경우 가져오지 않게하기 위함</param>
    public T GetRandomEnumValue<T>(int minNumber, int maxNumber)
    {
        T[] values = (T[])System.Enum.GetValues(typeof(T));

        // 랜덤 인덱스로 하나를 선택
        return values[Random.Range(minNumber, maxNumber + 1)];
    }
}
    
