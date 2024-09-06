using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillBubbleAnimView : BattleBaseView
{
    [SerializeField] private Image background;
    [SerializeField] private Text skillName;

    public void SetSkill(Skill skill)
    {
        skillName.text = skill.name;
        this.SetActive();
    }

    private void SetActive() //显示技能名字, 2.5秒后自动消失
    {
        background.gameObject.SetActive(true);
        skillName.gameObject.SetActive(true);
        _ = this.SetDisactive();
    }

    private async Task SetDisactive()
    {
        await Task.Delay(2500);
        background.gameObject.SetActive(false);
        skillName.gameObject.SetActive(false);
    }
}