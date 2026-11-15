using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// One options panel, used in the main menu and the pause menu.
public class OptionsMenu : MonoBehaviour
{
    // same names as the exposed mixer params, also used as PlayerPrefs keys
    private static readonly string[] VolumeKeys = { "MasterVol", "MusicVol", "SFXVol" };

    [SerializeField] private AudioMixer _mixer; // this scene's mixer

    [SerializeField] private GameObject _root; // what Close hides (the whole options canvas)

    [Header("UI")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    private readonly List<Resolution> _resolutions = new List<Resolution>();

    private void Awake()
    {
        SetupSlider(_masterSlider, VolumeKeys[0]);
        SetupSlider(_musicSlider, VolumeKeys[1]);
        SetupSlider(_sfxSlider, VolumeKeys[2]);

        _fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        _fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        BuildResolutionList();
        _resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetupSlider(Slider slider, string key)
    {
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, 1f));
        slider.onValueChanged.AddListener(value => SetVolume(key, value));
    }

    private void SetVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        if (_mixer != null) _mixer.SetFloat(key, ToDecibels(value));
    }

    // slider 0-1 -> mixer -80 to 0 dB
    private static float ToDecibels(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
    }

    private void SetFullscreen(bool on)
    {
        Screen.fullScreenMode = on ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    private void BuildResolutionList()
    {
        _resolutionDropdown.ClearOptions();
        List<string> labels = new List<string>();
        int current = 0;

        foreach (Resolution r in Screen.resolutions)
        {
            // skip the same size at a different refresh rate
            if (_resolutions.Exists(x => x.width == r.width && x.height == r.height)) continue;

            if (r.width == Screen.width && r.height == Screen.height) current = _resolutions.Count;
            _resolutions.Add(r);
            labels.Add(r.width + " x " + r.height);
        }

        _resolutionDropdown.AddOptions(labels);
        _resolutionDropdown.SetValueWithoutNotify(current);
    }

    private void SetResolution(int index)
    {
        Resolution r = _resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
    }

public void Close()
{
    PlayerPrefs.Save();
    (_root != null ? _root : gameObject).SetActive(false);
}

    // call this from Start() in every scene so saved volumes apply on load
    public static void ApplySaved(AudioMixer mixer)
    {
        if (mixer == null) return;
        foreach (string key in VolumeKeys)
            mixer.SetFloat(key, ToDecibels(PlayerPrefs.GetFloat(key, 1f)));
    }
}