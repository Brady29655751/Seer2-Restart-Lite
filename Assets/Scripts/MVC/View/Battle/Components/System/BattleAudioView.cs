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

        AudioClip bgm = null;

        if (!battle.settings.isMod)
            bgm = battleBGM[GetBattleBGMId(battle.settings.mode)];
        else {
            bool isRequestDone = false;
            RequestManager.instance.DownloadAudioClip("file://" + Application.persistentDataPath + "/Mod/BGM/fight/BGM_" + GetBattleBGMId(battle.settings.mode) + ".mp3",
                (clip) => { bgm = clip; isRequestDone = true; }, (error) => isRequestDone = true);

            while (!isRequestDone)
                yield return null;
        }

        AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BattleBGM);
    }

    private int GetBattleBGMId(BattleMode mode) {
        return mode switch {
            BattleMode.SPT => 1,
            BattleMode.SelfSimulation => 2,
            BattleMode.PVP => 2,
            BattleMode.Special => 3,
            _ => 0,
        };
    }
}
