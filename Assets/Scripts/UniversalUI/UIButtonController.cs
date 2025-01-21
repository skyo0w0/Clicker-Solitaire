using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    private bool _isPaused = false;
    private void PauseGame()
    {
        Time.timeScale = 0f; 
        _isPaused = true;
        Debug.Log("Game paused.");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; 
        _isPaused = false;
        Debug.Log("Game resumed.");
    }
    
    public void LoadScene(string sceneLabel)
    {
        if (string.IsNullOrEmpty(sceneLabel))
        {
            Debug.LogWarning("Scene label is empty or null.");
            return;
        }
        
        Addressables.LoadSceneAsync(sceneLabel, LoadSceneMode.Single).Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Scene '{sceneLabel}' loaded successfully.");
            }
            else
            {
                Debug.LogError($"Failed to load scene '{sceneLabel}': {handle.OperationException}");
            }
        };
        
        ResumeGame();
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    
    public void TogglePause(InputHandler inputHandler = null)
    {
        if (inputHandler == null)
        {
            Debug.LogError($"No input handler provided. On {gameObject.name}.");
        }
        
        if (_isPaused)
        {
            inputHandler.enabled = true;
            ResumeGame();
        }
        else
        {
            inputHandler.enabled = false;
            PauseGame();
        }
    }
    
    public void SetPanelActive(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
    

}
