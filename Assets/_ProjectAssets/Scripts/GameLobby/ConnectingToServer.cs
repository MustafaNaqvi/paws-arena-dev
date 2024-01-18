using Boom;
using Boom.Patterns.Broadcasts;
using Boom.Utility;
using UnityEngine;

public class ConnectingToServer : MonoBehaviour
{
    [Header("Managers")]
    public LobbyUIManager lobbyUIManager;

    [Header("Internals")]
    public GameObject connectButton;
    public GameObject logText;

    [SerializeField] private GameObject loginFailed;

    private void Awake()
    {
        UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        UserUtil.AddListenerDataChangeSelf<DataTypes.NftCollection>(NftDataChangeHandler);
    }

    private void OnDestroy()
    {
        UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.NftCollection>(NftDataChangeHandler);
    }


    private void LoginDataChangeHandler(MainDataTypes.LoginData data)
    {
        if (data.asAnon)
        {
            //Return, client is not logged in yet
            return;
        }
        var text = logText.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "Connection made. Waiting for NFTs...";
    }

    private void NftDataChangeHandler(Data<DataTypes.NftCollection> data)
    {
        if (UserUtil.IsDataValidSelf<DataTypes.NftCollection>() == false) return;

        if (UserUtil.IsUserLoggedIn() == false) return;

        $"Setup ICKitties Nft Collection".Log(typeof(ConnectingToServer).Name);

        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?&tokenid=hvtag-6ykor-uwiaa-aaaaa-cqace-eaqca-aaabd-a" });

        //Look for the ICKitties Collection and  add the user's nft images to the GameState.nfts
        foreach (var keyValue in data.elements)
        {
            var collection = keyValue.Value;

            if(collection.canisterId == "rw7qm-eiaaa-aaaak-aaiqq-cai") //IF ICKitties Collection, then ...
            {
                $"Kitty Nft Collection was found".Log(typeof(ConnectingToServer).Name);

                foreach (var token in collection.tokens)
                {
                    $"Kitty Nft data fetche, index: {token.index}, url: {token.url}".Log(typeof(ConnectingToServer).Name);
                    GameState.nfts.Add(new NFT() { imageUrl = token.url });
                }

                break;
            }
        }

        var loginDataResult = UserUtil.GetLogInData();
        if (loginDataResult.IsErr)
        {
            $"{loginDataResult.AsErr()}".Error(typeof(ConnectingToServer).Name);
            return;
        }
        
        GameState.principalId = UserUtil.GetPrincipal();

        var loginDataAsOk = loginDataResult.AsOk();

        //Connect to firebase
        FirebaseManager.Instance.TryLoginAndGetData(loginDataAsOk.principal, OnLoginFinished);
    }

    //NEW
    private void LogoutUser()
    {
        Broadcast.Invoke<UserLogout>();
    }

    public void ConnectToWallet()
    {
        LogIn();

        connectButton.SetActive(false);
        logText.SetActive(true);

        var text = logText.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "Waiting the connection with ICP Wallet to be approved...";
    }
    
    private void LogIn()
    {
        Broadcast.Invoke<UserLoginRequest>();
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
