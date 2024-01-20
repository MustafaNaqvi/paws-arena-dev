using Boom.Utility;
using Candid.World.Models;
using Candid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    public async void Call()
    {
        "Spin Wheel of Fortune".Log(typeof(LuckyWheelUI).Name);
        //Action Id
        var actionId = "battle_outcome_won";

        //Try get Action Config

        if (ConfigUtil.TryGetAction(CandidApiManager.Instance.WORLD_CANISTER_ID, actionId, out var actionConfig) == false)
        {
            $"Action of Id: {actionId} could not be found".Error(typeof(LuckyWheelUI).Name);
            throw new();
        }

        var callerActionConfig = actionConfig.callerAction;

        if (callerActionConfig == null)
        {
            $"callerAction is null".Warning(typeof(LuckyWheelUI).Name);
            throw new();
        }

        var callerOutcomesConfig = callerActionConfig.Outcomes;

        if (callerOutcomesConfig.Count < 3)
        {
            $"Action is missing Wheel of Fortune's Outcome".Warning(typeof(LuckyWheelUI).Name);
            throw new();
        }

        var possibleWheelOfFortuneOutcomesConfig = callerOutcomesConfig[2];

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
            $"{actionResult.AsErr().content}".Warning(typeof(LuckyWheelUI).Name);
            throw new();
        }

        var actionResultAsOk = actionResult.AsOk();

        var callerActionOutcome = actionResultAsOk.callerOutcomes;
        Debug.Log($"Configs Rewards, {JsonConvert.SerializeObject(possibleWheelOfFortuneOutcomesConfig)}");
        Debug.Log($"Rewards, {JsonConvert.SerializeObject(callerActionOutcome.entityEdits)}");

        var rewardName = "";
        var rewardAmount = 0.0;

        //Try filter from all the applied outcomes the one for the Wheel of Fortune

        foreach (var posibleOutcome in possibleWheelOfFortuneOutcomesConfig.PossibleOutcomes)
        {

            //If update is not of type entity then skip
            if (posibleOutcome.Option.Tag != ActionOutcomeOption.OptionInfoTag.UpdateEntity)
            {
                continue;
            }

            var outcomeEntityConfig = posibleOutcome.Option.AsUpdateEntity();

            foreach (var actionCallerEntityOutcome in callerActionOutcome.entityEdits)
            {
                //If one of the expected Wheel of Fortune outcomes is found on the applied outcomes then pick the one that matches
                if (outcomeEntityConfig.Eid == actionCallerEntityOutcome.Value.eid)
                {
                    rewardName = actionCallerEntityOutcome.Value.eid;

                    if (actionCallerEntityOutcome.Value.fields.TryGetValue("amount", out EntityFieldEdit.Base amountEdit) == false)
                    {
                        $"Entity of id: {rewardName} doesn't has field of id: amount".Warning(typeof(LuckyWheelUI).Name);
                        throw new();
                    }

                    if (amountEdit is not EntityFieldEdit.IncrementNumber amountIncrementEdit)
                    {
                        $"Issue casting value".Warning(typeof(LuckyWheelUI).Name);
                        throw new();
                    }

                    rewardAmount = amountIncrementEdit.Value;

                    goto breakPossibleOutcomesLoop;
                }
            }
        }

        breakPossibleOutcomesLoop: { }

        //Get reward by name
        Debug.Log($"Generated Reward: {rewardName}");
        var choosenReward = LuckyWheelRewardSO.Get(rewardName);

        Debug.Log($"Is reward found? : {choosenReward != null}");
    }
}
