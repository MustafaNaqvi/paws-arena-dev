using Boom;
using Boom.Patterns.Broadcasts;
using Boom.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingToServer : MonoBehaviour
{
    [Header("Managers")]
    public LobbyUIManager lobbyUIManager;

    [Header("Internals")]
    public GameObject connectButton;
    public GameObject logText;

    [SerializeField] private GameObject loginFailed;

    //NEW
    private void Awake()
    {
        UserUtil.AddListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        UserUtil.AddListenerDataChangeSelf<DataTypes.NftCollection>(NftDataChangeHandler);
        //UserUtil.UpdateDataSelf<DataTypes.NftCollection>(new DataTypes.NftCollection[0]);
    }

    private void OnDestroy()
    {
        //ExternalJSCommunication.Instance.onWalletConnected -= OnWalletConnected;
        //ExternalJSCommunication.Instance.onNFTsReceived -= OnNFTsReceived;

        //NEW
        UserUtil.RemoveListenerMainDataChange<MainDataTypes.LoginData>(LoginDataChangeHandler);
        UserUtil.RemoveListenerDataChangeSelf<DataTypes.NftCollection>(NftDataChangeHandler);
    }


    //NEW
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

    //NEW
    private void NftDataChangeHandler(Data<DataTypes.NftCollection> data)
    {
        if (UserUtil.IsDataValidSelf<DataTypes.NftCollection>() == false) return;

        if (UserUtil.IsUserLoggedIn() == false) return;

        $"Setup ICKitties Nft Collection".Log(typeof(ConnectingToServer).Name);

        GameState.nfts.Clear();

        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?&tokenid=hvtag-6ykor-uwiaa-aaaaa-cqace-eaqca-aaabd-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://images.entrepot.app/tnc/rw7qm-eiaaa-aaaak-aaiqq-cai/jairp-5ykor-uwiaa-aaaaa-cqace-eaqca-aacdq-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://images.entrepot.app/tnc/rw7qm-eiaaa-aaaak-aaiqq-cai/4gya5-nakor-uwiaa-aaaaa-cqace-eaqca-aaabu-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://images.entrepot.app/tnc/rw7qm-eiaaa-aaaak-aaiqq-cai/jxrpt-fikor-uwiaa-aaaaa-cqace-eaqca-aaajx-q" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://images.entrepot.app/tnc/rw7qm-eiaaa-aaaak-aaiqq-cai/hs6xe-7ikor-uwiaa-aaaaa-cqace-eaqca-aaaad-q" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://images.entrepot.app/tnc/rw7qm-eiaaa-aaaak-aaiqq-cai/ivtko-hikor-uwiaa-aaaaa-cqace-eaqca-aaaql-q" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://images.entrepot.app/tnc/rw7qm-eiaaa-aaaak-aaiqq-cai/xtmbl-nqkor-uwiaa-aaaaa-cqace-eaqca-aaacj-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=cfbp6-cikor-uwiaa-aaaaa-cqace-eaqca-aadkb-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=v3yzq-4ykor-uwiaa-aaaaa-cqace-eaqca-aaa5u-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=e7yl4-fqkor-uwiaa-aaaaa-cqace-eaqca-aaeeg-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=4cp2w-bykor-uwiaa-aaaaa-cqace-eaqca-aacz4-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=jairp-5ykor-uwiaa-aaaaa-cqace-eaqca-aacdq-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=xtmbl-nqkor-uwiaa-aaaaa-cqace-eaqca-aaacj-a" });
        GameState.nfts.Add(new NFT() { imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?tokenid=4gya5-nakor-uwiaa-aaaaa-cqace-eaqca-aaabu-a" });


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

        var loginDataAsOk = loginDataResult.AsOk();

        //Connect to firebase

        ExternalJSCommunication.Instance.WalletConnected();
        FirebaseManager.Instance.TryLoginAndGetData(loginDataAsOk.principal, OnLoginFinished);
    }

    //NEW
    private void LogoutUser()
    {
        Broadcast.Invoke<UserLogout>();
    }

    //NEW
    public void LogIn()
    {
        Broadcast.Invoke<UserLoginRequest>();
    }

    public void ConnectToWallet()
    {
        //NEW
        LogIn();

        ExternalJSCommunication.Instance.TryConnectWallet();
        connectButton.SetActive(false);
        logText.SetActive(true);

        var text = logText.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "Waiting the connection with ICP Wallet to be approved...";

        //ExternalJSCommunication.Instance.onWalletConnected += OnWalletConnected;
        //ExternalJSCommunication.Instance.onNFTsReceived += OnNFTsReceived;
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




    //NO LONGER NEEDED
    public void OnWalletConnected()
    {
        //ExternalJSCommunication.Instance.onWalletConnected -= OnWalletConnected;

        var text = logText.GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "Connection made. Waiting for NFTs...";
    }

    //NO LONGER NEEDED
    public void OnNFTsReceived()
    {
        ExternalJSCommunication.Instance.onNFTsReceived -= OnNFTsReceived;
        GameState.walletId = "asd";
        FirebaseManager.Instance.TryLoginAndGetData(GameState.principalId, OnLoginFinished);
    }
}
