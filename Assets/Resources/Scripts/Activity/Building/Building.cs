using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : FieldObject, ICustomUpdateMono
{
    public enum BuildingType
    {
        NONE = 0,
        TRADE = 1,
        MEDICAL = 2,
        BAR = 3,
        RESTAURANT = 4,
        INN = 5,
    }

    public Collider2D myCollider2D;
    [Header("Status")]
    public BuildingType buildingType;
    public string buildingName;
    public int builngLevel = 1;
    public float buildingDelay;
    public GameMoney.GameMoneyType giveItemType;
    private float scanDelay = 0.5f;
    [Header("Bool")]
    public bool isInteraction = false; //true일시 상호작용 불가
    private bool onScanning = false;
    [Header("Customer")]
    public List<Collider> scanHero = new List<Collider>();
    public List<HeroCharacter> customerList = new List<HeroCharacter>();
    public Transform interactionCenter = null;
    public Vector3 interactionRange = new Vector3(1f, 1f, 1f);
    public Transform popupTransform = null;
    private GameObject rewardText;
    private GameObject giveItem;
    private void Awake()
    {
        LoadPrefab();
    }
    
    protected void LoadPrefab()
    {
        rewardText = Resources.Load<GameObject>("Prefabs/FieldObject/BuildingRewardTxt");
        giveItem = Resources.Load<GameObject>("Prefabs/FieldObject/GiveItem");
    }

    public void OnEnable()
    {
        CustomUpdateManager.customUpdateMonos.Add(this);    
    }
    public void CustomUpdate()
    {
        StartCoroutine(characterScan());

        foreach(HeroCharacter waitingUnit in customerList)
        {
            waitingUnit.isWaitingBuilding = true;
        }

        if(customerList.Count > 0 && !isInteraction)
        {
            Interaction(customerList[0]);
        }
    }
    public void OnDisable()
    {
        CustomUpdateManager.customUpdateMonos.Remove(this);
    }   


    private IEnumerator characterScan()
    {
        if (!onScanning)
        {
            onScanning = true;

            scanHero.Clear();
            Collider[] colliders = Physics.OverlapBox(interactionCenter.position, interactionRange / 2, Quaternion.identity, 1 << 8);
            // 겹친 콜라이더에 대해 처리
            foreach (Collider hitCollider in colliders)
            {
                scanHero.Add(hitCollider);
            }

            foreach(Collider hero in scanHero)
            {
                foreach(HeroCharacter character in FieldManager.instance.heroList)
                {
                    if(hero == character.myCollider && character.targetBuilding == buildingType && !customerList.Contains(character))
                    {
                        customerList.Add(character);
                    }
                }
            }

            yield return new WaitForSeconds(scanDelay);
            onScanning = false;
        }
    }

    private void Interaction(HeroCharacter customer)
    {
        isInteraction = true;

        StartCoroutine(customer.popupController.Building_Interaction(this, buildingDelay));
    }

    //리스트로 받은 아이템 정보를 텍스트로 출력
    public void RewardPopup(GameMoney.GameMoneyType type, int count)
    {
        GameObject text = PoolManager.instance.Spawn(rewardText, interactionCenter.position, Vector3.one, Quaternion.identity, true, PoolManager.instance.spawnRoot);
        text.transform.position = popupTransform.position;

        BuildingRewardText floatText = text.GetComponent<BuildingRewardText>();
        floatText.TextSetting(type, count, popupTransform.position);
    }

    /// <summary>
    /// 건물 상호작용 애니메이션
    /// </summary>
    public IEnumerator Building_Interaction(HeroCharacter hero, float useDelay)
    {
        float giveDelay = 0.2f;
        int count = 1;
        for (int i = 0; i < count; i++)
        {
            AddGiveBuildingItem(giveItemType, Random.Range(1, 1), hero);
            yield return new WaitForSeconds(giveDelay);
        }

    }
    protected void AddGiveBuildingItem(GameMoney.GameMoneyType type, int count, HeroCharacter hero)
    {
        GameObject prefab = giveItem;
        GameObject popup = PoolManager.instance.Spawn(prefab, hero.popupCenter.position, new Vector3(0.5f, 0.5f, 0.5f)/*이 부분은 추후 이미지 사이즈가 통일될시 Vector3.One으로 대체해야함*/, Quaternion.identity, true, PoolManager.instance.spawnRoot);

        GiveItem popupItem = popup.GetComponent<GiveItem>();
        popupItem.character = hero;
        popupItem.ItemSetting(type, count, popupTransform.position);
    }

#if UNITY_EDITOR
    int segments = 100;
    bool drawWhenSelected = true;

    void OnDrawGizmosSelected()
    {
        if (drawWhenSelected)
        {
            if (interactionCenter != null)
            {
                //탐지 시야
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(interactionCenter.position, interactionRange);
            }
        }
    }
#endif
}
