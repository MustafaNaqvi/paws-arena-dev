using UnityEngine;

public class SettingsScreen : MonoBehaviour
{
    public CustomSlider masterVolume;
    public CustomSlider musicVolume;
    public CustomSlider sfxVolume;

    private void OnEnable()
    {
        ShowSettings(GameState.gameSettings);
    }

    public void Apply()
    {
        GameState.gameSettings.masterVolume = masterVolume.GetValue();
        GameState.gameSettings.musicVolume = musicVolume.GetValue();
        GameState.gameSettings.soundFXVolume = sfxVolume.GetValue();
        GameState.gameSettings.Apply();
        gameObject.SetActive(false);
    }
    
    private void ShowSettings(GameSettings gameSettings)
    {
        masterVolume.SetValue(gameSettings.masterVolume);
        musicVolume.SetValue(gameSettings.musicVolume);
        sfxVolume.SetValue(gameSettings.soundFXVolume);
    }
}
