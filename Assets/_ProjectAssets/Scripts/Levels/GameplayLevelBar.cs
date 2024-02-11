using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayLevelBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelDisplay;
    [SerializeField] private Image levelBar;
    
    private void OnEnable()
    {
        PlayerData.OnUpdatedExp += Show;
        Show();
    }

    private void OnDisable()
    {
        PlayerData.OnUpdatedExp -= Show;
    }

    private void Show()
    {
        levelDisplay.text = DataManager.Instance.PlayerData.Level.ToString();
        levelBar.fillAmount = (float)DataManager.Instance.PlayerData.ExperienceOnCurrentLevel / DataManager.Instance.PlayerData.ExperienceForNextLevel;
    }
}
