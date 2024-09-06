namespace U_Editor
{ 
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    public class ApplyTransformToPrefabs : EditorWindow
    {
        private GameObject selectedPrefab;

        [MenuItem("Tools/Apply Transform to Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<ApplyTransformToPrefabs>("Apply Transform to Prefabs");
        }

        private void OnGUI()
        {
            GUILayout.Label("Select a Prefab", EditorStyles.boldLabel);
            selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", selectedPrefab, typeof(GameObject), false);

            if (selectedPrefab == null)
            {
                EditorGUILayout.HelpBox("Please select a prefab.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Apply Transform to All Prefabs in Folder"))
            {
                ApplyTransform();
            }
        }

        private void ApplyTransform()
        {
            if (selectedPrefab == null)
            {
                Debug.LogWarning("No prefab selected.");
                return;
            }

            string folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedPrefab));
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab != selectedPrefab)
                {
                    ApplyTransformToPrefab(prefab);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Transform applied to all prefabs in folder.");
        }

        private void ApplyTransformToPrefab(GameObject prefab)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if(PrefabUtility.IsAnyPrefabInstanceRoot(instance))
                PrefabUtility.UnpackPrefabInstance(instance,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
            
            Transform selectedTransform = selectedPrefab.transform;
            instance.transform.rotation = selectedTransform.rotation;
            instance.transform.localScale = selectedTransform.localScale;

            GameObject p = PrefabUtility.SaveAsPrefabAsset(instance, AssetDatabase.GetAssetPath(prefab));
            p.transform.position = selectedTransform.position;;
            DestroyImmediate(instance);

        }
    }
}