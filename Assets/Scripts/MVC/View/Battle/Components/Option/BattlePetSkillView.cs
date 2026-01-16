using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon.StructWrapping;

public class BattlePetSkillView : BattleBaseView
{
    private UnitPetSystem petSystem;
    private BattlePet pet => petSystem?.pet;
    private bool interactable;
    private bool superSkillClickable = false;
    private bool evolve = false, token = false;

    [SerializeField] private Material shiningMaterial;
    [SerializeField] private IButton noOpSkillButton, superSkillButton, evolveSkillButton, tokenSkillButton;
    [SerializeField] private Image[] superSkillButtonBackground = new Image[3];
    [SerializeField] private BattlePetSkillBlockView[] skillBlockViews = new BattlePetSkillBlockView[4];

    public void SetPetSystem(UnitPetSystem petSystem)
    {
        if (petSystem?.pet == null)
            return;

        this.petSystem = petSystem;
        SetNormalSkill();
        SetInteractable(interactable);
    }

    public void SetInteractable(bool interactable)
    {
        var anger = -1;

        interactable &= (pet != null) && (!pet.isDead);

        if (interactable)
            anger = (pet.buffController.GetBuff(61) != null) ? int.MaxValue : pet.anger;

        SetNormalSkillInteractable(interactable, anger);
        SetSuperSkillInteractable(interactable, anger);
        SetNoOpSkillInteractable(interactable, (anger == -1) ? int.MaxValue : anger);
        SetEvolveSkillInteractable(interactable);
        SetTokenSkillInteractable(interactable && (petSystem.token != null));

        this.interactable = interactable;
    }

    private void SetNormalSkill()
    {
        var normalSkills = pet.skillController.normalSkills;
        for (int i = 0; i < skillBlockViews.Length; i++)
        {
            skillBlockViews[i].SetSkill((i < normalSkills.Count) ? normalSkills[i] : null, battle.settings.rule);
        }
    }

    private void SetNormalSkillInteractable(bool interactable, int petAnger)
    {
        var normalSkills = pet.skillController.normalSkills;
        for (int i = 0; i < normalSkills.Count; i++)
        {
            bool usable;
            if (normalSkills[i] == null)
                usable = false;
            else
                usable = battle.settings.rule switch
                {
                    BattleRule.PP => normalSkills[i].PP > 0,
                    BattleRule.Anger => petAnger >= normalSkills[i].anger,
                    _ => true,
                };

            skillBlockViews[i].SetInteractable(interactable && usable);
        }
    }

    private void SetSuperSkillInteractable(bool interactable, int petAnger)
    {
        var superSkill = pet.skillController.superSkill;
        bool usable = (superSkill != null) && battle.settings.rule switch
        {
            BattleRule.Anger => petAnger >= superSkill.anger,
            BattleRule.PP => superSkill.PP > 0,
            _ => true,
        };
        superSkillClickable = interactable && usable;
        superSkillButton.SetInteractable(true);
        superSkillButtonBackground[0].SetMaterial((interactable && usable) ? shiningMaterial : null);
        // superSkillButtonBackground[1].gameObject.SetActive(interactable);
        // superSkillButtonBackground[2].gameObject.SetActive(interactable);
    }

    private void SetEvolveSkill(bool evolve)
    {
        this.evolve = evolve;
        evolveSkillButton?.SetMaterial(evolve ? shiningMaterial : null);
    }

    private void SetEvolveSkillInteractable(bool interactable)
    {
        evolveSkillButton?.SetInteractable(interactable);
        if (!interactable)
            evolveSkillButton?.SetMaterial(null);
    }

    private void SetTokenSkill(bool token)
    {
        this.token = token;
        tokenSkillButton?.SetMaterial(token ? shiningMaterial : null);

        var state = new BattleState(battle.currentState);
        if (token)
            state.myUnit.petSystem.SwapTokenPet(state.myUnit.petSystem.cursor);

        UI.SetState(null, state);
        UI.ProcessQuery(true);
    }

    private void SetTokenSkillInteractable(bool interactable)
    {
        tokenSkillButton?.gameObject.SetActive(interactable);
        tokenSkillButton?.SetInteractable(interactable);
        if (!interactable)
        {
            tokenSkillButton?.SetMaterial(null);
            this.token = false;   
        }
    }

    public void SelectNormalSkill(int index)
    {
        if (!index.IsInRange(0, skillBlockViews.Length))
            return;

        var skill = new Skill(pet.skillController.normalSkills[index]);
        if (skill.type == SkillType.被动)
            return;

        if (battle.settings.parallelCount > 1)
            skill.SetParallelIndex(battle.currentState.myUnit.petSystem.cursor, battle.currentState.opUnit.petSystem.cursor);

        if (evolve)
        {
            skill.options.Set("evolve", "1");
            SetEvolveSkill(false);
        }

        battle.SetSkill(skill, true);
    }

    public void ShowNormalSkillInfo(int index)
    {
        if (!index.IsInRange(0, skillBlockViews.Length))
            return;

        var normalSkill = pet?.skillController?.normalSkills[index];

        infoPrompt.SetSkill(normalSkill);
        infoPrompt.SetPosition(new Vector2(80 + 175 * index, 109));

        /*
        float boxSizeY = Mathf.Max(150, normalSkill?.description.GetPreferredSize(12, 12, 21, 35).y ?? 0);
        float boxSizeYMedium = Mathf.Max(150, normalSkill?.description.GetPreferredSize(15, 12, 21, 35).y ?? 0);
        float boxSizeYLarge = Mathf.Max(150, normalSkill?.description.GetPreferredSize(24, 12, 21, 35).y ?? 0);
        var boxSize = (boxSizeY > 250) ? ((boxSizeYMedium > 300) ? new Vector2(300, boxSizeYLarge) : new Vector2(200, boxSizeYMedium)) : new Vector2(170, boxSizeY);
        descriptionBox.SetBoxSize(boxSize);
        descriptionBox.SetText(normalSkill?.description);
        descriptionBox.SetBoxPosition(new Vector2(80 + 175 * index, 109));
        */
    }

    public void SelectSuperSkill()
    {
        if (pet.skillController.superSkill == null)
            return;

        if (!superSkillClickable)
            return;

        var skill = new Skill(pet.skillController.superSkill);
        if (skill.type == SkillType.被动)
            return;
        
        if (battle.settings.parallelCount > 1)
            skill.SetParallelIndex(battle.currentState.myUnit.petSystem.cursor, battle.currentState.opUnit.petSystem.cursor);

        if (evolve)
        {
            skill.options.Set("evolve", "1");
            SetEvolveSkill(false);
        }

        battle.SetSkill(skill, true);
    }

    public void ShowSuperSkillInfo()
    {
        var superSkill = pet?.skillController?.superSkill;
        var description = superSkill?.description;

        if (description != null)
        {
            var addDescription = $"<color=#ff3300>【威力 {superSkill.power}】</color>\n";
            if (battle.settings.rule == BattleRule.PP)
                addDescription += battle.settings.rule switch
                {
                    BattleRule.Anger => $"<color=#ffbb33>【怒气 {superSkill.anger}】</color>\n",
                    BattleRule.PP => $"<color=#ffbb33>【次数 {superSkill.PP} / {superSkill.maxPP}】</color>\n",
                    _ => string.Empty
                };
                
            description = addDescription + description;

            var showSkill = new Skill(superSkill)
            {
                rawDescription = addDescription + superSkill.rawDescription,
            };
            infoPrompt.SetSkill(showSkill);
            infoPrompt.SetPosition(new Vector2(5, 120));
        } 
        else
        {
            SetInfoPromptContent("尚未习得必杀技");
            infoPrompt.SetPosition(new Vector2(5, 120));    
        }



        /*
        float boxSizeY = Mathf.Max(150, description?.GetPreferredSize(12, 14).y ?? 0);
        float boxSizeYMedium = Mathf.Max(150, description?.GetPreferredSize(15, 14).y ?? 0);
        float boxSizeYLarge = Mathf.Max(150, description?.GetPreferredSize(24, 14).y ?? 0);
        var boxSize = (boxSizeY > 250) ? ((boxSizeYMedium > 300) ? new Vector2(300, boxSizeYLarge) : new Vector2(200, boxSizeYMedium)) : new Vector2(170, boxSizeY);
        descriptionBox.SetBoxSize(boxSize);
        descriptionBox.SetText(description ?? "尚未习得必杀技");
        descriptionBox.SetBoxPosition(new Vector2(5, 115));
        */
    }

    public void SetNoOpSkillInteractable(bool interactable, int petAnger)
    {
        bool isNormalSkillUsable = battle.settings.rule switch
        {
            BattleRule.Anger => pet.skillController.normalSkills.Where(x => (x != null) && (x.type != SkillType.被动)).Any(x => petAnger >= x.anger),
            BattleRule.PP => pet.skillController.normalSkills.Where(x => (x != null) && (x.type != SkillType.被动)).Any(x => x.PP > 0),
            _ => true,
        };
        bool isSuperSkillUsable = (pet.skillController.superSkill != null) && (pet.skillController.superSkill.type != SkillType.被动) &&
        battle.settings.rule switch
        {
            BattleRule.Anger => petAnger >= pet.skillController.superSkill.anger,
            BattleRule.PP => pet.skillController.superSkill.PP > 0,
            _ => true,
        };
        bool isAnySkillUsable = isNormalSkillUsable || isSuperSkillUsable;
        noOpSkillButton.SetInteractable(interactable && ((battle.settings.mode == BattleMode.Card) || (!isAnySkillUsable)));
    }

    public void SelectNoOpSkill()
    {
        var isCardMode = battle.settings.mode == BattleMode.Card;
        var isPassivePetChangePhase = (battle.currentPhase?.phase ?? EffectTiming.None) == EffectTiming.OnPassivePetChange;

        if (isCardMode && !isPassivePetChangePhase)
        {
            var roundEndPhase = new RoundEndPhase();
            battle.currentPhase = roundEndPhase;
            battle.NextPhase();
        }

        var parallelSourceIndex = (battle.settings.parallelCount > 1) ? battle.currentState.myUnit.petSystem.cursor : -1;
        battle.SetSkill(Skill.GetNoOpSkill(parallelSourceIndex), true);

        if (isCardMode && !isPassivePetChangePhase)
        {
            var roundStartPhase = new RoundStartPhase();
            battle.currentPhase = roundStartPhase;
            battle.NextPhase();
        }
    }

    public void ShowNoOpSkillInfo()
    {
        if (battle.settings.mode == BattleMode.Card)
        {
            SetInfoPromptContentAtLeft("结束本轮");
            return;
        }
        infoPrompt.SetSkill(Skill.GetNoOpSkill(), false);
    }

    public void SelectEvolveSkill()
    {
        SetEvolveSkill(!evolve);
    }

    public void ShowEvolveSkillInfo()
    {
        infoPrompt.SetSkill(new Skill()
        {
            name = "专属动作" + (evolve ? "（正在使用）" : string.Empty),
            critical = 5,
            accuracy = 100,
            rawDescription = "手动点击使用当前精灵的专属动作[ENDL]次数全队共用（当前剩余 [ffbb33]" + petSystem.specialChance + "[-] 次）[ENDL][ENDL]"
                + "[ffbb33]【神迹觉醒】[-]消耗1次[ENDL][ffbb33]【暴走】[-]不消耗次数，需要特殊道具"
        });
    }

    public void SelectTokenSkill()
    {
        if (UI.currentState.myUnit.token == null)
        {
            Hintbox.OpenHintboxWithContent("当前精灵没有分身或附属精灵！", 16);
            return;
        }
        SetTokenSkill(!token);
    }

    public void ShowTokenSkillInfo()
    {
        infoPrompt.SetSkill(new Skill()
        {
            name = "附属精灵" + (token ? "（正在查看）" : string.Empty),
            critical = 5,
            accuracy = 100,
            rawDescription = "手动点击查看当前精灵的附属精灵",
        });
    }
}
