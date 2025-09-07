// 사운드 풀링 시스템 구현에만 기여했습니다.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SFXController : MonoBehaviour
{
    // SFX를 추가하실 때 여기에 추가해주세요.
    [Header("Common")]
    [SerializeField] private AudioClip buttonClick;
    
    [Header("InGame")]
    [SerializeField] private AudioClip stamp;
    [SerializeField] private AudioClip docSuccess;
    [SerializeField] private AudioClip docFail;
    [SerializeField] private AudioClip docSwap;
    [SerializeField] private AudioClip obsBugPostHit;
    [SerializeField] private AudioClip obsProcessTry;
    [SerializeField] private AudioClip obsHandHit;
    [SerializeField] private AudioClip obsFileEnvelopeOut;
    [SerializeField] private AudioClip newRecordResult;
    [SerializeField] private AudioClip newRecordScoreBar;
    [SerializeField] private AudioClip speedUp;
    [SerializeField] private AudioClip fever;
    [SerializeField] private AudioClip timeOutAlert;
    [SerializeField] private AudioClip gameStart;
    [SerializeField] private AudioClip scoreCalculating;
    [SerializeField] private AudioClip scoreCalculated;
    [SerializeField] private AudioClip gameOver;
    
    
    // 여기까지

    private List<AudioSource> _sfxSources;                       // 단발성 AudioSource (풀링)
    private int _poolSize = 20;                                  // 단발성 AudioSource 풀의 개수
    private Dictionary<AudioClip, List<AudioSource>> _activeSFX;   // 개별 단발 SFX 추적
    
    private Dictionary<AudioClip, AudioSource> _loopSources;    // 반복용 AudioSource
    private bool _isSFXOn = true;       // SFX가 켜져있는지 여부
    public bool GetIsSFXOn() => _isSFXOn;

    private void Awake()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXController(this);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // AudioSource 초기화
        _sfxSources = new List<AudioSource>();
        _loopSources = new Dictionary<AudioClip, AudioSource>();
        _activeSFX = new Dictionary<AudioClip, List<AudioSource>>();

        // 풀 초기화
        for (int i = 0; i < _poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _sfxSources.Add(src);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        
    }

    // SFX를 추가하신 뒤, 아래 함수 모음에 재생 함수를 작성해주세요. 그리고 작성하신 함수를 통해 사용하시면 됩니다.
    // 1번 재생 : PlaySFX()
    // 반복 재생 : PlayLoopSFX()
    // 반복 재생 중지 : StopLoopSFX()
    #region PlaySFX 함수 모음

    public void PlayButtonClick() => PlaySFX(buttonClick);

    public void PlayStamp() => PlaySFX(stamp);
    
    public void PlayDocSuccess() => PlaySFX(docSuccess, 0.6f);
    public void PlayDocFail() => PlaySFX(docFail, 0.6f);
    public void PlayDocSwap() => PlaySFX(docSwap);
    public void PlayObsBugPostHit() => PlaySFX(obsBugPostHit);
    public void PlayObsProcessTry() => PlaySFX(obsProcessTry);
    public void PlayObsHandHit() => PlaySFX(obsHandHit);
    public void PlayObsFileEnvelopeOut() => PlaySFX(obsFileEnvelopeOut);
    public void PlaySpeedUp() => PlaySFX(speedUp);
    public void PlayFever() => PlaySFX(fever);
    public void PlayTimeOutAlert() => PlayLoopSFX(timeOutAlert);
    public void StopTimeOutAlert() => StopLoopSFX(timeOutAlert);
    public void PlayNewRecordResult() => PlaySFX(newRecordResult);
    public void PlayNewRecordScoreBar() => PlaySFX(newRecordScoreBar);
    public void PlayGameStart() => PlaySFX(gameStart);
    public void PlayScoreCalculating() => PlaySFX(scoreCalculating);
    public void StopScoreCalculating() => StopSFX(scoreCalculating);
    public void PlayScoreCalculated() => PlaySFX(scoreCalculated);
    public void PlayGameOver() => PlaySFX(gameOver);
    #endregion

    // 단발 SFX 재생 (풀링 + 추적)
    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (!_isSFXOn || clip == null) return;

        AudioSource src = GetAvailableSource();
        src.clip = clip;
        src.volume = volume;
        src.mute = !_isSFXOn;
        src.Play();

        // 개별 SFX 추적
        if (!_activeSFX.ContainsKey(clip))
            _activeSFX[clip] = new List<AudioSource>();
        _activeSFX[clip].Add(src);

        // 재생 완료 후 제거
        StartCoroutine(RemoveAfterPlay(src, clip));
    }

    private System.Collections.IEnumerator RemoveAfterPlay(AudioSource src, AudioClip clip)
    {
        yield return new WaitWhile(() => src.isPlaying);
        if (_activeSFX.ContainsKey(clip))
            _activeSFX[clip].Remove(src);
    }

    // 사용 가능한 AudioSource 가져오기 (풀링)
    private AudioSource GetAvailableSource()
    {
        foreach (var src in _sfxSources)
        {
            if (!src.isPlaying)
                return src;
        }

        // 모두 사용 중이면 새로 생성
        var newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.playOnAwake = false;
        _sfxSources.Add(newSrc);
        return newSrc;
    }

    // 특정 단발 SFX 강제 정지
    public void StopSFX(AudioClip clip)
    {
        if (!_activeSFX.ContainsKey(clip)) return;

        foreach (var src in _activeSFX[clip])
        {
            if (src != null && src.isPlaying)
                src.Stop();
        }
        _activeSFX[clip].Clear();
    }

    // 모든 단발 SFX 강제 정지
    public void StopAllSFX()
    {
        foreach (var kv in _activeSFX)
        {
            foreach (var src in kv.Value)
            {
                if (src != null && src.isPlaying)
                    src.Stop();
            }
            kv.Value.Clear();
        }
    }
    
    // SFX 반복 재생
    private void PlayLoopSFX(AudioClip clip)
    {
        if (clip == null) return;
        if (_loopSources.ContainsKey(clip)) return;     // 이미 재생 중이면 패스
        
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.clip = clip;
        if (!_isSFXOn) src.mute = true;
        src.Play();
        _loopSources[clip] = src;
    }
    
    // SFX 반복 재생 중지
    private void StopLoopSFX(AudioClip clip)
    {
        if (clip == null || !_loopSources.ContainsKey(clip)) return;

        var src = _loopSources[clip];
        if (src != null && src.isPlaying)
        {
            src.Stop();
            Destroy(src);
        }
        
        _loopSources.Remove(clip);
    }

    // _isSFXOn 조정
    public void SetSFXOn(bool isSFXOn)
    {
        _isSFXOn = isSFXOn;

        _isSFXOn = isSFXOn;

        // 단발
        foreach (var src in _sfxSources)
        {
            if (src != null) src.mute = !_isSFXOn;
        }

        // 루프
        foreach (var key in _loopSources)
        {
            if (key.Value != null) key.Value.mute = !_isSFXOn;
        }
    }
}