using TMPro;
using UnityEngine;

public class ClickerCounterComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private ClickerComponent _clicker;
    
    private void OnEnable()
    {
        _clicker.OnClick += OnCountUpdate;
    }

    private void OnDisable()
    {
        _clicker.OnClick -= OnCountUpdate;
    }

    private void OnCountUpdate(int count)
    {
        _counterText.text = count.ToString();
    }
}
