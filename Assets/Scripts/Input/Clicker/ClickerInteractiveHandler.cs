using UnityEngine;


public class ClickerInteractiveHandler : MonoBehaviour
{
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private LayerMask clickerLayer;

    private void OnEnable()
    {
        inputHandler.OnMouseClick += OnMouseClick;
    }
    
    private void OnDisable()
    {
        inputHandler.OnMouseClick -= OnMouseClick;
    }
    
    
    private void OnMouseClick(Vector2 clickPos)
    {
        
        Collider2D hit = Physics2D.OverlapCircle(clickPos, 0.1f, clickerLayer);

        if (hit != null)
        {
            ClickerComponent clicker = hit.GetComponent<ClickerComponent>();
            if (clicker != null)
            {
                clicker.Click(clickPos);
            }
        }
    }

    
}
