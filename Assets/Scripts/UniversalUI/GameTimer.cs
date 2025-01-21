using TMPro;
using UnityEngine;


public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; // Ссылка на UI Text
    private float elapsedTime = 0f; // Время с начала игры
    private bool isRunning = true; // Флаг работы таймера

    private void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            
            UpdateTimerUI();
        }
    }
    
    public void StopTimer()
    {
        isRunning = false;
    }
    
    public void StartTimer()
    {
        isRunning = true;
    }
    
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        timerText.text = FormatTime(elapsedTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}