using Boom;
using UnityEngine;
using UnityEngine.UI;

public class CopyPrincipal : MonoBehaviour
{
    [SerializeField] private Button button;

    private void OnEnable()
    {
        button.onClick.AddListener(Copy);        
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Copy);
    }

    private void Copy()
    {
        GUIUtility.systemCopyBuffer = UserUtil.GetPrincipal();
    }
}
