using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPanel : Panel
{
    [SerializeField] private List<Panel> initPanels = new List<Panel>();

    public override void Init() {
        initPanels.ForEach(x => {
            x.SetActive(true);
            x.SetActive(false);
        });
    }


}
