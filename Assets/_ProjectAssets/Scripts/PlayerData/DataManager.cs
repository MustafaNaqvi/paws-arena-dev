using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public PlayerData PlayerData { get; } = new ();

    public GameData GameData { get; } = new();

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

    private void OnDisable()
    {
        PlayerData.UnsubscribeEvents();
    }

    public void Setup()
    {
        PlayerData.SubscribeEvents();
    }
}
