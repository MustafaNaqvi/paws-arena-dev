using System;
using System.Collections.Generic;
using BoomDaoWrapper;

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
    
    private const string SEASON_KEY = "season";
    private const string SEASON_NUMBER = "number";
    private const string SEASON_START = "startDate";
    private const string SEASON_END = "endDate";
    private const string GLASS_MILK_PRICE = "milkGlass";
    private const string BOTTLE_MILK_PRICE = "milkBottle";
    private const string PRICE_TAG = "price";
    private const string AMOUNT_OF_REWARDS = "amountOfRewards";
    private const string PREMIUM_REWARDS = "battlePassRewardPremium";
    private const string NORMAL_REWARDS = "battlePassRewardNormal";
    private const string TYPE = "type";
    private const string AMOUNT = "amount";

    private List<LevelReward> seasonRewards;

    public int SeasonNumber => BoomDaoUtility.Instance.GetConfigDataAsInt(SEASON_KEY, SEASON_NUMBER);
    public DateTime SeasonStarts => BoomDaoUtility.Instance.GetConfigDataAsDate(SEASON_KEY, SEASON_START);
    public DateTime SeasonEnds => BoomDaoUtility.Instance.GetConfigDataAsDate(SEASON_KEY, SEASON_END);
    public bool HasSeasonEnded => DateTime.UtcNow > SeasonEnds;
    public bool HasSeasonStarted => DateTime.UtcNow > SeasonStarts;
    // public bool IsSeasonActive => HasSeasonStarted && !HasSeasonEnded;
    public bool IsSeasonActive => true;
    
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
            TryAddReward(NORMAL_REWARDS+_i,_i,false);
            TryAddReward(PREMIUM_REWARDS+_i,_i,true);
        }

        void TryAddReward(string _id,int _level,bool _isPremium)
        {
            List<ConfigData> _configsData = BoomDaoUtility.Instance.GetConfigData(_id);
            if (_configsData==default)
            {
                return;
            }

            int _type = int.Parse(_configsData.Find(_configData => _configData.Name == TYPE).Value);
            int _amount = int.Parse(_configsData.Find(_configData => _configData.Name == AMOUNT).Value);
            LevelRewardType _rewardType = (LevelRewardType)_type;
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
}
