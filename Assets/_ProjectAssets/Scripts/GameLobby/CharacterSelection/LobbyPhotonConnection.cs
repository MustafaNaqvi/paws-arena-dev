using TMPro;
using UnityEngine;

public class LobbyPhotonConnection : MonoBehaviour
{
    [Header("Managers")]
    public PhotonManager photonManager;

    [Header("Internals")]
    public GameObject startButton;
    public TextMeshProUGUI label;

    private void OnEnable()
    {
        Init();
        photonManager.OnCreatingRoom += OnCreatingRoom;
    }

    private void OnDisable()
    {
        photonManager.OnCreatingRoom -= OnCreatingRoom;
    }

    private void Init()
    {
        startButton.SetActive(true);
        label.text = string.Empty;
    }

    public void TryJoinRoom()
    {
        startButton.SetActive(false);
        label.text = string.Empty;

        photonManager.ConnectToRandomRoom();
    }

    private void OnCreatingRoom()
    {
        label.text = "No open match. Making a new one...";
    }
}
