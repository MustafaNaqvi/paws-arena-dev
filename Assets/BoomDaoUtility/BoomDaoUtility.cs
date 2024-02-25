using System;
using System.Collections.Generic;
using System.Linq;
using Boom;
using Boom.Patterns.Broadcasts;
using Boom.Values;
using Candid.World.Models;
using UnityEngine;
using WebSocketSharp;
using Action = System.Action;

namespace BoomDaoWrapper
{
    public class BoomDaoUtility : MonoBehaviour
    {
        public static BoomDaoUtility Instance;
        public static Action<string> OnDataUpdated;
        public const string ICK_KITTIES = "rw7qm-eiaaa-aaaak-aaiqq-cai";

        private const string AMOUNT_KEY = "amount";
        private const string VALUE_KEY = "value";

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
            UserUtil.AddListenerDataChangeSelf<DataTypes.Entity>(OnEntityDataChangeHandler);
        }

        private void OnDisable()
        {
            BroadcastState.Unregister<WaitingForResponse>(AllowLogin);
            UserUtil.RemoveListenerDataChangeSelf<DataTypes.Entity>(OnEntityDataChangeHandler);
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
                _callBack?.Invoke(default);
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
                _callBack?.Invoke(default);
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
                var _entityEdit = _keyValue.Value;
                string _entityId = _entityEdit.eid;
                bool _fieldAmountFound = _entityEdit.TryGetOutcomeFieldAsDouble(AMOUNT_KEY, out var _amount);
                if (_fieldAmountFound == false)
                {
                    break;
                }

                _incrementalOutcomes.Add(new ActionOutcome { Name = _entityId, Value = _amount.Value });
            }

            return _incrementalOutcomes;
        }

        #endregion

        #region Data

        public UResult<MainDataTypes.LoginData, string> GetLoginData => UserUtil.GetLogInData();
        public UResult<Data<DataTypes.NftCollection>, string> GetNftData => UserUtil.GetDataSelf<DataTypes.NftCollection>();

        public List<int> GetListOfInts(string _entityId)
        {
            List<string> _listOfStrings = GetListOfStrings(_entityId);
            if (_listOfStrings==default)
            {
                return default;
            }

            List<int> _listOfInts = new List<int>();
            foreach (var _string in _listOfStrings)
            {
                if (_string.IsNullOrEmpty())
                {
                    continue;
                }
                _listOfInts.Add(int.Parse(_string));
            }

            return _listOfInts;
        }

        public List<string> GetListOfStrings(string _entityId)
        {
            var _entityData = GetString(_entityId, VALUE_KEY);
            if (_entityData==default)
            {
                return default;
            }
            
            return _entityData.Split(',').ToList();
        }

        public string GetString(string _entityId, string _fieldName)
        {
            return EntityUtil.TryGetFieldAsText(UserUtil.GetPrincipal(), _entityId, _fieldName, out string _value) ? _value : string.Empty;
        }

        public double GetDouble(string _entityId, string _fieldName)
        {
            return EntityUtil.TryGetFieldAsDouble(UserUtil.GetPrincipal(), _entityId, _fieldName, out double _value) ? _value : 0;
        }

        public Dictionary<string, string> GetEntityData(string _entityId)
        {
            return EntityUtil.TryGetEntity(UserUtil.GetPrincipal(), _entityId, out DataTypes.Entity _entityData) ? _entityData.fields : default;
        }

        public void RemoveEntity(string _entityId)
        {
            //todo delete entity
        }

        private void OnEntityDataChangeHandler(Data<DataTypes.Entity> _changedEntities)
        {
            foreach (var _changedEntity in _changedEntities.elements.Values)
            {
                OnDataUpdated?.Invoke(_changedEntity.eid);
            }
        }

        public int GetConfigDataAsInt(string _configId, string _fieldName)
        {
            string _int = GetConfigDataAsString(_configId, _fieldName);
            if (_int == default)
            {
                return default;
            }

            return int.Parse(_int);
        }

        public DateTime GetConfigDataAsDate(string _configId, string _fieldName)
        {
            string _date = GetConfigDataAsString(_configId, _fieldName);
            if (_date == default)
            {
                return default;
            }
            int[] _time = _date.Split('.').Select(int.Parse).ToArray();
            DateTime _output = new DateTime(year: _time[2], month: _time[1], day: _time[0]);
            return _output;
        }

        private string GetConfigDataAsString(string _configId, string _fieldName)
        {
            List<ConfigData> _configs = GetConfigData(_configId);
            if (_configs == default)
            {
                return default;
            }

            foreach (var _config in _configs)
            {
                if (_config.Name == _fieldName)
                {
                    return _config.Value;
                }
            }

            return default;
        }

        public List<ConfigData> GetConfigData(string _configId)
        {
            bool _hasConfig = ConfigUtil.TryGetConfig(BoomManager.Instance.WORLD_CANISTER_ID, _configId, out MainDataTypes.AllConfigs.Config _config);
            if (!_hasConfig)
            {
                Debug.Log("Didn't manage to find config with id: " + _configId);
                return default;
            }

            List<ConfigData> _outcomes = new();
            foreach (var _field in _config.fields)
            {
                _outcomes.Add(new ConfigData { Name = _field.Key, Value = _field.Value });
            }

            return _outcomes;
        }

        public List<ActionOutcome> GetActionOutcomes(string _actionId)
        {
            bool _hasConfig = ConfigUtil.TryGetAction(BoomManager.Instance.WORLD_CANISTER_ID, _actionId, out MainDataTypes.AllAction.Action _action);
            if (!_hasConfig)
            {
                return default;
            }

            List<ActionOutcome> _actionOutcomes = new();
            foreach (var _outcome in _action.callerAction.Outcomes)
            {
                foreach (var _possibleOutcome in _outcome.PossibleOutcomes)
                {
                    var _outcomeOption = _possibleOutcome.Option;
                    if (_outcomeOption.Tag == ActionOutcomeOption.OptionInfoTag.UpdateEntity)
                    {
                        var _entityOutcome = _outcomeOption.AsUpdateEntity();
                        foreach (var _update in _entityOutcome.Updates)
                        {
                            if (_update.Value is IncrementNumber _increment)
                            {
                                _actionOutcomes.Add(new ActionOutcome { Name = _entityOutcome.Eid, Value = _increment.FieldValue.AsNumber() });
                            }
                        }
                    }
                }
            }

            return _actionOutcomes;
        }

        #endregion
    }
}