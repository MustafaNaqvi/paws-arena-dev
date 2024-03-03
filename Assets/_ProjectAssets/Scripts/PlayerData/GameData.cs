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
    
    public int GlassOfMilkPrice => BoomDaoUtility.Instance.GetConfigDataAsInt(GLASS_MILK_PRICE, PRICE_TAG);

    public int JugOfMilkPrice => BoomDaoUtility.Instance.GetConfigDataAsInt(BOTTLE_MILK_PRICE, PRICE_TAG);
    
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

}
