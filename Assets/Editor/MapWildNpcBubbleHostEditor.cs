using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapWildNpcBubbleHost))]
public class MapWildNpcBubbleHostEditor : Editor
{
    private SerializedProperty defaultStyleIdProperty;
    private SerializedProperty stylesProperty;

    private void OnEnable()
    {
        defaultStyleIdProperty = serializedObject.FindProperty("defaultStyleId");
        stylesProperty = serializedObject.FindProperty("styles");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultStylePopup();
        EditorGUILayout.Space(4f);
        EditorGUILayout.PropertyField(stylesProperty, includeChildren: true);
        EditorGUILayout.Space(6f);
        DrawSelectedStylePreview();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaultStylePopup()
    {
        List<string> ids = new List<string>();
        List<string> labels = new List<string>();

        for (int i = 0; i < stylesProperty.arraySize; i++)
        {
            SerializedProperty styleProperty = stylesProperty.GetArrayElementAtIndex(i);
            SerializedProperty idProperty = styleProperty.FindPropertyRelative("id");
            SerializedProperty prefabProperty = styleProperty.FindPropertyRelative("prefab");

            string id = idProperty.stringValue;
            GameObject prefab = prefabProperty.objectReferenceValue as GameObject;
            if (string.IsNullOrWhiteSpace(id))
                id = prefab != null ? prefab.name : string.Empty;

            if (string.IsNullOrWhiteSpace(id))
                continue;

            ids.Add(id);
            labels.Add(prefab != null ? $"{id} ({prefab.name})" : $"{id} (missing prefab)");
        }

        if (ids.Count == 0)
        {
            EditorGUILayout.PropertyField(defaultStyleIdProperty);
            EditorGUILayout.HelpBox("Add at least one style before choosing the default style.", MessageType.Info);
            return;
        }

        string currentId = defaultStyleIdProperty.stringValue;
        int currentIndex = ids.FindIndex(x => string.Equals(x, currentId, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0)
        {
            ids.Insert(0, currentId);
            labels.Insert(0, string.IsNullOrWhiteSpace(currentId) ? "<empty> (missing style)" : $"{currentId} (missing style)");
            currentIndex = 0;
        }

        EditorGUI.BeginChangeCheck();
        int selectedIndex = EditorGUILayout.Popup(new GUIContent("Default Style"), currentIndex, labels.ToArray());
        if (EditorGUI.EndChangeCheck())
            defaultStyleIdProperty.stringValue = ids[selectedIndex];
    }

    private void DrawSelectedStylePreview()
    {
        GameObject prefab = FindSelectedPrefab();
        if (prefab == null)
            return;

        Texture2D preview = AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab);
        if (preview == null)
        {
            EditorGUILayout.HelpBox("Preview is not ready yet. Reopen the Inspector or wait for Unity to generate it.", MessageType.None);
            return;
        }

        EditorGUILayout.LabelField("Selected Style Preview", EditorStyles.boldLabel);
        Rect rect = GUILayoutUtility.GetRect(180f, 120f, GUILayout.ExpandWidth(true));
        GUI.Box(rect, GUIContent.none);

        Rect imageRect = FitRect(rect, preview.width, preview.height);
        GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit, true);
    }

    private GameObject FindSelectedPrefab()
    {
        string currentId = defaultStyleIdProperty.stringValue;
        for (int i = 0; i < stylesProperty.arraySize; i++)
        {
            SerializedProperty styleProperty = stylesProperty.GetArrayElementAtIndex(i);
            SerializedProperty idProperty = styleProperty.FindPropertyRelative("id");
            if (!string.Equals(idProperty.stringValue, currentId, StringComparison.OrdinalIgnoreCase))
                continue;

            return styleProperty.FindPropertyRelative("prefab").objectReferenceValue as GameObject;
        }

        return null;
    }

    private static Rect FitRect(Rect container, float contentWidth, float contentHeight)
    {
        if (contentWidth <= 0f || contentHeight <= 0f)
            return container;

        float scale = Mathf.Min(container.width / contentWidth, container.height / contentHeight);
        float width = contentWidth * scale;
        float height = contentHeight * scale;
        return new Rect(
            container.x + (container.width - width) * 0.5f,
            container.y + (container.height - height) * 0.5f,
            width,
            height);
    }
}
