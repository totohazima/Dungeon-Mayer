using GDBA;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;
using static Pathfinding.GridGraph;

public class UICharacterCostume : MonoBehaviour
{
    [System.Serializable]
    public struct SCostumeInfo
    {
        public string key;
        public SpriteRenderer[] renderers;
    }
    public SCostumeInfo[] costumeInfos = null;

    [System.Serializable]
    public struct SWingCostumeInfo
    {
        public string key;
        public SpriteRenderer[] renderers;
    }
    public SWingCostumeInfo[] wingCostumeInfos = null;
    [System.Serializable]
    public struct SWeaponCostumeInfo
    {
        public string key;
        public SpriteRenderer[] renderers;
    }
    public SWeaponCostumeInfo[] weaponCostumeInfos = null;

    public string dressCostume_Code;
    private Texture costumeSprite;
    private Sprite[] sprites;
    private string texturePath;
    private Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();

    public void CostumeEquip_Process()
    {
        costumeSprite = null;
        sprites = null;

        LoadSprites();
        EquipCostumes();
    }

    private void LoadSprites()
    {
        texturePath = "Texture/Dresscostume/" + dressCostume_Code;
        // ���ҽ����� �ؽ�ó �ε�
        costumeSprite = Resources.Load<Texture>(texturePath);

        if (costumeSprite == null)
        {
            Debug.LogError("Failed to load texture from Resources.");
            return;
        }

        sprites = Resources.LoadAll<Sprite>(texturePath);

        if (sprites.Length == 0)
        {
            Debug.LogError("No sprites found in the texture.");
            return;
        }

        // ��ųʸ��� ��������Ʈ ����
        foreach (var sprite in sprites)
        {
            if (!spriteDictionary.ContainsKey(sprite.name))
            {
                spriteDictionary[sprite.name] = sprite;
            }
        }
    }

    private void EquipCostumes()
    {
        foreach (var costumeInfo in costumeInfos)
        {
            if (spriteDictionary.TryGetValue(costumeInfo.key, out Sprite sprite))
            {
                foreach (var renderer in costumeInfo.renderers)
                {
                    renderer.sprite = sprite; // ��������Ʈ ����
                }
            }
            else
            {
                foreach (var renderer in costumeInfo.renderers)
                {
                    renderer.sprite = null; // ��������Ʈ ����
                }
            }
        }
    }
}