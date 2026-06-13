using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Animations;

public class StartButton : IButton
{
    public Animator anim;
    [Header("Hover Feedback")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float hoverLift = 4f;
    [SerializeField] private float hoverLerpSpeed = 12f;

    private Vector3 baseScale;
    private Vector2 basePosition;
    private bool hoverActive;

    protected override void Awake()
    {
        base.Awake();
        baseScale = rect.localScale;
        basePosition = rect.anchoredPosition;
    }

    protected override void Update()
    {
        base.Update();

        if (rect == null)
            return;

        var targetScale = baseScale * (hoverActive ? hoverScale : 1f);
        var targetPosition = basePosition + (hoverActive ? (Vector2.up * hoverLift) : Vector2.zero);
        var progress = 1f - Mathf.Exp(-hoverLerpSpeed * Time.unscaledDeltaTime);
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, progress);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPosition, progress);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        base.OnPointerEnter(eventData);
        SetAnimActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        SetAnimActive(false);
    }

    protected void OnDisable()
    {
        hoverActive = false;

        if (rect == null)
            return;

        rect.localScale = baseScale;
        rect.anchoredPosition = basePosition;
    }

    public void SetAnimActive(bool active) {
        hoverActive = active;
        if (anim != null)
            anim.SetBool("OnPointerEnter", active);
    }

}
