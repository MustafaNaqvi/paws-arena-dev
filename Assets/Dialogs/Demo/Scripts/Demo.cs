using UnityEngine;
using UnityEngine.UI;

namespace DialogsDemo
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private Button showOkDialog;
        [SerializeField] private Button showYesNoDialog;

        private void OnEnable()
        {
            showOkDialog.onClick.AddListener(ShowOkDialog);
            showYesNoDialog.onClick.AddListener(ShowYesNoDialog);
        }

        private void OnDisable()
        {
            showOkDialog.onClick.RemoveListener(ShowOkDialog);
            showYesNoDialog.onClick.RemoveListener(ShowYesNoDialog);
        }

        private void ShowOkDialog()
        {
            DialogsManager.Instance.ShowOkDialog("This is ok dialog",OnOkPressed);
        }

        private void OnOkPressed()
        {
            Debug.Log("Ok was pressed");
        }

        private void ShowYesNoDialog()
        {
            DialogsManager.Instance.ShowYesNoDialog("This is yes no dialog", OnYesPressed, OnNoPressed);
        }

        private void OnYesPressed()
        {
            Debug.Log("Yes was pressed");
        }

        private void OnNoPressed()
        {
            Debug.Log("No was pressed");
        }
    }
}

