using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeDisplay : MonoBehaviour
{
    public static Action<ChallengeProgress> OnClaimPressed;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject claimHolder;
    [SerializeField] private Image rewardDisplay;
    [SerializeField] private TextMeshProUGUI descDisplay;
    [SerializeField] private TextMeshProUGUI progressDisplay;
    [SerializeField] private GameObject completed;

    private ChallengeProgress challengeProgress;
    
    public void Setup(ChallengeProgress _progress)
    {
        completed.SetActive(false);
        claimHolder.SetActive(false);
        challengeProgress = _progress;
        ChallengeData _challengeData = DataManager.Instance.GameData.GetChallengeByIdentifier(_progress.Identifier);
        if (_progress.Completed)
        {
            if (_progress.Claimed || _progress.IsClaiming)
            {
                completed.SetActive(true);
            }
            else
            {
                claimHolder.SetActive(true);
            }
            descDisplay.text = string.Empty;
            progressDisplay.text = string.Empty;
        }
        else
        {
            descDisplay.text = _challengeData.Description;
            progressDisplay.text = $"{_progress.Value}/{_challengeData.AmountNeeded}";
        }

        rewardDisplay.sprite = AssetsManager.Instance.GetItemSprite(_challengeData.RewardType);
    }

    private void OnEnable()
    {
        claimButton.onClick.AddListener(Claim);
    }

    private void OnDisable()
    {
        claimButton.onClick.RemoveListener(Claim);
    }

    private void Claim()
    {
        OnClaimPressed?.Invoke(challengeProgress);
        Setup(challengeProgress);
    }
}
