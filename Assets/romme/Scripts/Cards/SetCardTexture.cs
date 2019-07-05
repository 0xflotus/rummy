﻿using UnityEngine;

namespace romme.Cards
{
    [ExecuteAlways]
    [RequireComponent(typeof(Card))]
    public class SetCardTexture : MonoBehaviour
    {
        public Material Material;
        private Material[] localMaterials = new Material[1];
        private Card card;
        private MeshRenderer meshRend;

        private Card.CardRank currentRank;
        private Card.CardSuit currentSuit;

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
            Texture texture = Resources.Load<Texture>("cards/" + card.GetFileString());

            if (texture == null)
                Debug.Log("No texture for " + card + " (missing " + card.GetFileString() + ")");

            localMaterials[0] = new Material(Material) { mainTexture = texture };
            meshRend.materials = localMaterials;
        }
    }
}