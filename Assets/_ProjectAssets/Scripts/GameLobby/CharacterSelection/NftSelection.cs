using Anura.ConfigurationModule.Managers;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using BoomDaoWrapper;
using UnityEngine;
using UnityEngine.UI;

public class NftSelection : MonoBehaviour
{
    public PagesBehaviour pages;

    public Transform playerPlatformParent;
    public GameObject playerPlatformPrefab;
    public GameObject nftButtonPrefab;
    public Transform nftButtonsParent;

    [SerializeField] private GameObject message;
    [SerializeField] private Button reload;
    [SerializeField] private Button reloadNoKitty;
    [SerializeField] private GameObject noNftsMessage;
    [SerializeField] private Button enterArena;
    
    private List<GameObject> nftButtons = new();
    private GameObject playerPlatform;

    private int currentPage;
    private int pageSize = 9;

    private List<NFT> currentNfts = new();

    private void OnEnable()
    {
        enterArena.onClick.AddListener(EnterArena);
        reload.onClick.AddListener(RequestReload);
        reloadNoKitty.onClick.AddListener(RequestReload);
        pages.OnClick += OnPageSelected;
        BoomDaoUtility.OnUpdatedNftsData += ReloadNfts;
        GameState.SetSelectedNFT(null);
        
        InitNftScreen();
    }

    private void OnDisable()
    {
        enterArena.onClick.RemoveListener(EnterArena);
        reload.onClick.RemoveListener(RequestReload);
        reloadNoKitty.onClick.RemoveListener(RequestReload);
        ClearShownNfts();
        if (playerPlatform != null)
        {
            Destroy(playerPlatform);
            playerPlatform = null;
        }


        pages.OnClick -= OnPageSelected;
        BoomDaoUtility.OnUpdatedNftsData -= ReloadNfts;
    }

    private void EnterArena()
    {
        if (GameState.selectedNFT==null)
        {
            if (GameState.nfts.Count==0)
            {
                noNftsMessage.SetActive(true);
            }
            return;
        }

        SceneManager.Instance.LoadMainMenu();
    }

    private void RequestReload()
    {
        BoomDaoUtility.Instance.ReloadNfts();
    }
    
    private async void OnPageSelected(int _idx)
    {
        currentPage = _idx;
        await PopulateGridAsync();
    }

    private void ReloadNfts()
    {
        ClearShownNfts();
        InitNftScreen();
    }
    
    private void ClearShownNfts()
    {
        foreach (NFT _nfts in currentNfts)
        {
            Destroy(_nfts.imageTex);
            _nfts.imageTex = null;
        }
        currentNfts.Clear();
    }

    public async void InitNftScreen()
    {
        currentPage = 0;
        int _maxPages = (int)Math.Floor((GameState.nfts.Count - 1) * 1.0 / pageSize);
        pages.SetNumberOfPages(_maxPages + 1);
        await PopulateGridAsync();
        if (GameState.nfts.Count>0)
        {
            SelectNft(0);
        }
        message.SetActive(GameState.nfts.Count==0);
    }

    private List<NFT> GetNfts(int _pageNr, int _pageSize)
    {
        return GameState.nfts.Skip(_pageNr * _pageSize).Take(_pageSize).ToList();
    }
    
    private async UniTask PopulateGridAsync()
    {
        foreach (GameObject _but in nftButtons)
        {
            Destroy(_but);
        }

        nftButtons.Clear();

        foreach (NFT _nfts in currentNfts)
        {
            Destroy(_nfts.imageTex);
            _nfts.imageTex = null;
        }


        currentNfts = GetNfts(currentPage, pageSize);

        List<UniTask> _tasks = new List<UniTask>();
        int _idx = 0;
        foreach (NFT _nft in currentNfts)
        {
            GameObject _go = Instantiate(nftButtonPrefab, nftButtonsParent);
            nftButtons.Add(_go);
            _go.GetComponent<NFTImageButton>().SetLoadingState();
            _tasks.Add(_nft.GrabImage());
            _idx++;
        }

        for (int _i = currentNfts.Count; _i < 9; _i++)
        {
            GameObject _go = Instantiate(nftButtonPrefab, nftButtonsParent);
            nftButtons.Add(_go);
        }
        
        await UniTask.WhenAll(_tasks.ToArray());

        _idx = 0;
        foreach (NFT _nft in currentNfts)
        {
            _nft.RecoveryEndDate = DateTime.MinValue;
            if (DataManager.Instance.PlayerData.IsKittyHurt(_nft.imageUrl))
            {
                _nft.RecoveryEndDate = DataManager.Instance.PlayerData.GetKittyRecoveryDate(_nft.imageUrl);
            }
            
            nftButtons[_idx].GetComponent<NFTImageButton>().SetTexture(_nft.imageTex);
            nftButtons[_idx].GetComponent<RecoveryHandler>().ShowRecovery(_nft.RecoveryEndDate);
            nftButtons[_idx].GetComponent<Button>().onClick.RemoveAllListeners();

            int _crtIdx = _idx;
            nftButtons[_idx].GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectNft(_crtIdx);
            });
            _idx++;
        }

        SetSelectedNftGraphics();
    }

    private void SelectNft(int _idx)
    {
        if (playerPlatform != null)
        {
            Destroy(playerPlatform);
        }

        if (ConfigurationManager.Instance.GameConfig.enableDevLogs)
        {
            Debug.Log("Selected " + currentNfts[_idx].imageUrl);
        }

        GameState.SetSelectedNFT(currentNfts[_idx]);

        playerPlatform = Instantiate(playerPlatformPrefab, playerPlatformParent);
        playerPlatform.transform.localPosition = Vector3.zero;

        SetSelectedNftGraphics();
    }

    private void SetSelectedNftGraphics()
    {
        NFT _selectedNft = GameState.selectedNFT;
        for (int _i = 0; _i < nftButtons.Count; _i++)
        {
            if (_i >= currentNfts.Count)
            {
                break;
            }
            if (currentNfts[_i] == null)
            {
                break;
            }
            if (_selectedNft != null && currentNfts[_i].imageUrl == _selectedNft.imageUrl)
            {
                nftButtons[_i].GetComponent<NFTImageButton>().Select();
            }
            else
            {
                nftButtons[_i].GetComponent<NFTImageButton>().Deselect();
            }
        }
    }
}
