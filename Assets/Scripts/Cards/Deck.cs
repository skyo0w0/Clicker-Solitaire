using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Cards
{
    public class Deck : MonoBehaviour
    {
        #region Fields and Settings

        [Header("Card Settings")]
        [SerializeField] private AssetReference cardPrefab;

        [Header("Generation Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float cardOffset = 0.01f;

        [SerializeField] private List<Card> _allCards = new List<Card>();
        [SerializeField] private List<Card> deck = new List<Card>();

        [Header("Deck Settings")]
        [SerializeField] private CardStack _cardStack;
        [SerializeField] private Vector3 _cardOffset = new Vector3(2f, 2f, 0.01f);

        private readonly List<string> suits = new()
        {
            "Spades",
            "Heart",
            "Club",
            "Diamond"
        };

        private readonly Dictionary<string, List<Sprite>> suitSprites = new();
        
        [SerializeField] internal List<Card> TableCards = new List<Card>();

        #endregion

        #region Initialization

        private async void Awake()
        {
            foreach (var suit in suits)
            {
                var sprites = await LoadSpritesFromAddressables(suit);
                suitSprites[suit] = sprites.ToList();
            }

            await GenerateTable();
        }

        #endregion

        #region Card Generation

        private async UniTask GenerateTable()
        {
            foreach (var suit in suits)
            {
                var sprites = suitSprites[suit];

                for (int i = 0; i < sprites.Count; i++)
                {
                    var cardObject = await Addressables.InstantiateAsync(cardPrefab, spawnPoint).Task;

                    if (cardObject == null)
                    {
                        Debug.LogError("Failed to instantiate card prefab!");
                        return;
                    }

                    var card = cardObject.GetComponent<Card>();

                    if (card == null)
                    {
                        Debug.LogError("Card prefab does not have a Card component!");
                        Destroy(cardObject);
                        continue;
                    }

                    var cardName = sprites[i].name;
                    int cardNumber =
                        int.Parse(System.Text.RegularExpressions.Regex.Match(cardName, @"\d+$").Value);
                    card.name = cardName;
                    card.SetCardSuit(suit);
                    card.SetCardNumber(cardNumber);
                    card.FlipCard(false);
                    card.SetCardFront(sprites[i]);

                    _allCards.Add(card);
                    card.enabled = false;
                }
            }

            deck = _allCards.OrderBy(_ => Random.value).ToList();

            ArrangeDeck();
            ArrangePyramid();
            await CheckAllCardsBelow();
        }

        private void ArrangeDeck()
        {
            for (int i = 0; i < deck.Count; i++)
            {
                Transform cardTransform = deck[i].transform;
                cardTransform.localPosition = new Vector3(i * cardOffset, 0, 0);
                cardTransform.localRotation = Quaternion.identity;
            }
        }

        private void ArrangePyramid()
        {
            int[] pyramidLevels = { 3, 6, 9, 10 };
            float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;

            float pyramidTop = 0.5f;
            Vector3 screenPosition1 = Camera.main.ViewportToWorldPoint(new Vector3(0, pyramidTop, 0f));
            Vector3 screenPosition2 = Camera.main.ViewportToWorldPoint(new Vector3(1, pyramidTop, 0f));
            screenPosition1.z = screenPosition2.z = 0;

            for (int i = 0; i < 10; i++)
            {
                var newPosition = Vector3.Lerp(screenPosition1, screenPosition2, (1f / 11) * (i + 1));
                CardToTable(newPosition);

                if (i == 9) break;

                switch ((i + 1) % 3)
                {
                    case 1:
                        for (int j = 0; j < 3; j++)
                            CardToTable(newPosition + (_cardOffset * (j + 1)));
                        break;
                    case 2:
                        for (int j = 0; j < 2; j++)
                            CardToTable(newPosition + (_cardOffset * (j + 1)));
                        break;
                    case 0:
                        CardToTable(newPosition + _cardOffset);
                        break;
                }
            }
        }

        private void CardToTable(Vector3 cardPosition)
        {
            Card card = GetLastCard();
            card.transform.SetParent(null);
            deck.Remove(card);
            card.transform.localPosition = cardPosition;
            card.transform.localRotation = Quaternion.identity;
            card.SetStartPosition(cardPosition);
            card.IsTableCard = true;
            TableCards.Add(card);
        }
        

        #region Card Checking

        private async UniTask CheckAllCardsBelow()
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            foreach (Card card in new List<Card>(TableCards))
            {
                await card.HasCardsBelow();
            }
        }

        #endregion
        
        #endregion

        #region Card Utilities

        internal Card GetLastCard()
        {
            return deck[deck.Count - 1];
        }

        internal void AddCardToDeck(Card card)
        {
            deck.Add(card);
        }

        internal int GetDeckCardsCount()
        {
            return deck.Count;
        }

        internal async void StackCard(Card card)
        {
            if (deck.Count > 0)
            {
                card.transform.parent = null;
                deck.Remove(card);
                await _cardStack.Stack(card);
            }
        }

        #endregion
        
        #region Addressables Loading

        public async UniTask<Sprite[]> LoadSpritesFromAddressables(string label)
        {
            try
            {
                IList<Sprite> sprites = await Addressables.LoadAssetsAsync<Sprite>(label, null).Task;
                return sprites.ToArray();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load sprites with label '{label}': {ex.Message}");
                return new Sprite[0];
            }
        }

        public async UniTask<GameObject> LoadCardPrefab(string label)
        {
            try
            {
                Debug.Log($"Attempting to load card prefab with label: {label}");
                var prefab = await Addressables.LoadAssetAsync<GameObject>(label).Task;
                Debug.Log($"Card prefab loaded from label: {label}");
                return prefab;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading card prefab with label '{label}': {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}