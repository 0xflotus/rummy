﻿using System.Collections.Generic;
using UnityEngine;

namespace rummy.Cards
{

    public class DiscardStack : MonoBehaviour
    {
        private readonly Stack<Card> Cards = new Stack<Card>();
        public int CardCount => Cards.Count;

        public void AddCard(Card card) => Cards.Push(card);
        public Card DrawCard() => Cards.Pop();
        public Card PeekCard() => Cards.Peek();
        public Vector3 GetNextCardPos() => transform.position + Vector3.up * 0.0001f * Cards.Count;

        public List<Card> RecycleDiscardedCards()
        {
            Card lastDiscarded = Cards.Pop();
            var cards = RemoveCards();
            AddCard(lastDiscarded);
            lastDiscarded.transform.position = GetNextCardPos();
            return cards;
        }

        public void ResetStack()
        {
            var cards = RemoveCards();
            while (cards.Count > 0)
            {
                Card c = cards[0];
                cards.RemoveAt(0);
                Destroy(c.gameObject);
            }
        }

        private List<Card> RemoveCards()
        {
            List<Card> removedCards = new List<Card>();
            while (Cards.Count > 0)
                removedCards.Add(Cards.Pop());
            return removedCards;
        }
    }

}