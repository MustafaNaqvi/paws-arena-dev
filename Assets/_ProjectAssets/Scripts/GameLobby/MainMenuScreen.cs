using UnityEngine;

public class MainMenuScreen : MonoBehaviour
{
    public Transform playerPlatformPosition;
    public GameObject playerPlatformPrefab;

    private GameObject playerPlatform;

    private void OnEnable()
    {
        playerPlatform = Instantiate(playerPlatformPrefab, playerPlatformPosition);
        playerPlatform.transform.position = Vector3.zero;
    }

    private void OnDisable()
    {
        if(playerPlatform != null)
        {
            Destroy(playerPlatform);
        }
    }
}
