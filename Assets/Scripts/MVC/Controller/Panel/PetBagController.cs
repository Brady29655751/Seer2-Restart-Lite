using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBagController : Module
{
    private Pet[] petBag => GetPetBag();
    private bool isDemoMode = true;

    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetBagModel buttonModel;
    [SerializeField] private PetBagView buttonView;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetPersonalityController personalityController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetFeatureController featureController;
    [SerializeField] private PetSkinController skinController;
    [SerializeField] private PetBattleTestController battleTestController;

    protected override void Awake()
    {
        base.Awake();
        InitSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        buttonModel.SetMode(mode);
        buttonView.SetSwapButtonActive(mode == PetBagMode.Normal);
    }

    private void InitSubscriptions()
    {
        selectController.onSelectIndexEvent += buttonView.OnSelect;
    }

    private Pet[] GetPetBag()
    {
        return mode switch
        {
            PetBagMode.Normal => Player.instance.gameData.petBag,
            PetBagMode.YiTeRogue => YiTeRogueData.instance.petBag,
            _ => selectController.GetPetSelections(),
        };
    }

    public void RefreshPetBag()
    {
        selectController.SetStorage(petBag.ToList(), selectController.GetPage());
        selectController.Select(0);
    }

    public void SwapPetTeam()
    {
        if (mode != PetBagMode.Normal)
            return;

        if (Player.instance.petBag.TakeLast(6).All(x => x == null))
        {
            Hintbox.OpenHintboxWithContent("当前备用背包没有任何精灵", 16);
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            var tmp = Player.instance.petBag[i];
            Player.instance.petBag[i] = Player.instance.petBag[6 + i];
            Player.instance.petBag[6 + i] = tmp;
        }
        SaveSystem.SaveData();
        RefreshPetBag();
    }

    public void SetPetDrop(int dropIndex, RectTransform rectTransform = null)
    {
        if (mode != PetBagMode.Normal && mode != PetBagMode.YiTeRogue)
            return;

        buttonModel.SetPetDrop(dropIndex);
    }

    public void SetPetFirst()
    {
        buttonModel.SetPetFirst();
        RefreshPetBag();
    }

    public void SetPetHeal()
    {
        buttonModel.SetPetHeal();
        buttonView.OnSetPetHeal();
        selectController.RefreshView();
    }

    public void SetPetAnim()
    {
        demoController?.TogglePetAnimationMode();
    }

    public void SetPetTrain()
    {
        isDemoMode = !isDemoMode;
        demoController.gameObject.SetActive(isDemoMode);
        featureController.gameObject.SetActive(!isDemoMode);
    }

    public void ToBestPet(Personality personality)
    {
        personalityController.onSelectPersonalityEvent -= ToBestPet;
        personalityController.SetActive(false);
        if (buttonModel.SetPetTrain() == null)
            return;

        buttonView.OnSetPetTrain();
        RefreshPetBag();
    }

    public void SetPetPersonality(int value)
    {
        Func<Personality, bool> filter = value switch
        {
            -1 => (p) => ((int)p).IsInRange(0, 25),
            -2 => (p) => ((int)p).IsInRange(25, 55),
            _ => (p) => true,
        };

        personalityController.onSelectPersonalityEvent += ToPersonalityPet;
        personalityController.SetActive(true, () => personalityController.Filter(filter));
    }

    public void ToPersonalityPet(Personality personality)
    {
        personalityController.onSelectPersonalityEvent -= ToPersonalityPet;
        personalityController.SetActive(false);

        RefreshPetBag();

        if ((mode == PetBagMode.PVP) || (mode == PetBagMode.Dictionary))
            return;

        SaveSystem.SaveData();
    }

    public void SetPetHome()
    {
        if (!buttonModel.SetPetHome())
        {
            buttonView.OnSetPetHomeFailed();
        }
        RefreshPetBag();
    }

    public void OpenPetStoragePanel()
    {
        PetStoragePanel storagePanel = Panel.OpenPanel<PetStoragePanel>();
        storagePanel.onCloseEvent += RefreshPetBag;
    }

    public void TogglePetItemPanel()
    {
        buttonView.TogglePetItemPanel();
    }

    public void OpenBattleTestPanel()
    {
        if (Activity.Noob.GetData("battle") != "done")
        {
            Hintbox.OpenHintboxWithContent("完成新手战斗教学后解锁", 16);
            return;
        }
        battleTestController?.gameObject.SetActive(true);
    }

    public void TestBattle(int mapId)
    {
        var pets = selectController?.GetPetSelections();
        switch (mapId)
        {
            default:
                battleTestController?.TestBattle(pets);
                break;
            case 2:
            case -2:
                Map.TestBattle(mapId, pets);
                break;
            case 124:
                Map.TestBattle(124, 12402, "default", pets);
                break;
        }
    }

}
