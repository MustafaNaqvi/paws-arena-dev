using BoomDaoWrapper;
using TMPro;
using UnityEngine;

public class ConnectingToServer : MonoBehaviour
{
    [Header("Managers")]
    public LobbyUIManager lobbyUIManager;

    [Header("Internals")]
    public GameObject connectButton;
    public GameObject logText;

    [SerializeField] private GameObject loginFailed;
    private TextMeshProUGUI statusDisplay;

    public void ConnectToWallet()
    {
        BoomDaoUtility.Instance.Login(ConnectToFirebase);
        
        connectButton.SetActive(false);
        logText.SetActive(true);
        statusDisplay = logText.GetComponent<TextMeshProUGUI>();

        statusDisplay.text = "Waiting the connection with ICP Wallet to be approved...";
    }

    private void ConnectToFirebase()
    {
        statusDisplay.text = "Connection made!";

        SetupNftData();

        var _loginDataResult = BoomDaoUtility.Instance.GetLoginData;
        var _loginDataAsOk = _loginDataResult.AsOk();

        GameState.principalId = _loginDataAsOk.principal;
        FirebaseManager.Instance.TryLoginAndGetData(GameState.principalId, OnLoginFinished);
    }
    
    private void SetupNftData()
    {
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?&tokenid=hvtag-6ykor-uwiaa-aaaaa-cqace-eaqca-aaabd-a" });

        var _nftCollectionsResult = BoomDaoUtility.Instance.GetNFTData;
        if (_nftCollectionsResult.IsErr)
        {
            Debug.Log($"{_nftCollectionsResult.AsErr()} "+ nameof(ConnectingToServer));
            return;
        }

        var _nftCollectionsAsOk = _nftCollectionsResult.AsOk();

        foreach (var _keyValue in _nftCollectionsAsOk.elements)
        {
            var _collection = _keyValue.Value;

            if (_collection.canisterId != BoomDaoUtility.ICK_KITTIES)
            {
                continue;
            }
            
            foreach (var _token in _collection.tokens)
            {
                Debug.Log($"Kitty Nft data fetch, index: {_token.index}, url: {_token.url}: "+nameof(ConnectingToServer));
                GameState.nfts.Add(new NFT { imageUrl = _token.url });
            }
            break;
        }
    }

    private void OnLoginFinished(bool _result)
    {
        if (_result)
        {
            DataManager.Instance.SubscribeHandlers();
            lobbyUIManager.OpenNFTSelectionScreen();
            // ChallengesManager.Instance.Init();
        }
        else
        {
            loginFailed.SetActive(true);
        }
    }
}
