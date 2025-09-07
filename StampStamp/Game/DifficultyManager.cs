//제가 작성한 문서이나 레벨 상승 모니터링 메서드 부분은 제가 기여한 것이 아닙니다.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : Singleton<DifficultyManager>
{
    // 하루 시간 (게임 내 일과 시간 총량)
    private int _dayTime = 10;
    public int DayTime => _dayTime;
    
    // 난이도 상승 주기 (예: n일마다 난이도 증가)
    private int _difficultyIncreaseInterval = 5;
    
    // 난이도별 시간 감소 주기 (초당 1씩감소)
    private float[] _timeDecreaseRates = { 1.4f, 1f, 0.6f, 0.4f, 0.2f };
    
    // 서류 처리 보상 (난이도별 일과 시간 회복량)
    private float[] _reward = { 1f, 1f, 1.5f, 2f, 3f };
    
    // 피버시 서류 처리 보상
    private float[] _feverReward = { 0.75f, 0.75f, 1f, 1f, 2f };
    
    // 서류 처리로 차오르는 피버 값
    private int[] _feverValueReward = { 3, 3, 3, 3, 3 };
    
    // 서류 처리 실수 패널티 (난이도별 일과 시간 손실량)
    private int[] _penalty = { 3, 5, 7, 9, 12 };
    
    // 서류 처리 실수로 잃는 피버 값 (현재 피버값에 해당값만큼을 곱한 값이 차감됨)
    private float[] _feverValuePenalty = { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
    
    // 장애물 처리 횟수
    private int[] _obstacleProcessingCount = { 1, 1, 2, 2, 3 };

    // 장애물 등장 확률 (단위: 퍼센트)
    private int[] _obstacleSpawnProbability = { 4, 10, 12, 20, 30 };
    
    // 서류 연출 딜레이 시간 (버튼을 누른 뒤 다시 버튼을 누를 수 있게 되기까지의 시간)
    private float[] _documentDelay = { 0.6f, 0.5f, 0.4f, 0.3f, 0.2f };
    
    // 피버시 서류 연출 딜레이 시간 (버튼을 누른 뒤 다시 버튼을 누를 수 있게 되기까지의 시간)
    private float[] _feverDocumentDelay = { 0.4f, 0.35f, 0.3f, 0.25f, 0.2f };

    //현재 난이도와 상승 타이밍을 체크하기 위한 List
    private List<bool> levelMonitor = new List<bool>();
    public static event Action OnLevelChanged; //난이도 상승때 동작하기 위한 이벤트. 구독하면 됩니다.
    
    //현재 day에 맞춰 난이도를 산출하는 메서드
    public int GetLevel(int day)
    {
        return day / _difficultyIncreaseInterval;
    }
    
    //현재 day에 맞춰 난이도별 시간 감소 주기를 반환하는 메서드
    public float GetTimeDecreaseRate(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _timeDecreaseRates.Length - 1);
        return _timeDecreaseRates[level];
    }
    
    //현재 day에 맞춰 서류 처리 보상을 반환하는 메서드
    public float GetReward(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _reward.Length - 1);
        return _reward[level];
    }
    
    //현재 day에 맞춰 피버시 서류 처리 보상을 반환하는 메서드
    public float GetFeverReward(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _feverReward.Length - 1);
        return _feverReward[level];
    }
    
    //현재 day에 맞춰 피버 게이지 증가량을 반환하는 메서드
    public int GetFeverValueReward(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _feverValueReward.Length - 1);
        return _feverValueReward[level];
    }
    
    //현재 day에 맞춰 피버 게이지 감소량을 반환하는 메서드
    public float GetFeverValuePenalty(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _feverValuePenalty.Length - 1);
        return _feverValuePenalty[level];
    }
    
    //현재 day에 맞춰 서류 처리 실수 패널티를 반환하는 메서드
    public int GetPenalty(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _penalty.Length - 1);
        return _penalty[level];
    }
    
    //현재 day에 맞춰 장애물 처리 횟수를 반환하는 메서드
    public int GetObstacleProcessingCount(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _obstacleProcessingCount.Length - 1);
        return _obstacleProcessingCount[level];
    }

    //현재 day에 맞춰 장애물 등장 확률을 반환하는 메서드
    public int GetObstacleSpawnProbability(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _obstacleSpawnProbability.Length - 1);
        return _obstacleSpawnProbability[level];
    }
    
    // 현재 day에 맞춰 서류 연출 딜레이 시간을 반환하는 메서드
    public float GetDocumentDelay(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _documentDelay.Length - 1);
        return _documentDelay[level] / 4;
    }
    
    // 현재 day에 맞춰 피버시 서류 연출 딜레이 시간을 반환하는 메서드
    public float GetFeverDocumentDelay(int day)
    {
        int level = GetLevel(day);
        level = Mathf.Min(level, _feverDocumentDelay.Length - 1);
        return _feverDocumentDelay[level] / 4;
    }
    
    //레벨 상승을 감지하는 Monitor 초기화
    public void InitLevelMonitor()
    {
        levelMonitor?.Clear();
        
        //단계 수를 체크해 초기화
        var levelLimit = GetLevelLimit();
        for (int i = 0; i <= levelLimit; i++)
        {
            levelMonitor?.Add(false);
        }

        StartCoroutine(LevelMonitor());
    }
    
    //레벨 상승이 되면 OnLevelChanged을 구독한 액션을 일제히 실행
    public IEnumerator LevelMonitor()
    {
        var levelLimit = GetLevelLimit();
        
        //게임이 시작되었을때만 시작되고 게임이 종료되면 중단된다.
        while (GameManager.Instance.inGameController.GetGameStarted())
        {
            var level = GetLevel(GameManager.Instance.GetTimeController()._day);

            for (int i = 0; i <= levelLimit; i++)
            {
                if (!levelMonitor[i] && i == level)
                {
                    levelMonitor[i] = true;

                    if (i != 0) //시작난이도는 제외
                    {
                        //난이도 상승 시 피드백
                        OnLevelChanged?.Invoke();
                    }
                }
            }
            yield return null;
        }
    }
    
    //난이도 단계 최대치를 리턴
    public int GetLevelLimit()
    {
        return _obstacleProcessingCount.Length - 1;
    }
    
    
}
