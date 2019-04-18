﻿using System.Collections.Generic;
using UnityEngine;

namespace romme.Cards
{
    public class CardSpot : MonoBehaviour
    {
        public List<Card> Cards = new List<Card>();
        public bool HasCards => Cards.Count > 0;

        //TODO:
        // public bool isRun, isSet;
        // public Card.CardRank GetSetRank;
        // public Card.CardSuit GetRunSuit;

        public float startAngle;
        public float cardRadius = 2f;
        public float cardsAngleSpread = 180f;

        private void Update()
        {
            float deltaAngle = cardsAngleSpread / Cards.Count;

            for (int i = 0; i < Cards.Count; i++)
            {
                float x = cardRadius * Mathf.Cos((startAngle + i * deltaAngle) * Mathf.PI / 180f);
                float z = cardRadius * Mathf.Sin((startAngle + i * deltaAngle) * Mathf.PI / 180f);

                Cards[i].transform.position = transform.position + new Vector3(x, -0.1f * i, z);
            }
        }
    }

}