using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapWildNpcTalkController : MonoBehaviour
{
    private const string TalkImageRoot = "Assets/StaticResources/Sprites/Map/talk bubble";

    [Serializable]
    private class TalkImageEntry
    {
        public string id;
        public Sprite sprite;
    }

    [SerializeField] private MapWildNpcBubbleHost bubbleHost;
    [SerializeField] private List<TalkImageEntry> imageLibrary = new List<TalkImageEntry>();

    private MapWildNpcSpriteBubbleController bubbleView;
    private NpcWildTalkInfo talkInfo;
    private float stationaryTimer;
    private bool useStationarySchedule;

    private void Awake()
    {
        CacheReferences();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshEditorImageLibrary();
    }
#endif

    public void Init(NpcWildTalkInfo talkInfo, bool useStationarySchedule)
    {
        CacheReferences();
        bubbleView = bubbleHost?.GetOrCreate();
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

        NpcWildTalkContent content = talkInfo.GetRandomSelf();
        if (content?.IsValid != true)
            return;

        float duration = Mathf.Min(Mathf.Max(0.2f, talkInfo.duration), availableDuration);
        if (!content.IsImage)
        {
            bubbleView.ShowText(content.text, duration);
            return;
        }

        Sprite sprite = GetTalkImage(content.image);
        if (sprite == null)
        {
            Debug.LogWarning($"Wild NPC talk image not found: {content.image}", this);
            return;
        }

        bubbleView.ShowImage(sprite, duration);
    }

    private Sprite GetTalkImage(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        id = NormalizeImageId(id);
        Sprite sprite = FindImageEntry(id)?.sprite;
        if (sprite != null)
            return sprite;

#if UNITY_EDITOR
        return TryCacheEditorTalkImage(id);
#else
        return null;
#endif
    }

#if UNITY_EDITOR
    private Sprite TryCacheEditorTalkImage(string id)
    {
        Sprite sprite = LoadEditorTalkImage(id);
        if (sprite == null)
            return null;

        TalkImageEntry entry = FindImageEntry(id);
        if (entry == null)
        {
            entry = new TalkImageEntry { id = id };
            imageLibrary.Add(entry);
        }

        entry.sprite = sprite;
        EditorUtility.SetDirty(this);
        return sprite;
    }

    private static Sprite LoadEditorTalkImage(string id)
    {
        string normalizedId = id.Replace('\\', '/').Trim('/');

        if (normalizedId.Contains("/"))
        {
            foreach (string extension in new[] { ".png", ".jpg", ".jpeg" })
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{TalkImageRoot}/{normalizedId}{extension}");
                if (sprite != null)
                    return sprite;
            }
        }

        string fileName = System.IO.Path.GetFileNameWithoutExtension(normalizedId);
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        string[] guids = AssetDatabase.FindAssets($"{fileName} t:Sprite", new[] { TalkImageRoot });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.Equals(System.IO.Path.GetFileNameWithoutExtension(path), fileName, StringComparison.OrdinalIgnoreCase))
                continue;

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        return null;
    }

    private void RefreshEditorImageLibrary()
    {
        if (!AssetDatabase.IsValidFolder(TalkImageRoot))
            return;

        bool changed = imageLibrary.RemoveAll(x => x == null) > 0;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { TalkImageRoot });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string id = GetEditorTalkImageId(path);
            if (string.IsNullOrWhiteSpace(id) || FindImageEntry(id) != null)
                continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                continue;

            imageLibrary.Add(new TalkImageEntry { id = id, sprite = sprite });
            changed = true;
        }

        if (changed)
            EditorUtility.SetDirty(this);
    }

    private static string GetEditorTalkImageId(string assetPath)
    {
        string normalizedRoot = TalkImageRoot.Replace('\\', '/').TrimEnd('/');
        string normalizedPath = assetPath.Replace('\\', '/');
        if (!normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase))
            return System.IO.Path.GetFileNameWithoutExtension(normalizedPath);

        string relativePath = normalizedPath.Substring(normalizedRoot.Length + 1);
        string directory = System.IO.Path.GetDirectoryName(relativePath)?.Replace('\\', '/');
        string fileName = System.IO.Path.GetFileNameWithoutExtension(relativePath);
        return string.IsNullOrWhiteSpace(directory) ? fileName : $"{directory}/{fileName}";
    }
#endif

    private TalkImageEntry FindImageEntry(string id)
    {
        id = NormalizeImageId(id);
        return imageLibrary.Find(x =>
            x != null && string.Equals(NormalizeImageId(x.id), id, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeImageId(string id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? string.Empty
            : id.Replace('\\', '/').Trim('/');
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
        if (bubbleHost == null)
            bubbleHost = GetComponentInChildren<MapWildNpcBubbleHost>(true);
    }
}
