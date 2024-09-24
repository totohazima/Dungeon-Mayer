using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDA_Controller : MonoBehaviour
{ 
    // ������ ���� ����� Ŭ����
    class AttackerInfo
    {
        public Character attacker; // ������ ��ü
        public Coroutine countdownCoroutine; // Ÿ�̸ӿ� �ڷ�ƾ
    }

    // ������ ����Ʈ�� Ÿ�̸� ����
    private List<AttackerInfo> attackerList = new List<AttackerInfo>();
    private Dictionary<Character, AttackerInfo> attackerTimers = new Dictionary<Character, AttackerInfo>();

    // �����ڰ� ���� �� ȣ���ϴ� �Լ�
    public void OnAttacked(Character attacker)
    {
        if (attackerTimers.ContainsKey(attacker))
        {
            // �̹� ����Ʈ�� ������ Ÿ�̸� �ʱ�ȭ
            ResetAttackerTimer(attacker);
        }
        else
        {
            // ���ο� ������ ������ �߰�
            AddAttacker(attacker);
        }
    }

    // ���ο� ������ �߰� �Լ�
    protected void AddAttacker(Character attacker)
    {
        // ������ ������ ����Ʈ�� �߰�
        AttackerInfo newAttackerInfo = new AttackerInfo
        {
            attacker = attacker,
            countdownCoroutine = StartCoroutine(RemoveAttackerAfterTime(attacker, 10f)) // 10�� Ÿ�̸� ����
        };

        attackerList.Add(newAttackerInfo);
        attackerTimers.Add(attacker, newAttackerInfo);
    }

    // Ÿ�̸� �ʱ�ȭ �Լ�
    protected void ResetAttackerTimer(Character attacker)
    {
        if (attackerTimers.ContainsKey(attacker))
        {
            // ���� Ÿ�̸� ����
            StopCoroutine(attackerTimers[attacker].countdownCoroutine);
            // �� Ÿ�̸� ����
            attackerTimers[attacker].countdownCoroutine = StartCoroutine(RemoveAttackerAfterTime(attacker, 10f));
        }
    }

    // �����ڸ� 10�� �� ����Ʈ���� �����ϴ� �Լ�
    protected IEnumerator RemoveAttackerAfterTime(Character attacker, float time)
    {
        yield return new WaitForSeconds(time);

        // Ÿ�̸� ���� �� �����ڸ� ����Ʈ�� Dictionary���� ����
        if (attackerTimers.ContainsKey(attacker))
        {
            attackerList.Remove(attackerTimers[attacker]);
            attackerTimers.Remove(attacker);
        }
    }

    public void KDA_Calculator(Character killAttacker, Character myCharacter)
    {
        foreach (AttackerInfo attackerInfo in attackerList)
        {
            if (attackerInfo.attacker == killAttacker)
            {
                attackerInfo.attacker.playStatus_KDA.kill_Score++;
            }
            else
            {
                attackerInfo.attacker.playStatus_KDA.assist_Score++;
            }
        }

        myCharacter.playStatus_KDA.death_Score++;
    }

}

