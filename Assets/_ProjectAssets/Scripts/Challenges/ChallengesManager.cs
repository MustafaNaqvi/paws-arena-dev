using System;
using System.Collections;
using System.Collections.Generic;
using BoomDaoWrapper;
using Newtonsoft.Json;
using UnityEngine;

public class ChallengesManager : MonoBehaviour
{
    public static Action OnChallengeClaimed;
    public const int AMOUNT_OF_CHALLENGES = 3;
    
    public const string GENERATE_DAILY_CHALLENGE = "generateDailyChallenge";
    public const string CHALLENGE_NUMBER = "challengeNumber";
    public const string CHALLENGE_IDENTIFIER = "challengeIdentifier";
    
    public const string SET_RESET_TIME = "dailyChallengesSetResetTime";
    public const string NEXT_RESET = "nextReset";
    public const string DAILY_CHALLENGES = "dailyChallenges";
    public const string DAILY_CHALLENGE = "dailyChallenge";
    public const string CLAIMED_LUCKY_SPIN = "claimedLuckySpin";

    public const string PROGRESS_VALUE = "value";
    public const string PROGRESS_CLAIMED = "claimed";

    public const string CHALLENGE_ID = "id";
    public const string CHALLENGE_DESCRIPTION = "description";
    public const string CHALLENGE_AMOUNT_NEEDED = "amountNeeded";
    public const string CHALLENGE_REWARD_AMOUNT = "rewardAmount";
    public const string CHALLENGE_REWARD_TYPE = "rewardType";
    public const string CHALLENGE_CATEGORY = "category";

    public const string CHALLENGES_REWARD_LUCKY_SPIN = "challengesLuckyWheel";
    public const string CHALLENGES_CLAIM_DAILY_CHALLENGE = "setChallengeAsClaimed";
    
    private const string UPDATE_CHALLENGE_PROGRESS = "increaseChallengeProgress";

    public static ChallengesManager Instance;
    private bool isGeneratingNewChallenges;
    private bool isSubscribed;
    
    private List<ChallengeUpdateProgress> progressToUpdate = new();
    private bool isUpdatingProgress;

    private List<ChallengeProgress> challengesToClaim = new();
    private bool isClaiming;

    private void OnEnable()
    {
        ChallengeDisplay.OnClaimPressed += ClaimedChallenge;
    }

    private void OnDisable()
    {
        ChallengeDisplay.OnClaimPressed -= ClaimedChallenge;
        UnsubscribeEvents();
    }

    private void ClaimedChallenge(ChallengeProgress _challengeProgress)
    {
        if (_challengeProgress.IsClaiming)
        {
            return;
        }

        foreach (ChallengeProgress _claimingChallenge in challengesToClaim)
        {
            if (DataManager.Instance.GameData.GetChallengeByIdentifier(_claimingChallenge.Identifier)==DataManager.Instance.GameData
            .GetChallengeByIdentifier(_challengeProgress.Identifier))
            {
                return;
            }
        }
        challengesToClaim.Add(_challengeProgress);
        TryClaim();
    }

    private void TryClaim()
    {
        if (isClaiming)
        {
            return;
        }

        if (challengesToClaim.Count==0)
        {
            return;
        }

        isClaiming = true;
        var _challengeProgress = challengesToClaim[0];
        _challengeProgress.IsClaiming = true;
        
        List<ActionParameter> _parameters = new List<ActionParameter>
        {
            new() { Key = CHALLENGE_NUMBER, Value = PlayerData.DAILY_CHALLENGE_PROGRESS+DataManager.Instance.GameData.GetChallengeIndex(_challengeProgress) }
        };
        
        Debug.Log($"{CHALLENGE_NUMBER}: meaning challenge name is {PlayerData.DAILY_CHALLENGE_PROGRESS+DataManager.Instance.GameData.GetChallengeIndex(_challengeProgress)}, calling for action {CHALLENGES_CLAIM_DAILY_CHALLENGE}");
        
        BoomDaoUtility.Instance.ExecuteActionWithParameter(CHALLENGES_CLAIM_DAILY_CHALLENGE, _parameters, _ =>
        {
            _challengeProgress.Claimed = true;
            ChallengeData _challengeData = DataManager.Instance.GameData.GetChallengeByIdentifier(_challengeProgress.Identifier);
            string _claimActionId;
            switch (_challengeData.RewardType)
            {
                case ItemType.CommonShard:
                    _claimActionId = RewardActions.REWARD_N_COMMON_CRYSTALS;
                    break;
                case ItemType.UncommonShard:
                    _claimActionId = RewardActions.REWARD_N_UNCOMMON_CRYSTALS;
                    break;
                case ItemType.RareShard:
                    _claimActionId = RewardActions.REWARD_N_RARE_CRYSTALS;
                    break;
                case ItemType.EpicShard:
                    _claimActionId = RewardActions.REWARD_N_EPIC_CRYSTALS;
                    break;
                case ItemType.LegendaryShard:
                    _claimActionId = RewardActions.REWARD_N_LEGENDARY_CRYSTALS;
                    break;
                case ItemType.Snack:
                    _claimActionId = RewardActions.REWARD_N_SNACKS;
                    break;
                case ItemType.JugOfMilk:
                    _claimActionId = RewardActions.REWARD_N_JUG_OF_MILKS;
                    break;
                case ItemType.GlassOfMilk:
                    _claimActionId = RewardActions.REWARD_N_GLASS_OF_MILKS;
                    break;
                case ItemType.SeasonExperience:
                    _claimActionId = RewardActions.REWARD_N_XPS;
                    break;
                default:
                    throw new Exception("Don't know how to reward item type: "+_challengeData.RewardType);
            }
        
            List<ActionParameter> _claimedParameters = new List<ActionParameter>
            {
                new() { Key = BoomDaoUtility.AMOUNT_KEY, Value = _challengeData.RewardAmount.ToString() }
            };
            BoomDaoUtility.Instance.ExecuteActionWithParameter(_claimActionId,_claimedParameters,_ =>
            {
                challengesToClaim.RemoveAt(0);
                isClaiming = false;
                OnChallengeClaimed?.Invoke();
                TryClaim();
            });
        });
        
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Setup()
    {
        if (DataManager.Instance.GameData.HasDailyChallenges)
        {
            SubscribeEvents();
            StartCheckForReset();
        }
        else
        {
            GenerateChallenges();
        }
    }

    private void GenerateChallenges()
    {
        GenerateNewChallenges(StartCheckForReset);
    }

    private void StartCheckForReset()
    {
        StartCoroutine(CheckForReset());
    }

    private IEnumerator CheckForReset()
    {
        while (true)
        {
            if (DateTime.UtcNow > DataManager.Instance.GameData.DailyChallenges.NextReset)
            {
                GenerateNewChallenges(StartCheckForReset);
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }
    
    private void GenerateNewChallenges(Action _callBack)
    {
        if (isGeneratingNewChallenges)
        {
            return;
        }
        
        UnsubscribeEvents();
        isGeneratingNewChallenges = true;

        BoomDaoUtility.Instance.ExecuteAction(PlayerData.RESET_AMOUNT_OF_GAMES_PLAYED_TODAY, null);
        int _counter = 0;
        CheckForFinishCreating(null);

        void CheckForFinishCreating(List<ActionOutcome> _)
        {
            if (_counter<AMOUNT_OF_CHALLENGES)
            {
                List<ActionParameter> _parameters = new List<ActionParameter>
                {
                    new () { Key = CHALLENGE_NUMBER, Value = DAILY_CHALLENGE+_counter },
                    new () { Key = CHALLENGE_IDENTIFIER, Value = Guid.NewGuid().ToString() },
                    new () { Key = "progressNumber" , Value = PlayerData.DAILY_CHALLENGE_PROGRESS+_counter }
                };
                Debug.Log(JsonConvert.SerializeObject(_parameters));
                BoomDaoUtility.Instance.ExecuteActionWithParameter(GENERATE_DAILY_CHALLENGE,_parameters, CheckForFinishCreating);
                _counter++;
            }
            else
            {
                long _nextResetTime = Utilities.DateTimeToNanoseconds(DateTime.UtcNow.AddDays(1));
                List<ActionParameter> _parameters = new List<ActionParameter> { new() { Key = NEXT_RESET, Value = _nextResetTime.ToString() } };
                BoomDaoUtility.Instance.ExecuteActionWithParameter(SET_RESET_TIME,_parameters, Finish);
            }
        }

        void Finish(List<ActionOutcome> _)
        {
            SubscribeEvents();
            _callBack?.Invoke();
        }
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
        {
            return;
        }

        isSubscribed = true;
        foreach (var _challengeProgress in DataManager.Instance.PlayerData.ChallengeProgresses)
        {
            if (_challengeProgress.Completed)
            {
                continue;
            }

            ChallengeData _challengeData = DataManager.Instance.GameData.GetChallengeByIdentifier(_challengeProgress.Identifier);
            switch (_challengeData.Category)
            {
                case ChallengeCategory.WinGame:
                    EventsManager.OnWonGame += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.CraftItem:
                    EventsManager.OnCraftedItem += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.CraftShard:
                    EventsManager.OnCraftedCrystal += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.GainExperience:
                    EventsManager.OnGotExperience += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.GainLeaderboardPoints:
                    EventsManager.OnWonLeaderboardPoints += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinWithFullHp:
                    EventsManager.OnWonGameWithFullHp += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.LoseMatch:
                    EventsManager.OnLostGame += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.DealDamage:
                    EventsManager.OnDealtDamageToOpponent += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.UseMilkBottle:
                    EventsManager.OnUsedMilkBottle += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.HealYourKitty:
                    EventsManager.OnHealedKitty += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.PlayMatch:
                    EventsManager.OnPlayedMatch += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootRocket:
                    EventsManager.OnUsedRocket += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootCannon:
                    EventsManager.OnUsedCannon += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootTripleRocket:
                    EventsManager.OnUsedTripleRocket += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootPlane:
                    EventsManager.OnUsedAirplane += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootMouse:
                    EventsManager.OnUsedMouse += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootArrow:
                    EventsManager.OnUsedArrow += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinMatchesInARow:
                    EventsManager.OnWonGame += _challengeProgress.IncreaseAmount;
                    EventsManager.OnLostGame += _challengeProgress.Reset;
                    break;
                case ChallengeCategory.WinMatchWithLessThan10Hp:
                    EventsManager.OnWonWithHpLessThan10 += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinMatchWithLessThan20Hp:
                    EventsManager.OnWonWithHpLessThan20 += _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinMatchWithLessThan30Hp:
                    EventsManager.OnWonWithHpLessThan30 += _challengeProgress.IncreaseAmount;
                    break;
            }
        }
    }
    
    private void UnsubscribeEvents()
    {
        if (!isSubscribed)
        {
            return;
        }

        isSubscribed = false;
        foreach (var _challengeProgress in DataManager.Instance.PlayerData.ChallengeProgresses)
        {
            if (_challengeProgress.Completed)
            {
                continue;
            }
            ChallengeData _challengeData = DataManager.Instance.GameData.GetChallengeByIdentifier(_challengeProgress.Identifier);

            switch (_challengeData.Category)
            {
                case ChallengeCategory.WinGame:
                    EventsManager.OnWonGame -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.CraftItem:
                    EventsManager.OnCraftedItem -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.CraftShard:
                    EventsManager.OnCraftedCrystal -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.GainExperience:
                    EventsManager.OnGotExperience -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.GainLeaderboardPoints:
                    EventsManager.OnWonLeaderboardPoints -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinWithFullHp:
                    EventsManager.OnWonGameWithFullHp -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.LoseMatch:
                    EventsManager.OnLostGame -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.DealDamage:
                    EventsManager.OnDealtDamageToOpponent -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.UseMilkBottle:
                    EventsManager.OnUsedMilkBottle -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.HealYourKitty:
                    EventsManager.OnHealedKitty -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.PlayMatch:
                    EventsManager.OnPlayedMatch -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootRocket:
                    EventsManager.OnUsedRocket -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootCannon:
                    EventsManager.OnUsedCannon -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootTripleRocket:
                    EventsManager.OnUsedTripleRocket -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootPlane:
                    EventsManager.OnUsedAirplane -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootMouse:
                    EventsManager.OnUsedMouse -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.ShootArrow:
                    EventsManager.OnUsedArrow -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinMatchesInARow:
                    EventsManager.OnWonGame -= _challengeProgress.IncreaseAmount;
                    EventsManager.OnLostGame -= _challengeProgress.Reset;
                    break;
                case ChallengeCategory.WinMatchWithLessThan10Hp:
                    EventsManager.OnWonWithHpLessThan10 -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinMatchWithLessThan20Hp:
                    EventsManager.OnWonWithHpLessThan20 -= _challengeProgress.IncreaseAmount;
                    break;
                case ChallengeCategory.WinMatchWithLessThan30Hp:
                    EventsManager.OnWonWithHpLessThan30 -= _challengeProgress.IncreaseAmount;
                    break;
            }
        }
    }

    public void IncreaseProgress(ChallengeUpdateProgress _progressData)
    {
        progressToUpdate.Add(_progressData);
        TryUpdate();
    }

    private void TryUpdate()
    {
        if (isUpdatingProgress)
        {
            return;
        }

        if (progressToUpdate.Count==0)
        {
            return;
        }

        isUpdatingProgress = true;
        BoomDaoUtility.Instance.ExecuteActionWithParameter(UPDATE_CHALLENGE_PROGRESS, progressToUpdate[0].Parameters, FinishedUpdating);
    }

    private void FinishedUpdating(List<ActionOutcome> _)
    {
        isUpdatingProgress = false;
        Debug.Log("Finished updating: "+JsonConvert.SerializeObject(progressToUpdate[0]));
        progressToUpdate.RemoveAt(0);
        TryUpdate();
    }
}