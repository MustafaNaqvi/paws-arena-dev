using System;
using System.Collections.Generic;
using BoomDaoWrapper;

[Serializable]
public class ChallengeProgress
{
    private const string CHALLENGE_UPDATE_KEY = "challangeId";
    public static Action<string> UpdatedProgress;
    public string Identifier;
    public int Value;
    public bool Claimed;
    public bool IsClaiming; //used locally
    
    public bool Completed
    {
        get
        {
            ChallengeData _challengeData = DataManager.Instance.GameData.GetChallengeByIdentifier(Identifier);
            return _challengeData.AmountNeeded - Value <= 0;
        }
    }

    public void IncreaseAmount()
    {
        if (Completed)
        {
            return;
        }

        Value++;
        IncreaseChallengeProgress(1);
        UpdatedProgress?.Invoke(Identifier);
    }

    public void IncreaseAmount(int _amount)
    {
        if (Completed)
        {
            return;
        }
        
        Value+=_amount;
        IncreaseChallengeProgress(_amount);
        UpdatedProgress?.Invoke(Identifier);
    }

    private void IncreaseChallengeProgress(int _amount)
    {        
        List<ActionParameter> _parameters = new List<ActionParameter>
        {
            new () { Key = CHALLENGE_UPDATE_KEY, Value = PlayerData.DAILY_CHALLENGE_PROGRESS+DataManager.Instance.GameData.GetChallengeIndex(Identifier) },
            new () { Key = BoomDaoUtility.AMOUNT_KEY, Value = _amount.ToString() }
        };

        ChallengeUpdateProgress _progress = new ChallengeUpdateProgress { Parameters = _parameters };
        ChallengesManager.Instance.IncreaseProgress(_progress);
    }

    public void Reset()
    {
        Value = 0;
        UpdatedProgress?.Invoke(Identifier);
    }
}