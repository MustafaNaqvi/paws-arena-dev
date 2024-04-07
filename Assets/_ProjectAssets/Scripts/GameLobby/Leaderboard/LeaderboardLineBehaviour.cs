using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardLineBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [DllImport("__Internal")]
    public static extern void CopyToClipboard(string _text);
    
    public GameObject principalIdTooltip;
    public TMPro.TextMeshProUGUI principalIdText;
    public Image principalIdIcon;
    public Image highlight;

    [Header("Icons")]
    public Sprite copySprite;
    public Sprite copiedSprite;

    private void Start()
    {
        principalIdTooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        principalIdTooltip.SetActive(true);
        principalIdIcon.sprite = copySprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        principalIdTooltip.SetActive(false);
    }

    public void SetPrincipalId(string principalId, int idx)
    {
        principalIdText.text = "Principal ID: " + principalId;

        if (principalId == GameState.principalId)
        {
            highlight.enabled = true;
        }
        else
        {
            if (idx % 2 == 0)
            {
                GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 0.28f);
            }
        }
    }

    public void SavePrincipalIdToClipboard()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            CopyToClipboard(principalIdText.text);
        }
        else
        {
            GUIUtility.systemCopyBuffer = principalIdText.text;
        }
        principalIdIcon.sprite = copiedSprite;
    }
}
