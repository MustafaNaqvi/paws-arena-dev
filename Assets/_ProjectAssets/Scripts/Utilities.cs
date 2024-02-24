using System;
using System.Text.RegularExpressions;

public static class Utilities
{
    
    private static readonly DateTime UNIX_EPOCH = new (2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long DateTimeToNanoseconds(DateTime _dateTime)
    {
        TimeSpan _duration = _dateTime.ToUniversalTime() - UNIX_EPOCH;
        long _ticks = _duration.Ticks;
        long _nanoseconds = _ticks * 100;
        return _nanoseconds;
    }

    public static DateTime NanosecondsToDateTime(long _nanoseconds)
    {
        long _ticks = _nanoseconds / 100; 
        DateTime _dateTime = UNIX_EPOCH.AddTicks(_ticks);
        return _dateTime;
    }
    
    public static string RemoveWhitespacesUsingRegex(string _source)
    {
        return Regex.Replace(_source, @"\s", string.Empty);
    }
    
    public static ItemType GetRewardType(string _key)
    {
        switch (_key)
        {
            case PlayerData.COMMON_SHARD:
                return ItemType.CommonShard;
            case PlayerData.UNCOMMON_SHARD:
                return ItemType.UncommonShard;
            case PlayerData.RARE_SHARD:
                return ItemType.RareShard;
            case PlayerData.EPIC_SHARD:
                return ItemType.EpicShard;
            case PlayerData.LEGENDARY_SHARD:
                return ItemType.LegendaryShard;
            case PlayerData.MILK_GLASS:
                return ItemType.GlassOfMilk;
            case PlayerData.MILK_BOTTLE:
                return ItemType.JugOfMilk;
            default:
                throw new Exception($"Don't know how to reward {_key}");
        }
    }

    public static CraftingRecepieSO EquipmentRarityToCraftingRecipe(EquipmentRarity _rarity)
    {
        switch (_rarity)
        {
            case EquipmentRarity.Common:
                return CraftingRecepieSO.Get(LuckyWheelRewardType.Common);
            case EquipmentRarity.Uncommon:
                return CraftingRecepieSO.Get(LuckyWheelRewardType.Uncommon);
            case EquipmentRarity.Rare:
                return CraftingRecepieSO.Get(LuckyWheelRewardType.Rare);
            case EquipmentRarity.Epic:
                return CraftingRecepieSO.Get(LuckyWheelRewardType.Epic);
            case EquipmentRarity.Legendary:
                return CraftingRecepieSO.Get(LuckyWheelRewardType.Legendary);
            default:
                throw new Exception($"Don't know how to convert {_rarity} to recepie");
        }
    }
}
