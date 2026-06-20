using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] musicTracks;

    [Header("SFX")]
    [SerializeField] private bool blockSfxWhenPaused = true;

    public float MusicVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;
    public bool SfxPaused { get; private set; }

    private readonly List<TrackedSfxSource> trackedSfxSources = new List<TrackedSfxSource>();
    private int currentMusicIndex;

    private class TrackedSfxSource
    {
        public AudioSource Source;
        public float BaseVolume;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        SetupMusicSource();
        ApplyMusicVolume();
    }

    private void Start()
    {
        if (musicTracks != null && musicTracks.Length > 0)
            PlayMusicTrack(0);
    }

    private void Update()
    {
        CleanupDestroyedSfxSources();
        AdvanceMusicPlaylist();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        ApplySfxVolumes();
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
    }

    public void SetSfxPaused(bool paused)
    {
        SfxPaused = paused;

        for (int i = trackedSfxSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = trackedSfxSources[i].Source;
            if (source == null)
                continue;

            if (paused)
                source.Pause();
            else
                source.UnPause();
        }
    }

    public void RegisterSfxHierarchy(GameObject root)
    {
        if (root == null)
            return;

        AudioSource[] sources = root.GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource source in sources)
        {
            if (source == null || source == musicSource)
                continue;

            RegisterSfxSource(source);
        }
    }

    public void PlaySfx(AudioClip clip, Vector3 position)
    {
        if (clip == null || (blockSfxWhenPaused && SfxPaused))
            return;

        AudioSource.PlayClipAtPoint(clip, position, SfxVolume);
    }

    private void SetupMusicSource()
    {
        if (musicSource == null)
            return;

        musicSource.ignoreListenerPause = true;
        musicSource.loop = musicTracks != null && musicTracks.Length == 1;
    }

    private void PlayMusicTrack(int index)
    {
        if (musicSource == null || musicTracks == null || musicTracks.Length == 0)
            return;

        currentMusicIndex = Mathf.Clamp(index, 0, musicTracks.Length - 1);
        musicSource.clip = musicTracks[currentMusicIndex];
        musicSource.loop = musicTracks.Length == 1;
        musicSource.volume = MusicVolume;
        musicSource.Play();
    }

    private void AdvanceMusicPlaylist()
    {
        if (musicSource == null || musicTracks == null || musicTracks.Length <= 1)
            return;

        if (!musicSource.isPlaying)
        {
            int nextIndex = (currentMusicIndex + 1) % musicTracks.Length;
            PlayMusicTrack(nextIndex);
        }
    }

    private void RegisterSfxSource(AudioSource source)
    {
        foreach (TrackedSfxSource tracked in trackedSfxSources)
        {
            if (tracked.Source == source)
            {
                ApplySfxVolume(tracked);
                return;
            }
        }

        TrackedSfxSource entry = new TrackedSfxSource
        {
            Source = source,
            BaseVolume = source.volume
        };

        trackedSfxSources.Add(entry);
        ApplySfxVolume(entry);

        if (SfxPaused)
            source.Pause();
    }

    private void ApplySfxVolumes()
    {
        for (int i = trackedSfxSources.Count - 1; i >= 0; i--)
            ApplySfxVolume(trackedSfxSources[i]);
    }

    private void ApplySfxVolume(TrackedSfxSource tracked)
    {
        if (tracked.Source == null)
            return;

        tracked.Source.volume = tracked.BaseVolume * SfxVolume;
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = MusicVolume;
    }

    private void CleanupDestroyedSfxSources()
    {
        for (int i = trackedSfxSources.Count - 1; i >= 0; i--)
        {
            if (trackedSfxSources[i].Source == null)
                trackedSfxSources.RemoveAt(i);
        }
    }
}
