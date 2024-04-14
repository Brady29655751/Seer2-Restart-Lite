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

        AudioClip bgm = GetBattleBGM(battle.settings.mode);
        AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BattleBGM);

    }

    private AudioClip GetBattleBGM(BattleMode mode) {
        return mode switch {
            BattleMode.SPT => battleBGM[1],
            BattleMode.PVP => battleBGM[2],
            BattleMode.Special => battleBGM[3],
            _ => battleBGM[0],
        };
    }
}
