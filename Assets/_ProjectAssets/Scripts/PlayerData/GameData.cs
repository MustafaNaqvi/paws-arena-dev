using System;
using System.Collections.Generic;
using BoomDaoWrapper;

[Serializable]
public class GameData
{
    private int levelBaseExp;
    private int levelBaseScaler;
    private int respinPrice;
    private List<LevelReward> seasonRewards = new ();
    private int guildPrice;
    private int guildMaxPlayers;
    private Dictionary<string, GuildData> guilds = new ();

    public int LevelBaseExp
    {
        get
        {
            return levelBaseExp;
        }
        set
        {
            levelBaseExp = value;
        }
    }

    public int LevelBaseScaler
    {
        get
        {
            return levelBaseScaler;
        }
        set
        {
            levelBaseScaler = value;
        }
    }

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

    public List<LevelReward> SeasonRewards
    {
        get => seasonRewards;
        set => seasonRewards = value;
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

    public int SeasonNumber => BoomDaoUtility.Instance.GetConfigDataAsInt(SEASON_KEY, SEASON_NUMBER);
    public DateTime SeasonStarts => BoomDaoUtility.Instance.GetConfigDataAsDate(SEASON_KEY, SEASON_START);
    public DateTime SeasonEnds => BoomDaoUtility.Instance.GetConfigDataAsDate(SEASON_KEY, SEASON_END);
    public bool HasSeasonEnded => DateTime.UtcNow > SeasonEnds;
    public bool HasSeasonStarted => DateTime.UtcNow > SeasonStarts;
    public bool IsSeasonActive => HasSeasonStarted && !HasSeasonEnded;
    
    public int GlassOfMilkPrice => BoomDaoUtility.Instance.GetConfigDataAsInt(GLASS_MILK_PRICE, PRICE_TAG);

    public int JugOfMilkPrice => BoomDaoUtility.Instance.GetConfigDataAsInt(BOTTLE_MILK_PRICE, PRICE_TAG);
}
