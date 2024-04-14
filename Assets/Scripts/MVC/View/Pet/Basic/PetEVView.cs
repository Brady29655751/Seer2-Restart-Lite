using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetEVView : Module
{
    [SerializeField] private Sprite startButtonSprite;
    [SerializeField] private Sprite confirmButtonSprite;
    [SerializeField] private IButton startButton;
    [SerializeField] private IButton resetButton;
    [SerializeField] private Text evStorageText;

    public void SetPet(Pet pet) {
        gameObject.SetActive(pet != null);
        if (pet == null)
            return;
        SetEVStorage(pet.talent.evStorage);
        SetStartButtonSprite(startButtonSprite);
    }

    public void OnBeforeSetEV(int evStorage) {
        SetEVStorage(evStorage);
        SetStartButtonSprite(confirmButtonSprite);
    }

    public void OnSetEV(int evStorage) {
        SetEVStorage(evStorage);
    }

    public void OnAfterSetEV(int evStorage) {
        SetEVStorage(evStorage);
        SetStartButtonSprite(startButtonSprite);
    }


    private void SetStartButtonSprite(Sprite sprite) {
        startButton.SetSprite(sprite);
    }

    private void SetEVStorage(int evStorage) {
        evStorageText.text = evStorage.ToString();
    }

}
