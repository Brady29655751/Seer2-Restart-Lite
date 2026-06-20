using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapWildNpcWanderController : MonoBehaviour
{
    private const float TargetReachThreshold = 1.5f;
    private const float StuckCheckInterval = 1.25f;
    private const float StuckDistanceThreshold = 0.5f;
    private const int MaxTargetAttempts = 12;

    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private MapWildNpcSpriteBubbleController bubbleController;

    private MapNavigator navigator;
    private NpcInfo info;
    private NpcWanderInfo wanderInfo;
    private RectTransform rect;
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

    private void Awake()
    {
        CacheReferences();
    }

    public static bool CanWander(NpcInfo info)
    {
        return info != null &&
               info.wanderInfo != null &&
               info.wanderInfo.enabled &&
               info.battleHandler != null &&
               info.battleHandler.Count > 0 &&
               info.eventHandler != null &&
               info.eventHandler.Any(handler =>
                   handler.type == ButtonEventType.OnPointerClick &&
                   handler.action == NpcAction.Battle);
    }

    public void Init(Map map, MapNavigator navigator, NpcInfo info)
    {
        CacheReferences();
        this.navigator = navigator;
        this.info = info;
        wanderInfo = info?.wanderInfo;
        enabled = CanWander(info);
        if (!enabled)
        {
            Stop();
            return;
        }

        bubbleController?.Init(info);
        spawnPos = rect == null ? Vector2.zero : rect.anchoredPosition;
        targetPos = spawnPos;
        lastStuckCheckPos = spawnPos;
        buttonBasePos = buttonRect == null ? Vector2.zero : buttonRect.anchoredPosition;
        initialized = map != null && navigator != null && info != null && wanderInfo != null && rect != null;

        Log($"init map={map?.id} npc={info?.id} spawn={spawnPos} reachable={navigator?.IsTargetReachable(spawnPos)}");
    }

    public void Stop()
    {
        initialized = false;
        hasTarget = false;
        pathQueue.Clear();
        holdTimer = 0f;
        idleTimer = 0f;
        tickTimer = 0f;
        stuckTimer = 0f;
        SetVisualOffset(Vector2.zero);
        bubbleController?.HideImmediate();
    }

    private void OnDisable()
    {
        Stop();
    }

    private void CacheReferences()
    {
        if (rect == null)
            rect = transform as RectTransform;
        if (buttonRect == null)
            buttonRect = transform.Find("View/Button") as RectTransform;
        if (bubbleController == null)
            bubbleController = GetComponentInChildren<MapWildNpcSpriteBubbleController>(true);
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
        float tick = Mathf.Max(0.01f, wanderInfo.moveTick);
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
        float radius = Mathf.Max(16f, wanderInfo.radius);
        Vector2 stepRange = wanderInfo.stepRange;
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
        float radius = Mathf.Max(24f, wanderInfo.radius * 0.6f);
        Vector2 current = GetPosition();
        yield return current + Vector2.right * radius;
        yield return current + Vector2.left * radius;
        yield return current + Vector2.up * radius;
        yield return current + Vector2.down * radius;
        yield return spawnPos;
    }

    private bool TrySetPathToNearestReachable(Vector2 candidate, ref int nearestFail, ref int tooCloseFail, ref int pathFail)
    {
        float searchRadius = Mathf.Max(24f, wanderInfo.radius * 0.5f);
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

        if (wanderInfo.flip && Mathf.Abs(delta.x) > 0.05f)
            SetButtonFacing(delta.x);

        float step = Mathf.Max(1f, wanderInfo.speed) * Mathf.Max(0.01f, elapsed);
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

        float scaleX = directionX > 0f ? wanderInfo.faceRightScale : -wanderInfo.faceRightScale;
        float absX = Mathf.Abs(buttonRect.localScale.x);
        if (absX <= 0.001f)
            absX = 1f;

        buttonRect.localScale = new Vector3(absX * Mathf.Sign(scaleX), buttonRect.localScale.y, buttonRect.localScale.z);
    }

    private void ToggleStepBob()
    {
        float bob = Mathf.Max(0f, wanderInfo.bob);
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
        stuckTimer += Mathf.Max(0.01f, wanderInfo.moveTick);
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
        Vector2 range = wanderInfo?.idleRange ?? new Vector2(2f, 6f);
        float min = Mathf.Max(0f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        return UnityEngine.Random.Range(min, max);
    }

    private float GetSnapHoldTime()
    {
        float tick = wanderInfo == null ? 0f : Mathf.Max(0.01f, wanderInfo.moveTick);
        float snap = wanderInfo == null ? 0f : Mathf.Max(0f, wanderInfo.snap);
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

        if (wanderInfo?.bubble == null || UnityEngine.Random.value > Mathf.Clamp01(wanderInfo.bubble.chance))
            return;

        string text = wanderInfo.bubble?.GetRandomSelf();
        if (string.IsNullOrWhiteSpace(text))
            return;

        float duration = Mathf.Min(Mathf.Max(0.2f, wanderInfo.bubble.duration), idleDuration);
        bubbleController.Show(text, duration);
    }

    private void Log(string message)
    {
        if (wanderInfo == null || !wanderInfo.debug)
            return;

        Debug.Log($"[WildNpcWander] {message}");
    }
}

