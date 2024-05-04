using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopAllController : Module
{
    [SerializeField] private WorkshopAllModel allModel;
    [SerializeField] private WorkshopAllView allView;
    
    
    public override void Init() {
        if (SaveSystem.IsModExists())
            allView.CheckCurrentMod();
    }

    public void CreateMod() {
        if (!SaveSystem.TryCreateMod(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        Hintbox.OpenHintboxWithContent("创建成功，开始发挥你的创意吧！", 16);
        CheckCurrentMod();
    }

    private void CheckCurrentMod() {
        allView.CheckCurrentMod();
    }

    public void OpenModPetDictionaryPanel() {
        var petStorage = Database.instance.petInfoDict.Where(entry => PetInfo.IsMod(entry.Key))
            .OrderByDescending(entry => entry.Key).Select(entry => Pet.GetExamplePet(entry.Key)).ToList();

        if (petStorage.Count == 0) {
            Hintbox.OpenHintboxWithContent("你目前还没有制作任何创意精灵哦！", 16);
            return;
        }

        var petDictionaryPanel = Panel.OpenPanel<PetDictionaryPanel>();
        petDictionaryPanel.SelectMode(PetDictionaryMode.Workshop);
        petDictionaryPanel.SetStorage(petStorage);
    }

    public void OpenAllSkillPanel() {
        allView.OpenAllSkillPanel();
    }

    public void OpenAllBuffPanel() {
        allView.OpenAllBuffPanel();
    }
    
    
    
    
    
}
