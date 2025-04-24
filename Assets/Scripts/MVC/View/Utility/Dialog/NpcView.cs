using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

public class NpcView : Module
{
    private ResourceManager RM => ResourceManager.instance;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image image;
    [SerializeField] private IButton button;
    [SerializeField] private Text nameText;
    
    public void SetNpcInfo(NpcInfo info) {
        SetRaycastTarget(info.raycastTarget);
        SetRect(info.pos, info.size, info.rotation);
        SetName(info.name);
        SetNamePos(info.namePos);
        SetIcon(info.resId);
        SetColor(info.color);
    }

    public void SetRaycastTarget(bool isRaycastTarget) {
        if (button?.image == null)
            return;

        button.image.raycastTarget = isRaycastTarget;
    }

    public void SetPosition(Vector2 pos) => rect.anchoredPosition = pos;
    public void SetRotation(Vector3 rotation) => button.rect.rotation = Quaternion.Euler(rotation);

    public void SetRect(Vector2 pos, Vector2 size, Quaternion rotation) {
        rect.SetAsLastSibling();
        rect.anchoredPosition = pos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        button.rect.rotation = rotation;
    }

    public void SetColor(Color color) {
        if (button?.image == null)
            return;

        button.image.color = color;
    }

    public void SetName(string name) {
        nameText.text = name;
    }

    public void SetNamePos(Vector2 namePos) {
        nameText.rectTransform.anchoredPosition = namePos;
    }

    public void SetIcon(string resId) {
        button.SetSprite(  NpcInfo.GetIcon(resId));
    }

    public void SetSprite(Sprite sprite) {
        button.SetSprite(sprite);
    }

    public void SetBGM(AudioClip bgm) {
        button.SetBGM(bgm);
    }

    public void SetAction(NpcController npc, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        NpcInfo info = npc.GetInfo();
        if (info.description != null) {
            button.onPointerEnterEvent.AddListener(() => infoPrompt.SetActive(true));
            button.onPointerExitEvent.AddListener(() => infoPrompt.SetActive(false));
            button.onPointerOverEvent.AddListener(() => infoPrompt.SetInfoPromptWithAutoSize(info.description, TextAnchor.MiddleLeft));
        }

        Action onArrive = (() => {});
        foreach (var handler in info.eventHandler) {
            UnityEvent pointerEvent = NpcHandler.GetButtonEvent(button, handler);
            Action handlerAction = NpcHandler.GetNpcEntity(npc, handler, npcList);
            if (handler.type != ButtonEventType.OnPointerClick) {
                pointerEvent?.AddListener(handlerAction.Invoke);
                continue;
            } 
            Action onOldArrive = onArrive;
            onArrive = () => { onOldArrive?.Invoke(); handlerAction?.Invoke(); };
        }
        Action onClick = (info.transport == null) ? onArrive : (() => TeleportHandler.Transport(info.transportPos, onArrive));
        button.onPointerClickEvent.AddListener(onClick.Invoke);
    }

    public void SetFarmAction(NpcController npc, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        var info = npc.GetInfo();
        var activity = Activity.Find("farm");

        void ShowPlantInfo() {
            var plant = activity.GetData("land[" + info.id + "].plant", "none");
            if (int.TryParse(plant, out var plantId)) {
                var item = Item.GetItemInfo(plantId);
                var ripeDate = DateTime.Parse(activity.GetData("land[" + info.id + "].date[ripe]", DateTime.MaxValue.ToString()));
                var totalTime = TimeSpan.Parse(item.options["time"]);
                var now = DateTime.Now;
                var ripeNeedTime = (now >= ripeDate) ? TimeSpan.Zero : (ripeDate - now);
                infoPrompt.SetPlant(item, ripeNeedTime, totalTime);
            } else
                infoPrompt.SetPlant(null, TimeSpan.Zero, TimeSpan.MaxValue);
        }

        void PlantAction() {
            var now = DateTime.Now;
            var plant = activity.GetData("land[" + info.id + "].plant", "none");

            // No plant currently.
            if (!int.TryParse(plant, out var plantId)) {
                var seed = (int)Player.GetSceneData("seed", 0);
                // No seed specified. Choose seed.
                if (seed == 0) {
                    MapManager.instance.SetPlantPanelActive(true);
                    return;
                }
                // Plant the seed.
                var item = Item.GetItemInfo(seed);
                var totalTime = TimeSpan.Parse(item.options["time"]);
                activity["land[" + info.id + "].plant"] = seed.ToString();
                activity["land[" + info.id + "].date[ripe]"] = (now + totalTime).ToString();
                SaveSystem.SaveData();
                return;
            }

            // Not ripe yet.
            var date = DateTime.Parse(activity["land[" + info.id + "].date[ripe]"]);
            if (now < date)
                return;
            
            // Ripe.
            var harvestInfo = Item.GetItemInfo(plantId);
            var specialEffect = harvestInfo.effects.Find(x => (x.ability == EffectAbility.None) && x.abilityOptionDict.ContainsKey("plant"));
            var isSpecialSuccess = (specialEffect != null) && specialEffect.Condition(Player.instance.pet, null);
            if (isSpecialSuccess) {
                plant = harvestInfo.name;
                plantId = int.Parse(specialEffect.abilityOptionDict.Get("plant", harvestInfo.id.ToString()));
                harvestInfo = Item.GetItemInfo(plantId);
            }
            var harvestNum = (int)Identifier.GetNumIdentifier(harvestInfo.options["num"]);
            var harvest = new Item(plantId, harvestNum);

            activity["land[" + info.id + "].plant"] = "none";
            activity["land[" + info.id + "].date[ripe]"] = "none";
            Item.Add(harvest);
            Item.OpenHintbox(harvest);

            if (isSpecialSuccess)
                Hintbox.OpenHintboxWithContent("<color=#ffbb33>你的" + plant + "变异了！</color>", 20);

            foreach (var e in harvestInfo.effects.Where(x => x.ability == EffectAbility.SetPlayer)) {
                if (e.Condition(Player.instance.pet, null))
                    e.SetPlayer(null, npc, null, npcList);
            }
        }

        button.onPointerEnterEvent.AddListener(() => infoPrompt.SetActive(true));
        button.onPointerExitEvent.AddListener(() => infoPrompt.SetActive(false));
        button.onPointerOverEvent.AddListener(ShowPlantInfo);
        button.onPointerClickEvent.AddListener(PlantAction);
    }
}
