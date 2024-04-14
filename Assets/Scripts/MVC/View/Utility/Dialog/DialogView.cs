using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogView : Module
{
    private ResourceManager RM => ResourceManager.instance;
    [SerializeField] private Color32 initTextColor = new Color32(252, 237, 105, 255);
    [SerializeField] private Color32 hoverTextColor = new Color32(119, 226, 12, 255);
    [SerializeField] private GameObject functionTextPrefab;
    [SerializeField] private GameObject replyTextPrefab;

    [SerializeField] private Vector2 stdIconSize = new Vector2(150, 150);
    [SerializeField] private Image icon;
    [SerializeField] private IText npcName;
    [SerializeField] private IText content;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private RectTransform functionRect;
    [SerializeField] private RectTransform replyRect;

    public void OpenDialog(DialogInfo info) {
        SetIconAndName(  info.icon, info.pos, info.size, info.name);
        SetContent(info.content);
        SetFunction(info.functionHandler);
        SetReply(info.replyHandler);
    }

    private void SetIconAndName(Sprite sprite, Vector2 iconPos, Vector2 iconSize, string name) {
        if (icon != null) {
            icon.SetSprite(sprite);
            icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize.x);
            icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize.y);
            icon.rectTransform.anchoredPosition = iconPos;
        }
        npcName?.SetText(name);
    }

    private void SetContent(string text) {
        content?.SetText(text);
        content?.text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.size.y);
    }

    private void SetFunction(List<NpcButtonHandler> functionHandler) {
        functionRect.DestoryChildren();

        float functionY = Mathf.Min(-55, contentRect.anchoredPosition.y - content.size.y - 5);
        functionRect.anchoredPosition = new Vector2(functionRect.anchoredPosition.x, functionY);

        float handlerX = 0;
        IText lastText = null;

        for (int i = 0; i < functionHandler.Count; i++) {
            NpcButtonHandler handler = functionHandler[i];
            if (handler.typeId == "branch") {
                lastText?.onPointerClickEvent.AddListener(x => {
                    NpcController npc = DialogManager.instance.currentNpc;
                    Dictionary<int, NpcController> npcList = (Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList");
                    NpcHandler.GetNpcEntity(npc, handler, npcList)?.Invoke();
                });
                continue;
            }

            GameObject obj = Instantiate(functionTextPrefab, functionRect);
            RectTransform rect = obj.GetComponent<RectTransform>();
            IText text = obj.GetComponent<IText>();

            text.SetText("<sprite name=\"settings\"> <u>" + handler.description + "</u>");
            text.onPointerEnterEvent.AddListener(x => text.SetColor(hoverTextColor));
            text.onPointerExitEvent.AddListener(x => text.SetColor(initTextColor));
            text.onPointerClickEvent.AddListener(x => {
                NpcController npc = DialogManager.instance.currentNpc;
                Dictionary<int, NpcController> npcList = (Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList");
                NpcHandler.GetNpcEntity(npc, handler, npcList)?.Invoke();
            });

            rect.anchoredPosition = new Vector2(handlerX, 0);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.size.x);

            handlerX += (text.size.x + 10);
            lastText = text;
        }
    }
    
    private void SetReply(List<NpcButtonHandler> replyHandler) {
        replyRect.DestoryChildren();

        float handlerX = 0;
        float handlerSize = replyRect.rect.size.x / replyHandler.Count(x => x.typeId != "branch");
        IText lastText = null;

        for (int i = 0; i < replyHandler.Count; i++) {
            NpcButtonHandler handler = replyHandler[i];
            if (handler.typeId == "branch") {
                lastText?.onPointerClickEvent.AddListener(x => {
                    NpcController npc = DialogManager.instance.currentNpc;
                    Dictionary<int, NpcController> npcList = (Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList");
                    NpcHandler.GetNpcEntity(npc, handler, npcList)?.Invoke();
                });
                continue;
            }

            GameObject obj = Instantiate(replyTextPrefab, replyRect);
            RectTransform rect = obj.GetComponent<RectTransform>();
            IText text = obj.GetComponent<IText>();
            
            text.SetText("<u>" + handler.description + "</u>");
            text.onPointerEnterEvent.AddListener(x => text.SetColor(hoverTextColor));
            text.onPointerExitEvent.AddListener(x => text.SetColor(initTextColor));
            text.onPointerClickEvent.AddListener(x => {
                NpcController npc = DialogManager.instance.currentNpc;
                Dictionary<int, NpcController> npcList = (Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList");
                NpcHandler.GetNpcEntity(npc, handler, npcList)?.Invoke();
            });

            rect.anchoredPosition = new Vector2(handlerX, 0);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(handlerSize, text.size.x));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(replyRect.rect.size.y, text.size.y));

            handlerX -= (text.size.x + 10);
            lastText = text;
        }
    }

}
