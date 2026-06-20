using System.Collections.Generic;
using System;
using UnityEngine;

public class MapWildNpcWanderController : MonoBehaviour
{
    private const float TargetReachThreshold = 1.5f;
    private const float StuckCheckInterval = 1.25f;
    private const float StuckDistanceThreshold = 0.5f;
    private const int MaxTargetAttempts = 12;

    [SerializeField] private NpcController npcController;

    private MapNavigator navigator;
    private NpcInfo info;
    private NpcWildMovementInfo movementInfo;
    private RectTransform rect;
    private readonly Queue<Vector2> pathQueue = new Queue<Vector2>();
    private Vector2 spawnPos;
    private Vector2 targetPos;
    private Vector2 lastStuckCheckPos;
    private float idleTimer;
    private float holdTimer;
    private float tickTimer;
    private float stuckTimer;
    private bool initialized;
    private bool hasTarget;
    private bool bobRaised;

    public event Action<float> RestStarted;

    private void Awake()
    {
        CacheReferences();
    }

    public void Init(Map map, MapNavigator navigator, NpcInfo info, NpcWildMovementInfo movementInfo)
    {
        CacheReferences();
        this.navigator = navigator;
        this.info = info;
        this.movementInfo = movementInfo;
        enabled = movementInfo?.isWander == true;
        if (!enabled)
        {
            Stop();
            return;
        }

        spawnPos = rect == null ? Vector2.zero : rect.anchoredPosition;
        targetPos = spawnPos;
        lastStuckCheckPos = spawnPos;
        initialized = map != null && navigator != null && info != null && movementInfo != null && rect != null;

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
    }

    private void OnDisable()
    {
        Stop();
    }

    private void CacheReferences()
    {
        if (rect == null)
            rect = transform as RectTransform;
        if (npcController == null)
            npcController = GetComponent<NpcController>();
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
        float tick = Mathf.Max(0.01f, movementInfo.moveTick);
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
        float radius = Mathf.Max(16f, movementInfo.radius);
        Vector2 stepRange = movementInfo.stepRange;
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
        float radius = Mathf.Max(24f, movementInfo.radius * 0.6f);
        Vector2 current = GetPosition();
        yield return current + Vector2.right * radius;
        yield return current + Vector2.left * radius;
        yield return current + Vector2.up * radius;
        yield return current + Vector2.down * radius;
        yield return spawnPos;
    }

    private bool TrySetPathToNearestReachable(Vector2 candidate, ref int nearestFail, ref int tooCloseFail, ref int pathFail)
    {
        float searchRadius = Mathf.Max(24f, movementInfo.radius * 0.5f);
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

        if (movementInfo.flip && Mathf.Abs(delta.x) > 0.05f)
            npcController?.SetVisualFacing(delta.x, movementInfo.originalFacesRight);

        float step = Mathf.Max(1f, movementInfo.speed) * Mathf.Max(0.01f, elapsed);
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
        RestStarted?.Invoke(idleTimer);
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

    private void ToggleStepBob()
    {
        float bob = Mathf.Max(0f, movementInfo.bob);
        if (npcController == null || bob <= 0f)
        {
            SetVisualOffset(Vector2.zero);
            return;
        }

        bobRaised = !bobRaised;
        SetVisualOffset(bobRaised ? new Vector2(0f, bob) : Vector2.zero);
    }

    private void SetVisualOffset(Vector2 offset)
    {
        npcController?.SetVisualOffset(offset);
    }

    private void CheckStuck()
    {
        stuckTimer += Mathf.Max(0.01f, movementInfo.moveTick);
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
        Vector2 range = movementInfo?.idleRange ?? new Vector2(2f, 6f);
        float min = Mathf.Max(0f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        return UnityEngine.Random.Range(min, max);
    }

    private float GetSnapHoldTime()
    {
        float tick = movementInfo == null ? 0f : Mathf.Max(0.01f, movementInfo.moveTick);
        float snap = movementInfo == null ? 0f : Mathf.Max(0f, movementInfo.snap);
        return snap * tick;
    }

    private float GetIdleDuration()
    {
        return GetRandomIdleTime() + GetSnapHoldTime();
    }

    private void Log(string message)
    {
        if (movementInfo == null || !movementInfo.debug)
            return;

        Debug.Log($"[WildNpcWander] {message}");
    }
}


