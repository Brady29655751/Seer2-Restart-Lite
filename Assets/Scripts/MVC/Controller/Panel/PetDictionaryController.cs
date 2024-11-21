using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictionaryController : Module
{
    public VersionPetData petData => GameManager.versionData.petData;
    public List<Pet> petDictionary => petData.petAllWithMod.Where(x => !x.info.ui.hide).ToList();
    public List<Pet> petTopic => petData.petTopic;
    public List<Pet> petWorkshop = new List<Pet>();

    [SerializeField] private PetDictionaryMode mode = PetDictionaryMode.Topic;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetBagPanel petBagPanel;

    public override void Init()
    {
        base.Init();
        SelectMode((int)mode);
    }

    public PetDictionaryMode GetMode() => mode;

    public List<Pet> GetStorage() {
        return mode switch {
            PetDictionaryMode.All => petDictionary,
            PetDictionaryMode.Topic => petTopic,
            PetDictionaryMode.Workshop => petWorkshop,
            _ => new List<Pet>(),
        };
    }

    public void SelectMode(int modeId) {
        if (mode == PetDictionaryMode.Workshop)
            return;
            
        mode = (PetDictionaryMode)modeId;
        SetPetStorage(GetStorage());
    }

    public void SetPetStorage(List<Pet> storage) {
        if ((mode == PetDictionaryMode.Workshop) && (ListHelper.IsNullOrEmpty(petWorkshop)))
            petWorkshop = storage.Where(x => !x.info.ui.hide).ToList();

        var petStorage = (mode == PetDictionaryMode.Workshop) ? petWorkshop : storage.ToList();
        selectController.SetStorage(petStorage);
        selectController.Select(0);
    }

    public void TestBattle(int mapId) {
        Map.GetMap(mapId, OnLoadTestBattleMapSuccess, (error) => {
            Hintbox.OpenHintboxWithContent(error, 16);
        });
    }

    private void OnLoadTestBattleMapSuccess(Map map) {
        if (map == null) {
            Hintbox.OpenHintboxWithContent("加载的地图为空", 16);
            return;
        }
        var npcId = map.id * 100 + (map.id > 0 ? 1 : -1);
        var battleInfo = map?.entities?.npcs?.Find(x => x.id == npcId)?.battleHandler?.Find(x => x?.id == "test");
        if (battleInfo == null) {
            Hintbox.OpenHintboxWithContent("测试的NPC战斗信息为空", 16);
            return;
        }

        var player = petBagPanel?.petBag?.Select(BattlePet.GetBattlePet).ToArray();
        var enemy = battleInfo.enemyInfo?.Select(BattlePet.GetBattlePet).ToArray();

        if ((player == null) || (enemy == null)) {
            Hintbox.OpenHintboxWithContent("测试的精灵信息为空", 16);
            return;
        }
        BattleSettings settings = new BattleSettings(battleInfo.settings){ 
            isSimulate = true, 
            isCaptureOK = false 
        };
        Battle battle = new Battle(player, enemy, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }
}

public enum PetDictionaryMode {
    All = 0,
    Topic = 1,
    Workshop = 2,
}
