// 정답및 오답에 따라 텍스트가 나타나는 연출메서드 (RisingEffect)에 기여했습니다.

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassificationUIController : MonoBehaviour
{
    [SerializeField] private Image greenBox;
    [SerializeField] private Image redBox;
    
    private Tweener effectTweener;

    [SerializeField] private TMP_Text _timerEffectText;
    private RectTransform _timerEffectRect;
    private Vector2 _timerEffectStartAnchoredPos;
    
    private void Awake()
    {
        InitBox(greenBox);
        InitBox(redBox);
        
        _timerEffectRect = _timerEffectText.GetComponent<RectTransform>();
        _timerEffectStartAnchoredPos = _timerEffectRect.anchoredPosition; // 시작 위치 
        //Debug.Log(_timerEffectStartAnchoredPos);
    }

    private void InitBox(Image box)
    {
        if (box != null)
        {
            var c = box.color;
            c.a = 0f; 
            box.color = c;
            box.raycastTarget = false; 
        }
    }

    public void TriggerSuccessEffect() // 성공 효과 트리거
    {
        // TriggerEffect(greenBox);
        RisingEffect(true);
    }

    public void TriggerFailEffect() // 실패 효과 트리거
    {
        TriggerEffect(redBox);
        RisingEffect(false);
    }

    private void TriggerEffect(Image box)
    {
        if (box == null) return;

        // 기존 트윈 정리 (최종값 적용하지 않고 그냥 종료)
        box.DOKill(false);

        // 초기화
        var c = box.color;
        c.a = 0.5f;
        box.color = c;

        // 새로운 트윈 시작
        box.DOFade(0f, 0.5f).OnComplete(() =>
        {
            // 안전하게 종료
            box.DOKill(false);
        });
    }

    private void RisingEffect(bool isCorrect)
    {
        // 트윈 정리 (기존 효과 즉시 끊기)
        _timerEffectRect.DOKill(true);   // true = 마지막 값을 적용하고 종료
        _timerEffectText.DOKill(true);

        // 초기화
        _timerEffectText.gameObject.SetActive(true);
        _timerEffectText.text = isCorrect ? "+ 시간 증가!!" : "- 시간 감소...";
        _timerEffectText.color = isCorrect ? Color.green : Color.red;

        // 알파값 강제 세팅
        var col = _timerEffectText.color;
        col.a = 1f;
        _timerEffectText.color = col;

        // 위치 초기화
        _timerEffectRect.anchoredPosition = _timerEffectStartAnchoredPos;

        // 새 연출 시작 (OnComplete 안 씀)
        _timerEffectRect.DOAnchorPosY(_timerEffectStartAnchoredPos.y + 50f, 1f);
        _timerEffectText.DOFade(0f, 1f);
    }
}
