using System;
using System.Collections.Generic;
using UnityEngine;

public class MapWildNpcBubbleHost : MonoBehaviour
{
    [Serializable]
    private class StyleEntry
    {
        public string id = string.Empty;
        public GameObject prefab;
    }

    [SerializeField] private string defaultStyleId = "white";
    [SerializeField] private List<StyleEntry> styles = new List<StyleEntry>();

    private MapWildNpcSpriteBubbleController currentView;
    private string currentStyleId;

    public MapWildNpcSpriteBubbleController GetOrCreate(string styleId = null)
    {
        string requestedId = string.IsNullOrWhiteSpace(styleId) ? GetConfiguredStyleId() : styleId;
        if (currentView != null && string.Equals(currentStyleId, requestedId, StringComparison.OrdinalIgnoreCase))
            return currentView;

        StyleEntry style = styles.Find(x =>
            x != null && x.prefab != null && string.Equals(x.id, requestedId, StringComparison.OrdinalIgnoreCase));
        style ??= styles.Find(x => x?.prefab != null);
        if (style == null)
            return null;

        if (currentView != null)
            Destroy(currentView.gameObject);

        GameObject instance = Instantiate(style.prefab, transform, false);
        currentView = instance.GetComponent<MapWildNpcSpriteBubbleController>();
        if (currentView == null)
        {
            Destroy(instance);
            return null;
        }

        currentView.name = style.prefab.name;
        currentStyleId = style.id;
        return currentView;
    }

    private string GetConfiguredStyleId()
    {
        if (!Application.isPlaying || Player.instance?.gameData?.settingsData == null)
            return defaultStyleId;

        string styleId = Player.instance.gameData.settingsData.wildNpcBubbleStyle;
        return string.IsNullOrWhiteSpace(styleId) ? defaultStyleId : styleId;
    }
}
