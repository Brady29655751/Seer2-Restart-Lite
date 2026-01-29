using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAudioView : BattleBaseView
{
    [SerializeField] private AudioClip endLoadingSound;
    [SerializeField] private AudioClip[] battleBGM;
    private Dictionary<string, string> options = new Dictionary<string, string>();

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
        var path = "BGM/fight/BGM_" + GetWorldId(battle.settings.mode) + GetBattleBGMId(battle.settings.mode) + ".mp3";
        ResourceManager.instance.GetLocalAddressables<AudioClip>(path, battle.settings.isMod,
            (bgm) => AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BattleBGM));
    }

    private string GetWorldId(BattleMode mode) {
        var worldId = mode switch {
            BattleMode.YiTeRogue    => 0,
            BattleMode.Card         => 1,
            _   =>  Player.instance.currentMap.worldId,
        };
        
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
            BattleMode.Card => 4,
            _ => (int)mode,
        };
    }

    public void SetState(BattleState lastState, BattleState currentState)
    {
        if (currentState == null)
            return;

        var path = currentState.options.Get("bgm_path");
        var mod = currentState.options.Get("bgm_mod");

        if (path == options.Get("bgm_path") && (mod == options.Get("bgm_mod")))
            return;

        ResourceManager.instance.GetLocalAddressables<AudioClip>(path, bool.Parse(mod), (clip) =>
        {
            options.Set("bgm_path", path);
            options.Set("bgm_mod", mod);
            AudioSystem.instance.PlayMusic(clip, AudioVolumeType.BattleBGM);
        });
    }
}
