using System;
using System.Collections.Generic;
using System.Linq;
using Boom;
using Boom.Patterns.Broadcasts;
using Boom.Values;
using Candid;
using Candid.World.Models;
using UnityEngine;
using Action = System.Action;

namespace BoomDaoWrapper
{
    public class BoomDaoUtility : MonoBehaviour
    {
        public static BoomDaoUtility Instance;
        public const string ICK_KITTIES = "rw7qm-eiaaa-aaaak-aaiqq-cai";
        
        private const string AMOUNT_KEY = "amount";
        private const string NAME_KEY = "name";
        private const string DEFAULT_KEY = "None";

        private Action loginCallback;
        private bool canLogin;

        public bool CanLogin => canLogin;

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

        private void OnEnable()
        {
            BroadcastState.Register<WaitingForResponse>(AllowLogin);
        }

        private void OnDisable()
        {
            BroadcastState.Unregister<WaitingForResponse>(AllowLogin);
        }


        #region Login

        private void AllowLogin(WaitingForResponse _response)
        {
            bool _completed = _response.value;
            canLogin = _completed;
        }

        public void Login(Action _callBack)
        {
            loginCallback = _callBack;
            Broadcast.Invoke<UserLoginRequest>();
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        }

        private void LoginDataChangeHandler(MainDataTypes.LoginData _data)
        {
            if (_data.state != MainDataTypes.LoginData.State.LoggedIn)
            {
                return;
            }
            
            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
            loginCallback?.Invoke();
        }

        #endregion

        #region Actions
        
        public async void ExecuteAction(string _actionId, Action<List<ActionOutcome>> _callBack)
        {
            var _actionResult = await ActionUtil.ProcessAction(_actionId);
            if (_actionResult.IsErr)
            {
                string _errorMessage = _actionResult.AsErr().content;
                Debug.LogError(_errorMessage);
                _callBack?.Invoke(null);
                return;
            }

            var _expectedResult = _actionResult.AsOk();
            _callBack?.Invoke(GetActionOutcomes(_expectedResult));
        }
        
        public async void ExecuteActionWithParameter(string _actionId, List<ActionParameter> _parameters, Action<List<ActionOutcome>> _callBack)
        {
            List<Field> _fields = _parameters.Select(_parameter => new Field(_parameter.Key, _parameter.Value)).ToList();
            var _actionResult = await ActionUtil.ProcessAction(_actionId, _fields);
            bool _isError = _actionResult.IsErr;
            if (_isError)
            {
                return;
            }
            
            var _expectedResult = _actionResult.AsOk();
            _callBack?.Invoke(GetActionOutcomes(_expectedResult));
        }

        private List<ActionOutcome> GetActionOutcomes(ProcessedActionResponse _action)
        {
            var _callerOutcomes = _action.callerOutcomes;
            var _entityOutcomes = _callerOutcomes.entityOutcomes;

            List<ActionOutcome> _incrementalOutcomes = new();

            foreach (var _keyValue in _entityOutcomes)
            {
                Debug.Log($"{_keyValue.Key},{_keyValue.Value}");
                var _entityEdit = _keyValue.Value;
                string _entityId = _entityEdit.eid;
                bool _configEntityNameFound =
                    _entityEdit.GetConfigFieldAs(BoomManager.Instance.WORLD_CANISTER_ID, NAME_KEY, out string _entityName, DEFAULT_KEY);

                if (_configEntityNameFound == false)
                {
                     Debug.LogWarning($"Could not find the config field name of entityId: {_entityId}");
                    continue;
                }

                bool _fieldAmountFound = _entityEdit.TryGetOutcomeFieldAsDouble(AMOUNT_KEY, out var _amount);
                Debug.Log(_fieldAmountFound);
                if (_fieldAmountFound == false)
                {
                    break;
                }

                _incrementalOutcomes.Add(new ActionOutcome {Name = _entityName,Value = _amount.Value});
            }

            return _incrementalOutcomes;
        }

        #endregion

        #region Data

        public UResult<MainDataTypes.LoginData, string> GetLoginData => UserUtil.GetLogInData();
        public UResult<Data<DataTypes.NftCollection>, string> GetNFTData => UserUtil.GetDataSelf<DataTypes.NftCollection>();

        public string GetString(string _entityId,string _fieldName)
        {
            return EntityUtil.TryGetFieldAsText(UserUtil.GetPrincipal(), _entityId, _fieldName, out string _value) 
                ? _value 
                : string.Empty;
        }   
        
        public double GetDouble(string _entityId,string _fieldName)
        {
            return EntityUtil.TryGetFieldAsDouble(UserUtil.GetPrincipal(), _entityId, _fieldName, out double _value) 
                ? _value 
                : 0;
        }

        #endregion

    }
}