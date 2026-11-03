using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject startScreenPanel;
    public GameObject instructionsPanel;
    public GameObject creditsPanel;

    bool gameStartRequested;

    public void StartGame()
    {
        if (gameStartRequested)
            return;

        gameStartRequested = true;

        if (startScreenPanel != null) startScreenPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        IntroCutsceneController cutscene = GetComponent<IntroCutsceneController>();
        if (cutscene == null)
            cutscene = gameObject.AddComponent<IntroCutsceneController>();

        cutscene.Play(LoadGameScene);
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene("Game");
    }

    public void ShowInstructions()
    {
        if (startScreenPanel != null) startScreenPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        if (startScreenPanel != null) startScreenPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void ShowStartScreen()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (startScreenPanel != null) startScreenPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
