using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SPTBossPanel : Panel
{
    [SerializeField] private List<GameObject> bossPageObjects = new List<GameObject>();
    private List<SPTBossPageController> bossPages = new List<SPTBossPageController>();

    protected override void Awake()
    {
        base.Awake();
        bossPages = bossPageObjects.Select(x => x.GetComponent<SPTBossPageController>()).ToList();
    }

    public override void SetPanelIdentifier(string id, string param)
    {
        switch (id)
        {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "page":
                SetPage(int.Parse(param));
                return;
        }

    }

    public void SetPage(int page) {
        for (int i = 0; i < bossPageObjects.Count; i++)
            bossPageObjects[i]?.SetActive(i == page);
    }
}
