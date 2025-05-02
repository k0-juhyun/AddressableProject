// Assets/Scripts/LoadingController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;

public class LoadingController : MonoBehaviour
{
    [Tooltip("UI Text to display download percentage (e.g. '23%')")]
    public Text percentText;

    [Tooltip("Addressable labels or keys to download, e.g. 'game'")]
    public string[] labelsToDownload;

    [Tooltip("Name of the scene to load after download completes")]
    public string targetSceneName = "GameScene";

    private void Start()
    {
        StartCoroutine(DownloadAndLoad());
    }

    private IEnumerator DownloadAndLoad()
    {
        var handle = Addressables.DownloadDependenciesAsync(new List<object>(labelsToDownload));

        while (!handle.IsDone)
        {
            if (percentText != null)
                percentText.text = $"{handle.PercentComplete * 100f:F0}%";
            yield return null;
        }

        if (percentText != null)
            percentText.text = "100%";

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            yield return new WaitForSeconds(0.3f);
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"Resource download failed: {handle.OperationException}");
            if (percentText != null)
                percentText.text = "Download Failed";
        }

        Addressables.Release(handle);
    }
}
