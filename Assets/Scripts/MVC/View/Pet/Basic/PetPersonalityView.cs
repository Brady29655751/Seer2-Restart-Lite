using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetPersonalityView : Module
{
    public ResourceManager RM => ResourceManager.instance;
    [SerializeField] private Image relationGraph;
    [SerializeField] private IText personalityText;
    [SerializeField] private IText noteText;

    public void SetPersonalityText(Personality personality) {
        personalityText?.SetText(personality.ToString());
    }

    public void SetNote(string note) {
        noteText?.SetText(note);
    }

    public void SetRelationGraph(Personality personality) {
        int id = (int)personality;
        Sprite sprite = RM.GetSprite("Personality/Relation/" + id);
        relationGraph?.SetSprite(sprite);
    }

}
