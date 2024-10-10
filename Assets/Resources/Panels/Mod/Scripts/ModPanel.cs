using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModPanel : Panel
{
    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] private IText loadingText;

    protected Dictionary<int, NpcController> npcDict = new Dictionary<int, NpcController>();

    public override void ClosePanel() {
        ResourceManager.instance.UnloadResources();
        base.ClosePanel(); 
        Resources.UnloadUnusedAssets();
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "data":
                SetPanelData(ResourceManager.LoadXML<PanelData>("Panels/" + param));
                return;
        }
    }

    public void SetPanelData(PanelData panelData) {
        if (panelData == null) {
            loadingText.SetText("加载自制面板失败");
            return;
        }

        loadingText.gameObject.SetActive(false);

        // change size.
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelData.size.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelData.size.y);

        // load npc.
        foreach (var npcInfo in panelData.npcs) {
            var prefab = ResourceManager.instance.GetPrefab("Map/Npc");
            GameObject obj = Instantiate(prefab, rectTransform);
            NpcController npc = obj.GetComponent<NpcController>();
            NpcHandler.CreateNpc(npc, npcInfo, npcDict, infoPrompt);
        }

        foreach (var npcInfo in panelData.npcs) {
            var autoActionList = npcInfo.eventHandler.Where(x => x.type == ButtonEventType.Auto)
                .Select(x => NpcHandler.GetNpcEntity(npcDict.Get(npcInfo.id), x, npcDict)).ToList();
            autoActionList.ForEach(x => x?.Invoke());
        }

        ESCButton.transform.SetParent(rectTransform);
        ESCButton.transform.SetAsLastSibling();
    }
    
    
}
