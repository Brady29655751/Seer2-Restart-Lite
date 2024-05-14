using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModPanel : Panel
{
    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] private IText loadingText;

    protected Dictionary<int, NpcController> npcDict = new Dictionary<int, NpcController>();

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

        ESCButton.transform.SetParent(rectTransform);
        ESCButton.transform.SetAsLastSibling();
    }
    
    
}
