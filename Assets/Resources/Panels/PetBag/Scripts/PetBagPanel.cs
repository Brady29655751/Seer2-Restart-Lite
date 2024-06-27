using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBagPanel : Panel
{
    private Pet[] playerPetBag => Player.instance.petBag;
    private List<Item> playerItemBag => Player.instance.gameData.itemBag;

    public Pet[] petBag => selectController?.GetPetSelections();

    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetFeatureController featureController;
    [SerializeField] private PetStatusController statusController;
    [SerializeField] private PetSwapSkillController skillController;
    [SerializeField] private PetItemController itemController;
    [SerializeField] private PetInfoController infoController;
    [SerializeField] private PetSkinController skinController;

    [SerializeField] private PetPersonalityController personalityController;
    [SerializeField] private PetSwitchController switchController;
    [SerializeField] private PetTeamController teamController;

    protected override void Awake() {
        base.Awake();
        InitSelectSubscriptions();
        InitOtherSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        switch (mode) {
            case PetBagMode.Normal:
                SetPetBag(playerPetBag);
                SetItemBag(playerItemBag);
                break;
            case PetBagMode.PVP:
                break;
            case PetBagMode.YiTeRogue:
                break;
            default:
                break;
        }
    }

    private void InitSelectSubscriptions() {
        selectController.onSelectPetEvent += demoController.SetPet;
        
        if (featureController != null)
            selectController.onSelectPetEvent += featureController.SetPet;

        selectController.onSelectPetEvent += statusController.SetPet;
        selectController.onSelectPetEvent += skillController.SetPet;
        selectController.onSelectPetEvent += itemController.SetPet;
        selectController.onSelectPetEvent += infoController.SetPet;
        selectController.onSelectPetEvent += skinController.SetPet;

        if (personalityController != null)
            selectController.onSelectPetEvent += personalityController.SetPet;
    }

    private void InitOtherSubscriptions() {
        statusController.onSetEVSuccessEvent += OnSetEVSuccess;
        itemController.onItemUsedEvent += OnItemUsed;
        skinController.onSetSkinSuccessEvent += OnSetSkinSuccess;

        if (teamController != null)
            teamController.onSelectTeamSuccessEvent += RefreshPetBag;

        if (switchController != null)
            switchController.onSwitchSuccessEvent += OnSwitchPetSuccess;

        if (personalityController != null)
            personalityController.onSelectPersonalityEvent += OnSetPersonalitySuccess;
    }

    private void OnSetEVSuccess(Pet pet) {
        selectController.RefreshView();
    }

    private void OnItemUsed(Item item, int usedNum) {
        RefreshPetBag(playerPetBag);
        SetItemBag(playerItemBag);
    }

    private void OnSetSkinSuccess() {
        // Only change skin when petBag is player's true petBag.
        RefreshPetBag(playerPetBag);
    }

    private void OnSwitchPetSuccess(Pet pet) {
        int cursor = selectController.GetCursor().FirstOrDefault();
        var newBag = petBag.Select((x, i) => (i == cursor) ? pet : x).ToArray();
        RefreshPetBag(newBag);
    }

    private void OnSetPersonalitySuccess(Personality personality) {
        RefreshPetBag(petBag);
    }

    private void RefreshPetBag(Pet[] bag) {
        int cursor = selectController.GetCursor().FirstOrDefault();
        SetPetBag(bag);
        selectController.Select(cursor);
    }

    public void SetPetBag(Pet[] bag) {
        List<Pet> storage = bag.ToList();
        selectController.SetStorage(storage);
        selectController.Select(0);
    }

    public void SetItemBag(List<Item> bag) {
        itemController.SetItemBag(bag);
    }
}

public enum PetBagMode {
    Normal = 0,
    PVP = 1,
    YiTeRogue = 2,
}