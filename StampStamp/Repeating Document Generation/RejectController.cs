using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RejectController : MonoBehaviour, IPoolable
{
    public GameObject OriginalPrefab { get; set; } 
    
    public Sprite _idleSprite;
    public Sprite _strokeSprite;
    
    private Image _img;
    
    private void Awake()
    {
        _img = GetComponent<Image>();
    }
    public void Initialize()
    {
        if (_idleSprite != null)
            _img.sprite = _idleSprite;
    }

    public void SetStroke()
    {
        if (_strokeSprite != null)
            _img.sprite = _strokeSprite;
    }
}
