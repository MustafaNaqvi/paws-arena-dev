using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateFriendlyMatch : MonoBehaviour
{
    [SerializeField] private Button showCreateFriendly;
    [SerializeField] private GameObject holder;
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private LobbyUIManager lobbyUIManager;

    private void OnEnable()
    {
        showCreateFriendly.onClick.AddListener(ShowPanel);
        closeButton.onClick.AddListener(Close);
        continueButton.onClick.AddListener(JoinRoom);
        PhotonManager.OnFailedToCreateRoom += EnableNewRoomCreation;
        PhotonManager.OnJoinedFriendlyRoom += ShowConnecting;
    }

    private void OnDisable()
    {
        showCreateFriendly.onClick.RemoveListener(ShowPanel);
        closeButton.onClick.RemoveListener(Close);
        continueButton.onClick.RemoveListener(JoinRoom);
        PhotonManager.OnFailedToCreateRoom -= EnableNewRoomCreation;
        PhotonManager.OnJoinedFriendlyRoom -= ShowConnecting;
    }

    private void ShowPanel()
    {
        roomName.text = string.Empty;
        holder.SetActive(true);
    }

    private void Close()
    {
        holder.SetActive(false);
    }

    private void JoinRoom()
    {
        string _roomName = roomName.text;
        if (!ValidateName(_roomName))
        {
            return;
        }
        
        ManageInteractables(false);
        lobbyUIManager.TryConnectToFriendlyRoom(_roomName);
    }

    private bool ValidateName(string _roomName)
    {
        if (string.IsNullOrEmpty(_roomName))
        {
            DialogsManager.Instance.ShowOkDialog("Please enter room name");
            return false;
        }

        int _length = _roomName.Length;
        if (_length<5)
        {
            DialogsManager.Instance.ShowOkDialog("Room name must have more than 5 characters");
            return false;
        }

        if (_length>10)
        {
            DialogsManager.Instance.ShowOkDialog("Room name must have less than 10 characters");
            return false;
        }

        return true;
    }

    private void EnableNewRoomCreation()
    {
        DialogsManager.Instance.ShowOkDialog("Room with this name already exists, please select another name");
        ManageInteractables(true);
    }

    private void ShowConnecting()
    {
        lobbyUIManager.GoToConnecting();
    }

    private void ManageInteractables(bool _status)
    {
        roomName.interactable = _status;
        closeButton.interactable = _status;
        continueButton.interactable = _status;
    }
}
