using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PetBattleTestController : Module
{
    [SerializeField] private IInputField idInputField, levelInputField, statusInputField, initBuffInputField, 
        headerSkillInputField, loopSkillInputField, superSkillInputField;

    [SerializeField] private Toggle emblemToggle;

    public bool TryGetBossInfo(out BossInfo bossInfo) {
        bossInfo = null;
        if (string.IsNullOrEmpty(idInputField.inputString) || (!int.TryParse(idInputField.inputString, out var id)) || (Pet.GetPetInfo(id) == null)) {
            Hintbox.OpenHintboxWithContent("精灵序号错误", 16);
            return false;
        }

        if (string.IsNullOrEmpty(levelInputField.inputString) || (!int.TryParse(levelInputField.inputString, out var level)) || (level < 1)) {
            Hintbox.OpenHintboxWithContent("等级错误", 16);
            return false;
        }

        var statusList = statusInputField.inputString?.Split('/');
        var status = new BattleStatus(Status.zero);
        if (statusList != null) {
            for (int i = 0; i < statusList.Length; i++) {
                var spec = statusList[i].Split(':');
                if ((spec.Length == 2) && int.TryParse(spec[1], out var stat))
                    status[spec[0]] = stat;
                else if ((spec.Length == 1) && int.TryParse(spec[0], out stat))
                    status[i] = stat;
                else {
                    Hintbox.OpenHintboxWithContent("能力值格式错误", 16);
                    return false;
                }
            }
        }

        if (string.IsNullOrEmpty(loopSkillInputField.inputString)) {
            Hintbox.OpenHintboxWithContent("循环技能不能为空", 16);
            return false;
        }

        int superSkillId = 0;
        if ((!string.IsNullOrEmpty(superSkillInputField.inputString)) && (!int.TryParse(superSkillInputField.inputString, out superSkillId))) {
            Hintbox.OpenHintboxWithContent("必杀错误", 16);
            return false;
        }        

        bossInfo = new BossInfo(){
            petId = id,
            level = level,
            hasEmblem = emblemToggle.isOn,
            status = status,
            initBuffIds = initBuffInputField.inputString ?? string.Empty,
            headerSkillIds = headerSkillInputField.inputString ?? string.Empty,
            loopSkillIds = loopSkillInputField.inputString ?? string.Empty,
            normalSkillIds = string.Empty,
            superSkillId = superSkillId,
        };

        return true;
    }

    public void TestBattle(Pet[] pets) {
        if (!TryGetBossInfo(out var bossInfo))
            return;

        Map.TestBattle(bossInfo, pets);
    }
}
