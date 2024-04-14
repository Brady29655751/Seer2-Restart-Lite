using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetNumView : BattleBaseView
{
    [SerializeField] private Sprite hasPetSprite, noPetSprite;
    [SerializeField] private Image[] petNumImages = new Image[6];

    public void SetPetBag(BattlePet[] petBag) {
        int petNum = petBag.Count(x => (x != null));
        int aliveNum = petBag.Count(x => (x != null) && (!x.isDead));

        for (int i = 0; i < 6; i++) {
            petNumImages[i].sprite = (i < petNum) ? hasPetSprite : noPetSprite;
            petNumImages[i].color = i.IsInRange(aliveNum, petNum) ? Color.gray : Color.white; 
        }
    }

}
