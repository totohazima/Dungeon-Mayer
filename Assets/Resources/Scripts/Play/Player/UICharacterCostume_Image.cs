using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterCostume_Image : MonoBehaviour
{
    [System.Serializable]
    public struct SCostumeInfo_Image
    {
        public string key;
        public Image[] renderers;
    }
    public SCostumeInfo_Image[] costumeInfos = null;

    [System.Serializable]
    public struct SWingCostumeInfo_Image
    {
        public string key;
        public Image[] renderers;
    }
    public SWingCostumeInfo_Image[] wingCostumeInfos = null;

    [System.Serializable]
    public struct SWeaponCostumeInfo_Image
    {
        public string key;
        public Image[] renderers;
    }
    public SWeaponCostumeInfo_Image[] weaponCostumeInfos = null;

    public string dressCostume_Code;
    private Texture costumeSprite;
    private Sprite[] Images;
    private string texturePath;
    private Dictionary<string, Sprite> ImageDictionary = new Dictionary<string, Sprite>();

    public void CostumeEquip_Process()
    {
        costumeSprite = null;
        Images = null;

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

        Images = Resources.LoadAll<Sprite>(texturePath);

        if (Images.Length == 0)
        {
            Debug.LogError("No sprites found in the texture.");
            return;
        }

        // ��ųʸ��� ��������Ʈ ����
        foreach (var sprite in Images)
        {
            if (!ImageDictionary.ContainsKey(sprite.name))
            {
                ImageDictionary[sprite.name] = sprite;
            }
        }
    }

    private void EquipCostumes()
    {
        foreach (var costumeInfo in costumeInfos)
        {
            if (ImageDictionary.TryGetValue(costumeInfo.key, out Sprite sprite))
            {
                foreach (var renderer in costumeInfo.renderers)
                {
                    ChangeImageColor(new Color(1f, 1f, 1f, 1f), renderer);
                    renderer.sprite = sprite; // ��������Ʈ ����
                }
            }
            else
            {
                foreach (var renderer in costumeInfo.renderers)
                {
                    ChangeImageColor(new Color(1f, 1f, 1f, 0f), renderer);
                    renderer.sprite = null; // ��������Ʈ ����
                }
            }
        }
    }

    public void ChangeImageColor(Color newColor, Image image)
    {
        if (image != null)
        {
            image.color = newColor; // �̹��� ���� ����
        }
    }
}
