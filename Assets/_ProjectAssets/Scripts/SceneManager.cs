using UnityEngine;
using UnityEngine.AddressableAssets;

public class SceneManager : MonoBehaviour
{
    // private const string LOADING = "NftSelection";
    // private const string NFT_SELECTION = "NftSelection";
    // private const string MAIN_MENU = "MainMenu";
    // private const string LEADERBOARD = "Leaderboard";
    // private const string TUTORIAL = "Tutorial";
    // private const string AFTER_GAME = "AfterGame";
    // public const string GAME_ROOM = "GameRoom";
    // public const string SINGLE_PLAYER = "SinglePlayerGameRoom";
    // public const string GAME_SCENE = "GameScene";
    // public const string SINGLE_PLAYER_GAME = "SinglePlayerGame";

    public AssetReference loginScene;
    public AssetReference nftSelectionScene;
    public AssetReference mainMenuScene;
    public AssetReference leaderboardScene;
    public AssetReference tutorialScene;
    public AssetReference afterGameScene;
    public AssetReference gameRoomScene;
    public AssetReference singlePlayerScene;
    public AssetReference gameScene;
    public AssetReference singlePlayerGameScene;
    private AssetReference _currentlyLoadedScene;
    
    public static SceneManager Instance;

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNftSelection()
    {
        LoadScene(nftSelectionScene);
    }

    public void LoadLeaderboard()
    {
        LoadScene(leaderboardScene);
    }

    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    public void LoadTutorial()
    {
        LoadScene(tutorialScene);
    }

    public void LoadAfterGame()
    {
        LoadScene(afterGameScene);
    }

    public void Reload()
    {
        LoadScene(_currentlyLoadedScene);
    }

    public void LoadScene(AssetReference sceneInstance)
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene(_key);
        Addressables.LoadSceneAsync(sceneInstance);
        _currentlyLoadedScene = sceneInstance;
    }
}
