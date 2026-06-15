using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class NpcHandler
{
    public static void CreateNpc(NpcController npc, NpcInfo info, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt)
    {
        npc.SetActive(info.active);
        npc.SetNpcInfo(info);
        npc.SetAction(npcList, infoPrompt);
        npcList.Add(info.id, npc);
    }

    public static void CreateFarm(NpcController npc, NpcInfo info, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt)
    {
        npc.SetActive(info.active);
        npc.SetNpcInfo(info);
        npc.SetFarmAction(npcList, infoPrompt);
        npcList.Add(info.id, npc);
    }

    public static void CreateAnimal(NpcController npc, NpcInfo info, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt)
    {
        npc.SetActive(info.active);
        npc.SetNpcInfo(info);
        npc.SetAnimalAction(npcList, infoPrompt);
        npcList.Add(info.id, npc);
    }

    public static UnityEvent GetButtonEvent(IButton button, NpcButtonHandler handler)
    {
        return handler.type switch
        {
            ButtonEventType.OnPointerClick => button.onPointerClickEvent,
            ButtonEventType.OnPointerEnter => button.onPointerEnterEvent,
            ButtonEventType.OnPointerExit => button.onPointerExitEvent,
            ButtonEventType.OnPointerOver => button.onPointerOverEvent,
            _ => null,
        };
    }

    public static Func<bool> GetNpcCondition(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList)
    {
        if ((handler.condition == null) || (handler.condition.Count == 0))
            return () => true;

        Func<bool> condition = () => true;
        for (int i = 0; i < handler.condition.Count; i++)
        {
            var conditionOr = handler.condition[i].Split('|');

            Func<bool> oldCondition = new Func<bool>(condition);
            Func<bool> newCondition = () => false;

            for (int j = 0; j < conditionOr.Length; j++)
            {
                Func<bool> orCondition = new Func<bool>(newCondition);
                var split = Operator.SplitCondition(conditionOr[j], out var op, toHalf: false);
                newCondition = () => orCondition.Invoke() || (NpcConditionHandler.GetNpcCondition(op, split.Key, split.Value).Invoke());
            }
            condition = () => oldCondition.Invoke() && newCondition.Invoke();
        }
        return condition;
    }

    public static Action GetNpcAction(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList)
    {
        NpcInfo npcInfo = npc?.GetInfo();
        return handler.action switch
        {
            NpcAction.SetNpcParam => () => NpcActionHandler.SetNpcParam(npc, handler, npcList),
            NpcAction.OpenHintbox => () => NpcActionHandler.OpenHintbox(npc, handler, npcList),
            NpcAction.OpenPanel => () => NpcActionHandler.OpenPanel(handler),
            NpcAction.OpenDialog => () => NpcActionHandler.OpenDialog(npcInfo, handler),
            NpcAction.Teleport => () => NpcActionHandler.Teleport(handler),
            NpcAction.SetItem => () => NpcActionHandler.SetItem(handler),
            NpcAction.GetPet => () => NpcActionHandler.GetPet(handler),
            NpcAction.RemovePet => () => NpcActionHandler.RemovePet(handler),
            NpcAction.SetPet => () => NpcActionHandler.SetPet(handler),
            NpcAction.EvolvePet => () => NpcActionHandler.EvolvePet(handler),
            NpcAction.SetMission => () => NpcActionHandler.SetMission(handler),
            NpcAction.SetActivity => () => NpcActionHandler.SetActivity(handler),
            NpcAction.Battle => () => NpcActionHandler.StartBattle(npcInfo, handler),
            NpcAction.Player => () => NpcActionHandler.SetPlayer(handler),
            NpcAction.SetMail => () => NpcActionHandler.SetMail(handler),
            NpcAction.Fish => () => NpcActionHandler.Fish(),
            NpcAction.MiniGame => () => NpcActionHandler.MiniGame(handler),
            NpcAction.Callback => () => NpcActionHandler.Callback(npc, handler, npcList),
            _ => () => handler.callback?.ForEach(x => x?.Invoke()),
        };
    }

    public static Action GetNpcEntity(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList)
    {
        NpcInfo npcInfo = npc?.GetInfo();
        Func<bool> condition = GetNpcCondition(npc, handler, npcList);
        Action action = GetNpcAction(npc, handler, npcList);
        return () =>
        {
            if (Player.instance.isShootMode ^ (handler.typeId == "shoot"))
                return;

            if (!condition.Invoke())
                return;

            action?.Invoke();
        };
    }
}

public class MapWildNpcWanderController : MonoBehaviour
{
    private const float TargetReachThreshold = 1.5f;
    private const float StuckCheckInterval = 1.25f;
    private const float StuckDistanceThreshold = 0.5f;
    private const int MaxTargetAttempts = 12;

    private MapNavigator navigator;
    private NpcInfo info;
    private RectTransform rect;
    private RectTransform buttonRect;
    private MapWildNpcSpriteBubbleController bubbleController;
    private readonly Queue<Vector2> pathQueue = new Queue<Vector2>();
    private Vector2 spawnPos;
    private Vector2 targetPos;
    private Vector2 lastStuckCheckPos;
    private Vector2 buttonBasePos;
    private float idleTimer;
    private float holdTimer;
    private float tickTimer;
    private float stuckTimer;
    private bool initialized;
    private bool hasTarget;
    private bool bobRaised;

    public static bool CanWander(NpcInfo info)
    {
        return info != null &&
               info.wander &&
               info.battleHandler != null &&
               info.battleHandler.Count > 0 &&
               info.eventHandler != null &&
               info.eventHandler.Any(handler =>
                   handler.type == ButtonEventType.OnPointerClick &&
                   handler.action == NpcAction.Battle);
    }

    public void Init(Map map, MapNavigator navigator, NpcInfo info)
    {
        this.navigator = navigator;
        this.info = info;
        rect = transform as RectTransform;
        buttonRect = transform.Find("View/Button") as RectTransform;
        bubbleController = gameObject.AddComponent<MapWildNpcSpriteBubbleController>();
        bubbleController.Init(info);
        spawnPos = rect == null ? Vector2.zero : rect.anchoredPosition;
        targetPos = spawnPos;
        lastStuckCheckPos = spawnPos;
        buttonBasePos = buttonRect == null ? Vector2.zero : buttonRect.anchoredPosition;
        initialized = map != null && navigator != null && info != null && rect != null;

        Log($"init map={map?.id} npc={info?.id} spawn={spawnPos} reachable={navigator?.IsTargetReachable(spawnPos)}");
    }

    private void OnDisable()
    {
        SetVisualOffset(Vector2.zero);
        bubbleController?.HideImmediate();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (holdTimer > 0f)
        {
            holdTimer -= Time.deltaTime;
            return;
        }

        if (!hasTarget)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
                ChooseNextPath();
            return;
        }

        tickTimer += Time.deltaTime;
        float tick = Mathf.Max(0.01f, info.wanderMoveTick);
        if (tickTimer < tick)
            return;

        float elapsed = tickTimer;
        tickTimer = 0f;
        MoveOneTick(elapsed);
    }

    private void ChooseNextPath()
    {
        pathQueue.Clear();
        hasTarget = false;
        SetVisualOffset(Vector2.zero);
        int nearestFail = 0;
        int tooCloseFail = 0;
        int pathFail = 0;

        for (int i = 0; i < MaxTargetAttempts; i++)
        {
            if (TrySetPathToNearestReachable(GetRandomCandidate(), ref nearestFail, ref tooCloseFail, ref pathFail))
                return;
        }

        foreach (var candidate in GetFallbackCandidates())
        {
            if (TrySetPathToNearestReachable(candidate, ref nearestFail, ref tooCloseFail, ref pathFail))
                return;
        }

        idleTimer = GetRandomIdleTime() + GetSnapHoldTime();
        Log($"path failed from={GetPosition()} nearestFail={nearestFail} tooClose={tooCloseFail} pathFail={pathFail}");
    }

    private Vector2 GetRandomCandidate()
    {
        float radius = Mathf.Max(16f, info.wanderRadius);
        Vector2 stepRange = info.wanderStepRange;
        float minStep = Mathf.Clamp(Mathf.Min(stepRange.x, stepRange.y), 1f, radius);
        float maxStep = Mathf.Clamp(Mathf.Max(stepRange.x, stepRange.y), minStep, radius);
        float distance = UnityEngine.Random.Range(minStep, maxStep);
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        Vector2 candidate = GetPosition() + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        Vector2 fromSpawn = candidate - spawnPos;
        if (fromSpawn.magnitude > radius)
            candidate = spawnPos + fromSpawn.normalized * radius;

        return candidate;
    }

    private IEnumerable<Vector2> GetFallbackCandidates()
    {
        float radius = Mathf.Max(24f, info.wanderRadius * 0.6f);
        Vector2 current = GetPosition();
        yield return current + Vector2.right * radius;
        yield return current + Vector2.left * radius;
        yield return current + Vector2.up * radius;
        yield return current + Vector2.down * radius;
        yield return spawnPos;
    }

    private bool TrySetPathToNearestReachable(Vector2 candidate, ref int nearestFail, ref int tooCloseFail, ref int pathFail)
    {
        float searchRadius = Mathf.Max(24f, info.wanderRadius * 0.5f);
        if (!navigator.TryFindNearestReachablePoint(candidate, searchRadius, out var reachable))
        {
            nearestFail++;
            return false;
        }

        if ((reachable - GetPosition()).sqrMagnitude < 64f)
        {
            tooCloseFail++;
            return false;
        }

        List<Vector2> path;
        if (navigator.HasLineOfSight(GetPosition(), reachable))
            path = new List<Vector2> { reachable };
        else if (!navigator.TryFindPath(GetPosition(), reachable, out path))
        {
            pathFail++;
            return false;
        }

        SetPath(path);
        Log($"path ok npc={info?.id} from={GetPosition()} target={reachable} points={path.Count}");
        return true;
    }

    private void SetPath(List<Vector2> path)
    {
        pathQueue.Clear();
        foreach (var point in path)
            pathQueue.Enqueue(point);

        hasTarget = pathQueue.Count > 0;
        if (hasTarget)
            targetPos = pathQueue.Dequeue();

        lastStuckCheckPos = GetPosition();
        stuckTimer = 0f;
        tickTimer = 0f;
        holdTimer = 0f;
    }

    private void MoveOneTick(float elapsed)
    {
        Vector2 current = GetPosition();
        Vector2 delta = targetPos - current;
        if (delta.magnitude <= TargetReachThreshold)
        {
            ArriveAtTarget();
            return;
        }

        if (info.wanderFlip && Mathf.Abs(delta.x) > 0.05f)
            SetButtonFacing(delta.x);

        float step = Mathf.Max(1f, info.wanderSpeed) * Mathf.Max(0.01f, elapsed);
        Vector2 next = Vector2.MoveTowards(current, targetPos, step);
        SetPosition(SnapToPixel(next));
        ToggleStepBob();
        CheckStuck();
    }

    private void ArriveAtTarget()
    {
        SetPosition(SnapToPixel(targetPos));
        SetVisualOffset(Vector2.zero);
        bobRaised = false;

        if (pathQueue.Count > 0)
        {
            targetPos = pathQueue.Dequeue();
            holdTimer = GetSnapHoldTime();
            return;
        }

        hasTarget = false;
        idleTimer = GetIdleDuration();
        TryShowIdleBubble(idleTimer);
    }

    private Vector2 GetPosition()
    {
        return rect == null ? Vector2.zero : rect.anchoredPosition;
    }

    private void SetPosition(Vector2 pos)
    {
        if (rect != null)
            rect.anchoredPosition = pos;
    }

    private Vector2 SnapToPixel(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    private void SetButtonFacing(float directionX)
    {
        if (buttonRect == null)
            return;

        float scaleX = directionX > 0f ? info.wanderFaceRightScale : -info.wanderFaceRightScale;
        float absX = Mathf.Abs(buttonRect.localScale.x);
        if (absX <= 0.001f)
            absX = 1f;

        buttonRect.localScale = new Vector3(absX * Mathf.Sign(scaleX), buttonRect.localScale.y, buttonRect.localScale.z);
    }

    private void ToggleStepBob()
    {
        float bob = Mathf.Max(0f, info.wanderBob);
        if (buttonRect == null || bob <= 0f)
        {
            SetVisualOffset(Vector2.zero);
            return;
        }

        bobRaised = !bobRaised;
        SetVisualOffset(bobRaised ? new Vector2(0f, bob) : Vector2.zero);
    }

    private void SetVisualOffset(Vector2 offset)
    {
        if (buttonRect != null)
            buttonRect.anchoredPosition = buttonBasePos + offset;
    }

    private void CheckStuck()
    {
        stuckTimer += Mathf.Max(0.01f, info.wanderMoveTick);
        if (stuckTimer < StuckCheckInterval)
            return;

        Vector2 current = GetPosition();
        if ((current - lastStuckCheckPos).magnitude <= StuckDistanceThreshold)
        {
            pathQueue.Clear();
            hasTarget = false;
            SetVisualOffset(Vector2.zero);
            idleTimer = GetIdleDuration();
            Log($"stuck reset npc={info?.id} pos={current}");
        }

        lastStuckCheckPos = current;
        stuckTimer = 0f;
    }

    private float GetRandomIdleTime()
    {
        Vector2 range = info?.wanderIdleRange ?? new Vector2(2f, 6f);
        float min = Mathf.Max(0f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        return UnityEngine.Random.Range(min, max);
    }

    private float GetSnapHoldTime()
    {
        float tick = info == null ? 0f : Mathf.Max(0.01f, info.wanderMoveTick);
        float snap = info == null ? 0f : Mathf.Max(0f, info.wanderSnap);
        return snap * tick;
    }

    private float GetIdleDuration()
    {
        return GetRandomIdleTime() + GetSnapHoldTime();
    }

    private void TryShowIdleBubble(float idleDuration)
    {
        if (bubbleController == null || idleDuration < 0.2f)
            return;

        if (UnityEngine.Random.value > Mathf.Clamp01(info.wanderBubbleChance))
            return;

        string text = info.wanderBubble?.GetRandomSelf();
        if (string.IsNullOrWhiteSpace(text))
            return;

        float duration = Mathf.Min(Mathf.Max(0.2f, info.wanderBubbleDuration), idleDuration);
        bubbleController.Show(text, duration);
    }

    private void Log(string message)
    {
        if (info == null || !info.wanderDebug)
            return;

        Debug.Log($"[WildNpcWander] {message}");
    }
}
// 野生宠物随机触发的Bubble参数
public class MapWildNpcSpriteBubbleController : MonoBehaviour
{
    private const int MaxTextLength = 30;
    private const float FadeTime = 0.18f;
    private const int BubbleFontSize = 17;
    private const float MinBubbleWidth = 50f;
    private const float MaxBubbleWidth = 200f;
    private const float MinBubbleHeight = 72f;
    private const float BubblePaddingX = 15f;
    private const float BubblePaddingY = 20f;
    private const float BubbleTailHeight = 15f;
    private const float BubbleBottomOffset = 7f;
    private const string BubbleSpritePath = "Sprites/UI/WildNpcBubbleIntegrated";
    private const TextAnchor BubbleTextAnchor = TextAnchor.MiddleLeft; // MiddleLeft左对齐，MiddleCenter居中对齐，MiddleRight右对齐

    private NpcInfo info;
    private RectTransform bubbleRect;
    private CanvasGroup canvasGroup;
    private Text bubbleText;
    private Image bubbleImage;
    private float remainingTime;
    private float totalDuration;

    public void Init(NpcInfo info)
    {
        this.info = info;
        EnsureView();
        HideImmediate();
    }

    public void Show(string content, float duration)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        EnsureView();
        string text = TrimText(content.Trim());
        bubbleText.text = text;
        bubbleText.alignment = BubbleTextAnchor;
        Vector2 bubbleSize = GetBubbleSize(text);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bubbleSize.x);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bubbleSize.y);
        bubbleRect.anchoredPosition = GetBubblePosition();
        ApplyTextPadding();

        remainingTime = Mathf.Max(0.2f, duration);
        totalDuration = remainingTime;
        bubbleRect.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    public void HideImmediate()
    {
        remainingTime = 0f;
        totalDuration = 0f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (bubbleRect != null)
            bubbleRect.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (bubbleRect == null || !bubbleRect.gameObject.activeSelf)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            HideImmediate();
            return;
        }

        if (remainingTime < FadeTime)
            canvasGroup.alpha = Mathf.Clamp01(remainingTime / FadeTime);
        else if (totalDuration - remainingTime < FadeTime)
            canvasGroup.alpha = Mathf.Clamp01((totalDuration - remainingTime) / FadeTime);
        else
            canvasGroup.alpha = 1f;
    }

    private void EnsureView()
    {
        if (bubbleRect != null)
            return;

        var bubbleObject = new GameObject("Wild Npc Bubble", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        bubbleRect = bubbleObject.GetComponent<RectTransform>();
        bubbleRect.SetParent(transform, false);
        bubbleRect.anchorMin = new Vector2(0.5f, 0.5f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
        bubbleRect.pivot = new Vector2(0.5f, 0f);
        bubbleRect.localScale = Vector3.one;

        canvasGroup = bubbleObject.GetComponent<CanvasGroup>();
        bubbleImage = bubbleObject.GetComponent<Image>();
        bubbleImage.sprite = Resources.Load<Sprite>(BubbleSpritePath);
        bubbleImage.type = Image.Type.Simple;
        bubbleImage.preserveAspect = false;
        bubbleImage.color = Color.white;
        bubbleImage.raycastTarget = false;

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(bubbleRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);

        bubbleText = textObject.GetComponent<Text>();
        bubbleText.alignment = BubbleTextAnchor;
        bubbleText.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        bubbleText.fontSize = BubbleFontSize;
        bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bubbleText.verticalOverflow = VerticalWrapMode.Truncate;
        bubbleText.raycastTarget = false;
        bubbleText.supportRichText = false;
        bubbleText.font = GetBubbleFont();

        var shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(1f, 1f, 1f, 0.4f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    private Font GetBubbleFont()
    {
        var nameText = transform.Find("View/Name")?.GetComponent<Text>();
        if (nameText?.font != null)
            return nameText.font;

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private Vector2 GetBubblePosition()
    {
        float npcHeight = Mathf.Max(40f, info?.size.y ?? 40f);
        return new Vector2(0f, npcHeight * 0.5f + BubbleBottomOffset);
    }

    private Vector2 GetBubbleSize(string text)
    {
        float maxTextWidth = MaxBubbleWidth - BubblePaddingX * 2f;
        float preferredWidth = bubbleText.cachedTextGeneratorForLayout.GetPreferredWidth(
            text,
            bubbleText.GetGenerationSettings(new Vector2(maxTextWidth, 0f))) / bubbleText.pixelsPerUnit;
        float width = Mathf.Clamp(preferredWidth + BubblePaddingX * 2f, MinBubbleWidth, MaxBubbleWidth);

        float contentWidth = Mathf.Max(40f, width - BubblePaddingX * 2f);
        float preferredHeight = bubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(
            text,
            bubbleText.GetGenerationSettings(new Vector2(contentWidth, 0f))) / bubbleText.pixelsPerUnit;
        float height = Mathf.Max(MinBubbleHeight, preferredHeight + BubblePaddingY * 2f + BubbleTailHeight);
        return new Vector2(width, height);
    }

    private void ApplyTextPadding()
    {
        var textRect = bubbleText.rectTransform;
        textRect.offsetMin = new Vector2(BubblePaddingX, BubblePaddingY + BubbleTailHeight);
        textRect.offsetMax = new Vector2(-BubblePaddingX, -BubblePaddingY);
    }

    private string TrimText(string text)
    {
        if (text.Length <= MaxTextLength)
            return text;

        return text.Substring(0, MaxTextLength);
    }
}
