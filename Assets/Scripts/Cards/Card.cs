using System;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class Card : MonoBehaviour
{
    // --- Параметры карты ---
    public event Action OnCardStacked;
    public event Action OnCardUnstacked;
    
    [Header("Card Settings")]
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite _backSprite;
    [SerializeField] private int _number;
    [SerializeField] private string _suit;

    [Header("Position and Layers")]
    [SerializeField] private Vector3 _startPosition;
    [SerializeField] private Vector2 _boxSize = new Vector2(1f, 0.2f);
    [SerializeField] private Vector2 _boxOffset = new Vector2(0f, -0.6f);
    [SerializeField] private LayerMask _cardLayer;
    [SerializeField] private LayerMask _cardStackLayer;

    [Header("References")]
    [SerializeField] private InputHandler _inputHandler;

    private bool _isStacked = false;
    private int _cardBelowCount = 0;
    internal bool IsTableCard = false;

    private SpriteRenderer spriteRenderer;
    private bool isFaceUp = false;

    #region Initialization

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FlipCard(false); // Устанавливаем рубашку по умолчанию
        _startPosition = transform.position;

        if (_inputHandler == null)
        {
            _inputHandler = FindFirstObjectByType<InputHandler>();
        }
    }

    #endregion

    #region Card State Management

    public void FlipCard(bool faceUp)
    {
        isFaceUp = faceUp;
        this.enabled = faceUp;
        spriteRenderer.sprite = faceUp ? frontSprite : _backSprite;
    }

    public async UniTask FlipWithAnimation(bool faceUp)
    {
        isFaceUp = faceUp;
        this.enabled = faceUp;

        // Анимация переворота через ось Y
        await transform.DORotate(new Vector3(0, 90, 0), 0.2f).AsyncWaitForCompletion();

        // Меняем спрайт после поворота
        spriteRenderer.sprite = faceUp ? frontSprite : _backSprite;

        // Завершаем анимацию поворота
        await transform.DORotate(new Vector3(0, 0, 0), 0.2f).AsyncWaitForCompletion();
    }

    public async UniTask Undo()
    {
        await transform
            .DOMove(_startPosition, 0.5f)
            .SetEase(Ease.OutQuad) 
            .OnComplete(() => enabled = IsTableCard) 
            .AsyncWaitForCompletion(); 
    }

    internal void SetCardFront(Sprite cardFront) => frontSprite = cardFront;
    internal void SetCardNumber(int number) => _number = number;
    internal void SetCardSuit(string suit) => _suit = suit;
    internal string GetCardSuit() => _suit;
    internal int GetCardNumber() => _number;
    internal void SetStartPosition(Vector3 position) => _startPosition = position;

    internal void SetStackedStatus(bool isStacked)
    {
        _isStacked = isStacked;
        if (_isStacked == false)
        {
            OnCardUnstacked?.Invoke();
        }
        else
        {
            OnCardStacked?.Invoke();
        }
    }

    #endregion

    #region Input Handling

    private void HandleMouseDrag(Vector2 mousePosition)
    {
        if (!enabled) return;

        Vector3 worldPosition = new Vector3(mousePosition.x, mousePosition.y, -3);
        transform.position = worldPosition;
    }

    private async void OnClickCanceled()
    {
        _inputHandler.OnClickCanceled -= OnClickCanceled;
        _inputHandler.OnMouseMove -= HandleMouseDrag;

        if (!enabled) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.1f, _cardStackLayer);
        if (hit != null)
        {
            CardStack cardStack = hit.GetComponent<CardStack>();
            if (cardStack.CanStack(this))
            {
                await cardStack.Stack(this);
            }
            else
            {
                await Undo();
            }
        }
        else
        {
            await Undo();
        }

        Debug.Log("Card dropped!");
    }

    internal void OnCardClicked()
    {
        if (!enabled) return;

        _inputHandler.OnClickCanceled += OnClickCanceled;
        _inputHandler.OnMouseMove += HandleMouseDrag;
    }

    #endregion

    #region Card Interaction

    internal async UniTask HasCardsBelow()
    {
        Vector2 boxCenter = (Vector2)_startPosition + _boxOffset;

        // Получаем все объекты внутри области OverlapBox
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, _boxSize, 0f, _cardLayer);

        Debug.Log(hits.Length);
        foreach (Collider2D hit in hits)
        {
            Card card = hit.GetComponent<Card>();
            card.OnCardStacked += CardBelowStacked;
            card.OnCardUnstacked += CardBelowUnstacked;
            _cardBelowCount++;
        }

        await FlipIfCan();
    }

    private async UniTask FlipIfCan()
    {
        if (_cardBelowCount > 0)
        {
            FlipCard(false);
        }
        else if (_cardBelowCount == 0)
        {
            await FlipWithAnimation(true);
        }
    }

    private async void CardBelowStacked()
    {
        _cardBelowCount--;
        await FlipIfCan();
    }

    private async void CardBelowUnstacked()
    {
        _cardBelowCount++;
        await FlipIfCan();
    }

    #endregion

    #region Debugging

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector2 boxCenter = (Vector2)transform.position + _boxOffset;
        Gizmos.DrawWireCube(boxCenter, _boxSize);
    }

    #endregion
}
