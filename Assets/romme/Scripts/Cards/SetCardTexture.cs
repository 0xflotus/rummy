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

            //FIXME: try if there's still lots of lag (GC?) when stopping after running hundreds of games 
            meshRend.sharedMaterial = new Material(meshRend.sharedMaterial != null ? meshRend.sharedMaterial : meshRend.material) { mainTexture = texture };
        }
    }
}