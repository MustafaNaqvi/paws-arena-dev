using UnityEngine;
using UnityEngine.UI;

public class ShowChallenges : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private ChallengesPanel challengesPanel;
    
    private void OnEnable()
    {
        button.onClick.AddListener(Show);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Show);
    }

    private void Show()
    {
        challengesPanel.Setup();
    }

    private void Update()
    {
        button.interactable = DataManager.Instance.PlayerData.ChallengeProgresses.Count > 0;
    }
}
