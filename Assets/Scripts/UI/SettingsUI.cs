// Assets/Scripts/UI/SettingsUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsUI : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private Button _bgmMuteBtn;
    [SerializeField] private Slider _bgmSlider;

    [Header("SFX")]
    [SerializeField] private Button _sfxMuteBtn;
    [SerializeField] private Slider _sfxSlider;

    [Header("닫기")]
    [SerializeField] private Button _closeBtn;

    private static readonly Color COLOR_ACTIVE = new Color(0.2f, 0.6f, 1f);
    private static readonly Color COLOR_MUTED = new Color(0.35f, 0.35f, 0.35f);
    private static readonly Color COLOR_SLIDER_ON = new Color(1f, 1f, 1f, 1f);
    private static readonly Color COLOR_SLIDER_OFF = new Color(1f, 1f, 1f, 0.35f);

    private Image _bgmBtnImage;
    private Image _sfxBtnImage;

    private void Awake()
    {
        _bgmBtnImage = _bgmMuteBtn.GetComponent<Image>();
        _sfxBtnImage = _sfxMuteBtn.GetComponent<Image>();

        // ★ 슬라이더에 포인터업 이벤트 추가
        AddPointerUpSound(_bgmSlider);
        AddPointerUpSound(_sfxSlider);
    }

    // ── 포인터업 이벤트 등록 ─────────────────────
    private void AddPointerUpSound(Slider slider)
    {
        // EventTrigger 컴포넌트 추가 (없으면 자동 생성)
        var trigger = slider.gameObject.GetComponent<EventTrigger>()
                   ?? slider.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entry.callback.AddListener(_ => AudioManager.Instance?.PlayButton());
        trigger.triggers.Add(entry);
    }

    private void OnEnable()
    {
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

    private void RefreshAll()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        _bgmSlider.SetValueWithoutNotify(am.BGMVolume);
        _sfxSlider.SetValueWithoutNotify(am.SFXVolume);

        RefreshBGM();
        RefreshSFX();
    }

    private void RefreshBGM()
    {
        bool muted = AudioManager.Instance.BGMMuted;
        _bgmBtnImage.color = muted ? COLOR_MUTED : COLOR_ACTIVE;
        SetSliderAlpha(_bgmSlider, muted);
    }

    private void RefreshSFX()
    {
        bool muted = AudioManager.Instance.SFXMuted;
        _sfxBtnImage.color = muted ? COLOR_MUTED : COLOR_ACTIVE;
        SetSliderAlpha(_sfxSlider, muted);
    }

    private void SetSliderAlpha(Slider slider, bool muted)
    {
        var fill = slider.fillRect?.GetComponent<Image>();
        var handle = slider.handleRect?.GetComponent<Image>();
        var bg = slider.GetComponentInChildren<Image>();

        if (fill) fill.color = muted ? COLOR_SLIDER_OFF : COLOR_SLIDER_ON;
        if (handle) handle.color = muted ? COLOR_SLIDER_OFF : COLOR_SLIDER_ON;
        if (bg) { var c = bg.color; c.a = muted ? 0.35f : 1f; bg.color = c; }
    }

    private void OnBGMMute()
    {
        AudioManager.Instance.ToggleBGMMute();
        RefreshBGM();
    }

    private void OnSFXMute()
    {
        AudioManager.Instance.ToggleSFXMute();
        RefreshSFX();
    }

    private void OnBGMSlider(float val)
        => AudioManager.Instance.SetBGMVolume(val);

    private void OnSFXSlider(float val)
        => AudioManager.Instance.SetSFXVolume(val); // ★ PlayButton() 없음

    private void OnClose()
        => gameObject.SetActive(false);
}