using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BoomDaoWrapper;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public const string EARNED_XP_KEY = "earnedXp";
    private float snacks;
    private float jugOfMilk;
    private float glassOfMilk;
    private CraftingProcess craftingProcess;
    private bool hasPass;
    private List<ClaimedReward> claimedLevelRewards = new();
    private List<int> ownedEquiptables;
    private int seasonNumber;
    private List<int> ownedEmojis;
    private Challenges challenges = new();
    private string guildId = string.Empty;
    private int points;

    [JsonIgnore] public Action UpdatedSnacks;
    [JsonIgnore] public Action UpdatedJugOfMilk;
    [JsonIgnore] public Action UpdatedGlassOfMilk;
    [JsonIgnore] public Action UpdatedCraftingProcess;
    [JsonIgnore] public Action UpdatedClaimedLevels;
    [JsonIgnore] public Action UpdatedHasPass;
    [JsonIgnore] public Action UpdatedEquiptables;
    [JsonIgnore] public Action UpdatedSeasonNumber;
    [JsonIgnore] public Action UpdatedOwnedEmojis;
    [JsonIgnore] public Action UpdatedGuild;
    [JsonIgnore] public Action UpdatedPoints;

    public void SetStartingValues()
    {
        ownedEquiptables = new List<int>()
        {
            0,
            25,
            60,
            74,
            95
        };
        ownedEmojis = new List<int>()
        {
            0,
            1,
            2,
            3,
            4
        };
    }

    public float Snacks
    {
        get { return snacks; }
        set
        {
            snacks = value;
            UpdatedSnacks?.Invoke();
        }
    }

    public float JugOfMilk
    {
        get { return jugOfMilk; }
        set
        {
            jugOfMilk = value;
            UpdatedJugOfMilk?.Invoke();
        }
    }

    public float GlassOfMilk
    {
        get { return glassOfMilk; }
        set
        {
            glassOfMilk = value;
            UpdatedGlassOfMilk?.Invoke();
        }

    }

    public CraftingProcess CraftingProcess
    {
        get { return craftingProcess; }
        set
        {
            craftingProcess = value;
            UpdatedCraftingProcess?.Invoke();
        }
    }

    public void AddCollectedLevelReward(ClaimedReward _reward)
    {
        claimedLevelRewards.Add(_reward);
        UpdatedClaimedLevels?.Invoke();
    }

    public List<ClaimedReward> ClaimedLevelRewards
    {
        get { return claimedLevelRewards; }
        set { claimedLevelRewards = value; }
    }

    public bool HasPass
    {
        get { return hasPass; }
        set
        {
            hasPass = value;
            UpdatedHasPass?.Invoke();
        }
    }

    public bool HasClaimed(LevelReward _reward, int _level)
    {
        foreach (var _claimedReward in claimedLevelRewards)
        {
            if (_claimedReward.IsPremium == _reward.IsPremium && _claimedReward.Level == _level)
            {
                return true;
            }
        }

        return false;
    }

    public List<int> OwnedEquiptables
    {
        get { return ownedEquiptables; }
        set { ownedEquiptables = value; }
    }

    public void AddOwnedEquipment(int _id)
    {
        if (ownedEquiptables.Contains(_id))
        {
            return;
        }

        ownedEquiptables.Add(_id);
        UpdatedEquiptables?.Invoke();
    }

    public void RemoveOwnedEquipment(int _id)
    {
        if (!ownedEquiptables.Contains(_id))
        {
            return;
        }

        ownedEquiptables.Remove(_id);
        UpdatedEquiptables?.Invoke();
    }

    public int SeasonNumber
    {
        get => seasonNumber;
        set
        {
            seasonNumber = value;
            UpdatedSeasonNumber?.Invoke();
        }
    }

    public List<int> OwnedEmojis
    {
        get => ownedEmojis;
        set
        {
            ownedEmojis = value;
            ownedEmojis.Sort();
        }
    }

    public void AddOwnedEmoji(int _id)
    {
        if (ownedEmojis.Contains(_id))
        {
            return;
        }

        ownedEmojis.Add(_id);
        ownedEmojis.Sort();
        UpdatedOwnedEmojis?.Invoke();
    }

    public Challenges Challenges
    {
        get => challenges;
        set => challenges = value;
    }


    public string GuildId
    {
        get => guildId;
        set
        {
            guildId = value;
            UpdatedGuild?.Invoke();
        }
    }

    [JsonIgnore] public string PlayerId => FirebaseManager.Instance.PlayerId;
    [JsonIgnore] public bool IsInGuild => !string.IsNullOrEmpty(GuildId);

    [JsonIgnore]
    public GuildData Guild
    {
        get
        {
            if (!IsInGuild)
            {
                return null;
            }

            GuildData _guild;
            try
            {
                _guild = DataManager.Instance.GameData.Guilds[guildId];
            }
            catch
            {
                GuildId = string.Empty;
                return null;
            }

            if (_guild == null)
            {
                GuildId = string.Empty;
                return null;
            }

            bool _isStillInGuild = false;
            foreach (var _player in _guild.Players)
            {
                if (_player.Id == FirebaseManager.Instance.PlayerId)
                {
                    _isStillInGuild = true;
                }
            }


            if (!_isStillInGuild)
            {
                GuildId = string.Empty;
                return null;
            }

            _guild.ReorderPlayersByPoints();
            return _guild;
        }
    }

    public int Points
    {
        get => points;
        set
        {
            points = value;
            UpdatedPoints?.Invoke();
        }
    }

    // new system
    public static Action OnUpdatedShards;
    public static Action OnUpdatedExp;

    public const string NAME_KEY = "username";
    public const string KITTY_RECOVERY_KEY = "recoveryDate";
    public const string KITTY_KEY = "kitty_id";
    
    public const string UNCOMMON_SHARD = "uncommonShard";
    public const string RARE_SHARD = "rareShard";
    public const string EPIC_SHARD = "epicShard";
    public const string LEGENDARY_SHARD = "legendaryShard";
    public const string COMMON_SHARD = "commonShard";
    
    public const string NAME_ENTITY_ID = "user_profile";

    private const string XP = "xp";
    private const string AMOUNT_KEY = "amount";

    public int Level { get; private set; }
    public int ExperienceOnCurrentLevel { get; private set; }
    public int ExperienceForNextLevel { get; private set; }

    public string Username => BoomDaoUtility.Instance.GetString(NAME_ENTITY_ID, NAME_KEY);
    public double CommonShard => BoomDaoUtility.Instance.GetDouble(COMMON_SHARD, AMOUNT_KEY);
    public double UncommonShard => BoomDaoUtility.Instance.GetDouble(UNCOMMON_SHARD, AMOUNT_KEY);
    public double RareShard => BoomDaoUtility.Instance.GetDouble(RARE_SHARD, AMOUNT_KEY);
    public double EpicShard => BoomDaoUtility.Instance.GetDouble(EPIC_SHARD, AMOUNT_KEY);
    public double LegendaryShard => BoomDaoUtility.Instance.GetDouble(LEGENDARY_SHARD, AMOUNT_KEY);
    public double TotalCrystals => CommonShard + UncommonShard + RareShard + EpicShard + LegendaryShard;

    public double Experience => BoomDaoUtility.Instance.GetDouble(XP, AMOUNT_KEY);

    public void SubscribeEvents()
    {
        BoomDaoUtility.OnDataUpdated += RiseEvent;

        CalculateLevel();
    }

    public void UnsubscribeEvents()
    {
        BoomDaoUtility.OnDataUpdated += RiseEvent;
    }

    private void RiseEvent(string _key)
    {
        switch (_key)
        {
            case XP:
                CalculateLevel();
                OnUpdatedExp?.Invoke();
                break;
            case COMMON_SHARD:
            case UNCOMMON_SHARD:
            case RARE_SHARD:
            case EPIC_SHARD:
            case LEGENDARY_SHARD:
                OnUpdatedShards?.Invoke();
                break;
            default:
                Debug.Log($"{_key} got updated!, add handler?");
                break;
        }
    }

    public double GetAmountOfCrystals(LuckyWheelRewardType _type)
    {
        switch (_type)
        {
            case LuckyWheelRewardType.Common:
                return CommonShard;
            case LuckyWheelRewardType.Uncommon:
                return UncommonShard;
            case LuckyWheelRewardType.Rare:
                return RareShard;
            case LuckyWheelRewardType.Epic:
                return EpicShard;
            case LuckyWheelRewardType.Legendary:
                return LegendaryShard;
        }

        throw new Exception("Unsupported type of shards: " + _type);
    }

    public bool IsKittyHurt(string _kittyId)
    {
        return BoomDaoUtility.Instance.GetEntityData(_kittyId) != default;
    }

    public DateTime GetKittyRecoveryDate(string _kittyId)
    {
        Dictionary<string, string> _data = BoomDaoUtility.Instance.GetEntityData(_kittyId);
        DateTime _recoveryDate = default;

        if (_data.TryGetValue(KITTY_RECOVERY_KEY, out string _timeString))
        {
            _recoveryDate = DateTime.Parse(_timeString);
            if (_recoveryDate <= DateTime.UtcNow)
            {
                _recoveryDate = default;
                BoomDaoUtility.Instance.RemoveEntity(_kittyId);
            }
        }

        return _recoveryDate;
    }

    private void CalculateLevel()
    {
        CalculateLevel(Experience, out var _level, out var _expForNextLevel, out var _experienceOnCurrentLevel);

        Level = _level;
        ExperienceForNextLevel = _expForNextLevel;
        ExperienceOnCurrentLevel = _experienceOnCurrentLevel;
    }

    public static void CalculateLevel(double _exp, out int _level, out int _expForNextLevel, out int _experienceOnCurrentLevel)
    {
        double _experience = _exp;
        int _calculatedLevel = 1;
        float _calculatedExpForNextLevel = DataManager.Instance.GameData.LevelBaseExp;

        if (_experience < DataManager.Instance.GameData.LevelBaseExp)
        {
            _experienceOnCurrentLevel = (int)_experience;
            _calculatedExpForNextLevel = DataManager.Instance.GameData.LevelBaseExp;
        }
        else
        {
            while (_experience >= _calculatedExpForNextLevel)
            {
                _calculatedLevel++;
                _experience -= _calculatedExpForNextLevel;
                _calculatedExpForNextLevel = _calculatedExpForNextLevel +
                                             (_calculatedExpForNextLevel * ((float)DataManager.Instance.GameData.LevelBaseScaler / 100));
            }
        }

        _expForNextLevel = (int)_calculatedExpForNextLevel;
        _experienceOnCurrentLevel = (int)_experience;
        _level = _calculatedLevel;
    }
}