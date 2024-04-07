using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Image levelProgressDisplay;
    [SerializeField] private TextMeshProUGUI levelDisplay;
    [Space]
    [SerializeField] private RecoveryHandler mainRecoveryHandler;
    [SerializeField] private GameObject connectingToRoom;
    [SerializeField] private TextMeshProUGUI connectingToRoomText;
    [SerializeField] private PhotonManager photonManager;
    [SerializeField] private LobbyPhotonConnection lobbyPhotonConnection;
    [SerializeField] private Button lobby;
    [SerializeField] private GameObject settingsHolder;
    [SerializeField] private Button settings;
    [SerializeField] private Button fightButton;
    [SerializeField] private Button tutorialButton;

    private void OnEnable()
    {
        tutorialButton.onClick.AddListener(ShowTutorial);
        settings.onClick.AddListener(ShowSettings);
        lobby.onClick.AddListener(ShowLobby);
        GameState.selectedNFT.UpdatedRecoveryTime += CheckIfShouldStopRecovering;
        PlayerData.OnUpdatedExp += ShowLevelProgress;
        fightButton.onClick.AddListener(JoinRoom);

        ShowLevelProgress();
        mainRecoveryHandler.ShowRecovery(GameState.selectedNFT.RecoveryEndDate);
    }

    private void OnDisable()
    {
        tutorialButton.onClick.RemoveListener(ShowTutorial);
        settings.onClick.RemoveListener(ShowSettings);
        lobby.onClick.RemoveListener(ShowLobby);
        GameState.selectedNFT.UpdatedRecoveryTime -= CheckIfShouldStopRecovering;
        PlayerData.OnUpdatedExp -= ShowLevelProgress;
        fightButton.onClick.RemoveListener(JoinRoom);
    }

    private void ShowTutorial()
    {
        SceneManager.Instance.LoadTutorial();
    }

    private void ShowSettings()
    {
        settingsHolder.SetActive(true);
    }

    private void ShowLobby()
    {
        SceneManager.Instance.LoadLeaderboard();
    }

    private void CheckIfShouldStopRecovering()
    {
        if (GameState.selectedNFT.RecoveryEndDate <= DateTime.UtcNow)
        {
            mainRecoveryHandler.StopRecovery();
        }
    }
    
    private void ShowLevelProgress()
    {
        levelProgressDisplay.fillAmount = DataManager.Instance.PlayerData.ExperienceOnCurrentLevel / (float)DataManager.Instance.PlayerData.ExperienceForNextLevel;
        levelDisplay.text = DataManager.Instance.PlayerData.Level.ToString();
    }

    private void Start()
    {
        if (GameState.selectedNFT == null)
        {
            SceneManager.Instance.LoadNftSelection();
        }
    }

    private void JoinRoom()
    {
        if (!GameState.selectedNFT.CanFight)
        {
            RecoveryMessageDisplay.Instance.ShowMessage();
            SceneManager.Instance.LoadNftSelection();
            return;
        }
        
        connectingToRoom.SetActive(true);

        connectingToRoomText.text = "Connecting to Multiplayer Server(" + PhotonNetwork.CloudRegion + ")...";

        photonManager.OnConnectedServer += () =>
        {
            connectingToRoomText.text = "Connected succeeded!";
            lobbyPhotonConnection.TryJoinRoom();
        };

        photonManager.Connect();
    }
    
    public void GoToConnecting()
    {
        connectingToRoom.SetActive(true);
        connectingToRoomText.text = "Connecting to Multiplayer Server(" + PhotonNetwork.CloudRegion + ")...";
    }
    
    public void TryConnectToFriendlyRoom(string _name)
    {
        if (!GameState.selectedNFT.CanFight)
        {
            RecoveryMessageDisplay.Instance.ShowMessage();
            SceneManager.Instance.LoadNftSelection();
            return;
        }

        photonManager.OnConnectedServer += () =>
        {
            connectingToRoomText.text = "Connected succeeded!";
            lobbyPhotonConnection.photonManager.JoinFriendlyRoom(_name);;
        };

        photonManager.Connect();
    }
}
