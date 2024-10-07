using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldHelper;
using StatusHelper;
using Unity.VisualScripting;
public class FieldActivity : MonoBehaviour, ICustomUpdateMono
{
    public Transform getTransform;
    public FieldMap.Field controlField;
    public List<HeroCharacter> inCharacters = new List<HeroCharacter>();
    public LayerMask scanLayer;
    private bool onScanning = false;
    public Vector3 boxSize = new Vector3(1f, 1f, 1f);
    [HideInInspector] public FieldSpawner mySpawner;
    [Header("Monsters")]
    public List<EnemyCharacter> monsters = new List<EnemyCharacter>();
    [Header("Boss")]
    public bool isBossSpawned = false;
    public List<EnemyCharacter> bosses = new List<EnemyCharacter>();
    [HideInInspector] public int maxBossPoint = 100;
    [Range(0, 100)]public int bossPoint = 0;

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
        if(!onScanning)
        {
            StartCoroutine(ScanCharacter());
        }

        if(bossPoint >= maxBossPoint && !FieldManager.instance.isAlreadyBossSpawn)
        {
            BossSpawn();
        }

        AlwaysEliteTargetting();
    }

    protected void BossSpawn()
    {
        Debug.Log("���� ��ȯ");
        bossPoint = 0;

        //ī�޶� �̵� ����
        CameraUsable camera = FieldManager.instance.cameraUsable;
        camera.subCameraUsable.AddCoroutine(camera.subCameraUsable.CameraBossTracking(camera.transform.position, getTransform.position, 20f, this));

        //���� ��ȯ ����
        StartCoroutine(mySpawner.BossSpawn());

        //��ĵ�� ĳ���͵��� Ÿ���� ������ �������Ѿ� ��
        HeroEliteCombatCalc(true);
    }

    protected IEnumerator ScanCharacter()
    {
        onScanning = true;

        inCharacters.Clear();

        Collider[] hitColliders = Physics.OverlapBox(getTransform.position, boxSize / 2, Quaternion.identity, scanLayer);

        // ��ģ �ݶ��̴��� ���� ó��
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

        yield return new WaitForSeconds(0.5f);
        onScanning = false;
    }
    protected void CharacterFieldCalc(HeroCharacter character)
    {
        if(character.myField != controlField)
        {
            character.myField = controlField;
            character.isFieldEnter = true;

        }
    }

    public void HeroEliteCombatCalc(bool isCombat)
    {
        foreach (HeroCharacter character in inCharacters)
        {
            character.isEliteCombat = isCombat;
        }
    }

    //������ ������ ���¿��� �ʵ� �� ���ֵ鿡�� ���� Ÿ����
    public void AlwaysEliteTargetting()
    {
        if (isBossSpawned)
        {
            foreach (HeroCharacter character in inCharacters)
            {
                if (character.targetUnit == null)
                {
                    character.targetUnit = bosses[0].myObject;
                }
            }
        }
    }
   

#if UNITY_EDITOR
    int segments = 100;
    bool drawWhenSelected = true;

    void OnDrawGizmosSelected()
    {
        if (drawWhenSelected)
        {
            //Ž�� �þ�
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(getTransform.position, boxSize);
        }
    }

   
#endif
}
