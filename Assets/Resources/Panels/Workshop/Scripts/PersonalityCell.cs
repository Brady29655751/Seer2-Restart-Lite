using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalityCell : UIModule
{
    public RectTransform rectTransform;
    [SerializeField] private IButton button;
    [SerializeField] private TextCell nameCell;
    [SerializeField] private List<TextCell> statusCells;

    public Personality currentPersonality { get; private set; }
    public Status buff => Status.GetPersonalityBuff(currentPersonality);

    public void SetPersonality(Personality personality) {
        currentPersonality = personality;
        gameObject.SetActive(((int)personality) >= 0);
        nameCell.SetText(personality.ToString());
        for (int i = 0; i < statusCells.Count; i++) {
            if ((buff[i] == 1) || (i >= 6)) {
                statusCells[i].SetText("-");
                statusCells[i].SetTextColor(Color.white);
                continue;
            }

            statusCells[i].SetText(buff[i].ToString());
            statusCells[i].SetTextColor((buff[i] > 1) ? Color.green : Color.red);
        }
    }

    public void SetInfoPrompt(InfoPrompt prompt) {
        infoPrompt = prompt;
    }

    public void SetCallback(Action<Personality> callback) {
        nameCell?.SetCallback(() => callback?.Invoke(currentPersonality));
    }
}
