﻿using UnityEngine;

namespace romme.Cards
{
    [ExecuteAlways]
    [RequireComponent(typeof(Card))]
    public class SetCardTexture : MonoBehaviour
    {
        private Card card;
        private MeshRenderer meshRend;

        private Card.CardRank currentRank;
        private Card.CardSuit currentSuit;
        //private Card.CardColor currentColor;

        private void Start()
        {
            card = GetComponent<Card>();
            meshRend = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            if (currentRank == card.Rank && currentSuit == card.Suit)
                return;

            currentRank = card.Rank;
            currentSuit = card.Suit;
            Texture texture = Resources.Load<Texture>("cards/" + card);

            if (texture == null)
                Debug.Log("No texture for " + card);

            meshRend.sharedMaterial = new Material(meshRend.material) { mainTexture = texture };
        }
    }
}