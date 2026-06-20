using UnityEngine;

public class MapWildNpcTalkController : MonoBehaviour
{
    [SerializeField] private MapWildNpcSpriteBubbleController bubbleView;

    private NpcWildTalkInfo talkInfo;
    private float stationaryTimer;
    private bool useStationarySchedule;

    private void Awake()
    {
        CacheReferences();
    }

    public void Init(NpcWildTalkInfo talkInfo, bool useStationarySchedule)
    {
        CacheReferences();
        this.talkInfo = talkInfo;
        this.useStationarySchedule = useStationarySchedule;
        bubbleView?.Init();

        enabled = talkInfo?.HasSelf == true;
        if (!enabled)
        {
            Stop();
            return;
        }

        if (useStationarySchedule)
            ScheduleNextStationaryTalk();
    }

    public void OnRestStarted(float restDuration)
    {
        if (useStationarySchedule || talkInfo?.HasSelf != true || restDuration < 0.2f)
            return;

        TryShowTalk(restDuration);
    }

    public void Stop()
    {
        talkInfo = null;
        stationaryTimer = 0f;
        useStationarySchedule = false;
        bubbleView?.HideImmediate();
        enabled = false;
    }

    private void Update()
    {
        if (!useStationarySchedule || talkInfo?.HasSelf != true)
            return;

        stationaryTimer -= Time.deltaTime;
        if (stationaryTimer > 0f)
            return;

        TryShowTalk(Mathf.Max(0.2f, talkInfo.duration));
        ScheduleNextStationaryTalk();
    }

    private void TryShowTalk(float availableDuration)
    {
        if (bubbleView == null || UnityEngine.Random.value > Mathf.Clamp01(talkInfo.chance))
            return;

        string text = talkInfo.GetRandomSelf();
        if (string.IsNullOrWhiteSpace(text))
            return;

        float duration = Mathf.Min(Mathf.Max(0.2f, talkInfo.duration), availableDuration);
        bubbleView.Show(text, duration);
    }

    private void ScheduleNextStationaryTalk()
    {
        Vector2 range = talkInfo?.intervalRange ?? new Vector2(5f, 10f);
        float min = Mathf.Max(0.2f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        stationaryTimer = UnityEngine.Random.Range(min, max);
    }

    private void CacheReferences()
    {
        if (bubbleView == null)
            bubbleView = GetComponentInChildren<MapWildNpcSpriteBubbleController>(true);
    }
}
