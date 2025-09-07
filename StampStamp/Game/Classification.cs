// documentcontroller.cs와 연결되는 부분, difficultmanager.cs와 연결되는 부분에 참여했습니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Classification : MonoBehaviour
{
    public bool obstacle; //장애물 유무 true: 장애물 있음, false: 장애물 없음
    public bool clean; //반려요소 true: 반려요소 없음, false: 반려요소 있음
    public bool confirm; //승인 여부 true: 승인버튼 클릭, false: 반려버튼 클릭
    public bool fever = false; //피버 여부 true: 피버 상태, false: 일반 상태
    //bool success; //분류 성공 여부 true: 성공, false: 실패

    int combo = 0; //콤보 횟수
    int maxCombo = 0; //최대 콤보 횟수
    float feverValue = 0; //피버 게이지
    float scoreMag = 1.0f; //점수 배율
    float score = 0; //점수

    public ScoreUIController scoreUIController; //점수 UI 컨트롤러
    public DocumentController docController;
    private ClassificationUIController classificationUIController; //분류 UI 컨트롤러

    public void Initialize()
    {
        docController = GameManager.Instance.GetDocumentController();
        classificationUIController = UIManager.Instance.inGameUIController.classificationUIController;
    }

    public void scoreMagnification()
    {
        switch (combo)
        {
            case int n when (n < 5):
                scoreMag = 1.0f; //콤보 없음
                break;
            case int n when (n >= 5 && n <= 10):
                scoreMag = 1.1f; //콤보 1.1배
                break;
            case int n when (n >= 11 && n <= 20):
                scoreMag = 1.2f; //콤보 1.2배
                break;
            case int n when (n >= 21 && n <= 30):
                scoreMag = 1.3f; //콤보 1.3배
                break;
            case int n when (n >= 31 && n <= 40):
                scoreMag = 1.4f; //콤보 1.4배
                break;
            case int n when (n >= 41 && n <= 50):
                scoreMag = 1.5f; //콤보 1.5배
                break;
            case int n when (n >= 51 && n <= 60):
                scoreMag = 1.6f; //콤보 1.6배
                break;
            case int n when (n >= 61 && n <= 70):
                scoreMag = 1.7f; //콤보 1.7배
                break;
            case int n when (n >= 71 && n <= 80):
                scoreMag = 1.8f; //콤보 1.8배
                break;
            case int n when (n >= 81 && n <= 90):
                scoreMag = 1.9f; //콤보 1.9배
                break;
            case int n when (n >= 91 && n <= 100):
                scoreMag = 2.0f; //콤보 2배
                break;
            case int n when (n >= 101):
                scoreMag = 2.5f; //콤보 2.5배 
                break;
        }
    } //점수 배율 조정
    public void DocumentClassification() //서류 분류 메소드
    {
        float time = GameManager.Instance.GetTimeController()._remainedTimerTime; // 남은 일과시간
        int day = GameManager.Instance.GetTimeController()._day; // 남은 진행일수
        // float time = TimeController.Instance._remainedTime; // 남은 일과시간
        // int day = TimeController.Instance._day; // 남은 진행일수

        if (obstacle) // 장애물이 있을 때 
        {
            time -= DifficultyManager.Instance.GetPenalty(day); //일과시간 감소
            GameManager.Instance.GetTimeController().UpdateTimeUI(); //타임 ui 업데이트
            combo = 0; //콤보 초기화
            feverValue -= feverValue * DifficultyManager.Instance.GetFeverValuePenalty(day); //피버 게이지 감소
            scoreMagnification(); //점수 배율 적용
            UpdateScoreMagUI(); //점수 배율 UI 갱신
            UpdateComboUI();
            UpdateFeverUI(); //피버 게이지 UI 갱신
            classificationUIController.TriggerFailEffect(); //분류 실패 이펙트 실행
            AudioManager.Instance.SFX.PlayDocFail(); //실패 사운드 재생
            docController.ObstacleOutline(); // 장애물 아웃라인 강조 메서드 호출
        }
        else // 장애물이 없을 때
        {
            if (clean) // 반려요소가 없을 때
            {
                if (confirm) // 승인 버튼 클릭 시
                {
                    //success = true;
                    
                    combo += 1; //콤보 증가
                    if (!fever)
                    {
                        time += DifficultyManager.Instance.GetReward(day); //일과시간 증가
                        feverValue += DifficultyManager.Instance.GetFeverValueReward(day);
                    }
                    else
                    {
                        time += DifficultyManager.Instance.GetFeverReward(day);
                    }    
                    
                    score += ((1 * day) * scoreMag); //점수 증가
                    scoreMagnification(); //점수 배율 적용
                    UpdateScoreUI(); //점수 UI 갱신
                    UpdateScoreMagUI(); //점수 배율 UI 갱신
                    GameManager.Instance.inGameController.CheckNewRecord(score); //NewRecord 체크
                    UpdateComboUI();
                    UpdateFeverUI(); //피버 게이지 UI 갱신
                    classificationUIController.TriggerSuccessEffect(); //분류 성공 이펙트 실행
                    AudioManager.Instance.SFX.PlayDocSuccess(); //성공 사운드 재생
                    if (combo > maxCombo)
                    {
                        maxCombo = combo; //최대 콤보 갱신
                    }
                    if (feverValue >= 100) // 피버 게이지가 100 이상일 때
                    {
                        if (!fever)
                        {
                            fever = true; // 피버 상태로 변경
                            feverValue = 0; // 피버 게이지 초기화
                            //피버동안 버튼 둘다 V로 변경
                            StartCoroutine(UIManager.Instance.inGameUIController.interactionUIController.FeverMode());
                        }
                    }
                    docController.RemoveDocument(); // 서류 재생성
                }
                else // 반려 버튼 클릭 시
                {
                    //success = false;
                    time -= DifficultyManager.Instance.GetPenalty(day); //일과시간 감소
                    combo = 0; //콤보 초기화
                    feverValue -= feverValue * DifficultyManager.Instance.GetFeverValuePenalty(day); //피버 게이지 감소
                    scoreMagnification(); //점수 배율 적용
                    UpdateScoreMagUI(); //점수 배율 UI 갱신
                    UpdateComboUI();
                    UpdateFeverUI(); //피버 게이지 UI 갱신
                    classificationUIController.TriggerFailEffect(); //분류 실패 이펙트 실행
                    AudioManager.Instance.SFX.PlayDocFail(); //실패 사운드 재생
                    docController.RemoveDocument(); // 반려요소 아웃라인 강조 메서드 호출
                }
            }
            else // 반려요소가 있을 때
            {
                if (confirm) // 승인 버튼 클릭 시
                {
                    //success = false;
                    time -= DifficultyManager.Instance.GetPenalty(day); //일과시간 감소
                    combo = 0; //콤보 초기화
                    feverValue -= feverValue * DifficultyManager.Instance.GetFeverValuePenalty(day); //피버 게이지 감소
                    scoreMagnification(); //점수 배율 적용
                    UpdateScoreMagUI(); //점수 배율 UI 갱신
                    UpdateComboUI();
                    UpdateFeverUI(); //피버 게이지 UI 갱신
                    classificationUIController.TriggerFailEffect(); //분류 실패 이펙트 실행
                    AudioManager.Instance.SFX.PlayDocFail(); //실패 사운드 재생
                    docController.RejectOutline(); //반려요소 아웃라인 강조 메서드 호출
                }
                else // 반려 버튼 클릭 시
                {
                    //success = true;
                    combo += 1; //콤보 증가
                    if (!fever)
                    {
                        time += DifficultyManager.Instance.GetReward(day); //일과시간 증가
                        feverValue += DifficultyManager.Instance.GetFeverValueReward(day);
                    }
                    else
                    {
                        time += DifficultyManager.Instance.GetFeverReward(day);
                    }    
                    score += ((1 * day) * scoreMag); //점수 증가
                    scoreMagnification(); //점수 배율 적용
                    UpdateScoreUI(); //점수 UI 갱신
                    UpdateScoreMagUI(); //점수 배율 UI 갱신
                    GameManager.Instance.inGameController.CheckNewRecord(score); //NewRecord 체크
                    UpdateComboUI();
                    UpdateFeverUI(); //피버 게이지 UI 갱신
                    classificationUIController.TriggerSuccessEffect(); //분류 성공 이펙트 실행
                    AudioManager.Instance.SFX.PlayDocSuccess(); //성공 사운드 재생
                    if (combo > maxCombo)
                    {
                        maxCombo = combo; //최대 콤보 갱신
                    }
                    if (feverValue >= 100) // 피버 게이지가 100 이상일 때
                    {
                        if (!fever)
                        {
                            fever = true; // 피버 상태로 변경
                            feverValue = 0; // 피버 게이지 초기화
                            //피버동안 버튼 둘다 V로 변경
                            StartCoroutine(UIManager.Instance.inGameUIController.interactionUIController.FeverMode());
                        }
                    }
                    docController.RemoveDocument(); // 서류 재생성
                }
            }
        }

        GameManager.Instance.GetTimeController().SetRemainedTimer(time); // 남은 일과시간 갱신
        GameManager.Instance.GetTimeController().SetDay(day); // 남은 진행일수 갱신
        GameManager.Instance.GetTimeController().UpdateTimeUI(); //타임 ui 업데이트
    }

    public void UpdateScoreUI() // 점수 UI 갱신 메소드
    {
        if (UIManager.Instance.inGameUIController.scoreUIController.score is var scoreText && scoreText != null)
            scoreText.text = GameManager.Instance.GetClassification().score.ToString("F0");
    }

    public void UpdateScoreMagUI() // 점수 배율 UI 갱신 메소드
    {
        if (UIManager.Instance.inGameUIController.scoreUIController.scoreMag is var scoreMagText && scoreMagText != null)
            scoreMagText.text = "x" + GameManager.Instance.GetClassification().scoreMag.ToString("F1");
    }

    public void UpdateComboUI() // 콤보 UI 갱신 메소드
    {
        if (UIManager.Instance.inGameUIController.comboUIController.comboText is var comboText && comboText != null)
            comboText.text = GameManager.Instance.GetClassification().combo.ToString();
    }

    public void UpdateFeverUI() // 피버 게이지 UI 갱신 메소드
    {
        var feverSlider = UIManager.Instance.inGameUIController.feverUIController.feverSlider;
        if (feverSlider != null)
        {
            feverSlider.value = feverValue / 100; // 현재 Classification의 feverValue로 갱신
        }
    }
    public void InitScore()
    {
        score = 0; // 점수 초기화
        combo = 0; // 콤보 초기화
        maxCombo = 0; // 최대 콤보 초기화
        feverValue = 0; // 피버 게이지 초기화
        scoreMag = 1.0f; // 점수 배율 초기화
        UpdateScoreUI(); // 점수 UI 갱신
        UpdateScoreMagUI(); // 점수 배율 UI 갱신
        UpdateComboUI(); // 콤보 UI 갱신
        UpdateFeverUI(); // 피버 게이지 UI 갱신
    }

    public int GetMaxCombo()
    {
        return maxCombo;
    }

    public float GetScore()
    {
        return score;
    }
}
