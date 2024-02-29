using System.Runtime.InteropServices;
using Boom;
using UnityEngine;
using UnityEngine.UI;

public class CopyPrincipal : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void CopyToClipboard(string _text);
    
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
        string _text = UserUtil.GetPrincipal();
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            CopyToClipboard(_text);
        }
        else
        {
            GUIUtility.systemCopyBuffer = _text;
        }
    }
}