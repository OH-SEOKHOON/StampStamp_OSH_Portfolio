//타이머 갱신 메서드 (IEnumerator Timer)를 time.delta감소에서 특정 시간이 지나면 감소하는 방식으로 수정하는 방식으로 기여했고, 그 외 난이도에 따라 변수가 동적 할당되야하는 메서드들은 difficultmanager를 호출하는 방식으로 수정했습니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeController : Singleton<TimeController>
{
    [Header("타이머 길이")]
    [SerializeField] float constTimerValue = 60.0f;
    bool isTimeRunning = false; // 타이머 실행 중인지
    float remainedTimerTime; // 남은 일과 시간
    private float currentDecreaseInterval; // 현재 감소 간격

    //[Header("하루 길이")]
    private float dayTime;
    float elapsedDayTime = 0f; // 하루 중 몇시간 얼마나 지났는지

    [Header("현재 날짜")]
    [SerializeField] int day = 1;

    public float _remainedTimerTime => remainedTimerTime;
    public float _remainedDayTime => dayTime - elapsedDayTime; // 하루 남은 시간
    public float _dayTime => dayTime;
    public int _day => day;
    public bool _isTimeRunning => isTimeRunning;

    public void SetRemainedTimer(float value) => remainedTimerTime = Mathf.Max(0f, value);
    public void SetDay(int value) => day = Mathf.Max(1, value);

    public void InitTimeController()
    {
        ResetTimer();
    }

    public void StartRunningTimer()
    {
        if (isTimeRunning) return;
        isTimeRunning = true;
        StartCoroutine(Timer());
    }

    public void StopTime()
    {
        isTimeRunning = false;
        UIManager.Instance.inGameUIController.backGroundUIController.rotateDaycycle.PauseCycle();
    }

    IEnumerator Timer()
    {
        UIManager.Instance.inGameUIController.backGroundUIController.rotateDaycycle.ResumeCycle();
        
        float timerAccumulator = 0f; // 누적 시간
        float prevDisplayedTime = -1f; // 이전 UI 표시값 저장

        while (isTimeRunning)
        {
            timerAccumulator += Time.deltaTime;

            // 정수 단위 감소
            while (timerAccumulator >= currentDecreaseInterval)
            {
                remainedTimerTime -= 1f;
                timerAccumulator -= currentDecreaseInterval;
            }

            // 남은 시간이 1초 이하일 때 소수점 단위 감소
            if (remainedTimerTime <= 1f && remainedTimerTime > 0f)
            {
                remainedTimerTime -= Time.deltaTime;
                remainedTimerTime = Mathf.Max(0f, remainedTimerTime);
            }

            // UI 갱신 최소화: 표시값이 바뀔 때만
            float displayTime = remainedTimerTime > 1f ? Mathf.Floor(remainedTimerTime) : Mathf.Round(remainedTimerTime * 10f) / 10f;
            if (!Mathf.Approximately(prevDisplayedTime, displayTime))
            {
                UpdateTimeUI();
                prevDisplayedTime = displayTime;
            }

            // 타이머 종료 처리
            if (remainedTimerTime <= 0f)
            {
                remainedTimerTime = 0f;
                UpdateTimeUI();
                GameManager.Instance.inGameController.Dispose();
                StopTime();
                yield break;
            }

            // 하루 시간 갱신
            elapsedDayTime += Time.deltaTime;
            if (elapsedDayTime >= dayTime) HandleDayEnd();
            UpdateDayUI();

            yield return null;
        }
    }

    void HandleDayEnd() // 하루가 끝나면 일과 시간 및 남은 시간 초기화
    {
        day++;                  // 하루 일수 증가
        elapsedDayTime = 0f;    // 하루 남은 시간 초기화
        
        // day가 바뀌었으니 감소 간격 갱신
        currentDecreaseInterval = DifficultyManager.Instance.GetTimeDecreaseRate(day);
    }

    public void ResetTimer()
    {
        remainedTimerTime = constTimerValue;
        elapsedDayTime = 0f;
        day = 1;
        
        // 하루당 시간 할당
        dayTime = DifficultyManager.Instance.DayTime;
        // 초기 감소 간격 할당
        currentDecreaseInterval = DifficultyManager.Instance.GetTimeDecreaseRate(day);
        
        isTimeRunning = false;
        UIManager.Instance.inGameUIController.backGroundUIController.rotateDaycycle.ResetCycle();
        UpdateTimeUI();
        UpdateDayUI();
    }

    public void UpdateTimeUI()
    {
        if (UIManager.Instance.inGameUIController.timeUIController.timerText is var timeText && timeText != null)
        {
            if (remainedTimerTime > 1f)
            {
                // 1초 이상: 정수 표시
                timeText.text = $"{remainedTimerTime:F0}";
            }
            else if (remainedTimerTime > 0f)
            {
                // 0~1초: 소수점 1자리 표시
                timeText.text = $"{remainedTimerTime:F1}";
            }
            else
            {
                // 0초 이하: 0 표시
                timeText.text = "0";
            }
        }
    }

    void UpdateDayUI()
    {
        if (UIManager.Instance.inGameUIController.timeUIController.dayText is var dayText && dayText != null)
            dayText.text = $"{day}";
    }
}
