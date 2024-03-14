using System.Collections.Generic;
using Photon.Pun;
using BoomDaoWrapper;
using UnityEngine;

public class PlayerNicknameButton : MonoBehaviour
{
    private const string SET_NAME_ACTION_ID = "setPlayerName";
    
    [SerializeField] private TMPro.TextMeshProUGUI nicknameText;
    [SerializeField] private InputModal inputModal;

    private void Start()
    {
        string _nickname = DataManager.Instance.PlayerData.Username;
        
        nicknameText.text = "";
        if (!string.IsNullOrEmpty(_nickname.Trim()))
        {
            SetPlayerName(_nickname);
        }
        else
        {
            EnableEdit(false);
        }
    }

    private void EnableEdit(bool _isCancelable)
    {
        inputModal.Show("Nickname", "Nickname", _isCancelable, SaveNewName);
    }
    
    private void SaveNewName(string _nickname)
    {
        inputModal.ManageButton(false);
        BoomDaoUtility.Instance.ExecuteActionWithParameter(
            SET_NAME_ACTION_ID,
            new List<ActionParameter>(){ new() {Key = PlayerData.NAME_KEY, Value = _nickname}}, _outcomes =>
            {
                HandleSetNameFinished(_outcomes, _nickname);
            });
        
    }

    private void HandleSetNameFinished(List<ActionOutcome> _, string _newName)
    {
        inputModal.ManageButton(true);
        SetPlayerName(_newName);
    }

    private void SetPlayerName(string _newName)
    {
        GameState.nickname = PhotonNetwork.NickName = nicknameText.text = _newName;
        inputModal.Hide();
    }
}
