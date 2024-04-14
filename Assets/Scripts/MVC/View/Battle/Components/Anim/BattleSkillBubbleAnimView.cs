using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillBubbleAnimView : BattleBaseView
{
    [SerializeField] private Image background;
    [SerializeField] private Text skillName;

    public void SetSkill(Skill skill) {
        skillName.text = skill.name;
    }

    public void SetActive(bool active) {
        background.gameObject.SetActive(active);
        skillName.gameObject.SetActive(active);
    }
}
