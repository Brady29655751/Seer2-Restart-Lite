using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoobController : Module
{
    [SerializeField] private ActivityContentView basicGuideView;
    [SerializeField] private GameObject advanceGuideObject;

    public static NoobCheckPoint noobCheckPoint => GetNoobCheckPoint(); 

    public override void Init() {
        Refresh();
    }

    public static NoobCheckPoint GetNoobCheckPoint() {
        if (Activity.Noob.GetData("pet_bag") != "done")
            return NoobCheckPoint.PetBag;
        
        if (Activity.Noob.GetData("map") != "done")
            return NoobCheckPoint.Map;

        if (Activity.Noob.GetData("train") != "done")
            return NoobCheckPoint.Train;

        if (Activity.Noob.GetData("battle") != "done")
            return NoobCheckPoint.Battle;

        return NoobCheckPoint.Max;
    }

    public void Refresh() {
        basicGuideView.SetActivity(Activity.GetActivityInfo("noob_tutor_" + ((int)noobCheckPoint + 1)));
        basicGuideView?.gameObject.SetActive(Player.instance.gameData.IsNoob());
        advanceGuideObject?.SetActive(!Player.instance.gameData.IsNoob());
    }

    public void BasicGuideLink() {
        switch (noobCheckPoint) {
            default:
                break;
            case NoobCheckPoint.PetBag:
                var petBagPanel = Panel.OpenPanel<PetBagPanel>();
                void OnClosePetBag() {
                    petBagPanel.onCloseEvent -= OnClosePetBag;
                    Refresh();
                }
                petBagPanel.onCloseEvent += OnClosePetBag;
                break;
            case NoobCheckPoint.Map:
                TeleportHandler.Teleport(71);
                break;
            case NoobCheckPoint.Train:
                TeleportHandler.Teleport(61);
                break;
            case NoobCheckPoint.Battle:
                Map.TestBattle(2, 201, "default");
                break;
        }
    }
}

public enum NoobCheckPoint 
{
    PetBag = 0,
    Map = 1,
    Train = 2,
    Battle = 3,
    Max = 4,
}
