using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

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
    
}
