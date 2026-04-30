// Assets/Scripts/Core/AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("BGM 클립")]
    [SerializeField] private AudioClip _menuBGM;
    [SerializeField] private AudioClip _gameBGM;

    [Header("SFX 클립")]
    [SerializeField] private AudioClip _stoneSFX;
    [SerializeField] private AudioClip _winSFX;
    [SerializeField] private AudioClip _loseSFX;
    [SerializeField] private AudioClip _forbiddenSFX;
    [SerializeField] private AudioClip _buttonSFX;

    // ── 상태 ────────────────────────────────────
    public float BGMVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;
    public bool BGMMuted { get; private set; } = false;
    public bool SFXMuted { get; private set; } = false;

    private const string KEY_BGM_VOL = "BGMVolume";
    private const string KEY_SFX_VOL = "SFXVolume";
    private const string KEY_BGM_MUTE = "BGMMuted";
    private const string KEY_SFX_MUTE = "SFXMuted";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadSettings();
        Apply();
    }

    // ── BGM ─────────────────────────────────────
    public void PlayMenuBGM()
    {
        if (_menuBGM == null) return;
        if (_bgmSource.clip == _menuBGM && _bgmSource.isPlaying) return;
        _bgmSource.clip = _menuBGM;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void PlayGameBGM()
    {
        if (_gameBGM == null) return;
        if (_bgmSource.clip == _gameBGM && _bgmSource.isPlaying) return;
        _bgmSource.clip = _gameBGM;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void StopBGM() => _bgmSource.Stop();

    // ── SFX ─────────────────────────────────────
    public void PlayStone() => PlaySFX(_stoneSFX);
    public void PlayWin() => PlaySFX(_winSFX);
    public void PlayLose() => PlaySFX(_loseSFX);
    public void PlayForbidden() => PlaySFX(_forbiddenSFX);
    public void PlayButton() => PlaySFX(_buttonSFX);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || SFXMuted) return;
        _sfxSource.PlayOneShot(clip, SFXVolume);
    }

    // ── 볼륨 설정 ────────────────────────────────
    public void SetBGMVolume(float vol)
    {
        BGMVolume = Mathf.Clamp01(vol);
        _bgmSource.volume = BGMMuted ? 0f : BGMVolume;
        PlayerPrefs.SetFloat(KEY_BGM_VOL, BGMVolume);
    }

    public void SetSFXVolume(float vol)
    {
        SFXVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat(KEY_SFX_VOL, SFXVolume);
    }

    public void ToggleBGMMute()
    {
        BGMMuted = !BGMMuted;
        _bgmSource.volume = BGMMuted ? 0f : BGMVolume;
        PlayerPrefs.SetInt(KEY_BGM_MUTE, BGMMuted ? 1 : 0);
    }

    public void ToggleSFXMute()
    {
        SFXMuted = !SFXMuted;
        PlayerPrefs.SetInt(KEY_SFX_MUTE, SFXMuted ? 1 : 0);
    }

    // ── 저장 / 불러오기 ──────────────────────────
    private void LoadSettings()
    {
        BGMVolume = PlayerPrefs.GetFloat(KEY_BGM_VOL, 1f);
        SFXVolume = PlayerPrefs.GetFloat(KEY_SFX_VOL, 1f);
        BGMMuted = PlayerPrefs.GetInt(KEY_BGM_MUTE, 0) == 1;
        SFXMuted = PlayerPrefs.GetInt(KEY_SFX_MUTE, 0) == 1;
    }

    private void Apply()
    {
        _bgmSource.volume = BGMMuted ? 0f : BGMVolume;
    }

    private void OnApplicationQuit() => PlayerPrefs.Save();
}