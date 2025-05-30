using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAudioView : BattleBaseView
{
    [SerializeField] private AudioClip endLoadingSound;
    [SerializeField] private AudioClip[] battleBGM;

    public override void Init()
    {
        StartCoroutine(PlayBattleBGMRoutine());
    }

    private IEnumerator PlayBattleBGMRoutine() {
        AudioSystem.instance.PlaySound(endLoadingSound, AudioVolumeType.BattleSE);
        
        yield return new WaitForSeconds(1.5f);

        /*
        AudioClip bgm = null;

        if (!battle.settings.isMod) {
            bgm = battleBGM[GetBattleBGMId(battle.settings.mode)];
            AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BattleBGM);
            yield break;
        }
        */

        ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/fight/BGM_" + GetWorldId() + GetBattleBGMId(battle.settings.mode) + ".mp3", battle.settings.isMod,
            (bgm) => AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BattleBGM));
    }

    private string GetWorldId() {
        var worldId = Player.instance.currentMap.worldId;
        return (worldId == 0) ? string.Empty : (worldId + "_");
    }

    private int GetBattleBGMId(BattleMode mode) {
        return mode switch {
            BattleMode.Record => 2,
            BattleMode.Normal => 0,
            BattleMode.SPT => 1,
            BattleMode.SelfSimulation => 2,
            BattleMode.PVP => 2,
            BattleMode.Special => 3,
            BattleMode.YiTeRogue => 4,
            _ => (int)mode,
        };
    }
}
