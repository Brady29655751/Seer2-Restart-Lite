using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleRecordBlockView : Module
{
    [SerializeField] private List<PetSelectBlockView> myPetBag, opPetBag;
    [SerializeField] private Text resultText, dateText;

    private BattleRecord currentRecord;
    
    public void SetBattleRecord(BattleRecord record) {
        currentRecord = record;
        for (int i = 0; i < myPetBag.Count; i++) {
            myPetBag[i]?.SetPet(record.myPetBag.ElementAtOrDefault(i));
            opPetBag[i]?.SetPet(record.opPetBag.ElementAtOrDefault(i));
        }
        resultText?.SetText(record.resultNote);
        resultText?.SetColor(record.resultColor);
        dateText?.SetText(record.date.ToString("yyyy/MM/dd HH:mm"));
    }

    public void PlayRecord() {
        Player.instance.currentBattleRecord = currentRecord;
        Player.instance.currentBattle = new Battle(currentRecord.myPetBag, currentRecord.opPetBag, currentRecord.settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }

}
