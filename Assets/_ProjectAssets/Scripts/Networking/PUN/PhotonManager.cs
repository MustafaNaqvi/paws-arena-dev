using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public event Action OnStartedConnection;
    public event Action OnConnectedServer;
    public event Action OnCreatingRoom;
    public event Action OnRoomLeft;
    public static Action OnFailedToCreateRoom;
    public static Action OnJoinedFriendlyRoom;

    [SerializeField]
    private byte maxPlayersPerRoom = 2;
    private string gameVersion = "1";

    private bool isRoomCreated = false;
    private bool isSinglePlayer = false;
    private string friendlyRoomName;


    #region ACTIONS
    public void Connect()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        OnStartedConnection?.Invoke();
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
        else
        {
            OnConnectedServer?.Invoke();
        }
    }

    public void ConnectToRandomRoom()
    {
        isSinglePlayer = false;
        PhotonNetwork.JoinRandomRoom();
    }

    public void TryLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void JoinFriendlyRoom(string _roomName)
    {
        friendlyRoomName = _roomName;
        PhotonNetwork.JoinRoom(friendlyRoomName);
    }
    
    #endregion
    #region CALLBACKS

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "principalId", GameState.principalId } });

        OnConnectedServer?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        isRoomCreated = true;

        string roomName = Guid.NewGuid().ToString();
        PhotonNetwork.CreateRoom(roomName, new RoomOptions{ MaxPlayers = maxPlayersPerRoom });
        GameState.roomName = roomName;
        OnCreatingRoom?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        isRoomCreated = true;

        PhotonNetwork.CreateRoom(friendlyRoomName, 
            new RoomOptions
            {
                IsVisible = false,
                MaxPlayers = maxPlayersPerRoom,
            });
        GameState.roomName = friendlyRoomName;
        OnCreatingRoom?.Invoke();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OnFailedToCreateRoom?.Invoke();
    }

    public void CreateSinglePlayerRoom()
    {
        isRoomCreated = true;
        isSinglePlayer = true;
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
        OnCreatingRoom?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        if (isRoomCreated)
        {
            if (!isSinglePlayer)
            {
                PhotonNetwork.LoadLevel("GameRoom");
            }
            else
            {
                PhotonNetwork.LoadLevel("SinglePlayerGameRoom");
            }
        }

        if (PhotonNetwork.CurrentRoom is { IsVisible: false })
        {
            OnJoinedFriendlyRoom?.Invoke();
        }
    }

    public override void OnLeftRoom()
    {
        OnRoomLeft?.Invoke();
    }
    #endregion
}
