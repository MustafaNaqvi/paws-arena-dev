using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Newtonsoft.Json;
using TMPro;
using System.Collections.Generic;
using Candid.World.Models;
using Candid;
using Boom.Utility;
using static EntityFieldEdit;

public class LuckyWheelUI : MonoBehaviour
{
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

    private bool requestedToSeeReward = false;
    private int currentRespinPrice;

    public async void RequestReward()
    {
        //int _rewardId = -1;
        //try
        //{
        //    string resp = await NetworkManager.GETRequestCoroutine("/leaderboard/spin-the-wheel?matchId=" + PhotonNetwork.CurrentRoom.Name,
        //    (code, err) =>
        //    {
        //        Debug.LogWarning($"Failed to get reward type {err} : {code}");
        //    }, true);

        //    LuckyWheelRewardResponse _response = JsonConvert.DeserializeObject<LuckyWheelRewardResponse>(resp);
        //    _rewardId = _response.RewardType;
        //}
        //catch
        //{
        //    _rewardId = 1;
        //}

        //choosenReward = LuckyWheelRewardSO.Get(_rewardId);
        //if (requestedToSeeReward)
        //{
        //    Setup();
        //}

        try
        {
            "Spin Wheel of Fortune".Log(typeof(LuckyWheelUI).Name);
            //Action Id
            var actionId = "battle_outcome_won";

            //Try get Action Config

            if(ConfigUtil.TryGetAction(CandidApiManager.Instance.WORLD_CANISTER_ID, actionId, out var actionConfig) == false)
            {
                $"Action of Id: {actionId} could not be found".Error(typeof(LuckyWheelUI).Name);
                throw new();
            }

            var callerActionConfig = actionConfig.callerAction;

            if(callerActionConfig == null)
            {
                $"callerAction is null".Warning(typeof(LuckyWheelUI).Name);
                throw new();
            }

            var callerOutcomesConfig = callerActionConfig.Outcomes;

            if (callerOutcomesConfig.Count < 2)
            {
                $"Action is missing Wheel of Fortune's Outcome".Warning(typeof(LuckyWheelUI).Name);
                throw new();
            }

            var possibleWheelOfFortuneOutcomesConfig = callerOutcomesConfig[1];


            //ACtion Arguments Setup

            var currentKittyId = "kitty_0";
            var currentKityHealth = 40;
            var totalBattleXp = 120;

            List<Field> args = new() 
            {
                 new Field("kitty_id", currentKittyId),
                  new Field("new_health", $"{currentKityHealth}"),
                   new Field("total_battle_xp", $"{totalBattleXp}"),
            };

            //Process Action

            var actionResult = await ActionUtil.ProcessAction(actionId, args);

            if (actionResult.IsErr)
            {
                $"{actionResult.AsErr()}".Warning(typeof(LuckyWheelUI).Name);
                throw new();
            }

            var actionResultAsOk = actionResult.AsOk();

            var callerActionOutcome = actionResultAsOk.callerOutcomes;

            var rewardName = "";
            var rewardAmount = 0.0;

            //Try filter from all the applied outcomes the one for the Wheel of Fortune

            foreach (var posibleOutcome in possibleWheelOfFortuneOutcomesConfig.PossibleOutcomes)
            {
                //If update is not of type entity then skip
                if(posibleOutcome.Option.Tag != ActionOutcomeOption.OptionInfoTag.UpdateEntity)
                {
                    continue;
                }

                var outcomeEntityConfig = posibleOutcome.Option.AsUpdateEntity();

                foreach (var actionCallerEntityOutcome in callerActionOutcome.entityEdits)
                {
                    //If one of the expected Wheel of Fortune outcomes is found on the applied outcomes then pick the one that matches
                    if(outcomeEntityConfig.Eid == actionCallerEntityOutcome.Value.eid)
                    {
                        rewardName = actionCallerEntityOutcome.Value.eid;

                        if(actionCallerEntityOutcome.Value.fields.TryGetValue("amount", out EntityFieldEdit.Base amountEdit) == false)
                        {
                            $"Entity of id: {rewardName} doesn't has field of id: amount".Warning(typeof(LuckyWheelUI).Name);
                            throw new();
                        }

                        if(amountEdit is not EntityFieldEdit.IncrementNumber amountIncrementEdit)
                        {
                            $"Issue casting value".Warning(typeof(LuckyWheelUI).Name);
                            throw new();
                        }

                        rewardAmount = amountIncrementEdit.Value;

                        goto breakPossibleOutcomesLoop;
                    }
                }

                breakPossibleOutcomesLoop : 
                {
                    break;
                }
            }


            //Get reward by name
            choosenReward = LuckyWheelRewardSO.Get(rewardName);

        }
        catch
        {
            choosenReward = LuckyWheelRewardSO.Get(1);
        }

        choosenReward = LuckyWheelRewardSO.Get(1);
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
        switch (_reward.Type)
        {
            case ItemType.Common:
                DataManager.Instance.PlayerData.Crystals.CommonCrystal++;
                break;
            case ItemType.Uncommon:
                DataManager.Instance.PlayerData.Crystals.UncommonCrystal++;
                break;
            case ItemType.Rare:
                DataManager.Instance.PlayerData.Crystals.RareCrystal++;
                break;
            case ItemType.Epic:
                DataManager.Instance.PlayerData.Crystals.EpicCrystal++;
                break;
            case ItemType.Legendary:
                DataManager.Instance.PlayerData.Crystals.LegendaryCrystal++;
                break;
            case ItemType.Gift:
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
        if (DataManager.Instance.PlayerData.Snacks< currentRespinPrice)
        {
            insuficiantSnacksForRespin.SetActive(true);
            insuficiantSnacksText.text = $"You don't have enaught Snacks.\n(takes {currentRespinPrice} for respin)";
            buyMilk.Setup();
            return;
        }

        DataManager.Instance.PlayerData.Snacks -= currentRespinPrice;
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
