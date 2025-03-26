using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordRoomController : Module
{
    [SerializeField] private GameObject battleRecordBlockPrefab;
    [SerializeField] private RectTransform scrollContentRect;

    public override void Init()
    {
        base.Init();
        InitBattleRecords();    
    }

    private void InitBattleRecords() 
    {
        var battleRecordStorage = Player.instance.gameData.battleRecordStorage;
        if (battleRecordStorage == null)
            return;

        for (int i = battleRecordStorage.Count - 1; i >= 0; i--) 
        {
            var obj = Instantiate(battleRecordBlockPrefab, scrollContentRect);
            var recordBlock = obj.GetComponent<BattleRecordBlockView>();
            recordBlock?.SetBattleRecord(battleRecordStorage[i]);
        }
    }

}
