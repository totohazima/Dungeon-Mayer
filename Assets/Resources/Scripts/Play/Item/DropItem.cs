using GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : FieldObject, IPointerClickHandler
{
    [Header("ItemInfo")]
    public GameMoney.GameMoneyType moneyType;
    public int dropCount = 1;
    [SerializeField] private bool isGetItem = false; //true�� ��� ���� ����

    public float delay = 1f;
    public float elapsedTime = 0f; 
    public void Drop_Animation(Vector3 dropPos, Vector3 ownerPos)
    {
        float delay = 0.1f;
        Vector3 bouncePos = CalculateControlPoint(ownerPos, dropPos, 0.3f);
        //Vector3 bouncePos = new Vector3(dropPos.x, dropPos.y + 0.4f, dropPos.z);

        LTDescr tween = LeanTween.move(gameObject, bouncePos, delay).setEase(LeanTweenType.easeOutQuart);
        tween.setOnComplete(DropDown_Animation);

        void DropDown_Animation()
        {
            LTDescr tween = LeanTween.move(gameObject, dropPos, delay).setEase(LeanTweenType.easeInSine).setFrom(bouncePos);
            tween.setOnComplete(End_Animation);
        }
        void End_Animation()
        {
            isGetItem = true;
        }
    }

    public IEnumerator Drop_Animations(Vector3 startPos, Vector3 endPos)
    {
        delay = 1f;
        elapsedTime = 0f;
        Vector3 middlePos = CalculateControlPoint(startPos, endPos, 2f);

        while (elapsedTime < delay)
        {
            float t = elapsedTime / delay;
            Vector3 point = CalculateQuadraticBezierPoint(t, startPos, middlePos, endPos);

            transform.position = point;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        isGetItem = true;
    }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isGetItem)
        {
            GetItem();
        }
    }

    protected void GetItem()
    {
        PlayerInfo playerInfo = GameManager.instance.gameDataBase.playerInfo;

        switch(moneyType)
        {
            case GameMoney.GameMoneyType.GOLD:
                playerInfo.gold += dropCount;
                break;
            case GameMoney.GameMoneyType.RUBY:
                playerInfo.ruby += dropCount;
                break;
        }

        ShowFloatingText();
        Disappear();
    }

    protected void ShowFloatingText()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/FieldObject/ItemRewardTxt");
        GameObject text = PoolManager.instance.Spawn(prefab, myObject.position, Vector3.one, Quaternion.identity, true, myObject.parent);

        ItemRewardText floatText = text.GetComponent<ItemRewardText>();
        floatText.TextSetting(moneyType, dropCount);
    }
    protected void Disappear()
    {
        isGetItem = false;
        PoolManager.instance.Release(gameObject);    
    }

    /// <summary>
    /// ������ ����� Ư�� ������ ��ǥ�� ����ϴ� �Լ�
    /// </summary>
    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    /// <summary>
    /// �������� ����, � ���̸� �Է¹޾� �������� ����ϴ� �Լ�
    /// </summary>
    Vector3 CalculateControlPoint(Vector3 startPoint, Vector3 endPoint, float curveHeight)
    {
        // �� ���� �߰� ������ ���
        Vector3 middlePoint = (startPoint + endPoint) / 2f;

        // �߰� �������� ���� �Ʒ��� �������� �־� ��� ���� (� ���� ����)
        middlePoint.y += curveHeight; // y������ �־����� ���

        return middlePoint; // ������ ��ȯ
    }

}
