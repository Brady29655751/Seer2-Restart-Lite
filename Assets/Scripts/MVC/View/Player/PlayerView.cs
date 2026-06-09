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
    [SerializeField] protected Vector2 visualCenterOffset = new Vector2(0, 0);
    [SerializeField] protected UniGifImage followerGif;

    protected Vector2 lastDirection = Vector2.zero;

    // 0: Front Left,   1: Back Left
    // 2: Front Right,  3: Back Right
    public Sprite GetPlayerDirectionSprite(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return null;
        }

        int idx = -1;
        if (direction.x <= 0)
        {
            idx = (direction.y <= 0) ? 0 : 1;
        }
        else
        {
            idx = (direction.y <= 0) ? 2 : 3;
        }
        return playerDirectionSprite[idx];
    }

    public virtual void SetDirection(Vector2 direction)
    {
        Sprite directionSprite = GetPlayerDirectionSprite(direction);
        if (directionSprite != null)
        {
            playerButton.SetSprite(directionSprite);
        }

        SetFollowerDirection(direction);

        lastDirection = direction;
    }

    public void SetPlayerPosition(Vector2 currentPos) {
        Vector2 actualOffset = GetFlippedOffset();
        playerRect.anchoredPosition = currentPos - actualOffset;
    }
    
    /// <summary>
    /// 获取角色视觉中心点的实际Canvas位置
    /// </summary>
    public Vector2 GetVisualCenterPosition() {
        Vector2 actualOffset = GetFlippedOffset();
        return playerRect.anchoredPosition + actualOffset;
    }
    
    /// <summary>
    /// 根据角色朝向返回正确的偏移量
    /// </summary>
    protected Vector2 GetFlippedOffset() {
        if (playerRect == null)
            return visualCenterOffset;
        float xSign = (playerRect.localScale.x < 0) ? -1f : 1f;
        return new Vector2(visualCenterOffset.x * xSign, visualCenterOffset.y);
    }
    
    /// <summary>
    /// 翻转角色朝向，同时补偿位置确保视觉中心点保持不变
    /// </summary>
    protected void FlipHorizontal(bool faceLeft) {
        if (playerRect == null)
            return;
        
        float targetScaleX = faceLeft ? 1f : -1f;
        if (Mathf.Approximately(playerRect.localScale.x, targetScaleX))
            return;
        
        Vector2 oldOffset = GetFlippedOffset();
        playerRect.localScale = new Vector3(targetScaleX, 1f, 1f);
        Vector2 newOffset = GetFlippedOffset();
        
        Vector2 delta = oldOffset - newOffset;
        playerRect.anchoredPosition += delta;
        
        if (playerNameText != null)
            playerNameText.rectTransform.localScale = playerRect.localScale;
        if (achievementText != null)
            achievementText.rectTransform.localScale = playerRect.localScale;
    }
    public virtual void SetFollowerDirection(Vector2 direction)
    {
        var follower = Player.instance.follower;
        if (Animal.IsNullOrEmpty(follower))
        {
            followerGif.image.SetSprite(SpriteSet.Empty);
            return;
        }

        var dirName = direction.GetDirectionName();
        if (string.IsNullOrEmpty(dirName))
        {
            followerGif.Reset();
        }
        else
        {
            var gifInfo = follower.GetGifInfo(dirName);
            if (gifInfo != null)
            {
                var anchorX = (gifInfo.AnimScale.x + 1) / 2;
                followerGif.image.rectTransform.anchorMin = followerGif.image.rectTransform.anchorMax = new Vector2(anchorX, 0);
                followerGif.image.rectTransform.localScale = gifInfo.AnimScale;
                followerGif.SetGifFromUrl(gifInfo.GifPath, speed: gifInfo.AnimSpeed * 2.5f, useGifSize: gifInfo.UseAnimSize);
            }      
            else
            {
                followerGif.image.rectTransform.anchorMin = followerGif.image.rectTransform.anchorMax = Vector2.right;
                followerGif.image.rectTransform.localScale = Vector3.one;
                followerGif.image.SetSprite(follower.Icon);
            }

            followerGif.image.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void SetPlayerSize(Vector2 size)
    {
        playerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        playerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public void SetPlayerName(string value)
    {
        playerNameText.text = value;
    }

    public void SetPlayerAchievement(string value)
    {
        achievementText.text = value;
    }

    public PlayerInfoPanel OpenPlayerInfoPanel()
    {
        return Panel.OpenPanel<PlayerInfoPanel>();
    }

    public void SetPlayerSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            playerButton.SetSprite(sprite);
            // 使用 LINQ 优化赋值操作，将相同的 Sprite 赋值给数组中的每个元素
            playerDirectionSprite = Enumerable.Repeat(sprite, 4).ToArray();
        }
    }

    public void SetFollowerSprite(Sprite sprite)
    {
        sprite ??= SpriteSet.Empty;
        followerGif.image.SetSprite(sprite);   
        followerGif.image.SetSize(sprite.texture.GetTextureSize());
    }

    public void Shoot(int shootId, Vector2 canvasPos, List<Action> callbacks = null)
    {
        if (Item.Find(shootId) == null)
        {
            Hintbox.OpenHintboxWithContent("射击道具【" + Item.GetItemInfo(shootId).name + "】耗尽了哦", 16);
            return;
        }

        shootId = Item.GetShootId(shootId);

        var prefab = ResourceManager.instance.GetPrefab("Map/Npc");
        GameObject obj = Instantiate(prefab, playerRect.parent);
        NpcController npc = obj.GetComponent<NpcController>();
        var item = Item.GetItemInfo(shootId);
        var size = item.effects.FirstOrDefault()?.abilityOptionDict.Get("shootSize")?.ToVector2(delimeter: '/') ?? item.icon.GetResizedSize(Vector2.one * 50);
        var visualCenter = GetVisualCenterPosition();
        var direction = canvasPos - visualCenter;
        var rotation = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        npc.SetNpcInfo(new NpcInfo()
        {
            id = shootId,
            name = string.Empty,
            npcSize = size.x + "," + size.y,
            npcPos = visualCenter.x + "," + visualCenter.y,
            npcRotation = rotation.x + "," + rotation.y + "," + rotation.z,
            raycastTarget = false,
        });
        npc.SetSprite(item.icon);
        StartCoroutine(ShootCoroutine(npc, canvasPos, callbacks));
    }

    private IEnumerator ShootCoroutine(NpcController npc, Vector2 targetPos, List<Action> callbacks)
    {
        var id = npc.GetInfo().id;
        var item = Item.GetItemInfo(id);
        var effect = item?.effects.FirstOrDefault();
        var initPos = GetVisualCenterPosition();
        var distance = targetPos - npc.GetInfo().size / 2 - initPos;
        var speed = float.Parse(effect.abilityOptionDict.Get("speed", "600"));
        var rotation = (effect == null) ? 0 : int.Parse(effect.abilityOptionDict.Get("rotation", (item.type == ItemType.Shoot) ? "0" : "720"));
        float time = 0f, maxTime = distance.magnitude / speed;
        while (time < maxTime)
        {
            npc.SetPosition(initPos + distance.normalized * time * speed);
            if (rotation != 0)
                npc.SetRotation(new Vector3(0, 0, time * rotation));
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        var newIconPath = effect.abilityOptionDict.Get("resId");
        var icon = string.IsNullOrEmpty(newIconPath) ? item.icon : NpcInfo.GetIcon(newIconPath);

        if (icon != null)
        {
            var size = effect.abilityOptionDict.Get("size")?.ToVector2(delimeter: '/') ?? icon.GetResizedSize(Vector2.one * 50);
            var offset = effect.abilityOptionDict.Get("offset", (-size.x / 2) + "/" + (-size.y / 2)).ToVector2(delimeter: '/');
            npc.SetSprite(icon);
            npc.SetRect(targetPos + offset, size, Quaternion.identity);
        }
        else
            Destroy(npc.gameObject);

        if ((item.type == ItemType.Shoot) && item.removable)
            Item.Remove(item.id, 1);

        callbacks?.ForEach(x => x?.Invoke());
    }
}
