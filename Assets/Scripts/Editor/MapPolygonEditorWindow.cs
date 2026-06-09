namespace U_Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEngine;

    public class MapPolygonEditorWindow : EditorWindow
    {
        private enum EditMode
        {
            Mask,
            Collision,
        }

        private class MapOption
        {
            public int id;
            public string name;
            public string path;
            public string label;
        }

        private const float CanvasWidth = 960f;
        private const float CanvasHeight = 540f;
        private const float SidebarWidth = 360f;
        private const float PointPickRadius = 9f;

        private int mapId = 70;
        private string mapPath = "Assets/Resources/Data/Maps/70.xml";
        private string mapFolderPath = "Assets/Resources/Data/Maps";
        private string cachedMapFolderPath;
        private List<MapOption> mapOptions = new List<MapOption>();
        private string[] mapOptionLabels = new string[0];
        private Map map;
        private MapGeometry geometry = new MapGeometry();
        private Texture2D backgroundTexture;
        private string backgroundTexturePath = string.Empty;
        private string resourceRootPath;
        private EditMode editMode = EditMode.Mask;
        private int selectedPolygonIndex = -1;
        private int draggingPointIndex = -1;
        private float zoom = 1f;
        private Vector2 canvasScrollPos;
        private Vector2 sidebarScrollPos;
        private Vector2 polygonListScrollPos;
        private Vector2 scrollPos;

        [MenuItem("Tools/Map Polygon Tool")]
        public static void ShowWindow()
        {
            GetWindow<MapPolygonEditorWindow>("Map Polygon Tool");
        }

        private void OnEnable()
        {
            resourceRootPath = GetDefaultResourceRootPath();
            mapFolderPath = NormalizePathSeparators(Path.GetDirectoryName(mapPath));
            RefreshMapOptions();
            LoadMap();
        }

        private void OnDisable()
        {
            ReleaseBackgroundTexture();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawPreviewPane();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true));
            sidebarScrollPos = EditorGUILayout.BeginScrollView(sidebarScrollPos);

            EditorGUILayout.Space(2);
            DrawMapPicker();
            EditorGUILayout.Space(8);
            DrawResourceControls();

            if (map == null)
            {
                EditorGUILayout.HelpBox("Load a map XML before editing.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.Space(8);
            DrawMapInfo();
            EditorGUILayout.Space(8);
            DrawPolygonList();
            EditorGUILayout.Space(8);
            DrawSelectedPolygonInfo();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewPane()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(map == null ? "Preview" : ShortenMiddle($"Preview  {map.id}  {map.name}", 40), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Mouse Wheel Zoom  {Mathf.RoundToInt(zoom * 100f)}%", EditorStyles.miniLabel);
            if (GUILayout.Button("1:1", EditorStyles.toolbarButton, GUILayout.Width(36)))
                zoom = 1f;
            EditorGUILayout.EndHorizontal();

            if (map == null)
                EditorGUILayout.HelpBox("Load a map XML to preview polygons.", MessageType.Info);
            else
                DrawCanvas();

            EditorGUILayout.EndVertical();
        }

        private void DrawMapPicker()
        {
            EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            mapFolderPath = DrawFullWidthTextField("XML Folder", mapFolderPath);
            if (EditorGUI.EndChangeCheck())
            {
                mapFolderPath = NormalizePathSeparators(mapFolderPath);
                RefreshMapOptions();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Map XML Folder", GetAbsolutePath(mapFolderPath), string.Empty);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    mapFolderPath = ToProjectRelativePath(selectedPath);
                    RefreshMapOptions();
                }
            }

            if (GUILayout.Button("Selection"))
                LoadSelection();

            if (GUILayout.Button("Refresh"))
                RefreshMapOptions();

            using (new EditorGUI.DisabledScope(map == null))
            {
                if (GUILayout.Button("Save"))
                    SaveMapGeometry();
            }
            EditorGUILayout.EndHorizontal();

            RefreshMapOptionsIfNeeded();
            if (mapOptionLabels.Length == 0)
            {
                EditorGUILayout.HelpBox("No map XML files found in this folder.", MessageType.Warning);
                return;
            }

            int currentIndex = FindMapOptionIndex(mapPath);
            int popupIndex = Mathf.Clamp(currentIndex, 0, mapOptionLabels.Length - 1);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Map", EditorStyles.miniBoldLabel);
            int selectedIndex = EditorGUILayout.Popup(popupIndex, mapOptionLabels);
            if (EditorGUI.EndChangeCheck() && selectedIndex >= 0 && selectedIndex < mapOptions.Count)
                SelectMapOption(selectedIndex);

            if (currentIndex < 0)
                EditorGUILayout.HelpBox("Current map is not in the selected folder.", MessageType.Info);
        }

        private void DrawMapInfo()
        {
            EditorGUILayout.LabelField($"Map: {map.id}  {map.name}", EditorStyles.boldLabel);
            DrawPathPreview(mapPath);
        }

        private string DrawFullWidthTextField(string label, string value)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            return EditorGUILayout.TextField(value);
        }

        private void DrawPathPreview(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string text = ShortenMiddle(path, 54);
            EditorGUILayout.LabelField(new GUIContent(text, path), EditorStyles.miniLabel);
        }

        private string ShortenMiddle(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            int leftLength = Mathf.Max(4, maxLength / 2 - 2);
            int rightLength = Mathf.Max(4, maxLength - leftLength - 3);
            return text.Substring(0, leftLength) + "..." + text.Substring(text.Length - rightLength);
        }

        private void DrawResourceControls()
        {
            EditorGUILayout.LabelField("External Resources", EditorStyles.boldLabel);

            resourceRootPath = DrawFullWidthTextField("Resource Root", resourceRootPath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Resource Root", resourceRootPath, string.Empty);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    resourceRootPath = selectedPath;
                    ReloadBackgroundTexture();
                }
            }

            if (GUILayout.Button("Default"))
            {
                resourceRootPath = GetDefaultResourceRootPath();
                ReloadBackgroundTexture();
            }

            if (GUILayout.Button("Reload"))
                ReloadBackgroundTexture();
            EditorGUILayout.EndHorizontal();

            if (backgroundTexture == null)
                EditorGUILayout.HelpBox("No preview image found. Check Resource Root.", MessageType.Warning);
            else
                DrawPathPreview(backgroundTexturePath);
        }

        private void RefreshMapOptionsIfNeeded()
        {
            string normalizedFolderPath = NormalizePathSeparators(mapFolderPath);
            if (string.Equals(cachedMapFolderPath, normalizedFolderPath, StringComparison.OrdinalIgnoreCase))
                return;

            RefreshMapOptions();
        }

        private void RefreshMapOptions()
        {
            mapFolderPath = NormalizePathSeparators(mapFolderPath);
            cachedMapFolderPath = mapFolderPath;
            mapOptions.Clear();

            string absoluteFolderPath = GetAbsolutePath(mapFolderPath);
            if (Directory.Exists(absoluteFolderPath))
            {
                mapOptions = Directory.GetFiles(absoluteFolderPath, "*.xml")
                    .Select(CreateMapOption)
                    .Where(option => option != null)
                    .OrderBy(option => option.id)
                    .ThenBy(option => option.name)
                    .ToList();
            }

            mapOptionLabels = mapOptions.Select(option => option.label).ToArray();
        }

        private MapOption CreateMapOption(string absolutePath)
        {
            string projectPath = ToProjectRelativePath(absolutePath);
            string fileName = Path.GetFileNameWithoutExtension(absolutePath);
            int id = int.TryParse(fileName, out var parsedId) ? parsedId : 0;
            string name = fileName;

            try
            {
                XElement root = XDocument.Load(absolutePath).Root;
                if (root != null)
                {
                    if (int.TryParse((string)root.Attribute("id"), out var xmlId))
                        id = xmlId;

                    string xmlName = (string)root.Attribute("name");
                    if (!string.IsNullOrEmpty(xmlName))
                        name = xmlName;
                }
            }
            catch (Exception)
            {
                // Keep malformed or partial XML files visible in the selector.
            }

            return new MapOption
            {
                id = id,
                name = name,
                path = projectPath,
                label = id + "  " + name
            };
        }

        private int FindMapOptionIndex(string path)
        {
            string normalizedPath = NormalizePathSeparators(ToProjectRelativePath(path));
            return mapOptions.FindIndex(option =>
                string.Equals(NormalizePathSeparators(option.path), normalizedPath, StringComparison.OrdinalIgnoreCase));
        }

        private void SelectMapOption(int index)
        {
            if (index < 0 || index >= mapOptions.Count)
                return;

            MapOption option = mapOptions[index];
            mapId = option.id;
            mapPath = option.path;
            LoadMap();
        }

        private void DrawPolygonList()
        {
            editMode = (EditMode)GUILayout.Toolbar((int)editMode, new[] { "Mask", "Collision" });
            selectedPolygonIndex = Mathf.Clamp(selectedPolygonIndex, -1, CurrentPolygons.Count - 1);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New"))
                AddPolygon();

            using (new EditorGUI.DisabledScope(SelectedPolygon == null))
            {
                if (GUILayout.Button("Delete"))
                    DeleteSelectedPolygon();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(SelectedPolygon == null))
            {
                if (GUILayout.Button("Undo Point"))
                    UndoPoint();

                if (GUILayout.Button("Clear Points"))
                    ClearPoints();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Polygons", EditorStyles.boldLabel);
            polygonListScrollPos = EditorGUILayout.BeginScrollView(polygonListScrollPos, GUILayout.MinHeight(110), GUILayout.MaxHeight(180));
            for (int i = 0; i < CurrentPolygons.Count; i++)
            {
                var polygon = CurrentPolygons[i];
                string title = $"{polygon.id}: {polygon.name} ({polygon.points.Count})";
                bool selected = GUILayout.Toggle(selectedPolygonIndex == i, title, "Button");
                if (selected)
                    selectedPolygonIndex = i;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCanvas()
        {
            Rect viewportRect = GUILayoutUtility.GetRect(
                1f, 1f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            HandleCanvasZoom(viewportRect);

            Rect contentRect = new Rect(0f, 0f, CanvasWidth * zoom, CanvasHeight * zoom);
            canvasScrollPos = GUI.BeginScrollView(viewportRect, canvasScrollPos, contentRect);
            Rect canvasRect = contentRect;

            EditorGUI.DrawRect(canvasRect, new Color(0.12f, 0.12f, 0.12f));
            DrawBackground(canvasRect);
            DrawGrid(canvasRect);
            DrawPolygons(canvasRect);
            HandleCanvasInput(canvasRect);
            EditorGUIUtility.AddCursorRect(canvasRect, MouseCursor.Arrow);
            GUI.EndScrollView();
        }

        private void DrawSelectedPolygonInfo()
        {
            var polygon = SelectedPolygon;
            if (polygon == null)
                return;

            EditorGUILayout.LabelField("Selected Polygon", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Id", GUILayout.Width(20));
            polygon.id = EditorGUILayout.IntField(polygon.id, GUILayout.Width(56));
            EditorGUILayout.LabelField("Name", GUILayout.Width(42));
            polygon.name = EditorGUILayout.TextField(polygon.name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);
            DrawPointHeader();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(220));
            var points = polygon.points;
            for (int i = 0; i < points.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector2 point = DrawPointField(i, points[i]);
                if (EditorGUI.EndChangeCheck())
                {
                    points[i] = ClampCanvasPoint(point);
                    polygon.points = points;
                    Repaint();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawPointHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("#", EditorStyles.miniBoldLabel, GUILayout.Width(24));
            GUILayout.Label("X", EditorStyles.miniBoldLabel, GUILayout.MinWidth(80));
            GUILayout.Label("Y", EditorStyles.miniBoldLabel, GUILayout.MinWidth(80));
            EditorGUILayout.EndHorizontal();
        }

        private Vector2 DrawPointField(int index, Vector2 point)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(index.ToString(), EditorStyles.miniLabel, GUILayout.Width(24));
            float x = EditorGUILayout.FloatField(point.x, GUILayout.MinWidth(80));
            float y = EditorGUILayout.FloatField(point.y, GUILayout.MinWidth(80));
            EditorGUILayout.EndHorizontal();
            return new Vector2(x, y);
        }

        private void LoadMap()
        {
            map = null;
            geometry = new MapGeometry();
            selectedPolygonIndex = -1;
            draggingPointIndex = -1;

            if (!File.Exists(mapPath))
            {
                backgroundTexture = null;
                backgroundTexturePath = string.Empty;
                return;
            }

            string text = File.ReadAllText(mapPath);
            map = ResourceManager.GetXML<Map>(text);
            map.geometry ??= new MapGeometry();
            map.geometry.EnsureLists();
            geometry = map.geometry;
            selectedPolygonIndex = CurrentPolygons.Count > 0 ? 0 : -1;
            ReloadBackgroundTexture();
        }

        private void LoadSelection()
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(selectedPath))
                return;

            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                mapFolderPath = selectedPath;
                RefreshMapOptions();
                return;
            }

            if (!selectedPath.EndsWith(".xml"))
                return;

            mapPath = selectedPath;
            mapFolderPath = NormalizePathSeparators(Path.GetDirectoryName(mapPath));
            if (int.TryParse(Path.GetFileNameWithoutExtension(selectedPath), out var selectedMapId))
                mapId = selectedMapId;

            RefreshMapOptions();
            LoadMap();
        }

        private void SaveMapGeometry()
        {
            if (map == null || !File.Exists(mapPath))
                return;

            geometry.EnsureLists();
            XDocument document = XDocument.Load(mapPath, LoadOptions.PreserveWhitespace);
            XElement root = document.Root;
            if (root == null)
                return;

            root.Elements("geometry").Remove();
            XElement geometryElement = CreateGeometryElement(geometry);
            if (geometryElement.HasElements)
            {
                XElement entitiesElement = root.Element("entities");
                if (entitiesElement != null)
                    entitiesElement.AddAfterSelf(geometryElement);
                else
                    root.Add(geometryElement);
            }

            File.WriteAllText(mapPath, document.ToString(SaveOptions.DisableFormatting));
            AssetDatabase.ImportAsset(mapPath);
            AssetDatabase.Refresh();
            RefreshMapOptions();
            EditorUtility.DisplayDialog("Map Polygon Tool", "Map geometry saved.", "OK");
        }

        private XElement CreateGeometryElement(MapGeometry mapGeometry)
        {
            var element = new XElement("geometry");
            AddPolygonElements(element, "mask", mapGeometry.masks);
            AddPolygonElements(element, "collision", mapGeometry.collisions);
            return element;
        }

        private void AddPolygonElements(XElement parent, string elementName, IEnumerable<MapPolygon> polygons)
        {
            foreach (var polygon in polygons ?? new List<MapPolygon>())
            {
                if (polygon.points.Count == 0)
                    continue;

                parent.Add(new XElement(elementName,
                    new XAttribute("id", polygon.id),
                    new XAttribute("name", polygon.name ?? string.Empty),
                    new XAttribute("points", MapGeometryUtility.FormatPoints(polygon.points))));
            }
        }

        private void AddPolygon()
        {
            var polygons = CurrentPolygons;
            int id = polygons.Count == 0 ? 1 : polygons.Max(x => x.id) + 1;
            var polygon = new MapPolygon
            {
                id = id,
                name = editMode == EditMode.Mask ? "Mask " + id : "Collision " + id,
                points = new List<Vector2>()
            };
            polygons.Add(polygon);
            selectedPolygonIndex = polygons.Count - 1;
        }

        private void DeleteSelectedPolygon()
        {
            if (SelectedPolygon == null)
                return;

            CurrentPolygons.RemoveAt(selectedPolygonIndex);
            selectedPolygonIndex = Mathf.Min(selectedPolygonIndex, CurrentPolygons.Count - 1);
        }

        private void UndoPoint()
        {
            var polygon = SelectedPolygon;
            if (polygon == null)
                return;

            var points = polygon.points;
            if (points.Count == 0)
                return;

            points.RemoveAt(points.Count - 1);
            polygon.points = points;
        }

        private void ClearPoints()
        {
            var polygon = SelectedPolygon;
            if (polygon == null)
                return;

            polygon.points = new List<Vector2>();
        }

        private void DrawBackground(Rect canvasRect)
        {
            if (backgroundTexture == null)
                return;

            GUI.DrawTexture(canvasRect, backgroundTexture, ScaleMode.StretchToFill, true);
        }

        private void HandleCanvasZoom(Rect viewportRect)
        {
            Event evt = Event.current;
            if (evt.type != EventType.ScrollWheel || !viewportRect.Contains(evt.mousePosition))
                return;

            float oldZoom = zoom;
            zoom = Mathf.Clamp(zoom - evt.delta.y * 0.05f, 0.25f, 4f);
            if (Mathf.Approximately(oldZoom, zoom))
                return;

            Vector2 mouseInViewport = evt.mousePosition - viewportRect.position;
            Vector2 contentMousePosition = canvasScrollPos + mouseInViewport;
            float zoomRatio = zoom / oldZoom;
            canvasScrollPos = contentMousePosition * zoomRatio - mouseInViewport;
            canvasScrollPos.x = Mathf.Clamp(canvasScrollPos.x, 0f, Mathf.Max(0f, CanvasWidth * zoom - viewportRect.width));
            canvasScrollPos.y = Mathf.Clamp(canvasScrollPos.y, 0f, Mathf.Max(0f, CanvasHeight * zoom - viewportRect.height));

            evt.Use();
            Repaint();
        }

        private void DrawGrid(Rect canvasRect)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.12f);
            for (int x = 0; x <= CanvasWidth; x += 120)
            {
                Vector2 start = CanvasToGui(new Vector2(x, 0), canvasRect);
                Vector2 end = CanvasToGui(new Vector2(x, CanvasHeight), canvasRect);
                Handles.DrawLine(start, end);
            }

            for (int y = 0; y <= CanvasHeight; y += 90)
            {
                Vector2 start = CanvasToGui(new Vector2(0, y), canvasRect);
                Vector2 end = CanvasToGui(new Vector2(CanvasWidth, y), canvasRect);
                Handles.DrawLine(start, end);
            }
            Handles.EndGUI();
        }

        private void DrawPolygons(Rect canvasRect)
        {
            Handles.BeginGUI();
            DrawPolygonGroup(geometry.masks, canvasRect, new Color(0.2f, 0.65f, 1f, 0.22f), new Color(0.2f, 0.7f, 1f, 0.95f), editMode == EditMode.Mask);
            DrawPolygonGroup(geometry.collisions, canvasRect, new Color(1f, 0.35f, 0.25f, 0.22f), new Color(1f, 0.28f, 0.18f, 0.95f), editMode == EditMode.Collision);
            Handles.EndGUI();
        }

        private void DrawPolygonGroup(List<MapPolygon> polygons, Rect canvasRect, Color fillColor, Color lineColor, bool isActiveMode)
        {
            for (int i = 0; i < polygons.Count; i++)
            {
                var polygon = polygons[i];
                var points = polygon.points;
                if (points.Count == 0)
                    continue;

                bool selected = isActiveMode && selectedPolygonIndex == i;
                DrawPolygonFill(points, canvasRect, selected ? fillColor * 1.4f : fillColor);

                Handles.color = selected ? Color.yellow : lineColor;
                var guiPoints = points.Select(point => (Vector3)CanvasToGui(point, canvasRect)).ToList();
                if (guiPoints.Count >= 2)
                {
                    var linePoints = guiPoints.ToList();
                    if (guiPoints.Count >= 3)
                        linePoints.Add(guiPoints[0]);
                    Handles.DrawAAPolyLine(selected ? 4f : 2f, linePoints.ToArray());
                }

                for (int j = 0; j < guiPoints.Count; j++)
                {
                    Handles.color = selected ? Color.yellow : lineColor;
                    Handles.DrawSolidDisc(guiPoints[j], Vector3.forward, selected ? 4.5f : 3.5f);
                }
            }
        }

        private void DrawPolygonFill(List<Vector2> points, Rect canvasRect, Color color)
        {
            if (points.Count < 3)
                return;

            var triangles = MapGeometryUtility.Triangulate(points);
            Handles.color = color;
            for (int i = 0; i < triangles.Count; i += 3)
            {
                Handles.DrawAAConvexPolygon(
                    (Vector3)CanvasToGui(points[triangles[i]], canvasRect),
                    (Vector3)CanvasToGui(points[triangles[i + 1]], canvasRect),
                    (Vector3)CanvasToGui(points[triangles[i + 2]], canvasRect));
            }
        }

        private void HandleCanvasInput(Rect canvasRect)
        {
            var polygon = SelectedPolygon;
            if (polygon == null)
                return;

            Event evt = Event.current;
            if (!canvasRect.Contains(evt.mousePosition) && draggingPointIndex < 0)
                return;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0)
                    {
                        var points = polygon.points;
                        draggingPointIndex = FindNearestPoint(points, evt.mousePosition, canvasRect);
                        if (draggingPointIndex < 0)
                        {
                            points.Add(GuiToCanvas(evt.mousePosition, canvasRect));
                            polygon.points = points;
                            draggingPointIndex = points.Count - 1;
                        }
                        evt.Use();
                    }
                    else if (evt.button == 1)
                    {
                        DeleteNearestPoint(polygon, evt.mousePosition, canvasRect);
                        evt.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (draggingPointIndex >= 0)
                    {
                        var points = polygon.points;
                        if (draggingPointIndex < points.Count)
                        {
                            points[draggingPointIndex] = GuiToCanvas(evt.mousePosition, canvasRect);
                            polygon.points = points;
                            Repaint();
                        }
                        evt.Use();
                    }
                    break;

                case EventType.MouseUp:
                    draggingPointIndex = -1;
                    break;
            }
        }

        private void DeleteNearestPoint(MapPolygon polygon, Vector2 guiPos, Rect canvasRect)
        {
            var points = polygon.points;
            int nearest = FindNearestPoint(points, guiPos, canvasRect);
            if (nearest < 0)
                return;

            points.RemoveAt(nearest);
            polygon.points = points;
            Repaint();
        }

        private int FindNearestPoint(List<Vector2> points, Vector2 guiPos, Rect canvasRect)
        {
            int nearest = -1;
            float nearestDistance = PointPickRadius * PointPickRadius;
            for (int i = 0; i < points.Count; i++)
            {
                float distance = (CanvasToGui(points[i], canvasRect) - guiPos).sqrMagnitude;
                if (distance > nearestDistance)
                    continue;

                nearestDistance = distance;
                nearest = i;
            }

            return nearest;
        }

        private Vector2 GuiToCanvas(Vector2 guiPoint, Rect canvasRect)
        {
            float x = (guiPoint.x - canvasRect.x) / canvasRect.width * CanvasWidth;
            float y = (canvasRect.yMax - guiPoint.y) / canvasRect.height * CanvasHeight;
            return ClampCanvasPoint(new Vector2(x, y));
        }

        private Vector2 CanvasToGui(Vector2 canvasPoint, Rect canvasRect)
        {
            float x = canvasRect.x + canvasPoint.x / CanvasWidth * canvasRect.width;
            float y = canvasRect.yMax - canvasPoint.y / CanvasHeight * canvasRect.height;
            return new Vector2(x, y);
        }

        private Vector2 ClampCanvasPoint(Vector2 point)
        {
            return new Vector2(Mathf.Clamp(point.x, 0, CanvasWidth), Mathf.Clamp(point.y, 0, CanvasHeight));
        }

        private void ReloadBackgroundTexture()
        {
            ReleaseBackgroundTexture();
            backgroundTexture = LoadBackgroundTexture(map);
            Repaint();
        }

        private Texture2D LoadBackgroundTexture(Map loadedMap)
        {
            backgroundTexturePath = string.Empty;
            if (loadedMap == null)
                return null;

            int resId = loadedMap.resId == 0 ? loadedMap.id : loadedMap.resId;
            foreach (string candidatePath in GetBackgroundCandidatePaths(resId))
            {
                if (!File.Exists(candidatePath))
                    continue;

                try
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                    {
                        name = Path.GetFileNameWithoutExtension(candidatePath),
                        hideFlags = HideFlags.HideAndDontSave
                    };

                    if (!texture.LoadImage(File.ReadAllBytes(candidatePath)))
                    {
                        DestroyImmediate(texture);
                        continue;
                    }

                    backgroundTexturePath = candidatePath;
                    return texture;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Map Polygon Tool failed to load preview image: {candidatePath}\n{exception.Message}");
                }
            }

            return null;
        }

        private void ReleaseBackgroundTexture()
        {
            if (backgroundTexture != null &&
                !EditorUtility.IsPersistent(backgroundTexture))
            {
                DestroyImmediate(backgroundTexture);
            }

            backgroundTexture = null;
            backgroundTexturePath = string.Empty;
        }

        private IEnumerable<string> GetBackgroundCandidatePaths(int resId)
        {
            string normalizedRoot = NormalizePath(resourceRootPath);
            bool isMod = Map.IsMod(resId);
            string activeRoot = isMod ? GetSiblingRootPath(normalizedRoot, "Mod") : normalizedRoot;
            string[] extensions = { ".png", ".jpg", ".jpeg" };

            foreach (string extension in extensions)
                yield return Path.Combine(activeRoot, "Maps", "bg", resId + extension);

            if (resId == 0)
            {
                foreach (string extension in extensions)
                    yield return Path.Combine(activeRoot, "Activities", "FirstPage" + extension);
            }
        }

        private string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Path.GetFullPath(".");

            return NormalizePathSeparators(Path.IsPathRooted(path) ? path : Path.GetFullPath(path));
        }

        private string ToProjectRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            string fullPath = GetAbsolutePath(path);
            string projectRoot = NormalizePathSeparators(Path.GetFullPath("."));
            string projectPrefix = projectRoot.TrimEnd('/') + "/";
            if (fullPath.StartsWith(projectPrefix, StringComparison.OrdinalIgnoreCase))
                return fullPath.Substring(projectPrefix.Length);

            return fullPath;
        }

        private string NormalizePathSeparators(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace("\\", "/");
        }

        private string GetSiblingRootPath(string rootPath, string siblingFolderName)
        {
            if (string.IsNullOrEmpty(rootPath))
                return rootPath;

            if (Path.GetFileName(rootPath).Equals(siblingFolderName, StringComparison.OrdinalIgnoreCase))
                return rootPath;

            string parentPath = Directory.GetParent(rootPath)?.FullName;
            return string.IsNullOrEmpty(parentPath) ? rootPath : Path.Combine(parentPath, siblingFolderName);
        }

        private string NormalizePath(string path)
        {
            return string.IsNullOrEmpty(path) ? GetDefaultResourceRootPath() : path;
        }

        private string GetDefaultResourceRootPath()
        {
            return Path.Combine(Application.persistentDataPath, "Resources");
        }

        private List<MapPolygon> CurrentPolygons
        {
            get
            {
                geometry.EnsureLists();
                return editMode == EditMode.Mask ? geometry.masks : geometry.collisions;
            }
        }

        private MapPolygon SelectedPolygon
        {
            get
            {
                if (selectedPolygonIndex < 0 || selectedPolygonIndex >= CurrentPolygons.Count)
                    return null;

                return CurrentPolygons[selectedPolygonIndex];
            }
        }
    }
}
