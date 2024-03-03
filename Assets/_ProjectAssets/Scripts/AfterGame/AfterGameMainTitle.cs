using System;
using System.Collections.Generic;
using BoomDaoWrapper;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.colorfulcoding.AfterGame
{
    public class AfterGameMainTitle : MonoBehaviour
    {
        private const string BATTLE_LOST_ACTION_KEY = "battle_outcome_lost";
        private const string HURT_KITTY = "hurtKitty";
        
        private const string INCREASE_LEADERBOARD_POINTS = "increaseLeaderboardPoints";
        
        
        public GameObject winTitle;
        public GameObject loseTitle;
        public GameObject drawTitle;
        public Image bg;

        public TextMeshProUGUI totalCoinsValue;
        public TextMeshProUGUI deltaPoints;
        public Color winColor;
        public Color loseColor;
        public Color drawColor;

        public GameObject reasonText;

        [Header("Cat Stand")]
        public SpriteRenderer standGlow;

        [SerializeField] private LuckyWheelUI luckyWheelUI;
        [SerializeField] private GameObject leaveButton;

        private void Start()
        {
            int checkIfIWon;
            EventsManager.OnPlayedMatch?.Invoke();
            
            SaveKittyHealth();
            
            //If unexpected error happened, we override result type
            if (GameState.pointsChange.gameResultType == 0)
            {
                checkIfIWon = 0;
            }
            else
            {
                checkIfIWon = GameResolveStateUtils.CheckIfIWon(GameState.gameResolveState);
            }

            if (checkIfIWon > 0)
            {
                if (GameState.selectedNFT.CanFight)
                {
                    EventsManager.OnWonGameWithFullHp?.Invoke();
                }

                if (PlayerManager.HealthAtEnd<=10)
                {
                    EventsManager.OnWonWithHpLessThan10?.Invoke();
                }
                if (PlayerManager.HealthAtEnd<=20)
                {
                    EventsManager.OnWonWithHpLessThan20?.Invoke();
                }
                if (PlayerManager.HealthAtEnd<=30)
                {
                    EventsManager.OnWonWithHpLessThan30?.Invoke();
                }
                
                EventsManager.OnWonGame?.Invoke();
                leaveButton.gameObject.SetActive(false);
                luckyWheelUI.RequestReward();
                winTitle.SetActive(true);
                bg.GetComponent<Image>().color = winColor;
                standGlow.color = winColor;
            }
            else if (checkIfIWon < 0)
            {
                List<ActionParameter> _parameters = new()
                {
                    new ActionParameter { Key = PlayerData.EARNED_XP_KEY, Value = DamageDealingDisplay.XpEarned.ToString()}
                };
                BoomDaoUtility.Instance.ExecuteActionWithParameter(BATTLE_LOST_ACTION_KEY,_parameters,null);
                EventsManager.OnLostGame?.Invoke();
                loseTitle.SetActive(true);
                bg.GetComponent<Image>().color = loseColor;
                standGlow.color = loseColor;
            }
            else
            {
                drawTitle.SetActive(true);
                bg.GetComponent<Image>().color = drawColor;
                standGlow.color = drawColor;
            }

            totalCoinsValue.text = "" + GameState.pointsChange.oldPoints;
            int _earnings = GameState.pointsChange.points;

            if (_earnings>0)
            {
                EventsManager.OnWonLeaderboardPoints?.Invoke(_earnings);
            }

            if (GameState.pointsChange.points != 0)
            {
                List<ActionParameter> _parameters = new()
                {
                    new ActionParameter { Key = GameData.LEADERBOARD_NICK_NAME, Value = DataManager.Instance.PlayerData.Username},
                    new ActionParameter { Key = GameData.LEADERBOARD_KITTY_URL, Value = GameState.selectedNFT.imageUrl},
                    new ActionParameter { Key = GameData.LEADERBOARD_POINTS, Value = GameState.pointsChange.points.ToString()}
                };
                Debug.Log(JsonConvert.SerializeObject(_parameters));
                BoomDaoUtility.Instance.ExecuteActionWithParameter(INCREASE_LEADERBOARD_POINTS, _parameters,null);
                LeanTween.value(gameObject, 0, GameState.pointsChange.points, 2f).setOnUpdate(val =>
                {
                    totalCoinsValue.text = "" + Math.Floor(GameState.pointsChange.oldPoints + val);
                    deltaPoints.text = "+" + Math.Floor(val);
                }).setEaseInOutCirc().setDelay(1f).setOnComplete(() =>
                {
                    if (checkIfIWon > 0)
                    {
                        luckyWheelUI.ShowReward();
                    }
                }
                );
            }

            if (!string.IsNullOrEmpty(GameState.pointsChange.reason))
            {
                reasonText.SetActive(true);
                reasonText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GameState.pointsChange.reason;
            }
        }

        private void SaveKittyHealth()
        {
            float _maxHp = 100;
            float _minutesItWillTakeToRecover = (RecoveryHandler.RecoveryInMinutes / _maxHp) * (_maxHp - PlayerManager.HealthAtEnd);
            if (_minutesItWillTakeToRecover <= 1)
            {
                return;
            }

            DateTime _recoveryEnds = DateTime.UtcNow.AddMinutes(_minutesItWillTakeToRecover);
            GameState.selectedNFT.RecoveryEndDate = _recoveryEnds;

            Debug.Log("Recovery ends: "+_recoveryEnds);
            BoomDaoUtility.Instance.ExecuteActionWithParameter(HURT_KITTY,
                new List<ActionParameter>
                {
                    new() { Key = PlayerData.KITTY_RECOVERY_KEY, Value = Utilities.DateTimeToNanoseconds(_recoveryEnds).ToString() },
                    new() { Key = PlayerData.KITTY_KEY, Value = GameState.selectedNFT.imageUrl }
                }, null);

        }
    }
}