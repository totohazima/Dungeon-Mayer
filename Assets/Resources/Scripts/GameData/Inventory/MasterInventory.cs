using GDBA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameInventory
{
    public class MasterInventory<T> where T : GameData
    {
        public List<T> inventory = new List<T>();
        public T this[int index]
        {
            get { return inventory[index]; }
            set { inventory[index] = value; }
        }
        //�κ��丮���� �����۰��� Ȯ��
        public int Count
        {
            get { return inventory.Count; }
        }

        //������ �߰�
        public virtual void Add(T item)
        {
            inventory.Add(item);
        }

        //������ ����
        public void Remove(T item)
        {
            inventory.Remove(item);
        }

        //�κ��丮 �ʱ�ȭ
        public void Clear()
        {
            inventory.Clear();
        }
    }
}
