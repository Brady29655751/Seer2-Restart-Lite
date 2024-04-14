using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSceneModel : Module
{
    public Map map { get; private set; }

    public void SetMap(Map map) {
        this.map = map;
    }
}
