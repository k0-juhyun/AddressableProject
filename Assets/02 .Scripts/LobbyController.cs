// Assets/Scripts/LobbyController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [Tooltip("Button that initiates the download and scene transition")]
    public Button downloadButton;

    private void Awake()
    {
        if (downloadButton != null)
            downloadButton.onClick.AddListener(OnDownloadClicked);
    }

    private void OnDownloadClicked()
    {
        SceneManager.LoadScene("LoadingScene");
    }
}
