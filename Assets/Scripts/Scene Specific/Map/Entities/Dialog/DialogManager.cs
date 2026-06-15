using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : Manager<DialogManager>
{
    [SerializeField] private RectTransform UILayer;
    [SerializeField] private RectTransform dialogLayer;
    [SerializeField] private DialogController dialogController;
    [SerializeField] private RectTransform dialogStoryLayer;
    [SerializeField] private DialogController dialogStoryController;
    [Header("Open Animation")]
    [SerializeField] private bool useOpenAnimation = true;
    [SerializeField, Min(0f)] private float openAnimationDuration = 0.18f;
    [SerializeField] private float openAnimationRiseDistance = 12f;
    [SerializeField] private AnimationCurve openAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public NpcController currentNpc { get; private set; }
    private Vector2 dialogLayerPosition;
    private Vector2 dialogStoryLayerPosition;
    private Coroutine dialogOpenAnimationCoroutine;
    private Coroutine dialogStoryOpenAnimationCoroutine;

    protected override void Awake()
    {
        base.Awake();
        dialogLayerPosition = dialogLayer.anchoredPosition;
        dialogStoryLayerPosition = dialogStoryLayer.anchoredPosition;
        ResetDialogLayerVisual(dialogLayer, dialogLayerPosition);
        ResetDialogLayerVisual(dialogStoryLayer, dialogStoryLayerPosition);
    }

    private void SetDialogLayerActive(bool acitve) {
        UILayer.gameObject.SetActive(!acitve);
        dialogLayer.gameObject.SetActive(acitve);
        if (acitve)
            PlayOpenAnimation(dialogLayer, dialogLayerPosition, ref dialogOpenAnimationCoroutine);
        else
            StopOpenAnimation(dialogLayer, dialogLayerPosition, ref dialogOpenAnimationCoroutine);
    }

    private void SetStoryDialogLayerActive(bool acitve) {
        UILayer.gameObject.SetActive(!acitve);
        dialogStoryLayer.gameObject.SetActive(acitve);
        if (acitve)
            PlayOpenAnimation(dialogStoryLayer, dialogStoryLayerPosition, ref dialogStoryOpenAnimationCoroutine);
        else
            StopOpenAnimation(dialogStoryLayer, dialogStoryLayerPosition, ref dialogStoryOpenAnimationCoroutine);
    }
    public void SetCurrentNpc(NpcInfo info) {
        currentNpc = ((Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList"))?.Get(info.id);
        Player.instance.currentNpcId = info.id;
    }

    public void OpenDialog(DialogInfo info) {
        Player.instance.isShootMode = false;
        dialogLayer.SetAsLastSibling();
        
        if (info == null) {
            CloseDialog();
            return;
        }

        if (dialogStoryLayer.gameObject.activeSelf)
        {
            SetStoryDialogLayerActive(false);
        }

        SetDialogLayerActive(true);
        dialogController.OpenDialog(info);
    }
    
    public void OpenStoryDialog(DialogInfo info) {
        if (info == null) {
            CloseDialog();
            return;
        }
        if (dialogLayer.gameObject.activeSelf)
        {
            SetDialogLayerActive(false);
        }

        SetStoryDialogLayerActive(true);
        dialogStoryController.OpenDialog(info);
    }

  
    public void CloseDialog() {
        SetDialogLayerActive(false);
        SetStoryDialogLayerActive(false);
        Player.instance.currentNpcId = 0;
    }

    private void PlayOpenAnimation(RectTransform layer, Vector2 targetPosition, ref Coroutine coroutine)
    {
        StopOpenAnimation(layer, targetPosition, ref coroutine);

        if (!useOpenAnimation || openAnimationDuration <= 0f)
        {
            ResetDialogLayerVisual(layer, targetPosition);
            return;
        }

        coroutine = StartCoroutine(OpenAnimationCoroutine(layer, targetPosition));
    }

    private void StopOpenAnimation(RectTransform layer, Vector2 targetPosition, ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        ResetDialogLayerVisual(layer, targetPosition);
    }

    private IEnumerator OpenAnimationCoroutine(RectTransform layer, Vector2 targetPosition)
    {
        CanvasGroup canvasGroup = GetCanvasGroup(layer);
        Vector2 startPosition = targetPosition + Vector2.down * openAnimationRiseDistance;
        float time = 0f;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        layer.anchoredPosition = startPosition;

        while (time < openAnimationDuration)
        {
            float progress = Mathf.Clamp01(time / openAnimationDuration);
            float curvedProgress = openAnimationCurve.Evaluate(progress);
            canvasGroup.alpha = curvedProgress;
            layer.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, curvedProgress);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        ResetDialogLayerVisual(layer, targetPosition);
    }

    private void ResetDialogLayerVisual(RectTransform layer, Vector2 targetPosition)
    {
        if (layer == null)
            return;

        CanvasGroup canvasGroup = GetCanvasGroup(layer);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        layer.anchoredPosition = targetPosition;
    }

    private CanvasGroup GetCanvasGroup(RectTransform layer)
    {
        CanvasGroup canvasGroup = layer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = layer.gameObject.AddComponent<CanvasGroup>();

        return canvasGroup;
    }

}
