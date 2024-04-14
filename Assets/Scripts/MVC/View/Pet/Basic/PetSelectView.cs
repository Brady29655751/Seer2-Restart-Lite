using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSelectView : Module
{
    [SerializeField] private Text storageCountText;
    [SerializeField] private PetSelectBlockView[] selectBlockViews = new PetSelectBlockView[6];

    public void SetStorageCountText(int count) {
        if (storageCountText == null)
            return;

        storageCountText.text = count.ToString();
    }

    public void SetStorageCountText(string countText) {
        if (storageCountText == null)
            return;

        storageCountText.text = countText;
    }

    public void SetSelections(Pet[] selections) {
        if (selections == null)
            return;
        
        for (int i = 0; i < selections.Length; i++) {    
            selectBlockViews[i].SetPet(selections[i]);
        }
    }

    public void Select(int index) {
        if (!index.IsInRange(0, selectBlockViews.Length))
            return;

        for (int i = 0; i < selectBlockViews.Length; i++) {
            selectBlockViews[i].SetChosen(i == index);
        }
    }
}
