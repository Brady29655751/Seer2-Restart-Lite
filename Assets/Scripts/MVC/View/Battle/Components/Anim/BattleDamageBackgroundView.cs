using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDamageBackgroundView : Module
{
    [SerializeField] private Image background;
    
    [SerializeField] private RectTransform rect;
    
    [SerializeField] private IAnimator anim;

    [SerializeField] private GameObject critical;
    
    [SerializeField] private GameObject damageNumberPrefab;
    
    private List<RectTransform> damageNumRectList = new List<RectTransform>();
    
    private List<Image> damageNumImageList = new List<Image>();
    private RectTransform criticalRect;

    public GameObject Critical => critical;

    public Image Background => background;

    public RectTransform Rect => rect;

    public IAnimator Anim => anim;
    
    protected override void Awake() {
        criticalRect = this.critical.GetComponent<RectTransform>();
    }

    private void Update()
    {
        foreach (Image img in damageNumImageList)
        {
                img.color = this.background.color;
        }
    }

    private void OnDestroy()
    {
        foreach (RectTransform rect in damageNumRectList)
        {
            Destroy(rect.gameObject);
        }
        this.damageNumImageList.Clear();
        this.damageNumRectList.Clear();
    }

    public void InstantiateDamageNum(int damage, bool isCritical) {
        string numString = "%" + damage.ToString();
        int len = numString.Length;
        bool isLenOdd = (len % 2 == 1);
        int i;
        for (i = 0; i < len; i++) {
            GameObject num = Instantiate(damageNumberPrefab, this.rect);
            RectTransform rect = num.GetComponent<RectTransform>();
            Image img = num.GetComponent<Image>();
            int deltaX = 45 * (i - len / 2);
            rect.anchoredPosition = isLenOdd ? new Vector2(deltaX, 0) : new Vector2(deltaX + 22.5f, 0);
            rect.localScale = 1.5f * Vector3.one;
            damageNumRectList.Add(rect);
            damageNumImageList.Add(img);
            img.sprite = ResourceManager.instance.GetSprite("Numbers/Damage/" + numString[i]);
        }
        criticalRect.anchoredPosition = new Vector3(criticalRect.anchoredPosition.x - 25 * (i - 4), criticalRect.anchoredPosition.y);
    }
}
