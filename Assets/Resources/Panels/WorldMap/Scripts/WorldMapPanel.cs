using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapPanel : Panel
{
    [SerializeField] private Text worldNameText;
    [SerializeField] private PageView pageView;
    [SerializeField] private List<GameObject> worldObjects;
    [SerializeField] private List<GameObject> debugObjectList;

    private int worldId = 0;
    private string[] worldNames = new string[]{ "阿卡迪亚星", "新世界" };

    public override void Init() {
        debugObjectList?.ForEach(x => x?.SetActive(GameManager.instance.debugMode));
        SetWorld(worldId);
    }

    public override void SetPanelIdentifier(string id, string param)
    {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "world":
                SetWorld(int.Parse(param));
                return;
        }
    }

    public void SetWorld(int world) {
        worldId = world;
        worldNameText?.SetText(worldNames[world]);
        worldObjects.ForEach((x, i) => x?.SetActive(i == world));
        pageView?.SetPage(worldId, 1);
    }

    public void PrevPage() {
        SetWorld((worldId - 1 + worldNames.Length) % worldNames.Length);
    }

    public void NextPage() {
        SetWorld((worldId + 1) % worldNames.Length);
    }
}
