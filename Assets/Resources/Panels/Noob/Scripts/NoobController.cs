using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoobController : Module
{
    [SerializeField] private ActivityContentView basicGuideView;

    public Activity noobActivity => Activity.Find("noob");
    public int basicCheckpoint => GetBasicCheckPoint(); 
    public const int MAX_BASIC_CHECKPOINT = 1;

    public override void Init() {
        Refresh();
    }

    public int GetBasicCheckPoint() {
        if (noobActivity.GetData("pet_bag") != "done")
            return 0;
        
        return MAX_BASIC_CHECKPOINT;
    }

    public void Refresh() {
        basicGuideView.SetActivity(Activity.GetActivityInfo("noob_basic_" + (basicCheckpoint + 1)));
    }

    public void BasicGuideLink() {
        switch (basicCheckpoint) {
            default:
                break;
            case 0:
                var petBagPanel = Panel.OpenPanel<PetBagPanel>();
                void OnClosePetBag() {
                    petBagPanel.onCloseEvent -= OnClosePetBag;
                    Refresh();
                }
                petBagPanel.onCloseEvent += OnClosePetBag;
                break;
        }
    }
}
