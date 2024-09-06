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

    public GameObject Critical => critical;

    public Image Background => background;

    public RectTransform Rect => rect;

    public IAnimator Anim => anim;
    
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
        this.critical.transform.position = new Vector3(this.critical.transform.position.x - 25 * (i - 4), this.critical.transform.position.y, this.critical.transform.position.z);

    }
}
