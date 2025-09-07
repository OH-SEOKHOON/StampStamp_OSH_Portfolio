using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ObstacleController : MonoBehaviour, IPoolable
{
    // UI 풀에서 반환할 원본 프리팹 참조
    public GameObject OriginalPrefab { get; set; }
    public Image obstacleImage;
	public Sprite _idleSprite;
    public List<Sprite> _strokeSprites;
        
    private DocumentController _documentController;
    private int _processCount;
    private int _obstacleObjIdx;
    private bool _processOver;
    private Quaternion _originalRotation;
    
    private Coroutine _idleRoutine;

	private void Awake()
    {
        obstacleImage = GetComponent<Image>();
    }
    /// <summary>
    /// 초기화: DocumentController 참조와 장애물 처리 카운트 지정
    /// </summary>
    public void Initialize(DocumentController documentController, int processCount, int obstacleObjIdx)
    {
        _documentController = documentController;
        _processCount = processCount;
        _obstacleObjIdx = obstacleObjIdx;
        _processOver = false;
        _originalRotation = transform.rotation;
        
        gameObject.SetActive(true);

        if (_idleSprite != null)
            obstacleImage.sprite = _idleSprite;
        
        StartIdleAnim();    
    }
    
    public void OnDisable()
    {
        if (DOTween.IsTweening(transform, true)) // true → 현재 재생 중인 트윈만 체크
        {
            transform.DOKill(); // 해당 타겟의 모든 트윈 종료
            transform.rotation = _originalRotation; // 회전중이라면 회전각 초기화.
            //애님의 변화가 있었다면 스프라이트 초기화
            GameManager.Instance.obstacleClearEffect.InitAnim(this, _obstacleObjIdx);
        }
    }
    
    
    /// <summary>
    /// 장애물 클릭/터치 처리
    /// </summary>
    public void ProcessHit()
    {
        _processCount--;
        Debug.Log("Obstacle hit, remaining: " + _processCount);
        if (_processCount <= 0)
        {
            //마지막 처리모션 한번만 허용되도록 수정
            if (_processOver) return;
            _processOver = true;
            StartCoroutine(TerminateSeq());

        }
        else
        {
            //연속처리 필요대상만 실행됨
            StopCoroutine(GameManager.Instance.obstacleClearEffect.HitAnim(this, _obstacleObjIdx));
            StartCoroutine(GameManager.Instance.obstacleClearEffect.HitAnim(this, _obstacleObjIdx));
        }
    }

    public int GetProcessCount()
    {
        return _processCount;
    }

    public IEnumerator TerminateSeq()
    {
        // DocumentController에 처리 완료 알림
        _documentController?.ObstacleCleared(gameObject);
        
        //장애물 처리 연출
        yield return StartCoroutine(GameManager.Instance.
            obstacleClearEffect.DefuseEffect(this, _obstacleObjIdx));

        // 풀에 반환 (UI 풀에도 동일하게 적용)
        DocumentPool.Instance.ReturnObject(gameObject);
    }
    
    public void StartIdleAnim()
    {
        if (_idleRoutine != null)
            StopCoroutine(_idleRoutine);

        _idleRoutine = StartCoroutine(GameManager.Instance.obstacleClearEffect.IdleAnim(this, _obstacleObjIdx));
    }

    public void StopIdleAnim()
    {
        if (_idleRoutine != null)
        {
            StopCoroutine(_idleRoutine);
            _idleRoutine = null;
        }
    }

    public void SetStroke()
    {
        // 리스트가 없거나 비어있으면 아무것도 하지 않음
        if (_strokeSprites == null || _strokeSprites.Count == 0)
            return;

        // IdleAnim만 멈춤
        StopIdleAnim();

        // 강조 스프라이트가 1개만 있는 경우
        if (_strokeSprites.Count == 1)
        {
            obstacleImage.sprite = _strokeSprites[0];
            return;
        }

        // 강조 스프라이트가 2개 이상일 경우 (서류 장애물)
        if (_processCount == 1 &&
            DifficultyManager.Instance.GetObstacleProcessingCount(GameManager.Instance.GetTimeController()._day) > 1)
        {
            // 열린서류
            obstacleImage.sprite = _strokeSprites[1];
        }
        else
        {
            // 닫힌서류
            obstacleImage.sprite = _strokeSprites[0];
        }
    }
}