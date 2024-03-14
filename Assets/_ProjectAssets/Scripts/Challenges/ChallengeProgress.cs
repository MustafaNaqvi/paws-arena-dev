using System;
using System.Collections.Generic;
using BoomDaoWrapper;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ChallengeProgress
{
    private const string UPDATE_CHALLENGE_PROGRESS = "increaseChallengeProgress";
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
            Debug.Log(Identifier);
            ChallengeData _challengeData = DataManager.Instance.GameData.GetChallengeByIdentifier(Identifier);
            Debug.Log(JsonConvert.SerializeObject(DataManager.Instance.GameData.DailyChallenges));
            Debug.Log(DataManager.Instance.GameData.DailyChallenges.Challenges.Count);
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
        BoomDaoUtility.Instance.ExecuteActionWithParameter(UPDATE_CHALLENGE_PROGRESS, _parameters, null);
    }

    public void Reset()
    {
        Value = 0;
        UpdatedProgress?.Invoke(Identifier);
    }
}