using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapSceneView : UIModule
{
    private static readonly Vector2 ReferenceCanvasSize = new Vector2(960, 540);

    public ResourceManager RM => ResourceManager.instance;
    public Player player => Player.instance;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Image background, pathMask;

    private Map map;
    private Map lastMap => Player.instance.lastMap;
    private Dictionary<int, NpcController> npcDict = new Dictionary<int, NpcController>();
    private Dictionary<int, NpcController> farmDict = new Dictionary<int, NpcController>();
    private Dictionary<int, NpcController> animalDict = new Dictionary<int, NpcController>();
    private Dictionary<int, GameObject> teleportDict = new Dictionary<int, GameObject>();
    private GameObject foregroundMaskRoot;
    private MapClickFeedbackView clickFeedbackView;

    public Vector2 GetCanvasSize()
    {
        return canvasRect.rect.size;
    }

    public void SetMap(Map map)
    {
        this.map = map;
        this.map.geometry?.EnsureLists();
        bool refreshBGM = (SceneLoader.instance.GetLastScene() != SceneId.Map) ||
            (lastMap == null) || (!map.music.ValueEquals(lastMap?.music)) ||
            ((Player.instance.currentBattle != null) && (Player.instance.currentBattle.settings.mode == BattleMode.PVP));

        SetResources(map.resources, refreshBGM);
        SetEntites(map.entities);
        CheckBattleResult();

        CheckDailyLogin();
    }

    public void SetResources(MapResources resources, bool refreshBGM = true)
    {
        if (refreshBGM)
            SetBGM(resources);

        SetBackground(resources);
        SetPathMask(resources);
        SetForegroundMasks(resources);
    }

    #region resources

    public void SetBGM(MapResources resources)
    {
        AudioSystem.instance.PlayMusic(resources.bgm);
        AudioSystem.instance.PlayEffect(resources.fx);
    }

    public void SetBackground(MapResources resources)
    {
        var prefab = resources.anim;
        if (prefab == null)
        {
            background.sprite = resources.bg;
            background.color = map.dream ? Color.gray : map.backgroundColor;
            return;
        }

        var anim = Instantiate(prefab, Camera.main.transform);
        anim.transform.localScale = map.anim?.animScale ?? Vector2.one;
        anim.transform.localPosition = map.anim?.animPos ?? Vector2.zero;
        background.color = Color.clear;
    }

    public void SetPathMask(MapResources resources)
    {
        pathMask.sprite = resources.pathMaskSprite;
    }

    public void SetForegroundMasks(MapResources resources)
    {
        ClearForegroundMasks();
        if (resources.bg == null)
        {
            return;
        }

        if (map.geometry == null)
        {
            return;
        }

        var masks = map.geometry.ValidMasks.ToList();
        if (masks.Count == 0)
        {
            return;
        }

        foregroundMaskRoot = new GameObject("Foreground Masks", typeof(RectTransform), typeof(Canvas));
        var rootRect = foregroundMaskRoot.GetComponent<RectTransform>();
        rootRect.SetParent(canvasRect, false);
        StretchToFill(rootRect);
        rootRect.SetSiblingIndex(GetForegroundMaskSiblingIndex(rootRect));
        ConfigureForegroundMaskCanvas(foregroundMaskRoot.GetComponent<Canvas>());

        var maskObject = new GameObject("Foreground Mask", typeof(RectTransform), typeof(MapForegroundMaskGraphic));
        var maskRect = maskObject.GetComponent<RectTransform>();
        maskRect.SetParent(rootRect, false);
        StretchToFill(maskRect);

        var graphic = maskObject.GetComponent<MapForegroundMaskGraphic>();
        graphic.color = map.dream ? Color.gray : map.backgroundColor;
        graphic.SetPolygons(resources.bg, masks, ReferenceCanvasSize);
        if (!graphic.hasGeneratedMask)
        {
            ClearForegroundMasks();
            return;
        }
    }

    private void ConfigureForegroundMaskCanvas(Canvas foregroundCanvas)
    {
        Canvas parentCanvas = canvasRect.GetComponentInParent<Canvas>();
        foregroundCanvas.overrideSorting = false;
        foregroundCanvas.additionalShaderChannels = parentCanvas == null
            ? AdditionalCanvasShaderChannels.None
            : parentCanvas.additionalShaderChannels;
    }

    private int GetForegroundMaskSiblingIndex(RectTransform maskRoot)
    {
        int foregroundIndex = 0;
        for (int i = 0; i < canvasRect.childCount; i++)
        {
            Transform child = canvasRect.GetChild(i);
            if (child == maskRoot)
                continue;

            if (child.GetComponent<PlayerController>() == null &&
                child.GetComponentInChildren<PlayerView>(true) == null)
                continue;

            foregroundIndex = Mathf.Max(foregroundIndex, i + 1);
        }

        return Mathf.Clamp(foregroundIndex, 0, canvasRect.childCount - 1);
    }

    private void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private void ClearForegroundMasks()
    {
        if (foregroundMaskRoot == null)
            return;

        Destroy(foregroundMaskRoot);
        foregroundMaskRoot = null;
    }

    public void PlayClickFeedback(Vector2 canvasPos)
    {
        EnsureClickFeedbackView();
        clickFeedbackView?.Play(canvasPos);
    }

    private void EnsureClickFeedbackView()
    {
        if (clickFeedbackView != null)
            return;

        RectTransform feedbackParent = GetClickFeedbackParent();
        var feedbackObject = new GameObject("Map Click Feedback", typeof(RectTransform), typeof(MapClickFeedbackView));
        var feedbackRect = feedbackObject.GetComponent<RectTransform>();
        feedbackRect.SetParent(feedbackParent, false);
        feedbackRect.anchorMin = Vector2.zero;
        feedbackRect.anchorMax = Vector2.zero;
        feedbackRect.pivot = new Vector2(0.5f, 0.5f);
        feedbackRect.sizeDelta = new Vector2(80f, 80f);
        feedbackRect.localScale = Vector3.one;

        clickFeedbackView = feedbackObject.GetComponent<MapClickFeedbackView>();
        feedbackObject.transform.SetSiblingIndex(GetClickFeedbackSiblingIndex(feedbackParent));
        clickFeedbackView.gameObject.SetActive(false);
    }

    private RectTransform GetClickFeedbackParent()
    {
        RectTransform backgroundParent = background == null
            ? null
            : background.rectTransform.parent as RectTransform;
        return backgroundParent == null ? canvasRect : backgroundParent;
    }

    private int GetClickFeedbackSiblingIndex(RectTransform feedbackParent)
    {
        if (background != null && background.rectTransform.parent == feedbackParent)
            return Mathf.Clamp(background.rectTransform.GetSiblingIndex() + 1, 0, feedbackParent.childCount - 1);

        return Mathf.Clamp(GetForegroundMaskSiblingIndex(null), 0, feedbackParent.childCount - 1);
    }

    #endregion

    #region  entites

    public void SetEntites(MapEntities entities)
    {
        SetFarm(entities.farms);
        SetAnimal(entities.animals);
        SetNpc(entities.npcs);
        SetTeleport(entities.teleports);

        if (!ListHelper.IsNullOrEmpty(entities.farms))
            StartCoroutine(CheckPlantCoroutine());

        if (!ListHelper.IsNullOrEmpty(entities.animals))
            StartCoroutine(CheckAnimalCoroutine());
    }

    public void SetNpc(List<NpcInfo> infos)
    {
        foreach (var npcInfo in infos)
        {
            var prefab = RM.GetPrefab("Map/Npc");
            GameObject obj = Instantiate(prefab, transform);
            NpcController npc = obj.GetComponent<NpcController>();
            NpcHandler.CreateNpc(npc, npcInfo, npcDict, infoPrompt);
        }
        Player.SetSceneData("mapNpcList", npcDict);

        foreach (var npcInfo in infos)
        {
            var autoActionList = npcInfo.eventHandler.Where(x => x.type == ButtonEventType.Auto)
                .Select(x => NpcHandler.GetNpcEntity(npcDict.Get(npcInfo.id), x, npcDict)).ToList();
            autoActionList.ForEach(x => x?.Invoke());
        }
    }

    public void SetFarm(List<NpcInfo> infos)
    {
        if (ListHelper.IsNullOrEmpty(infos))
            return;

        foreach (var npcInfo in infos)
        {
            var prefab = RM.GetPrefab("Map/Npc");
            GameObject obj = Instantiate(prefab, transform);
            NpcController npc = obj.GetComponent<NpcController>();
            NpcHandler.CreateFarm(npc, npcInfo, npcDict, infoPrompt);
            farmDict.Add(npcInfo.id, npc);
        }
    }

    public void SetAnimal(List<NpcInfo> infos)
    {
        if (ListHelper.IsNullOrEmpty(infos))
            return;

        foreach (var npcInfo in infos)
        {
            var prefab = RM.GetPrefab("Map/Npc");
            GameObject obj = Instantiate(prefab, transform);
            NpcController npc = obj.GetComponent<NpcController>();
            NpcHandler.CreateAnimal(npc, npcInfo, npcDict, infoPrompt);
            animalDict.Add(npcInfo.id, npc);
        }
    }

    private IEnumerator CheckPlantCoroutine()
    {
        while (true)
        {
            foreach (var land in farmDict)
            {
                var id = land.Key;
                var npc = npcDict.Get(id + 100);
                var plant = Plant.LoadData(id);

                if (Plant.IsNullOrEmpty(plant))
                {
                    npc.SetColor(Color.clear);
                }
                else
                {
                    var icon = plant.GetIcon(out var size, out var posOffset);
                    npc.SetSprite(icon);
                    npc.SetColor(Color.white);
                    npc.SetRect(npc.GetInfo().pos + posOffset, size, Quaternion.identity);   
                }

                /*
                var plantExpr = activity.GetData("land[" + id + "].plant", "none");
                var isPlantGrowing = int.TryParse(plantExpr, out var plantId);
                
                var plant = Item.GetItemInfo(plantId);
                var date = isPlantGrowing ? DateTime.Parse(activity.GetData("land[" + id + "].date[ripe]", DateTime.MaxValue.ToString())) : DateTime.MaxValue;
                var ripe = DateTime.Now >= date;
                
                var seedId = int.Parse(plant?.options.Get("seed", "600000") ?? "600000");
                var seed = Item.GetItemInfo(seedId);

                var item = ripe ? plant : seed;
                var size = Vector2.one * 50;
                var sizeOffset = Vector2.zero;

                var icon = ripe ? plantId : seedId;
                var overrideIcon = seed.effects.Find(x => x.abilityOptionDict.ContainsKey("icon"));

                if (isPlantGrowing)
                {
                    if (overrideIcon != null)
                    {
                        var iconList = overrideIcon.abilityOptionDict.Get("icon").ToIntList('/');
                        var iconId = iconList.Last();
                        if (!ripe)
                        {
                            var growth = 1 - (date - DateTime.Now) / TimeSpan.Parse(seed.options["time"]);
                            int index = (int)(growth * (iconList.Count - 1));
                            if (index >= (iconList.Count - 1))
                                index = iconList.Count - 1;
                            
                            iconId = iconList[index];
                        }
                        var sprite = ResourceManager.instance.GetLocalAddressables<Sprite>($"Maps/plant/{iconId}", ItemInfo.IsMod(iconId));
                        size = sprite?.texture.GetTextureSize() ?? size;
                        sizeOffset = overrideIcon.abilityOptionDict.Get("offset")?.ToVector2(sizeOffset, '/') ?? sizeOffset;
                        npc.SetSprite(sprite);
                    }
                    else
                    {
                        npc.SetIcon((ItemInfo.IsMod(icon) ? "Mod/" : string.Empty) + "Items/" + icon);
                    }   
                }

                var pos = npc.GetInfo().pos + new Vector2((50 - size.x) / 2, 0) + sizeOffset;

                npc.SetColor(isPlantGrowing ? Color.white : Color.clear);
                npc.SetRect(pos, size, Quaternion.identity);
                */
            }
            yield return null;
        }
    }

    private IEnumerator CheckAnimalCoroutine()
    {
        while (true)
        {
            foreach (var land in animalDict)
            {
                var id = land.Key;
                var npc = land.Value;
                var animal = Animal.LoadData(id);

                if (Animal.IsNullOrEmpty(animal))
                {
                    npc.SetColor(Color.clear);
                }
                else
                {
                    var icon = animal.GetIcon();
                    npc.SetSprite(icon);
                    npc.SetColor(Color.white);
                    npc.SetSize(icon?.texture.GetTextureSize() ?? Vector2.zero);
                    /*
                    var gif = animal.GetGifUrl("front");
                    if (!string.IsNullOrEmpty(gif))
                    {
                        npc.SetGif(new AnimInfo(){ id = gif });
                    }
                    */
                }
            }
            yield return null;
        }   
    }


    public void SetTeleport(List<TeleportInfo> infos)
    {
        foreach (var teleport in infos)
        {
            var prefab = RM.GetPrefab("Map/Teleport");
            GameObject obj = Instantiate(prefab, transform);
            TeleportHandler.CreateTeleport(teleportDict, obj, teleport, infoPrompt);
        }
        Player.SetSceneData("mapTeleportList", teleportDict);
    }

    #endregion

    public void CheckBattleResult()
    {
        if (player?.currentBattle?.info == null)
        {
            // player.currentBattle = null;
            return;
        }

        if ((player.currentNpcId == 0) || (player.currentNpcId.IsInRange(50000, 50100) && (player.currentMapId != 500)))
        {
            // player.currentBattle = null;
            return;
        }

        NpcController npc = npcDict.Get(Player.instance.currentNpcId);
        BattleInfo battleInfo = player.currentBattle.info;
        BattleResult result = player.currentBattle.result;
        Pet[] petBag = player.petBag;
        VersionPetData petData = GameManager.versionData.petData;
        List<Action> actionList = new List<Action>();

        if (result.isMyWin)
        {
            actionList = battleInfo.winHandler?.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();

            // 1周年争霸赛特殊活動
            if ((map.id.IsInRange(82, 90)) && ((npc?.GetInfo()?.id ?? 0) >= 8200))
            {
                var item = new Item(7, Random.Range(1, 5));
                Item.Add(item);
                Item.OpenHintbox(item);
            }

        }
        else if (result.isOpWin)
        {
            actionList = battleInfo.loseHandler?.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();
        }

        foreach (var action in actionList ?? new List<Action>())
        {
            action?.Invoke();
        }

        Player.instance.currentNpcId = 0;
        Player.instance.currentBattle.info = null;
    }

    private void CheckDailyLogin()
    {
        var activity = Activity.Find("daily_login");
        if (bool.Parse(activity.GetData("news", "false")))
            return;

        Panel.OpenPanel<NewsPanel>();
        activity.SetData("news", "true");
        SaveSystem.SaveData();
    }
}
