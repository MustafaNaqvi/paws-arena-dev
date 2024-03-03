using System.Linq;
using UnityEngine;

public class ChallengeAlert : MonoBehaviour
{
    [SerializeField] private GameObject alert;
    
    private void OnEnable()
    {
        ChallengesPanel.OnClosed += CheckForAlert;
        CheckForAlert();
    }

    private void OnDisable()
    {
        ChallengesPanel.OnClosed -= CheckForAlert;
    }

    private void CheckForAlert()
    {
        if (DataManager.Instance.PlayerData.ChallengeProgresses.Any(_challenge => _challenge.Completed&&!_challenge.Claimed))
        {
            alert.SetActive(true);
            return;
        }

        alert.SetActive(false);
    }
}
