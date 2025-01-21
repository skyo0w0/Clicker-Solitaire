using Cards;
using UnityEngine;

namespace Input
{
    public class CardInteractiveHandler : MonoBehaviour
    {
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private LayerMask cardLayer;
        [SerializeField] private LayerMask deckLayer;// Слой для поиска карт

        private void OnEnable()
        {
            inputHandler.OnMouseClick += OnMouseClick;
        }

        private void OnDisable()
        {
            inputHandler.OnMouseMove -= OnMouseClick;
        }

        private void OnMouseClick(Vector2 mousePosition)
        {
            Vector3 worldPosition = mousePosition;
            
            Collider2D hit = Physics2D.OverlapCircle(worldPosition, 0.1f, cardLayer);

            Collider2D deckHit = Physics2D.OverlapCircle(worldPosition, 0.1f, deckLayer);

            if (deckHit != null)
            {
                Deck deck = deckHit.GetComponent<Deck>();
                if (deckHit != null && deck.GetDeckCardsCount() != 0)
                {
                    deck.StackCard(deck.GetLastCard());
                    return;
                }
            }
            
            if (hit != null)
            {
                Card card = hit.GetComponent<Card>();
                if (card != null)
                {
                    card.OnCardClicked();
                }
            }
            
        }
    }
}