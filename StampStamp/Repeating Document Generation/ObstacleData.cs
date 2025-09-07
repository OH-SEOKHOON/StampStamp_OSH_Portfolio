using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new ObsData", menuName = "Game/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    public GameObject obstaclePrefab;
    
    // 장애물 인덱스
    public int obstacleObjIdx;
    
    // 장애물 체력
    [NonSerialized] public int processCount;
    
    // 장애물 스폰 포지션
    [NonSerialized] public float spawnPosX;
    [NonSerialized] public float spawnPosY;
}

public class ObstacleInstance
{
    public GameObject prefab;
    public int obstacleObjIdx;
    public int processCount;
    public Vector2 spawnPos;
}