using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillBubbleAnimView : BattleBaseView
{
    [SerializeField] private Timer timer;
    [SerializeField] private Image background;
    [SerializeField] private Text skillName;

    protected override void Awake()
    {
        base.Awake();
        timer.onDoneEvent += SetDisactive;
    }

    public void SetSkill(Skill skill)
    {
        skillName.text = skill.name + ((skill.combo != 1) ? ("（" + skill.combo + "连击）") : string.Empty);
        this.SetActive();
    }

    private void SetActive() //显示技能名字, 2.5秒后自动消失
    {
        background.gameObject.SetActive(true);
        skillName.gameObject.SetActive(true);
        timer.SetTimer(2);
    }

    private void SetDisactive(float leftTime) 
    {
        background.gameObject.SetActive(false);
        skillName.gameObject.SetActive(false);
    }

    /*
    private async Task SetDisactive()
    {
        await Task.Delay(2500);
        background.gameObject.SetActive(false);
        skillName.gameObject.SetActive(false);
    }
    */
}