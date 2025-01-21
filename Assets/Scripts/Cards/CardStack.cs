using System.Collections.Generic;
using System.Threading.Tasks;
using Cards;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    [Header("Card Stack Settings")]
    [SerializeField] private List<Card> cards = new List<Card>();
    [SerializeField] private Deck _deck;
    [SerializeField] private TextMeshProUGUI _text;

    private int _movesCount;

    #region Card Stack Logic

    // Проверяет, можно ли положить карту в текущую стопку
    internal bool CanStack(Card card)
    {
        if (cards.Count == 0)
        {
            return true;
        }

        var lastCard = cards[cards.Count - 1];
        var lastCardNumber = lastCard.GetCardNumber();

        if (lastCardNumber == card.GetCardNumber() + 1 || lastCardNumber == card.GetCardNumber() - 1)
        {
            return true;
        }

        if (card.GetCardNumber() == 1 && (lastCardNumber == 13 || lastCardNumber == 2))
        {
            return true;
        }

        if (card.GetCardNumber() == 13 && (lastCardNumber == 1 || lastCardNumber == 12))
        {
            return true;
        }

        return false;
    }

    // Добавляет карту в текущую стопку
    internal async Task Stack(Card card)
    {
        cards.Add(card);
        card.SetStackedStatus(true);
        card.transform.SetParent(transform);

        List<UniTask> tasks = new List<UniTask>
        {
            card.transform.DOMove(
                transform.localPosition + new Vector3(0, 0, -1f * cards.Count * 0.01f),
                0.5f
            ).SetEase(Ease.OutQuad).AsyncWaitForCompletion()
        };

        if (!card.IsTableCard)
        {
            tasks.Add(card.FlipWithAnimation(true));
        }
        else
        {
            _deck.TableCards.Remove(card);
        }

        await UniTask.WhenAll(tasks);
        card.enabled = false;
        card.transform.SetAsFirstSibling();

        AddMovesCount();
        CheckGameOver();
    }

    // Возвращает последнюю карту из стопки
    internal Card GetLastCard()
    {
        return cards[cards.Count - 1];
    }

    // Возвращает позицию последней карты
    public Vector3 GetLastCardPosition()
    {
        return cards.Count > 0 ? cards[cards.Count - 1].transform.position : transform.position;
    }

    #endregion

    #region Undo Logic

    // Отменяет последнее действие
    internal void Undo()
    {
        if (cards.Count == 0)
            return;

        Card card = GetLastCard();
        cards.Remove(card);

        List<UniTask> undoTasks = new List<UniTask>
        {
            card.Undo()
        };

        if (!card.IsTableCard)
        {
            _deck.AddCardToDeck(card);
            undoTasks.Add(card.FlipWithAnimation(false));
        }
        else
        {
            _deck.TableCards.Add(card);
            card.transform.SetParent(null);
        }

        AddMovesCount();
        UniTask.WhenAll(undoTasks);
        card.SetStackedStatus(false);
    }

    #endregion

    #region Moves Counter

    // Увеличивает счётчик ходов и обновляет текст
    private void AddMovesCount()
    {
        _movesCount++;
        _text.text = _movesCount.ToString();
    }

    #endregion

    #region Game State Checking

    // Проверяет состояние игры (победа/поражение)
    private void CheckGameOver()
    {
        if (_deck.TableCards.Count == 0)
        {
            Debug.Log("WINWINWINWINWIN");
        }
        else if (_deck.GetDeckCardsCount() == 0)
        {
            bool flag = false;
            foreach (Card card in _deck.TableCards)
            {
                if (card.enabled && CanStack(card))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                Debug.Log("LOSE");
            }
        }
    }

    #endregion
}


