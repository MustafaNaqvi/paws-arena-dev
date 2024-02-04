using System;
using System.Collections.Generic;
using Boom;
using Candid;
using UnityEngine;


namespace BoomDaoWrapper
{

    public class BoomDaoUtility : MonoBehaviour
    {
        public static BoomDaoUtility Instance;
        public const string BATTLE_WON_ACTION_KEY = "battle_outcome_won";
        
        private const string AMOUNT_KEY = "amount";

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

        public async void ExecuteIncrementalAction(string _actionId, Action<List<IncrementalActionOutcome>> _callBack)
        {
            Debug.Log(_actionId);
            var _actionResult = await ActionUtil.ProcessAction(_actionId);
            if (_actionResult.IsErr)
            {
                string _errorMessage = _actionResult.AsErr().content;
                Debug.LogError(_errorMessage);
                _callBack?.Invoke(null);
                return;
            }

            var _expectedResult = _actionResult.AsOk();

            var _callerOutcomes = _expectedResult.callerOutcomes;
            var _entityOutcomes = _callerOutcomes.entityOutcomes;

            List<IncrementalActionOutcome> _incrementalOutcomes = new();

            foreach (var _keyValue in _entityOutcomes)
            {
                var _entityEdit = _keyValue.Value;
                string _entityId = _entityEdit.eid;
                bool _configEntityNameFound =
                    _entityEdit.GetConfigFieldAs(BoomManager.Instance.WORLD_CANISTER_ID, "name", out string _entityName, "None");

                if (_configEntityNameFound == false)
                {
                    Debug.LogWarning($"Could not find the config field name of entityId: {_entityId}");
                    continue;
                }

                bool _fieldAmountFound = _entityEdit.TryGetOutcomeFieldAsDouble(AMOUNT_KEY, out var _amount);

                if (_fieldAmountFound == false)
                {
                    break;
                }

                _incrementalOutcomes.Add(new IncrementalActionOutcome {Name = _entityName,Value = _amount.Value});
            }
            
            _callBack?.Invoke(_incrementalOutcomes);
        }
    }
}