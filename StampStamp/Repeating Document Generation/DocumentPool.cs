using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 풀링 가능한 오브젝트는 이 인터페이스를 구현해야 함
/// </summary>
public interface IPoolable
{
    GameObject OriginalPrefab { get; set; }
}

public class DocumentPool : Singleton<DocumentPool>
{
    // 프리팹 별로 큐 관리
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    [NonSerialized] public Canvas canvas; // UI 풀이 붙을 Canvas
    
    public int defaultInitialSize = 7; // 프리팹 처음 등록될 때 기본으로 만들 개수
    
    
    /// <summary>
    /// 오브젝트 요청
    /// </summary>
    public GameObject GetObject(GameObject prefab, Vector2 anchoredPosition)
    {
        // 프리팹에 대한 풀 없으면 초기화
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
            PrewarmPool(prefab, defaultInitialSize);
        }

        GameObject obj = poolDictionary[prefab].Count > 0 ? poolDictionary[prefab].Dequeue() : Instantiate(prefab);

        // IPoolable이면 OriginalPrefab 기록
        var poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
            poolable.OriginalPrefab = prefab;

        // RectTransform 기반 위치 설정
        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.SetParent(canvas.transform, false);
            rect.anchoredPosition = anchoredPosition;
            rect.localRotation = Quaternion.identity;
        }
        else
        {
            obj.transform.position = anchoredPosition;
            obj.transform.rotation = Quaternion.identity;
        }

        obj.SetActive(true);
        return obj;
    }
    
    private void PrewarmPool(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);

            var poolable = obj.GetComponent<IPoolable>();
            if (poolable != null)
                poolable.OriginalPrefab = prefab;

            var rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.SetParent(canvas.transform, false);
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
            }

            poolDictionary[prefab].Enqueue(obj);
        }
    }

    /// <summary>
    /// 오브젝트 반환
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);

        var poolable = obj.GetComponent<IPoolable>();
        if (poolable == null || poolable.OriginalPrefab == null)
        {
            Debug.LogWarning("ReturnObject: OriginalPrefab not found, destroying object.");
            Destroy(obj);
            return;
        }

        GameObject prefabKey = poolable.OriginalPrefab;

        // RectTransform 초기화
        var rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.SetParent(canvas.transform, false);
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.identity;
        }

        if (!poolDictionary.ContainsKey(prefabKey))
            poolDictionary[prefabKey] = new Queue<GameObject>();

        poolDictionary[prefabKey].Enqueue(obj);
    }

    /// <summary>
    /// 풀 전체 초기화
    /// </summary>
    public void ClearPool()
    {
        foreach (var queue in poolDictionary.Values)
        {
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null)
                    Destroy(obj);
            }
        }

        poolDictionary.Clear();
        Debug.Log("DocumentPool cleared.");
    }
}
