using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DocumentController : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject _documentPrefab;
    [SerializeField] private List<GameObject> _rejectObjPrefabs;
    [SerializeField] private List<ObstacleData> _obstacleObjDatas;

    [Header("도장")]
    [SerializeField] private GameObject approvalStampPrefab;
    [SerializeField] private GameObject deniedStampPrefab;

    [Header("위치 및 이동")]
    [SerializeField] private Vector2 _docSpawnPos;
    [SerializeField] private Vector2 _docStopPos;
    [SerializeField] private Vector2 _docDespawnPos;
    private float _duration;

    private DocumentData _currentDocument;
    private List<ObstacleInstance> _currentObstacles = new List<ObstacleInstance>();
    private List<GameObject> _obstacleObjs = new List<GameObject>();

    private GameObject _docObj;
    private GameObject _rejectObj;

    private Vector2 _documentSize;

    [NonSerialized] public bool _isClickable;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

    public void InitDocuments()
    {
        _currentObstacles.Clear();
        _obstacleObjs.Clear();
        
        DocumentPool.Instance.canvas = _canvas;

        var image = _documentPrefab.GetComponent<Image>();
        _documentSize = image != null ? image.rectTransform.rect.size : Vector2.one;

        _isClickable = true;
        CreateDocument();
    }

    private void CreateDocument()
    {
        _currentDocument = new DocumentData
        {
            documentType = GameManager.Instance.GetClassification().fever || (Random.Range(0, 2) == 0),
            rejectObjIdx = Random.Range(0, _rejectObjPrefabs.Count)
        };
        GameManager.Instance.GetClassification().clean = _currentDocument.documentType;

        // 반려 요소 스폰 위치 계산
        var rejectRect = _rejectObjPrefabs[_currentDocument.rejectObjIdx].GetComponent<RectTransform>();
        Vector2 rejectSize = rejectRect != null ? rejectRect.rect.size : Vector2.zero;

        float minX = -_documentSize.x / 2f + rejectSize.x / 2f;
        float maxX = _documentSize.x / 2f - rejectSize.x / 2f;
        float minY = -_documentSize.y / 2f + rejectSize.y / 2f;
        float maxY = _documentSize.y / 2f - rejectSize.y / 2f;

        _currentDocument.spawnPosX = Random.Range(minX, maxX);
        _currentDocument.spawnPosY = Random.Range(minY, maxY);

        CreateObstacle();
    }

    private void CreateObstacle()
    {
        int day = GameManager.Instance.GetTimeController()._day;
        int processCount = DifficultyManager.Instance.GetObstacleProcessingCount(day);
        int obstacleType = Random.Range(0, _obstacleObjDatas.Count);

        // 장애물 인스턴스 생성
        _currentObstacles.Clear();

        if (obstacleType == 0 || obstacleType == 1)
        {
            for (int i = 0; i < processCount; i++)
            {
                var obstacle = new ObstacleInstance
                {
                    obstacleObjIdx = obstacleType,
                    prefab = _obstacleObjDatas[obstacleType].obstaclePrefab,
                    processCount = 1,
                    spawnPos = new Vector2(
                        Random.Range(-_documentSize.x / 2f, _documentSize.x / 2f),
                        Random.Range(-_documentSize.y / 2f, _documentSize.y / 2f)
                    )
                };
                _currentObstacles.Add(obstacle);
            }
        }
        else if (obstacleType == 2)
        {
            var obstacle = new ObstacleInstance
            {
                obstacleObjIdx = obstacleType,
                prefab = _obstacleObjDatas[obstacleType].obstaclePrefab,
                processCount = processCount,
                spawnPos = new Vector2(1f, -2f)
            };
            _currentObstacles.Add(obstacle);
        }
        else
        {
            var obstacle = new ObstacleInstance
            {
                obstacleObjIdx = obstacleType,
                prefab = _obstacleObjDatas[obstacleType].obstaclePrefab,
                processCount = processCount,
                spawnPos = Vector2.zero
            };
            _currentObstacles.Add(obstacle);
        }

        SpawnDocument();
    }

    private void SpawnDocument()
    {
        int day = GameManager.Instance.GetTimeController()._day;
        _duration = (GameManager.Instance.GetClassification().fever) ? DifficultyManager.Instance.GetFeverDocumentDelay(day) : DifficultyManager.Instance.GetDocumentDelay(day);
        
        _obstacleObjs.Clear();

        // 문서 생성
        _docObj = DocumentPool.Instance.GetObject(_documentPrefab, _docSpawnPos);
        _docObj.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>(), false);

        // 반려 요소 생성
        if (!_currentDocument.documentType)
        {
            _rejectObj = DocumentPool.Instance.GetObject(
                _rejectObjPrefabs[_currentDocument.rejectObjIdx],
                new Vector2(_currentDocument.spawnPosX, _currentDocument.spawnPosY)
            );
            _rejectObj.GetComponent<RectTransform>().SetParent(_docObj.GetComponent<RectTransform>(), false);
            
            _rejectObj.GetComponent<RejectController>().Initialize();
        }

        // 장애물 생성
        float chance = DifficultyManager.Instance.GetObstacleSpawnProbability(day);
        if (!GameManager.Instance.GetClassification().fever && Random.Range(0f, 100f) < chance)
        {
            GameManager.Instance.GetClassification().obstacle = true;

            foreach (var obstacle in _currentObstacles)
            {
                var obj = DocumentPool.Instance.GetObject(obstacle.prefab, obstacle.spawnPos);
                obj.GetComponent<RectTransform>().SetParent(_docObj.GetComponent<RectTransform>(), false);
                _obstacleObjs.Add(obj);

                var controller = obj.GetComponent<ObstacleController>();
                if (controller != null) controller.Initialize(this, obstacle.processCount, obstacle.obstacleObjIdx);
            }
        }
        
        var docRect = _docObj.GetComponent<RectTransform>();
        // 등장 연출
        docRect.DOAnchorPosX(_docStopPos.x, _duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // X 이동 완료 후 Y 이동
                docRect.DOAnchorPosY(_docStopPos.y, _duration)
                    .SetEase(Ease.Linear);
            });
        _isClickable = true;
    }

    public void ShowStamp(bool isApproved)
    {
        if (_docObj == null) return;

        // 기존 Instantiate 대신 풀에서 가져오기
        GameObject prefab = isApproved ? approvalStampPrefab : deniedStampPrefab;
        Vector2 anchoredPos = new Vector2(1f, -2f);

        // DocumentPool 사용
        GameObject stamp = DocumentPool.Instance.GetObject(prefab, anchoredPos);

        // _docObj 자식으로 붙이기
        var rect = stamp.GetComponent<RectTransform>();
        if (rect != null)
            rect.SetParent(_docObj.transform, false);
        else
            stamp.transform.SetParent(_docObj.transform, false);
    }

    public void ObstacleCleared(GameObject obstacleObj)
    {
        if (_obstacleObjs.Contains(obstacleObj))
            _obstacleObjs.Remove(obstacleObj);

        if (_obstacleObjs.Count == 0)
            GameManager.Instance.GetClassification().obstacle = false;
    }

    public void RemoveDocument()
    {
        int day = GameManager.Instance.GetTimeController()._day;
        _duration = (GameManager.Instance.GetClassification().fever) ? DifficultyManager.Instance.GetFeverDocumentDelay(day) : DifficultyManager.Instance.GetDocumentDelay(day);
        
        _isClickable = false;
        var docRect = _docObj.GetComponent<RectTransform>();

        // Y 먼저 이동
        docRect.DOAnchorPosY(_docDespawnPos.y, _duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Y 이동 완료 후 X 이동
                docRect.DOAnchorPosX(_docDespawnPos.x, _duration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        // X 이동 완료 후 ReloadDocument 호출
                        ReloadDocument();
                    });
            });
    }

    public void ReloadDocument(bool noLoop = false)
    {
        if (_docObj != null)
        {
            // 모든 자식 오브젝트 반환
            for (int i = _docObj.transform.childCount - 1; i >= 0; i--)
            {
                var child = _docObj.transform.GetChild(i).gameObject;
                DocumentPool.Instance.ReturnObject(child);
            }

            // 문서 자체 반환
            DocumentPool.Instance.ReturnObject(_docObj);
        }

        _currentObstacles.Clear();
        _obstacleObjs.Clear();
        _rejectObj = null;
        _docObj = null;
        GameManager.Instance.GetClassification().obstacle = false;

        if (!noLoop) CreateDocument();
    }

    public void RejectOutline()
    {
        _rejectObj.GetComponent<RejectController>().SetStroke();
        RemoveDocument();
    }

    public void ObstacleOutline()
    {
        foreach (var obstacle in _obstacleObjs)
        {
            obstacle.GetComponent<ObstacleController>().SetStroke();
        }
        RemoveDocument();
    }
    private void Update()
    {
        if (!_isClickable) return;
    
        if (TryGetInputPosition(out Vector2 inputPos))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = inputPos;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
    
            if (results.Count > 0)
            {
                var firstHit = results[0];
                var obstacle = firstHit.gameObject.GetComponent<ObstacleController>();
    
                if (obstacle != null)
                {
                    Vector2 canvasPos = ScreenToCanvasPosition(inputPos);
                    VfxManager.Instance.GetVFX(VFXType.OBSTOUCH, canvasPos, Quaternion.identity, Vector2.one);
    
                    obstacle.ProcessHit();
                }
            }
        }
    }
    
    private Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    {
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPos,
            _canvas.worldCamera,
            out canvasPos
        );
    
        return canvasPos;
    }
    
    private bool TryGetInputPosition(out Vector2 inputPos)
    {
        inputPos = Vector2.zero;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Ended)
            {
                inputPos = touch.position; return true;
            }
            
        } 
        else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            inputPos = Input.mousePosition; return true;
        } 
        return false;
    }
    
    
    // #region 터치 로직 수정본
    //
    // private float _tapStartTime = 0f;
    // private Vector2 _tapStartPos = Vector2.zero;
    // public float MaxTapDuration = 10f;
    // public float MaxTapMove = 20f;
    // private void Update()
    // {
    //     if (!_isClickable) return;
    //
    //     if (TryGetTapPosition(out Vector2 inputPos))
    //     {
    //         PointerEventData pointerData = new PointerEventData(EventSystem.current);
    //         pointerData.position = inputPos;
    //         List<RaycastResult> results = new List<RaycastResult>();
    //         EventSystem.current.RaycastAll(pointerData, results);
    //
    //         if (results.Count > 0)
    //         {
    //             var firstHit = results[0];
    //             var obstacle = firstHit.gameObject.GetComponent<ObstacleController>();
    //
    //             if (obstacle != null)
    //             {
    //                 Vector2 canvasPos = ScreenToCanvasPosition(inputPos);
    //                 VfxManager.Instance.GetVFX(VFXType.OBSTOUCH, canvasPos, Quaternion.identity, Vector2.one);
    //
    //                 obstacle.ProcessHit();
    //             }
    //         }
    //     }
    // }
    //
    // private Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    // {
    //     Vector2 canvasPos;
    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //         _canvas.transform as RectTransform,
    //         screenPos,
    //         _canvas.worldCamera,
    //         out canvasPos
    //     );
    //
    //     return canvasPos;
    // }
    //
    // private bool TryGetTapPosition(out Vector2 tapPos)
    // {
    //     tapPos = Vector2.zero;
    //
    //     // 터치 입력
    //     if (Input.touchCount > 0)
    //     {
    //         Touch touch = Input.GetTouch(0);
    //         if (touch.phase == TouchPhase.Began)
    //         {
    //             _tapStartTime = Time.time;
    //             _tapStartPos = touch.position;
    //         }
    //         else if (touch.phase == TouchPhase.Ended)
    //         {
    //             float duration = Time.time - _tapStartTime;
    //             float distance = Vector2.Distance(touch.position, _tapStartPos);
    //
    //             if (duration <= MaxTapDuration && distance <= MaxTapMove)
    //             {
    //                 tapPos = touch.position;
    //                 return true; // 짧은 탭 인정
    //             }
    //         }
    //     }
    //     
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         _tapStartTime = Time.time;
    //         _tapStartPos = Input.mousePosition;
    //     }
    //     else if (Input.GetMouseButtonUp(0))
    //     {
    //         float duration = Time.time - _tapStartTime;
    //         float distance = Vector2.Distance((Vector2)Input.mousePosition, _tapStartPos);
    //
    //         if (duration <= MaxTapDuration && distance <= MaxTapMove)
    //         {
    //             tapPos = Input.mousePosition;
    //             return true; // 짧은 탭 인정
    //         }
    //     }
    //
    //     return false;
    // }
    //
    // #endregion
}
