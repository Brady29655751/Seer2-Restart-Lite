using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSceneController : Module
{
    [SerializeField] private MapSceneModel sceneModel;
    [SerializeField] private MapSceneView sceneView;

    public void SetMap(Map map) {
        sceneModel.SetMap(map);
        sceneView.SetMap(map);
    }
}
