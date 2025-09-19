using UnityEngine;
using UnityEngine.UI;

public class SettingPanelController : PanelController
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    public void OnValueChanged(int type)
    {
        if (type == 0)
        {
            SoundManager.Instance.bgmAudioSource.volume = bgmSlider.value;
        }
        else
        {
            SoundManager.Instance.sfxAudioSource.volume = sfxSlider.value;
            SoundManager.Instance.PlaySFX("click");
        }
    }

    public void OnClickHideButton()
    {
        Hide();
    }
}
