using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

/// <summary>
/// ���̰��� �ִ� Ǯ�Ŵ����� �����
/// </summary>
namespace GameSystem
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager instance;
        public Transform spawnRoot; //Ȱ��ȭ�� �������� �θ�
        public Transform bufferRoot; //��Ȱ��ȭ�� �������� �θ�
        Dictionary<GameObject, Queue<GameObject>> playBuffer = new Dictionary<GameObject, Queue<GameObject>>();
        Dictionary<string, Queue<GameObject>> backBuffer = new Dictionary<string, Queue<GameObject>>();


        public static void InitInstance()
        {
            instance = FindObjectOfType<PoolManager>();
            instance.bufferRoot = instance.transform;
            DontDestroyOnLoad(instance.gameObject);
        }

        public void CreatePooling(GameObject poolObj, int count)
        {
            Queue<GameObject> pool = null;
            if (backBuffer.ContainsKey(poolObj.name) == false)
            {
                pool = new Queue<GameObject>();
                backBuffer[poolObj.name] = pool;
            }
            else
            {
                pool = backBuffer[poolObj.name];
            }

            if (pool.Count >= count)
            {
                return;
            }

            int preCount = count - pool.Count;
            for (int i = 0; i < preCount; i++)
            {
                backBuffer[poolObj.name].Enqueue(InstantiatePrefab(poolObj));
            }
        }

        private GameObject InstantiatePrefab(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab);
            if (bufferRoot != null)
            {
                obj.transform.parent = bufferRoot;
            }
            obj.SetActive(false);
            return obj;
        }

        public GameObject SpawnPooling(string prefabName, Vector3 position, Vector3 scale, Quaternion rotation, bool active, Transform parent)
        {
            if (backBuffer.ContainsKey(prefabName) == false)
            {
                return null;
            }

            GameObject clone = null;
            Queue<GameObject> pool = null;
            try
            {
                pool = backBuffer[prefabName];
                clone = pool.Dequeue();
                playBuffer.Add(clone, pool);
                if (parent != null)
                {
                    clone.transform.parent = parent;
                }
                else
                {
                    clone.transform.parent = spawnRoot;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("PoolManager ���ܹ߻�" + "\n������ ����: " + e.GetType() + "\n���� ����: " + e.StackTrace);
                return null;
            }

            clone.transform.position = position;
            clone.transform.rotation = rotation;
            clone.transform.localScale = scale;

            clone.SetActive(active);

            return clone;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Vector3 scale, Quaternion rotation, bool active, Transform parent)
        {
            if (backBuffer.ContainsKey(prefab.name) == false || backBuffer[prefab.name].Count < 1)
            {
                CreatePooling(prefab, 1);
            }

            return SpawnPooling(prefab.name, position, scale, rotation, active, parent);
        }

        /// <summary>
        /// Ǯ������ ������ ������Ʈ���� ���� ���� backBuffer�� ����Ŵ
        /// </summary>
        public void FalsedPrefab(GameObject falsedPrefab, string dictionaryName)
        {
            string deleteString = "(Clone)";
            string dicName = dictionaryName;
            dicName = dicName.Replace(deleteString, "");

            playBuffer.Remove(falsedPrefab);
            backBuffer[dicName].Enqueue(falsedPrefab);
            falsedPrefab.transform.SetParent(bufferRoot);
            falsedPrefab.SetActive(false);
        }
    }
}
