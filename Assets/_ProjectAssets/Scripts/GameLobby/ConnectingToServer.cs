using BoomDaoWrapper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectingToServer : MonoBehaviour
{
    [SerializeField] private Button connect;
    [SerializeField] private TextMeshProUGUI logText;

    private void OnEnable()
    {
        connect.onClick.AddListener(Connect);
    }

    private void OnDisable()
    {
        connect.onClick.RemoveListener(Connect);
    }

    private void Connect()
    {
        BoomDaoUtility.Instance.Login(FinishConnecting);
        logText.text = "Waiting the connection with ICP Wallet to be approved...";
    }

    private void FinishConnecting()
    {
        logText.text = "Connection made!";
        SetupNftData();

        var _loginDataResult = BoomDaoUtility.Instance.GetLoginData;
        var _loginDataAsOk = _loginDataResult.AsOk();

        GameState.principalId = _loginDataAsOk.principal;
        DataManager.Instance.Setup();
        ChallengesManager.Instance.Setup();
        
        SceneManager.Instance.LoadNftSelection();
    }
    
    private void SetupNftData()
    {
        //MockNfts();
        UpdateNfts();
        if (GameState.nfts.Count==0)
        {
            GameState.nfts.Add(new NFT()
            {
                imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.icp0.io/?&tokenid=cad6v-gqkor-uwiaa-aaaaa-cqace-eaqca-aadvq-a"
            });
        }
    }

    private void MockNfts()
    {
        if (Application.isEditor)
        {
            GameState.nfts.Add(new NFT()
            {
                imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.ic0.app/?&tokenid=hvtag-6ykor-uwiaa-aaaaa-cqace-eaqca-aaabd-a"
            });
            GameState.nfts.Add(new NFT()
            {
                imageUrl = "https://rw7qm-eiaaa-aaaak-aaiqq-cai.raw.icp0.io/?tokenid=o2lah-cikor-uwiaa-aaaaa-cqace-eaqca-aacyz-a"
            });
        }
    }

    public static void UpdateNfts()
    {
        var _nftCollectionsResult = BoomDaoUtility.Instance.GetNftData;
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
                GameState.nfts.Add(new NFT { imageUrl = _token.url });
            }
            break;
        }
    }
}
