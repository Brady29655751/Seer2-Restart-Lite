using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPrompt : IMonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image image;
    [SerializeField] private Text text;
    private RectTransform canvasRect;

    public Vector2 size => rect.rect.size;

    protected override void Awake()
    {
        base.Awake();
        canvasRect = GameObject.Find("Canvas")?.GetComponent<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
        rect.SetAsLastSibling();
    }

    public void SetPosition(Vector2 fixPos) {
        Vector2 mousePos = Input.mousePosition;
        Vector2 canvasPos = mousePos.GetCorrespondingPixel(Utility.GetScreenSize(), canvasRect.rect.size);
        rect.anchoredPosition = canvasPos + fixPos;
    }

    public void SetSize(Vector2 size) {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public void SetText(string content, TextAnchor align = TextAnchor.MiddleCenter, float lineSpacing = 1.2f) {
        text.text = content;
        text.alignment = align;
        text.lineSpacing = lineSpacing;
    }

    public void SetInfoPrompt(Vector2 size, string content, TextAnchor align = TextAnchor.MiddleCenter, float lineSpacing = 1.2f) {
        var fixPos = (Application.platform == RuntimePlatform.WindowsPlayer) ? new Vector2(10, 2) : new Vector2(60, 2);
        SetInfoPrompt(size, content, fixPos, align, lineSpacing);
    }

    public void SetInfoPromptWithAutoSize(string content, TextAnchor align, float lineSpacing = 1.2f) {
        int sizeX = 21 + (text.fontSize) * Mathf.Min(content.Length, 21);
        int sizeY = 35 + (text.fontSize + 6) * ((content.Length - 1) / 21);
        Vector2 size = new Vector2(sizeX, sizeY);
        SetInfoPrompt(content.GetPreferredSize(21, text.fontSize), content, align, lineSpacing);
    }

    public void SetInfoPrompt(Vector2 size, string content, Vector2 fixPos, TextAnchor align = TextAnchor.MiddleCenter, float lineSpacing = 1.2f) {
        SetPosition(fixPos);
        SetSize(size);
        SetText(content, align, lineSpacing);
    }

    public void SetSkill(Skill skill, bool showAtRight = true) {
        string header = "<size=18><color=#52e5f9>" + skill.name + "</color></size><size=4>\n\n</size>";
        string text = skill.description;
        Vector2 size = text.GetPreferredSize(15, 14, 21, 21 + 40);
        Vector2 fixPos = new Vector2(-size.x - 2, -size.y / 2 - 2);
        
        if (showAtRight) {
            SetInfoPrompt(size, header + text, TextAnchor.MiddleLeft);
        } else {
            SetInfoPrompt(size, header + text, fixPos, TextAnchor.MiddleLeft);
        }
    }

    public void SetItem(Item item, bool showAtRight = true) {
        string header = "<size=18><color=#52e5f9>" + item.name + "</color></size><size=4>\n\n</size>";
        string text = ((item.info.itemDescription == string.Empty) ? string.Empty : (item.info.itemDescription + "\n\n")) + item.info.effectDescription;
        Vector2 size = text.GetPreferredSize(15, 14, 21, 21 + 40);
        Vector2 fixPos = new Vector2(-size.x - 2, -size.y / 2 - 2);

        if (showAtRight) {
            SetInfoPrompt(size, header + text, TextAnchor.MiddleLeft);
        } else {
            SetInfoPrompt(size, header + text, fixPos, TextAnchor.MiddleLeft);
        }
    }
}
