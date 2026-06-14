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
        idleTimer = GetRandomIdleTime() + GetSnapHoldTime();
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
            idleTimer = GetRandomIdleTime() + GetSnapHoldTime();
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

    private void Log(string message)
    {
        if (info == null || !info.wanderDebug)
            return;

        Debug.Log($"[WildNpcWander] {message}");
    }
}
