using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapPanel : Panel
{
    [SerializeField] private List<GameObject> debugObjectList;

    public override void Init() {
        debugObjectList?.ForEach(x => x?.SetActive(GameManager.instance.debugMode));
    }
}
