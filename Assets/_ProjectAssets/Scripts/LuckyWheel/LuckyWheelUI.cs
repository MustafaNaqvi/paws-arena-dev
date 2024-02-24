using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BoomDaoWrapper;

public class LuckyWheelUI : MonoBehaviour
{
    private const string BATTLE_WON_ACTION_KEY = "battle_outcome_won";
    
    [SerializeField] private GameObject playerPlatform;
    [SerializeField] private LuckyWheel luckyWheel;
    [SerializeField] private LuckyWheelClaimDisplay rewardDisplay;
    // [SerializeField] private Button respinButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject insuficiantSnacksForRespin;
    [SerializeField] private TextMeshProUGUI insuficiantSnacksText;
    [SerializeField] private EquipmentsConfig equipments;
    [SerializeField] private bool CloseOnClaim;
    [SerializeField] private BuyMilk buyMilk;
    private LuckyWheelRewardSO choosenReward;
    public static EquipmentData EquipmentData = null;

    private bool requestedToSeeReward;
    private int currentRespinPrice;

    public void RequestReward()
    {
        Debug.Log("---- Dealt damage: "+DamageDealingDisplay.XpEarned);
        List<ActionParameter> _parameters = new()
        {
            new ActionParameter { Key = PlayerData.EARNED_XP_KEY, Value = DamageDealingDisplay.XpEarned.ToString()}
        };
        BoomDaoUtility.Instance.ExecuteActionWithParameter(BATTLE_WON_ACTION_KEY,_parameters, OnGotRewards);
    }
    
    private void OnGotRewards(List<ActionOutcome> _rewards)
    {
        if (_rewards==null)
        {
            return;
        }

        foreach (var _reward in _rewards)
        {
            switch (_reward.Name)
            {
                case PlayerData.COMMON_SHARD:
                case PlayerData.UNCOMMON_SHARD:
                case PlayerData.RARE_SHARD:
                case PlayerData.EPIC_SHARD:
                case PlayerData.LEGENDARY_SHARD:
                    choosenReward = LuckyWheelRewardSO.Get(_reward.Name);
                    break;
                case "gift":
                    Debug.Log("Received gift");
                    choosenReward = LuckyWheelRewardSO.Get(1);
                    break;
                default:
                    Debug.Log($"Don't know how to handle {_reward.Value} of {_reward.Name}");
                    break;
            }
        }

        if (requestedToSeeReward)
        {
            Setup();
        }
    }

    public void ShowReward()
    {
        requestedToSeeReward = true;
        if (choosenReward == null)
        {
            return;
        }

        Setup();
    }

    private void Setup()
    {
        if (playerPlatform!=null)
        {
            playerPlatform.SetActive(false);
        }
        gameObject.SetActive(true);

        // respinButton.gameObject.SetActive(false);
        claimButton.gameObject.SetActive(false);
        
        currentRespinPrice = DataManager.Instance.GameData.RespinPrice;
        SpinWheel();
    }

    private void OnEnable()
    {
        claimButton.onClick.AddListener(ClaimReward);
        // respinButton.onClick.AddListener(Respin);
    }

    private void OnDisable()
    {
        claimButton.onClick.RemoveListener(ClaimReward);
        // respinButton.onClick.RemoveListener(Respin);
    }

    private void ClaimReward()
    {
        Claim(choosenReward);
    }

    private void Claim(LuckyWheelRewardSO _reward)
    {
        //todo increase amount of crystals
        switch (_reward.Type)
        {
            case ItemType.CommonShard:
                // DataManager.Instance.PlayerData.Crystals.CommonCrystal++;
                break;
            case ItemType.UncommonShard:
                // DataManager.Instance.PlayerData.Crystals.UncommonCrystal++;
                break;
            case ItemType.RareShard:
                // DataManager.Instance.PlayerData.Crystals.RareCrystal++;
                break;
            case ItemType.EpicShard:
                // DataManager.Instance.PlayerData.Crystals.EpicCrystal++;
                break;
            case ItemType.LegendaryShard:
                // DataManager.Instance.PlayerData.Crystals.LegendaryCrystal++;
                break;
            case ItemType.GlassOfMilk:
                EquipmentData = equipments.CraftItem();
                DataManager.Instance.PlayerData.AddOwnedEquipment(EquipmentData.Id);
                break;
            default:
                throw new System.Exception("Dont know how to reward reward type: " + _reward.Type);
        }

        rewardDisplay.Setup(_reward,CloseAfterClaim);
    }

    private void CloseAfterClaim()
    {
        if (CloseOnClaim)
        {
            gameObject.SetActive(false);
        }
    }

    private void Respin()
    {
        //todo remove previous reward
        if (DataManager.Instance.PlayerData.Snacks< currentRespinPrice)
        {
            insuficiantSnacksForRespin.SetActive(true);
            insuficiantSnacksText.text = $"You don't have enaught Snacks.\n(takes {currentRespinPrice} for respin)";
            buyMilk.Setup();
            return;
        }

        // DataManager.Instance.PlayerData.Snacks -= currentRespinPrice;
        // respinButton.gameObject.SetActive(false);
        claimButton.gameObject.SetActive(false);
        choosenReward = null;
        currentRespinPrice *= 2;
        RequestReward();
    }

    private void SpinWheel()
    {
        luckyWheel.Spin(EnableButtons, choosenReward);
    }

    private void EnableButtons()
    {
        StartCoroutine(ShowRewardAnimationRoutine());
    }

    private IEnumerator ShowRewardAnimationRoutine()
    {
        yield return new WaitForSeconds(1);
        claimButton.gameObject.SetActive(true);
        // respinButton.gameObject.SetActive(true);
    }

    public void Close()
    {
        playerPlatform.SetActive(true);
        gameObject.SetActive(false);
    }
}
