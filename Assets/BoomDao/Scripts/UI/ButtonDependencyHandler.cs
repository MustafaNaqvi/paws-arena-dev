using Boom;
using Boom.Patterns.Broadcasts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDependencyHandler : MonoBehaviour
{
    [SerializeField, Header("Dependensies")] string[] actionsIds;

    [SerializeField] bool userIdEntityData;
    [SerializeField] bool userIdTokenData;
    [SerializeField] bool userIdNftData;

    [SerializeField] bool worldIdEntityData;
    [SerializeField] bool worldIdTokenData;
    [SerializeField] bool worldIdNftData;

    Button button;

    private void Start()
    {
        if (!button) button = GetComponent<Button>();

        if (button)
        {
            if (actionsIds.Length > 0)
            {
                foreach (var item in actionsIds)
                {
                    BroadcastState.Register<ActionExecutionState>(_Update, default, item);
                }
            }

            //We register LoginDataChangeHandler to MainDataTypes.LoginData change event to initialize userNameInputField with the user's username
            UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            if (userIdEntityData) UserUtil.AddListenerDataTypeLoadingStateChangeSelf<DataTypes.Entity>(DataTypeLoadingStateChange, default);
            if (userIdTokenData) UserUtil.AddListenerDataTypeLoadingStateChangeSelf<DataTypes.Token>(DataTypeLoadingStateChange, default);
            if (userIdNftData) UserUtil.AddListenerDataTypeLoadingStateChangeSelf<DataTypes.NftCollection>(DataTypeLoadingStateChange, default);

            if (worldIdEntityData) UserUtil.AddListenerDataTypeLoadingStateChange<DataTypes.Entity>(DataTypeLoadingStateChange, default, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdTokenData) UserUtil.AddListenerDataTypeLoadingStateChange<DataTypes.Token>(DataTypeLoadingStateChange, default, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdNftData) UserUtil.AddListenerDataTypeLoadingStateChange<DataTypes.NftCollection>(DataTypeLoadingStateChange, default, BoomManager.Instance.WORLD_CANISTER_ID);
        }

        _UpdateButton();
    }

    private void LoginDataChangeHandler(MainDataTypes.LoginData data)
    {
        if (data.state != MainDataTypes.LoginData.State.LoggedIn) return;
        _UpdateButton();
    }

    private void OnDestroy()
    {
        if (button)
        {
            if (actionsIds.Length > 0)
            {
                foreach (var item in actionsIds)
                {
                    BroadcastState.Unregister<ActionExecutionState>(_Update, item);
                }
            }

            UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);

            if (userIdEntityData) UserUtil.RemoveListenerDataTypeLoadingStateChangeSelf<DataTypes.Entity>(DataTypeLoadingStateChange);
            if (userIdTokenData) UserUtil.RemoveListenerDataTypeLoadingStateChangeSelf<DataTypes.Token>(DataTypeLoadingStateChange);
            if (userIdNftData) UserUtil.RemoveListenerDataTypeLoadingStateChangeSelf<DataTypes.NftCollection>(DataTypeLoadingStateChange);

            if (worldIdEntityData) UserUtil.RemoveListenerDataTypeLoadingStateChange<DataTypes.Entity>(DataTypeLoadingStateChange, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdTokenData) UserUtil.RemoveListenerDataTypeLoadingStateChange<DataTypes.Token>(DataTypeLoadingStateChange, BoomManager.Instance.WORLD_CANISTER_ID);
            if (worldIdNftData) UserUtil.RemoveListenerDataTypeLoadingStateChange<DataTypes.NftCollection>(DataTypeLoadingStateChange, BoomManager.Instance.WORLD_CANISTER_ID);
        }
    }


    private void DataTypeLoadingStateChange(DataLoadingState<DataTypes.NftCollection> state)
    {
        _UpdateButton();
    }

    private void DataTypeLoadingStateChange(DataLoadingState<DataTypes.Token> state)
    {
        _UpdateButton();
    }

    private void DataTypeLoadingStateChange(DataLoadingState<DataTypes.Entity> state)
    {
        _UpdateButton();
    }

    private void _Update(ActionExecutionState change)
    {
        _UpdateButton();
    }

    private void _UpdateButton()
    {

        if (actionsIds.Length > 0)
        {
            foreach (var actionDependency in actionsIds)
            {

                if(BroadcastState.TryRead(out ActionExecutionState state, actionDependency))
                {
                    if(state.inProcess)
                    {
                        button.interactable = false;

                        return;
                    }
                }
            }
        }

        if (userIdEntityData)
        {
            if (UserUtil.IsDataValidSelf<DataTypes.Entity>() == false || UserUtil.IsDataLoadingSelf<DataTypes.Entity>())
            {
                button.interactable = false;
                return;
            }
        }
        if (userIdTokenData)
        {
            if (UserUtil.IsDataValidSelf<DataTypes.Token>() == false || UserUtil.IsDataLoadingSelf<DataTypes.Token>())
            {
                button.interactable = false;
                return;
            }
        }
        if (userIdNftData)
        {
            if (UserUtil.IsDataValidSelf<DataTypes.NftCollection>() == false || UserUtil.IsDataLoadingSelf<DataTypes.NftCollection>())
            {
                button.interactable = false;
                return;
            }
        }

        string worldId = BoomManager.Instance.WORLD_CANISTER_ID;

        if (worldIdEntityData)
        {
            if (UserUtil.IsDataValid<DataTypes.Entity>(worldId) == false || UserUtil.IsDataLoading<DataTypes.Entity>(worldId))
            {
                button.interactable = false;
                return;
            }
        }
        if (worldIdTokenData)
        {
            if (UserUtil.IsDataValid<DataTypes.Token>(worldId) == false || UserUtil.IsDataLoading<DataTypes.Token>(worldId))
            {
                button.interactable = false;
                return;
            }
        }
        if (worldIdNftData)
        {
            if (UserUtil.IsDataValid<DataTypes.NftCollection>(worldId) == false || UserUtil.IsDataLoading<DataTypes.NftCollection>(worldId))
            {
                button.interactable = false;
                return;
            }
        }

        button.interactable = true;
    }
}
