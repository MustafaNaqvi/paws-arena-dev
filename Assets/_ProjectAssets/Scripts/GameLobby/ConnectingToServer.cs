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
    }

    private void OnDestroy()
    {
        UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
    }


    private void LoginDataChangeHandler(MainDataTypes.LoginData data)
    {
        if (data.state != MainDataTypes.LoginData.State.LoggedIn)
        {
            //Return, client is not logged in yet
            return;
        }
        var text = logText.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "Connection made!";

        SetupNftData();

        var loginDataResult = UserUtil.GetLogInData();

        var loginDataAsOk = loginDataResult.AsOk();

        GameState.principalId = loginDataAsOk.principal;

        //Connect to firebase
        FirebaseManager.Instance.TryLoginAndGetData(loginDataAsOk.principal, OnLoginFinished);
    }

    private void SetupNftData()
    {

        $"Setup ICKitties Nft Collection".Log(typeof(ConnectingToServer).Name);

        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?&tokenid=hvtag-6ykor-uwiaa-aaaaa-cqace-eaqca-aaabd-a" });

        var nftCollectionsResult = UserUtil.GetDataSelf<DataTypes.NftCollection>();

        if (nftCollectionsResult.IsErr)
        {
            $"{nftCollectionsResult.AsErr()}".Error (typeof(ConnectingToServer).Name);
            return;
        }

        var nftCollectionsAsOk = nftCollectionsResult.AsOk();

        //Look for the ICKitties Collection and  add the user's nft images to the GameState.nfts
        foreach (var keyValue in nftCollectionsAsOk.elements)
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
