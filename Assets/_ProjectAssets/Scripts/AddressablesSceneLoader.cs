using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;

public class AddressablesSceneLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReference> assetReferences = new();
    [SerializeField] private AssetReference loginScene;
    [SerializeField] private Slider assetsLoadingSlider;

    private void Awake()
    {
        var initializeOperationHandle = Addressables.InitializeAsync();
        initializeOperationHandle.Completed += InitializationComplete;
    }

    private void InitializationComplete(AsyncOperationHandle<IResourceLocator> obj)
    {
        StartCoroutine(LoadScenes());
    }

    private IEnumerator LoadScenes()
    {
        foreach (var operationHandle in from assetReference in assetReferences
                 where Addressables.GetDownloadSizeAsync(assetReference).Result > 0
                 select Addressables.DownloadDependenciesAsync(assetReference))
        {
            yield return new WaitUntil(() =>
            {
                assetsLoadingSlider.value = operationHandle.GetDownloadStatus().Percent;
                return operationHandle.IsDone;
            });
        }

        LoadLoginScene();
    }

    private void LoadLoginScene()
    {
        Addressables.LoadSceneAsync(loginScene);
    }
}