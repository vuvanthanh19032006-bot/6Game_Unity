using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip bgmClip;
    public AudioClip menuBgmClip;
    public AudioClip gameBgmClip;
    public AudioClip lineClearClip;
    public AudioClip tSpinClip;
    public AudioClip tetrisClip;
    public AudioClip dropClip;
    public AudioClip lockClip;
    public AudioClip rotateClip;
    public AudioClip moveClip;
    public AudioClip holdClip;
    public AudioClip gameOverClip;
    [Header("UI Clips")]
    public AudioClip uiClickClip;

    [Header("Volumes")]
    [Range(0f,1f)] public float bgmVolume = 0.6f;
    [Range(0f,1f)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }

    Coroutine bgmFade;

    public void PlayBGM(bool loop = true)
    {
        if (bgmClip == null || bgmSource == null) return;
        // instant play
        if (bgmFade != null) StopCoroutine(bgmFade);
        bgmSource.clip = bgmClip;
        bgmSource.loop = loop;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        if (bgmFade != null) StopCoroutine(bgmFade);
        bgmSource.Stop();
    }

    public void PlayMenuMusic(float fade = 0.4f)
    {
        if (menuBgmClip == null || bgmSource == null) return;
        CrossfadeTo(menuBgmClip, true, fade);
    }

    public void PlayGameMusic(float fade = 0.4f)
    {
        if (gameBgmClip == null || bgmSource == null) return;
        CrossfadeTo(gameBgmClip, true, fade);
    }

    public void PlayUISound()
    {
        PlayOneShot(uiClickClip, 1f);
    }

    void CrossfadeTo(AudioClip clip, bool loop, float duration)
    {
        if (bgmSource == null) return;
        if (bgmFade != null) StopCoroutine(bgmFade);
        bgmFade = StartCoroutine(CrossfadeRoutine(clip, loop, Mathf.Max(0.01f, duration)));
    }

    System.Collections.IEnumerator CrossfadeRoutine(AudioClip clip, bool loop, float duration)
    {
        float half = duration * 0.5f;
        float t = 0f;

        float startVol = bgmSource.isPlaying ? bgmSource.volume : 0f;
        // fade out
        while (t < half)
        {
            t += Time.deltaTime;
            float f = t / half;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, f);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float f = t / half;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, f);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
        bgmFade = null;
    }

    public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.volume = Mathf.Clamp01(sfxVolume * volumeScale);
        sfxSource.PlayOneShot(clip, sfxSource.volume);
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
    }
}
