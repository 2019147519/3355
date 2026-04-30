// Assets/Scripts/UI/SettingsUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private Button _bgmMuteBtn;
    [SerializeField] private Image _bgmMuteIcon;   // 스피커 아이콘 Image
    [SerializeField] private Sprite _bgmOnSprite;   // 🔊
    [SerializeField] private Sprite _bgmOffSprite;  // 🔇
    [SerializeField] private Slider _bgmSlider;

    [Header("SFX")]
    [SerializeField] private Button _sfxMuteBtn;
    [SerializeField] private Image _sfxMuteIcon;
    [SerializeField] private Sprite _sfxOnSprite;
    [SerializeField] private Sprite _sfxOffSprite;
    [SerializeField] private Slider _sfxSlider;

    [Header("닫기")]
    [SerializeField] private Button _closeBtn;

    private void OnEnable()
    {
        // 저장된 값 반영
        RefreshAll();

        _bgmMuteBtn.onClick.AddListener(OnBGMMute);
        _sfxMuteBtn.onClick.AddListener(OnSFXMute);
        _closeBtn.onClick.AddListener(OnClose);

        _bgmSlider.onValueChanged.AddListener(OnBGMSlider);
        _sfxSlider.onValueChanged.AddListener(OnSFXSlider);
    }

    private void OnDisable()
    {
        _bgmMuteBtn.onClick.RemoveAllListeners();
        _sfxMuteBtn.onClick.RemoveAllListeners();
        _closeBtn.onClick.RemoveAllListeners();

        _bgmSlider.onValueChanged.RemoveAllListeners();
        _sfxSlider.onValueChanged.RemoveAllListeners();
    }

    // ── 전체 갱신 ────────────────────────────────
    private void RefreshAll()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        _bgmSlider.SetValueWithoutNotify(am.BGMVolume);
        _sfxSlider.SetValueWithoutNotify(am.SFXVolume);

        RefreshBGMIcon();
        RefreshSFXIcon();
    }

    private void RefreshBGMIcon()
    {
        bool muted = AudioManager.Instance.BGMMuted;
        _bgmMuteIcon.sprite = muted ? _bgmOffSprite : _bgmOnSprite;

        // 슬라이더 투명도 — 음소거 시 흐리게
        var c = _bgmSlider.GetComponent<CanvasGroup>();
        if (c) c.alpha = muted ? 0.4f : 1f;
    }

    private void RefreshSFXIcon()
    {
        bool muted = AudioManager.Instance.SFXMuted;
        _sfxMuteIcon.sprite = muted ? _sfxOffSprite : _sfxOnSprite;

        var c = _sfxSlider.GetComponent<CanvasGroup>();
        if (c) c.alpha = muted ? 0.4f : 1f;
    }

    // ── 핸들러 ───────────────────────────────────
    private void OnBGMMute()
    {
        AudioManager.Instance.ToggleBGMMute();
        AudioManager.Instance.PlayButton();
        RefreshBGMIcon();
    }

    private void OnSFXMute()
    {
        AudioManager.Instance.ToggleSFXMute();
        AudioManager.Instance.PlayButton();
        RefreshSFXIcon();
    }

    private void OnBGMSlider(float val)
    {
        AudioManager.Instance.SetBGMVolume(val);
    }

    private void OnSFXSlider(float val)
    {
        AudioManager.Instance.SetSFXVolume(val);
        // SFX 슬라이더 조절 시 미리듣기
        AudioManager.Instance.PlayButton();
    }

    private void OnClose()
    {
        AudioManager.Instance.PlayButton();
        gameObject.SetActive(false);
    }
}