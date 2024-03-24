using System;
using System.Collections.Generic;
using BoomDaoWrapper;
using UnityEngine;

[Serializable]
public class GameData
{
    private int respinPrice;
    private int guildPrice;
    private int guildMaxPlayers;
    private Dictionary<string, GuildData> guilds = new ();

    public int RespinPrice
    {
        get
        {
            return respinPrice;
        }
        set
        {
            respinPrice = value;
        }
    }

    public int GuildPrice
    {
        get => guildPrice;
        set => guildPrice = value;
    }

    public Dictionary<string, GuildData> Guilds
    {
        get => guilds;
        set => guilds = value;
    }

    public int GuildMaxPlayers
    {
        get=> guildMaxPlayers;
        set => guildMaxPlayers=value;
    }

    public GuildRankingBorders RankingBorders;

    
    // new system
    
    public const string CLAIM_PREMIUM_REWARD = "battlePassPremium";
    public const string CLAIM_NORMAL_REWARD = "battlePassNormal";
    public const string LEADERBOARD_POINTS = "leaderboardPoints";
    public const string LEADERBOARD_KITTY_URL = "kittyUrl";
    public const string LEADERBOARD_NICK_NAME = "nickName";
    
    private const string SEASON_KEY = "season";
    private const string SEASON_NUMBER = "number";
    private const string SEASON_START = "startDate";
    private const string SEASON_END = "endDate";
    private const string GLASS_MILK_PRICE = "milkGlass";
    private const string BOTTLE_MILK_PRICE = "milkBottle";
    private const string PRICE_TAG = "price";
    private const string AMOUNT_OF_REWARDS = "amountOfRewards";
    private const string TYPE = "type";
    private const string AMOUNT = "amount";

    private List<LevelReward> seasonRewards;

    public int SeasonNumber => BoomDaoUtility.Instance.GetConfigDataAsInt(SEASON_KEY, SEASON_NUMBER);
    public DateTime SeasonStarts => BoomDaoUtility.Instance.GetConfigDataAsDate(SEASON_KEY, SEASON_START);
    public DateTime SeasonEnds => BoomDaoUtility.Instance.GetConfigDataAsDate(SEASON_KEY, SEASON_END);
    public bool HasSeasonEnded => DateTime.UtcNow > SeasonEnds;
    public bool HasSeasonStarted => DateTime.UtcNow > SeasonStarts;
    public bool IsSeasonActive => HasSeasonStarted && !HasSeasonEnded;
    
    public double GlassOfMilkPrice => BoomDaoUtility.Instance.GetConfigDataAsDouble(GLASS_MILK_PRICE, PRICE_TAG);

    public double JugOfMilkPrice => BoomDaoUtility.Instance.GetConfigDataAsDouble(BOTTLE_MILK_PRICE, PRICE_TAG);
    
    public List<LevelReward> SeasonRewards
    {
        get
        {
            if (seasonRewards==default)
            {
                FetchSeasonRewards();        
            }

            return seasonRewards;
        }
    }

    private void FetchSeasonRewards()
    {
        seasonRewards = new List<LevelReward>();
        int _amountOfRewards = BoomDaoUtility.Instance.GetConfigDataAsInt(SEASON_KEY, AMOUNT_OF_REWARDS);
        for (int _i = 1; _i <= _amountOfRewards; _i++)
        {
            TryAddReward(CLAIM_NORMAL_REWARD+_i,_i,false);
            TryAddReward(CLAIM_PREMIUM_REWARD+_i,_i,true);
        }

        void TryAddReward(string _id,int _level,bool _isPremium)
        {
            List<ActionOutcome> _actionOutcomes = BoomDaoUtility.Instance.GetActionOutcomes(_id);
            if (_actionOutcomes==default || _actionOutcomes.Count==0)
            {
                return;
            }

            ActionOutcome _outcome = _actionOutcomes[0];
            int _amount = Convert.ToInt32(_outcome.Value);
            ItemType _rewardType = Utilities.GetRewardType(_outcome.Name);
            LevelReward _levelReward = new LevelReward
            {
                Level = _level,
                Name = _rewardType.ToString(),
                Type = _rewardType,
                IsPremium = _isPremium,
                Parameter1 = _amount
            };
            
            seasonRewards.Add(_levelReward);
        }
    }

    public LeaderboardData GetLeaderboard
    {
        get
        {
            LeaderboardData _leaderboardData = new LeaderboardData();
            List<WorldDataEntry> _entries = BoomDaoUtility.Instance.GetWorldData(LEADERBOARD_POINTS,LEADERBOARD_NICK_NAME, LEADERBOARD_KITTY_URL);
            foreach (var _worldEntry in _entries)
            {
                string _pointsString = _worldEntry.GetProperty(LEADERBOARD_POINTS);
                int _points = Convert.ToInt32(_pointsString.Contains('.') ? _pointsString.Split('.')[0] : _pointsString);
                _leaderboardData.Entries.Add(new LeaderboardEntries
                {
                    PrincipalId = _worldEntry.PrincipalId,
                    Nickname = _worldEntry.GetProperty(LEADERBOARD_NICK_NAME),
                    Points = _points,
                    KittyUrl = _worldEntry.GetProperty(LEADERBOARD_KITTY_URL)
                });
            }

            _leaderboardData.FinishSetup(3);
            return _leaderboardData;
        }
    }

    public DailyChallenges DailyChallenges
    {
        get
        {
            DailyChallenges _dailyChallenges = new DailyChallenges();
            for (int _i = 0; _i < ChallengesManager.AMOUNT_OF_CHALLENGES; _i++)
            {
                ChallengeData _challenge = GetChallenge(_i);
                if (_challenge==default)
                {
                    continue;
                }
                _dailyChallenges.Challenges.Add(_challenge);
            }

            string _nextResetString = BoomDaoUtility.Instance.GetString(ChallengesManager.DAILY_CHALLENGES,ChallengesManager.NEXT_RESET);
            if (string.IsNullOrEmpty(_nextResetString))
            {
                return default;
            }
            ulong _nextResetLong = _nextResetString.Contains('.')
                ? Convert.ToUInt64(_nextResetString.Split('.')[0])
                : Convert.ToUInt64(_nextResetString);
            
            _dailyChallenges.NextReset = Utilities.NanosecondsToDateTime(_nextResetLong);
            return _dailyChallenges;
        }
    }

    public bool HasDailyChallenges => BoomDaoUtility.Instance.DoesEntityExist(ChallengesManager.DAILY_CHALLENGE + "0");
    
    private ChallengeData GetChallenge(int _index)
    {
        string _challengeId = ChallengesManager.DAILY_CHALLENGE + _index;
        if (!BoomDaoUtility.Instance.DoesEntityExist(_challengeId))
        {
            return default;
        }

        ChallengeData _challengeData = new ChallengeData();
        _challengeData.Id = BoomDaoUtility.Instance.GetInt(_challengeId, ChallengesManager.CHALLENGE_ID);
        _challengeData.Identifier = BoomDaoUtility.Instance.GetString(_challengeId, ChallengesManager.CHALLENGE_IDENTIFIER);
        _challengeData.Description = BoomDaoUtility.Instance.GetString(_challengeId, ChallengesManager.CHALLENGE_DESCRIPTION);
        _challengeData.AmountNeeded = BoomDaoUtility.Instance.GetInt(_challengeId, ChallengesManager.CHALLENGE_AMOUNT_NEEDED);
        _challengeData.RewardAmount = BoomDaoUtility.Instance.GetInt(_challengeId, ChallengesManager.CHALLENGE_REWARD_AMOUNT);
        _challengeData.RewardType = (ItemType)BoomDaoUtility.Instance.GetInt(_challengeId, ChallengesManager.CHALLENGE_REWARD_TYPE);
        _challengeData.Category = (ChallengeCategory)BoomDaoUtility.Instance.GetInt(_challengeId, ChallengesManager.CHALLENGE_CATEGORY);
        return _challengeData;
    }

    public ChallengeData GetChallengeByIdentifier(string _identifier)=>DataManager.Instance.GameData.DailyChallenges.Challenges.Find(_challenge => _challenge.Identifier == _identifier);

    public int GetChallengeIndex(ChallengeProgress _challengeProgress)
    {
        for (int _i = 0; _i < DailyChallenges.Challenges.Count; _i++)
        {
            ChallengeData _data = DailyChallenges.Challenges[_i];
            if (_data.Identifier==_challengeProgress.Identifier)
            {
                return _i;
            }
        }

        throw new Exception("Can't find index of challenge");
    }
    
    public int GetChallengeIndex(string _identifier)
    {
        for (int _i = 0; _i < DailyChallenges.Challenges.Count; _i++)
        {
            ChallengeData _data = DailyChallenges.Challenges[_i];
            if (_data.Identifier==_identifier)
            {
                return _i;
            }
        }

        throw new Exception("Can't find index of challenge");
    }
}
