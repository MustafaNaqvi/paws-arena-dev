using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public PlayerData PlayerData { get; private set; }
    public GameData GameData { get; private set; }

    private const string CRAFTING_PROCESS = "CraftingProcess";
    private const string CLAIMED_LEVELS = "ClaimedLevelRewards";
    private const string HAS_PASS = "HasPass";
    private const string OWNED_EQUIPTABLES = "OwnedEquiptables";
    private const string OWNED_EMOJIS= "OwnedEmojis";
    private const string CHALLENGES= "Challenges";
    private const string CHALLENGES_DATA= "Challenges/ChallengesData";
    private const string GOT_LUCKY_SPIN = "Challenges/ClaimedLuckySpin";
    private const string GUILD = "GuildId";

    private bool hasSubscribed = false;

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerData(string _data)
    {
        PlayerData = JsonConvert.DeserializeObject<PlayerData>(_data);
        if (PlayerData.SeasonNumber!=GameData.SeasonNumber)
        {
            PlayerData.SeasonNumber = GameData.SeasonNumber;
            PlayerData.ClaimedLevelRewards.Clear();
            PlayerData.HasPass = false;
            SaveClaimedLevels();
            SaveHasPass();
        }
        
    }

    public void SetGuildsData(Dictionary<string, GuildData> _guilds)
    {
        GameData.Guilds = _guilds;
        if (GameData.Guilds==null)
        {
            GameData.Guilds = new();
        }
    }

    public void CreatePlayerDataEmpty()
    {
        PlayerData = new PlayerData();
        PlayerData.SetStartingValues();
    }

    public void SetGameData(string _data)
    {
        GameData = JsonConvert.DeserializeObject<GameData>(_data);
    }

    public void SubscribeHandlers()
    {
        PlayerData.SubscribeEvents();
        hasSubscribed = true;
        PlayerData.UpdatedCraftingProcess += SaveCraftingProcess;
        PlayerData.UpdatedClaimedLevels += SaveClaimedLevels;
        PlayerData.UpdatedHasPass += SaveHasPass;
        PlayerData.UpdatedEquiptables += SaveEquiptables;
        PlayerData.UpdatedOwnedEmojis += SaveOwnedEmojis;
        ChallengeData.UpdatedProgress += SaveChallengeProgress;
        PlayerData.Challenges.UpdatedClaimedLuckySpin += SaveClaimedLuckySpin;
        PlayerData.UpdatedGuild += SaveGuild;
    }

    private void OnDestroy()
    {
        if (!hasSubscribed)
        {
            return;
        }
        PlayerData.UpdatedCraftingProcess -= SaveCraftingProcess;
        PlayerData.UpdatedClaimedLevels -= SaveClaimedLevels;
        PlayerData.UpdatedHasPass -= SaveHasPass;
        PlayerData.UpdatedEquiptables -= SaveEquiptables;
        PlayerData.UpdatedOwnedEmojis -= SaveOwnedEmojis;
        ChallengeData.UpdatedProgress -= SaveChallengeProgress;
        PlayerData.Challenges.UpdatedClaimedLuckySpin -= SaveClaimedLuckySpin;
        PlayerData.UpdatedGuild -= SaveGuild;
        PlayerData.UnsubscribeEvents();
    }

    private void SaveCraftingProcess()
    {
        FirebaseManager.Instance.SaveValue(CRAFTING_PROCESS, JsonConvert.SerializeObject(PlayerData.CraftingProcess));
    }

    private void SaveClaimedLevels()
    {
        FirebaseManager.Instance.SaveValue(CLAIMED_LEVELS, JsonConvert.SerializeObject(PlayerData.ClaimedLevelRewards));
    }

    private void SaveHasPass()
    {
        FirebaseManager.Instance.SaveValue(HAS_PASS, JsonConvert.SerializeObject(PlayerData.HasPass));
    }

    private void SaveEquiptables()
    {
        FirebaseManager.Instance.SaveValue(OWNED_EQUIPTABLES, JsonConvert.SerializeObject(PlayerData.OwnedEquiptables));
    }

    private void SaveOwnedEmojis()
    {
        FirebaseManager.Instance.SaveValue(OWNED_EMOJIS, JsonConvert.SerializeObject(PlayerData.OwnedEmojis));
    }

    public void SaveChallenges()
    {
        FirebaseManager.Instance.SaveValue(CHALLENGES,JsonConvert.SerializeObject(PlayerData.Challenges));
    }

    public void SaveChallengeProgress(int _id)
    {
        int _childNumber = 0;
        ChallengeData _challengeData = null;
        for (int i = 0; i < PlayerData.Challenges.ChallengesData.Count; i++)
        {
            if (PlayerData.Challenges.ChallengesData[i].Id == _id)
            {
                _challengeData = PlayerData.Challenges.ChallengesData[i];
                _childNumber = i;
            }
        }
        FirebaseManager.Instance.SaveValue(CHALLENGES_DATA+"/"+_childNumber,JsonConvert.SerializeObject(_challengeData));
    }

    private void SaveClaimedLuckySpin()
    {
        FirebaseManager.Instance.SaveValue(GOT_LUCKY_SPIN,JsonConvert.SerializeObject(PlayerData.Challenges.ClaimedLuckySpin));
    }

    private void SaveGuild()
    {
        FirebaseManager.Instance.SaveString(GUILD,PlayerData.GuildId);
    }
}
