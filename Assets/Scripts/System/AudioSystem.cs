using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class AudioSystem : Singleton<AudioSystem>
{
    private sealed class CachedAudioContentIdentity
    {
        public long length;
        public long writeTicks;
        public string identity;
    }

    private static readonly Dictionary<string, CachedAudioContentIdentity> AudioContentIdentityCache
        = new Dictionary<string, CachedAudioContentIdentity>(System.StringComparer.OrdinalIgnoreCase);

    public sealed class MusicPlaybackSnapshot
    {
        internal AudioClip clip;
        internal float time;
        internal float volume;
        internal bool isPlaying;
        internal string identity;
    }

    private SettingsData settingsData => Player.instance.gameData.settingsData;
    [SerializeField] private AudioSource musicSource = null;
    [SerializeField] private AudioSource soundSource = null;
    [SerializeField] private AudioSource effectSource = null;
    private string currentMusicIdentity;

    public string CurrentMusicIdentity => currentMusicIdentity;
    
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

    // BGM
    public void PlayMusic(AudioClip clip, AudioVolumeType volumeType = AudioVolumeType.BGM) {
        PlayMusicTracked(clip, volumeType, null);
    }

    public void PlayMusic(AudioClip clip, AudioVolumeType volumeType, string identity) {
        PlayMusicTracked(clip, volumeType, identity);
    }

    public bool PlayMusicTracked(AudioClip clip, AudioVolumeType volumeType = AudioVolumeType.BGM, string identity = null) {
        if (clip == null) {
            bool changed = musicSource.clip != null || musicSource.isPlaying;
            musicSource.Stop();
            currentMusicIdentity = null;
            return changed;
        }
        var volume = GetVolume(volumeType);
        string resolvedIdentity = string.IsNullOrWhiteSpace(identity) ? BuildClipMusicIdentity(clip) : identity;
        if ((clip == musicSource.clip) && (volume == musicSource.volume) && musicSource.isPlaying) {
            currentMusicIdentity = resolvedIdentity;
            return false;
        }
    
        musicSource.clip = clip;
        musicSource.volume = volume;
        currentMusicIdentity = resolvedIdentity;
        musicSource.Play();
        return true;
    }

    public MusicPlaybackSnapshot CaptureMusic()
    {
        if (musicSource == null)
            return null;

        return new MusicPlaybackSnapshot
        {
            clip = musicSource.clip,
            time = musicSource.clip == null ? 0f : musicSource.time,
            volume = musicSource.volume,
            isPlaying = musicSource.isPlaying,
            identity = currentMusicIdentity,
        };
    }

    public bool TryRestoreMusic(MusicPlaybackSnapshot snapshot, string expectedCurrentIdentity, bool restorePlayback = true)
    {
        if (snapshot == null || musicSource == null)
            return false;
        if (!string.IsNullOrWhiteSpace(expectedCurrentIdentity)
            && !string.Equals(currentMusicIdentity, expectedCurrentIdentity, System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!restorePlayback)
        {
            currentMusicIdentity = snapshot.identity;
            return true;
        }

        musicSource.Stop();
        musicSource.clip = snapshot.clip;
        musicSource.volume = snapshot.volume;
        currentMusicIdentity = snapshot.identity;
        if (snapshot.clip == null)
            return true;

        musicSource.time = Mathf.Clamp(snapshot.time, 0f, Mathf.Max(0f, snapshot.clip.length - .01f));
        if (snapshot.isPlaying)
            musicSource.Play();
        return true;
    }

    public static string BuildMapMusicIdentity(Map map)
    {
        if (map?.music != null && !string.IsNullOrWhiteSpace(map.music.bgm))
            return "map-bgm:" + map.music.category + "/" + map.music.bgm.Trim().ToLowerInvariant();
        return BuildClipMusicIdentity(map?.resources?.bgm);
    }

    public static string BuildResourceMusicIdentity(string source, string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
            return null;
        string normalizedSource = string.IsNullOrWhiteSpace(source) ? "auto" : source.Trim().ToLowerInvariant();
        string normalizedPath = resourcePath.Trim().Replace('\\', '/');
        if ((normalizedSource == "mod" || normalizedSource == "auto" || normalizedSource == "story")
            && TryBuildModAudioContentIdentity(normalizedPath, out string contentIdentity))
        {
            return contentIdentity;
        }
        return "resource:" + normalizedSource + ":" + normalizedPath.ToLowerInvariant();
    }

    private static bool TryBuildModAudioContentIdentity(string resourcePath, out string identity)
    {
        identity = null;
        if (string.IsNullOrWhiteSpace(resourcePath))
            return false;

        try
        {
            string normalizedPath = resourcePath.Replace('\\', '/').TrimStart('/');
            if (normalizedPath.StartsWith("Mod/", System.StringComparison.OrdinalIgnoreCase))
                normalizedPath = normalizedPath.Substring("Mod/".Length);
            string modRoot = Path.GetFullPath(Path.Combine(Application.persistentDataPath, "Mod"))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string physicalPath = Path.GetFullPath(Path.Combine(modRoot,
                normalizedPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!physicalPath.StartsWith(modRoot, System.StringComparison.OrdinalIgnoreCase))
                return false;
            if (!File.Exists(physicalPath) && File.Exists(physicalPath + ".mp3"))
                physicalPath += ".mp3";
            if (!File.Exists(physicalPath))
                return false;

            FileInfo info = new FileInfo(physicalPath);
            long writeTicks = info.LastWriteTimeUtc.Ticks;
            if (AudioContentIdentityCache.TryGetValue(physicalPath, out CachedAudioContentIdentity cached)
                && cached.length == info.Length && cached.writeTicks == writeTicks)
            {
                identity = cached.identity;
                return !string.IsNullOrWhiteSpace(identity);
            }

            using (FileStream stream = File.OpenRead(physicalPath))
            using (SHA256 sha256 = SHA256.Create())
            {
                string hash = System.BitConverter.ToString(sha256.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
                identity = "resource-content:sha256:" + hash;
            }
            AudioContentIdentityCache[physicalPath] = new CachedAudioContentIdentity
            {
                length = info.Length,
                writeTicks = writeTicks,
                identity = identity,
            };
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("无法计算外部 BGM 内容标识，将按路径识别：" + exception.Message);
            return false;
        }
    }

    private static string BuildClipMusicIdentity(AudioClip clip)
    {
        return clip == null ? null : "clip:" + (clip.name ?? string.Empty).Trim().ToLowerInvariant();
    }

    public void StopMusic() {
        if (musicSource == null)
            return;

        if (musicSource.clip != null || musicSource.isPlaying)
            musicSource.Stop();
        currentMusicIdentity = null;
    }

    // 一次性音效
    public void PlaySound(AudioClip clip, AudioVolumeType volumeType = AudioVolumeType.UI) {
        if (clip == null)
            return;
        
        soundSource.clip = clip;
        soundSource.volume = GetVolume(volumeType);
        soundSource.PlayOneShot(clip);
    }

    public void StopSound() {
        if ((soundSource == null) || (soundSource.clip == null))
            return;

        soundSource.Stop();
    }

    // 环境音效等特别音效
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
    None, BGM, UI, BattleBGM, BattleSE,
}
