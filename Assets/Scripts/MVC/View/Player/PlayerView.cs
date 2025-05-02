using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : Module
{
    [SerializeField] protected RectTransform playerRect;
    [SerializeField] protected IButton playerButton;
    [SerializeField] protected Sprite[] playerDirectionSprite = new Sprite[4];
    [SerializeField] protected Text playerNameText, achievementText;

    protected Vector2 lastDirection = Vector2.zero;

    // 0: Front Left,   1: Back Left
    // 2: Front Right,  3: Back Right
    public Sprite GetPlayerDirectionSprite(Vector2 direction) {
        if (direction == Vector2.zero) {
            return null;
        }

        int idx = -1;
        if (direction.x <= 0) {
            idx = (direction.y <= 0) ? 0 : 1;
        } else {
            idx = (direction.y <= 0) ? 2 : 3;
        }
        return playerDirectionSprite[idx];
    }

    public virtual void SetDirection(Vector2 direction) {
        Sprite directionSprite = GetPlayerDirectionSprite(direction);
        if (directionSprite != null) {
            playerButton.SetSprite(directionSprite); 
        }
        lastDirection = direction;
    }

    public void SetPlayerPosition(Vector2 currentPos) {
        playerRect.anchoredPosition = currentPos;
    }

    public void SetPlayerName(string value) {
        playerNameText.text = value;
    }

    public void SetPlayerAchievement(string value) {
        achievementText.text = value;
    }

    public PlayerInfoPanel OpenPlayerInfoPanel() {
        return Panel.OpenPanel<PlayerInfoPanel>();
    }
    
    public void SetPlayerSprite(Sprite sprite) {
        if (sprite != null) {
            playerButton.SetSprite(sprite); 
            // 使用 LINQ 优化赋值操作，将相同的 Sprite 赋值给数组中的每个元素
            playerDirectionSprite = Enumerable.Repeat(sprite, 4).ToArray();
        }
    }

    public void Shoot(int shootId, Vector2 canvasPos, List<Action> callbacks = null) {
        if (Item.Find(shootId) == null) {
            Hintbox.OpenHintboxWithContent("射击道具【" + Item.GetItemInfo(shootId).name + "】耗尽了哦", 16);
            return;
        }

        var prefab = ResourceManager.instance.GetPrefab("Map/Npc");
        GameObject obj = Instantiate(prefab, playerRect.parent);
        NpcController npc = obj.GetComponent<NpcController>();
        var item = Item.GetItemInfo(shootId);
        var size = item.effects.FirstOrDefault().abilityOptionDict.Get("shootSize")?.ToVector2(delimeter: '/') ?? item.icon.GetResizedSize(Vector2.one * 50);
        var direction = canvasPos - playerRect.anchoredPosition;
        var rotation = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        npc.SetNpcInfo(new NpcInfo(){
            id = shootId,
            name = string.Empty,
            npcSize = size.x + "," + size.y,
            npcPos = playerRect.anchoredPosition.x + "," + playerRect.anchoredPosition.y,
            npcRotation = rotation.x + "," + rotation.y + "," + rotation.z,
            raycastTarget = false,
        });
        npc.SetSprite(item.icon);
        StartCoroutine(ShootCoroutine(npc, canvasPos, callbacks));
    }

    private IEnumerator ShootCoroutine(NpcController npc, Vector2 targetPos, List<Action> callbacks) {
        var id = npc.GetInfo().id;
        var item = Item.GetItemInfo(id);
        var effect = item?.effects.FirstOrDefault();
        var initPos = playerRect.anchoredPosition;
        var distance = targetPos - npc.GetInfo().size / 2 - initPos;
        var speed = float.Parse(effect.abilityOptionDict.Get("speed", "600"));
        var rotation = (effect == null) ? 0 : int.Parse(effect.abilityOptionDict.Get("rotation", (item.type == ItemType.Shoot) ? "0" : "720"));
        float time = 0f, maxTime = distance.magnitude / speed;
        while (time < maxTime) {
            npc.SetPosition(initPos + distance.normalized * time * speed);
            if (rotation != 0)
                npc.SetRotation(new Vector3(0, 0, time * rotation));
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        var newIconPath = effect.abilityOptionDict.Get("resId");
        var icon = string.IsNullOrEmpty(newIconPath) ? item.icon : NpcInfo.GetIcon(newIconPath);

        if (icon != null) {
            var size = effect.abilityOptionDict.Get("size")?.ToVector2(delimeter: '/') ?? icon.GetResizedSize(Vector2.one * 50);
            var offset = effect.abilityOptionDict.Get("offset", (-size.x/2) + "/" + (-size.y/2)).ToVector2(delimeter: '/');
            npc.SetSprite(icon);
            npc.SetRect(targetPos + offset, size, Quaternion.identity);
        } else
            Destroy(npc.gameObject);

        if ((item.type == ItemType.Shoot) && item.removable)
            Item.Remove(item.id, 1);

        callbacks?.ForEach(x => x?.Invoke());
    }
}
