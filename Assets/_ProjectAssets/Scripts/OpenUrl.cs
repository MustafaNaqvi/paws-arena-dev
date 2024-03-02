using UnityEngine;
using UnityEngine.UI;

public class OpenUrl : MonoBehaviour
{
    [SerializeField] private string url;
    [SerializeField] private Button button;

    private void OnEnable()
    {
        button.onClick.AddListener(Open);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Open);
    }

    private void Open()
    {
        Application.OpenURL(url);
    }
}