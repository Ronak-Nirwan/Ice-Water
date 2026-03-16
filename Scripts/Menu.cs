using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject sessionPanel;
    public GameObject quitPanel;



    private void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (sessionPanel != null) sessionPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) 
        {
            QuitMenuOn();
        }
    }

    public void OnPlayButton()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (sessionPanel != null) sessionPanel.SetActive(true);
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit();
#endif
    }

    public void OnBackButton()
    {
        if (sessionPanel != null) sessionPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void Leave()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitMenuOn()
    {
        quitPanel.SetActive(true);
    }

    public void QuitMenuOff()
    {
        quitPanel.SetActive(false);
    }
}
