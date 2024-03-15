using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengesPanel : MonoBehaviour
{
    public static  Action OnClosed;
    [SerializeField] private ChallengeDisplay[] challengeDisplays;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI progressDisplay;
    [SerializeField] private TextMeshProUGUI timerDisplay;
    [SerializeField] private LuckyWheelUI luckyWheel;

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
        ChallengesManager.OnChallengeClaimed += Setup;
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        ChallengesManager.OnChallengeClaimed -= Setup;
    }

    private void Close()
    {
        OnClosed?.Invoke();
        gameObject.SetActive(false);
    }

    public void Setup()
    {
        int _completedChallenges = 0;
        for (int _i = 0; _i < DataManager.Instance.PlayerData.ChallengeProgresses.Count; _i++)
        {
            ChallengeProgress _challengeProgress = DataManager.Instance.PlayerData.ChallengeProgresses[_i];
            challengeDisplays[_i].Setup(_challengeProgress);
            if (_challengeProgress.Claimed)
            {
                _completedChallenges++;
            }
        }

        int _totalAmountOfChallenges = DataManager.Instance.GameData.DailyChallenges.Challenges.Count;
        
        progressDisplay.text = $"{_completedChallenges}/{_totalAmountOfChallenges} Completed";
        gameObject.SetActive(true);
        StartCoroutine(ShowTimer());
        if (_completedChallenges==_totalAmountOfChallenges&& !DataManager.Instance.PlayerData.HasClaimedChallengeSpin)
        {
            luckyWheel.RequestRewardChallenges();
            luckyWheel.ShowReward();
        }
    }
    
    private IEnumerator ShowTimer()
    {
        while (gameObject.activeSelf)
        {
            TimeSpan _timeLeft = DataManager.Instance.GameData.DailyChallenges.NextReset - DateTime.UtcNow;
            string _output = string.Empty;

            if (_timeLeft.TotalSeconds<0)
            {
                _output = "Finished";
            }
            else
            {
                _output += _timeLeft.Hours < 10 ? "0" + _timeLeft.Hours : _timeLeft.Hours;
                _output += "h ";
                _output += _timeLeft.Minutes < 10 ? "0" + _timeLeft.Minutes : _timeLeft.Minutes;
                _output += "m ";
                _output += _timeLeft.Seconds < 10 ? "0" + _timeLeft.Seconds : _timeLeft.Seconds;
                _output += "s ";
                _output += "remaining";
            }
            
            timerDisplay.text = _output;
            yield return new WaitForSeconds(1);
        }
    }
}
