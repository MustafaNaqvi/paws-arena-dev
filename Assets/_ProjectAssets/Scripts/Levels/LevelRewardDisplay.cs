using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using BoomDaoWrapper;

public class LevelRewardDisplay : MonoBehaviour
{
    private const string CLAIM_PREMIUM_REWARD = "battlePassPremium";
    private const string CLAIM_NORMAL_REWARD = "battlePassNormal";
    
    public static Action<LevelReward, Sprite> OnClaimed;
    [SerializeField] private Image rewardDisplay;
    [SerializeField] private Image background;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite premiumBackground;

    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimImage;
    [SerializeField] private Sprite normalClaim;
    [SerializeField] private Sprite premiumClaim;
    [SerializeField] private GameObject lockedImage;
    [SerializeField] private GameObject claimedObject;
    [SerializeField] private GameObject shadowPanel;
    [SerializeField] private bool isPremium;

    [SerializeField] private EquipmentsConfig equipments;
    [Space][Header("Sprites")]
    [SerializeField] private Sprite[] shards;
    [SerializeField] private Sprite snacks;
    [SerializeField] private Sprite jugOfMilk;
    [SerializeField] private Sprite glassOfMilk;


    private LevelReward reward;
    private int level;
    private bool canClaim;

    public bool CanClaim => canClaim;
    
    private void OnDisable()
    {
        claimButton.onClick.RemoveAllListeners();
    }

    public void Setup(LevelReward _reward, int _level)
    {
        claimButton.interactable = true;
        reward = _reward;
        level = _level;
        background.sprite = _reward.IsPremium ? premiumBackground : normalBackground;
        claimImage.sprite = _reward.IsPremium ? premiumClaim : normalClaim;
        rewardDisplay.gameObject.SetActive(true);
        rewardDisplay.sprite = GetSpriteForReward(_reward);
        if (DataManager.Instance.PlayerData.HasClaimed(_reward, level))
        {
            claimedObject.SetActive(true);
        }
        else if(DataManager.Instance.GameData.IsSeasonActive)
        {
            if (level <= DataManager.Instance.PlayerData.Level)
            {
                claimButton.onClick.AddListener(ClaimReward);
                if (_reward.IsPremium && !DataManager.Instance.PlayerData.HasPass)
                {
                    lockedImage.gameObject.SetActive(true);
                    claimButton.interactable = false;
                }
                else
                {
                    canClaim = true;
                }

                claimButton.gameObject.SetActive(true);
            }
            else
            {
                if (_reward.IsPremium)
                {
                    lockedImage.SetActive(true);
                }
            }
        }

        if (!claimButton.gameObject.activeSelf||!claimButton.interactable)
        {
            shadowPanel.SetActive(true);
        }
    }

    public void ClaimReward()
    {
        claimButton.interactable = false;
        string _actionId = reward.IsPremium ? CLAIM_PREMIUM_REWARD : CLAIM_NORMAL_REWARD;
        _actionId += reward.Level;
        BoomDaoUtility.Instance.ExecuteAction(_actionId, HandleClaimFinished);
    }

    private void HandleClaimFinished(List<ActionOutcome> _outcomes)
    {
        claimButton.interactable = true;
        if (_outcomes==default || _outcomes.Count==0)
        {
            return;
        }
        
        OnClaimed?.Invoke(reward,GetSpriteForReward(reward));
        SetupEmpty();
        Setup(reward,level);
    }

    private Sprite GetSpriteForReward(LevelReward _reward)
    {
        switch (_reward.Type)
        {
            case LevelRewardType.CommonShard:
                return shards[0];
            case LevelRewardType.UncommonShard:
                return shards[1];
            case LevelRewardType.RareShard:
                return shards[2];
            case LevelRewardType.EpicShard:
                return shards[3];
            case LevelRewardType.LegendaryShard:
                return shards[4];
            case LevelRewardType.Snack:
                return snacks;
            case LevelRewardType.JugOfMilk:
                return jugOfMilk;
            case LevelRewardType.GlassOfMilk:
                return glassOfMilk;
            case LevelRewardType.Item:
                return equipments.GetEquipmentData(_reward.Parameter1).Thumbnail;
            case LevelRewardType.Emote:
                return EmojiSO.Get(_reward.Parameter1).Sprite;
            case LevelRewardType.WeaponSkin:
                break;
            default:
                throw new Exception("Cant find sprite for reward type: " + _reward.Type);
        }
        return null;
    }

    public void SetupEmpty()
    {
        background.sprite = isPremium ? premiumBackground : normalBackground;
        claimButton.interactable = true;
        claimButton.gameObject.SetActive(false);
        lockedImage.SetActive(false);
        rewardDisplay.gameObject.SetActive(false);
        canClaim = false;
        shadowPanel.SetActive(false);
        claimedObject.SetActive(false);
    }
}
