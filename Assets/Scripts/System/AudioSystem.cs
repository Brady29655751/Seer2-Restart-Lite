using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : Singleton<AudioSystem>
{
    private SettingsData settingsData => Player.instance.gameData.settingsData;
    [SerializeField] private AudioSource musicSource = null;
    [SerializeField] private AudioSource soundSource = null;
    [SerializeField] private AudioSource effectSource = null;
    
    public float GetVolume(AudioVolumeType volumeType) {
        float volume = volumeType switch {
            AudioVolumeType.BGM => settingsData.BGMVolume,
            AudioVolumeType.UI => settingsData.UIVolume,
            AudioVolumeType.BattleBGM => settingsData.battleBGMVolume,
            AudioVolumeType.BattleSE => settingsData.battleSEVolume,
            _ => 10f,
        };
        return volume / 10f;
    }

    public void PlayMusic(AudioClip clip, AudioVolumeType volumeType = AudioVolumeType.BGM) {
        if (clip == null) {
            musicSource.Stop();
            return;
        }
        var volume = GetVolume(volumeType);
        if ((clip == musicSource.clip) && (volume == musicSource.volume))
            return;
    
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic() {
        if ((musicSource == null) || (musicSource.clip == null))
            return;

        musicSource.Stop();
    }

    public void PlaySound(AudioClip clip, AudioVolumeType volumeType = AudioVolumeType.UI) {
        if (clip == null)
            return;
        
        soundSource.clip = clip;
        soundSource.volume = GetVolume(volumeType);
        soundSource.PlayOneShot(clip);
    }

    public void PlayEffect(AudioClip clip, AudioVolumeType volumeType = AudioVolumeType.BGM) {
        if (clip == null) {
            StopEffect();
            return;
        }

        effectSource.clip = clip;
        effectSource.volume = GetVolume(volumeType);
        effectSource.Play();
    }

    public void StopEffect() {
        if ((effectSource == null) || (effectSource.clip == null))
            return;

        effectSource.Stop();
    }
}

public enum AudioVolumeType {
    BGM, UI, BattleBGM, BattleSE,
}
