using System.Collections.Generic;
using UnityEngine;
using Boom;
using Boom.Patterns.Broadcasts;
using Candid;
using TMPro;

public class ConnectingToServer : MonoBehaviour
{
    [Header("Managers")]
    public LobbyUIManager lobbyUIManager;

    [Header("Internals")]
    public GameObject connectButton;
    public TextMeshProUGUI logText;

    [SerializeField] private GameObject loginFailed;
    
    public void ConnectToWallet()
    {
        connectButton.SetActive(false);
        logText.gameObject.SetActive(true);
        logText.text = "Waiting the connection with ICP Wallet to be approved...";
        
        Broadcast.Invoke<UserLoginRequest>();
    }

    private void OnEnable()
    {
        BroadcastState.Register<WaitingForResponse>(AllowButtonInteractionHandler, true);
        UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        UserUtil.AddListenerDataChangeSelf<DataTypes.NftCollection>(NftDataChangeHandler);
    }

    private void OnDisable()
    {
        BroadcastState.Unregister<WaitingForResponse>(AllowButtonInteractionHandler);
        UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.NftCollection>(NftDataChangeHandler);
    }
    
    private void AllowButtonInteractionHandler(WaitingForResponse _response)
    {
        bool _completed = _response.value;
        connectButton.gameObject.SetActive(!_completed);
    }

    private void LoginDataChangeHandler(MainDataTypes.LoginData _data)
    {
        if (_data.state != MainDataTypes.LoginData.State.LoggedIn)
        {
            return;
        }
        
        logText.text = "Connection made. Waiting for NFTs...";
    }
    
    private void NftDataChangeHandler(Data<DataTypes.NftCollection> _data)
    {
        if (!IsLoggedIn())
        {
            return;
        }
        
        //Setup user principal id for API communication (will be deleted after full BoomDao implementation) 
        GameState.principalId = UserUtil.GetPrincipal();
        
        logText.text = "Connection made. Waiting for NFTs...";
        foreach (string _url in GetNftUrls(_data.elements))
        {
            GameState.nfts.Add(new NFT() { imageUrl = _url });
        }
        
        //Mock one NFT for testing:
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?&tokenid=hvtag-6ykor-uwiaa-aaaaa-cqace-eaqca-aaabd-a" });
        
        //Connect to firebase
        FirebaseManager.Instance.TryLoginAndGetData(GameState.principalId, OnLoginFinished);
    }

    private bool IsLoggedIn()
    {
        if (!UserUtil.IsDataValidSelf<DataTypes.NftCollection>())
        {
            return false;
        }

        return UserUtil.IsUserLoggedIn();
    }

    private List<string> GetNftUrls(Dictionary<string, DataTypes.NftCollection> _elements)
    {
        List<string> _urls = new List<string>();
        foreach (var _keyValue in _elements)
        {
            DataTypes.NftCollection _collection = _keyValue.Value;

            if (_collection.canisterId != BoomManager.Instance.WORLD_CANISTER_ID)
            {
                continue;
            }

            foreach (DataTypes.NftCollection.Nft _token in _collection.tokens)
            {
                _urls.Add(_token.url);
            }
            break;
        }

        return _urls;
    }

    private void OnLoginFinished(bool _result)
    {
        if (_result)
        {
            DataManager.Instance.SubscribeHandlers();
            lobbyUIManager.OpenNFTSelectionScreen();
            ChallengesManager.Instance.Init();
        }
        else
        {
            loginFailed.SetActive(true);
        }
    }
}
