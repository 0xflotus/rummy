﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using rummy.Cards;

namespace rummy.Utility
{

    public static class CardUtil
    {
        /// <summary>
        /// Returns a list of all possible combos which could be laid down, with sets and runs as well as joker
        /// combinations extracted from the given 'HandCards'
        /// <param name="allowLayingAll"> Whether combos are allowed which would require the player to lay down all cards from his hand ('HandCards').
        ///This is usually not useful, unless hypothetical hands are examined, where one card was removed before. </param>
        /// <param name="logMessage"> Whether to log when a duo set/run was NOT added because all necessary cards are already laid down </param>
        /// </summary>
        public static List<CardCombo> GetAllPossibleCombos(List<Card> HandCards, List<Card> LaidDownCards, bool allowLayingAll, bool logMessage)
        {
            var sets = GetPossibleSets(HandCards);
            var runs = GetPossibleRuns(HandCards);

            var jokerSets = GetPossibleJokerSets(HandCards, LaidDownCards, sets, runs, logMessage);
            sets.AddRange(jokerSets);

            var jokerRuns = GetPossibleJokerRuns(HandCards, LaidDownCards, sets, runs, logMessage);
            runs.AddRange(jokerRuns);

            var possibleCombos = GetAllPossibleCombos(sets, runs, HandCards.Count, allowLayingAll);
            return possibleCombos;
        }

        /// <summary>
        /// Get all possible card combinations which could be created from the given sets and runs.
        /// Each combo's sets and runs are sorted descending by value.
        /// <param name="handCardCount"> The number of cards on the player's hand </param>
        /// <param name="allowLayingAll"> Whether combos are allowed which would require the player to lay down all cards from his hand.
        /// This is usually not useful, unless hypothetical hands are examined, where one card was removed before. </param>
        /// </summary>
        private static List<CardCombo> GetAllPossibleCombos(List<Set> sets, List<Run> runs, int handCardCount, bool allowLayingAll)
        {
            var combos = new List<CardCombo>();
            GetPossibleSetAndRunCombos(combos, sets, runs, new CardCombo());
            GetPossibleRunCombos(combos, runs, new CardCombo());
            combos = combos.Where(ldc => ldc.PackCount > 0).ToList();

            if (allowLayingAll)
                return combos;

            //Don't allow combinations which cannot be reduced further but would require the player to lay down all cards
            //for example don't allow laying down 3 sets of 3, when having 9 cards in hand in total
            //Having 8 cards in hand, 2 sets of 4 is a valid combination since single cards can be kept on hand
            var possibleCombos = new List<CardCombo>();
            foreach (var combo in combos)
            {
                if (combo.CardCount < handCardCount ||
                    combo.Sets.Any(set => set.Count > 3) || combo.Runs.Any(run => run.Count > 3))
                {
                    combo.Sort();
                    possibleCombos.Add(combo);
                }
            }
            return possibleCombos;
        }

        private static void GetPossibleRunCombos(List<CardCombo> possibleRunCombos, List<Run> runs, CardCombo currentRunCombo)
        {
            for (int i = 0; i < runs.Count; i++)
            {
                CardCombo currentCombo = new CardCombo(currentRunCombo);
                currentCombo.AddRun(runs[i]);

                //This fixed run alone is also a possibility
                possibleRunCombos.Add(currentCombo);

                List<Run> otherRuns = runs.GetRange(i + 1, runs.Count - (i + 1)).Where(run => !run.Intersects(runs[i])).ToList();
                if (otherRuns.Count > 0)
                    GetPossibleRunCombos(possibleRunCombos, otherRuns, currentCombo);
            }
        }

        /// <summary>
        /// Calculates all possible combinations of card packs that could be laid down.
        /// Stores the result in the passed List of LaydownCards 'possibleCombos'
        /// <param name="possibleCombos"> The list which will contain all possible combinations in the end</param>
        /// </summary>
        private static void GetPossibleSetAndRunCombos(List<CardCombo> possibleCombos, List<Set> sets, List<Run> runs, CardCombo currentCombo)
        {
            for (int i = 0; i < sets.Count; i++)
            {
                CardCombo combo = new CardCombo(currentCombo);
                combo.AddSet(sets[i]);

                //This fixed set alone is also a possibility
                possibleCombos.Add(combo);

                //Get all runs which are possible with the current set
                List<Run> possibleRuns = runs.Where(run => !run.Intersects(sets[i])).ToList();
                GetPossibleRunCombos(possibleCombos, possibleRuns, combo);

                //Only sets which don't intersect the current one are possible combinations
                List<Set> otherSets = sets.GetRange(i + 1, sets.Count - i - 1).Where(set => !set.Intersects(sets[i])).ToList();
                if (otherSets.Count > 0)
                    GetPossibleSetAndRunCombos(possibleCombos, otherSets, possibleRuns, combo);
            }
        }

        /// <summary>
        /// Returns all possible sets which consist of 3 or 4 cards.
        /// </summary>
        public static List<Set> GetPossibleSets(List<Card> PlayerCards)
        {
            List<Set> possibleSets = new List<Set>();

            var cardsByRank = PlayerCards.GetCardsByRank().Where(rank => rank.Key != Card.CardRank.JOKER).ToList();
            foreach (var rank in cardsByRank)
                GetPossibleSets(possibleSets, rank.Value, new List<Card>());

            return possibleSets;
        }

        private static void GetPossibleSets(List<Set> possibleSets, List<Card> availableCards, List<Card> currentSet)
        {
            for (int i = 0; i < availableCards.Count; i++)
            {
                Card card = availableCards[i];
                var newSet = new List<Card>(currentSet) { card };

                if (newSet.Count > 4)
                    continue;

                if (IsValidSet(newSet))
                {
                    if (newSet.Count == 3 || newSet.Count == 4)
                        possibleSets.Add(new Set(newSet));
                }

                List<Card> otherCards = availableCards.
                        GetRange(i + 1, availableCards.Count - (i + 1)).
                        Where(c => c.Suit != card.Suit).
                        ToList();
                GetPossibleSets(possibleSets, otherCards, newSet);
            }
        }

        /// <summary>
        /// Returns all possible runs of length >= 3 which can be found in 'PlayerCards'
        /// <param name="PlayerCards">The list of cards in the player's hand</param>
        /// </summary>
        public static List<Run> GetPossibleRuns(List<Card> PlayerCards)
        {
            var possibleRuns = new List<List<Card>>();
            var playerCardsWithoutJokers = PlayerCards.Where(c => !c.IsJoker()).ToList();
            GetPossibleRuns(possibleRuns, playerCardsWithoutJokers, playerCardsWithoutJokers, new List<Card>());

            var runs = new List<Run>();
            foreach (var r in possibleRuns)
                runs.Add(new Run(r));
            return runs;
        }

        /// <summary>
        /// <param name="PlayerCardsWithoutJoker"> Must not contain any joker cards!</param>
        /// </summary>
        private static void GetPossibleRuns(List<List<Card>> possibleRuns, List<Card> PlayerCardsWithoutJoker, List<Card> availableCards, List<Card> currentRun, int minLength = 3, int maxLength = 14)
        {
            foreach (Card card in availableCards)
            {
                //A card cannot start a run if there's less than minLength higher ranks, unless it's an ACE (assuming minLength never is set to > 13)
                if (currentRun.Count == 0 && card.Rank != Card.CardRank.ACE && (int)card.Rank + (minLength - 1) > (int)Card.CardRank.ACE)
                    continue;

                var newRun = new List<Card>(currentRun) { card };
                if (newRun.Count >= minLength && newRun.Count <= maxLength)
                    possibleRuns.Add(newRun);

                List<Card> higherCards = GetCardOneRankHigher(PlayerCardsWithoutJoker, newRun.Last(), newRun.Count == 1);
                GetPossibleRuns(possibleRuns, PlayerCardsWithoutJoker, higherCards, newRun, minLength, maxLength);
            }
        }

        /// <summary>
        /// Returns all cards in PlayerCards which have the same suit and are one rank higher than 'card'.
        /// 'firstInRun': whether 'card' is the first card in a run. Used to determine whether ACE can connect to TWO or to KING
        /// </summary>
        /// <returns> The card or null if none was found </returns>
        private static List<Card> GetCardOneRankHigher(List<Card> PlayerCards, Card card, bool firstInRun)
        {
            List<Card> foundCards = new List<Card>();
            foreach (Card otherCard in PlayerCards)
            {
                if (otherCard == card || otherCard.Suit != card.Suit || foundCards.Contains(otherCard))
                    continue;
                //Allow going from ACE to TWO but only if ACE is the first card in the run
                if (firstInRun && card.Rank == Card.CardRank.ACE && otherCard.Rank == Card.CardRank.TWO)
                    foundCards.Add(otherCard);
                else if (otherCard.Rank == card.Rank + 1)
                    foundCards.Add(otherCard);
            }
            return foundCards;
        }

        /// <summary>
        /// Looks for duos which could form complete 3-card-runs using a joker and returns all possible combinations using the available joker cards
        /// </summary>
        public static List<Run> GetPossibleJokerRuns(List<Card> PlayerCards, List<Card> LaidDownCards, List<Set> possibleSets, List<Run> possibleRuns, bool logMessage)
        {
            //TODO check why possibleSets and possibleRuns are not used!
            List<Card> jokerCards = PlayerCards.Where(c => c.IsJoker()).ToList();
            if (!jokerCards.Any())
                return new List<Run>();

            var duoRuns = GetAllDuoRuns(PlayerCards, LaidDownCards, logMessage);
            if (!duoRuns.Any())
                return new List<Run>();

            var possibleJokerRuns = new List<Run>();

            foreach (var duo in duoRuns)
            {
                Card.CardColor runColor = duo.GetFirstCard().Color;
                var matchingJokers = jokerCards.Where(j => j.Color == runColor);
                foreach (var joker in matchingJokers)
                {
                    if (duo[1].Rank - duo[0].Rank == 1 || (duo[0].Rank == Card.CardRank.ACE && duo[1].Rank == Card.CardRank.TWO))
                    {
                        if (duo[0].Rank != Card.CardRank.ACE)
                        {
                            var newRun = new Run(new List<Card>() { joker, duo[0], duo[1] });
                            possibleJokerRuns.Add(newRun);
                        }
                        if (duo[1].Rank != Card.CardRank.ACE)
                        {
                            var newRun = new Run(new List<Card>() { duo[0], duo[1], joker });
                            possibleJokerRuns.Add(newRun);
                        }
                    }
                    else
                    {
                        var newRun = new Run(new List<Card>() { duo[0], joker, duo[1] });
                        possibleJokerRuns.Add(newRun);
                    }
                }
            }
            return possibleJokerRuns;
        }

        public static List<List<Card>> GetAllDuoRuns(List<Card> PlayerCards, List<Card> LaidDownCards, bool logMessage)
        {
            var duoRuns = new List<List<Card>>();
            var playerCardsWithoutJokers = PlayerCards.Where(c => !c.IsJoker()).ToList();
            GetPossibleRuns(duoRuns, playerCardsWithoutJokers, playerCardsWithoutJokers, new List<Card>(), 2, 2);

            // Don't bother keeping duo runs if all possible run combinations were laid down already
            var tmpRuns = new List<List<Card>>(duoRuns);
            foreach (var duoRun in tmpRuns)
            {
                Card c1 = duoRun[0];
                Card c2 = duoRun[1];
                Card.CardSuit runSuit = c1.Suit;

                Card.CardRank lowerRank = Card.CardRank.JOKER;
                int lowerCount = 0;
                bool anyLowerLeft = true;
                if (c1.Rank == Card.CardRank.TWO)
                    lowerRank = Card.CardRank.ACE;
                else if (c1.Rank > Card.CardRank.TWO)
                    lowerRank = c1.Rank - 1;
                if (lowerRank != Card.CardRank.JOKER)
                {
                    lowerCount = LaidDownCards.Count(c => c.Rank == lowerRank && c.Suit == runSuit);
                    anyLowerLeft = lowerCount < 2;
                }

                Card.CardRank higherRank = Card.CardRank.JOKER;
                int higherCount = 0;
                bool anyHigherLeft = true;
                if (c2.Rank < Card.CardRank.ACE)
                    higherRank = c2.Rank + 1;
                if (higherRank != Card.CardRank.JOKER)
                {
                    higherCount = LaidDownCards.Count(c => c.Rank == higherRank && c.Suit == runSuit);
                    anyHigherLeft = higherCount < 2;
                }

                if ((!anyLowerLeft && higherRank == Card.CardRank.JOKER) ||
                    (!anyHigherLeft && lowerRank == Card.CardRank.JOKER) ||
                    (!anyHigherLeft && !anyLowerLeft))
                {
                    if (logMessage)
                        Tb.I.GameMaster.LogMsg("Not saving " + c1 + c2 + " because all possible cards were already laid down twice.", LogType.Log);
                    duoRuns.Remove(duoRun);
                }
            }

            //Find duos with the card in the middle missing
            foreach (Card c1 in playerCardsWithoutJokers)
            {
                foreach (Card c2 in playerCardsWithoutJokers)
                {
                    if (c1 == c2 || c1.Suit != c2.Suit || c1.Rank == c2.Rank)
                        continue;

                    if ((c1.Rank == Card.CardRank.ACE && c2.Rank == Card.CardRank.THREE) ||
                        (c1.Rank == c2.Rank - 2))
                    {
                        var middleRank = c1.Rank + 1;
                        var middleSuit = c1.Suit;
                        if (LaidDownCards.Count(c => c.Rank == middleRank && c.Suit == middleSuit) < 2)
                            duoRuns.Add(new List<Card>() { c1, c2 });
                        else if (logMessage)
                            Tb.I.GameMaster.LogMsg("Not saving " + c1 + c2 +
                                " because " + Card.RankLetters[middleRank] + Card.SuitSymbols[middleSuit] +
                                " was already laid down twice!", LogType.Log);
                    }
                }
            }
            return duoRuns;
        }

        /// <summary>
        /// Returns all possible sets which can be created using joker cards excluding cards used in 'possibleSets' and 'possibleRuns'
        /// </summary>
        public static List<Set> GetPossibleJokerSets(List<Card> PlayerCards, List<Card> LaidDownCards, List<Set> possibleSets, List<Run> possibleRuns, bool logMessage)
        {
            //TODO check why possibleSets and possibleRuns are not used!
            List<Card> jokerCards = PlayerCards.Where(c => c.IsJoker()).ToList();
            if (!jokerCards.Any())
                return new List<Set>();

            var duoSets = GetAllDuoSets(PlayerCards, LaidDownCards, logMessage);
            if (!duoSets.Any())
                return new List<Set>();

            var possibleJokerSets = new List<Set>();

            // First, create trios out of duos where both cards have the same color
            // for each trio, save all posslbe combinations with available jokers
            for (int i = 0; i < 2; i++)
            {
                Card.CardColor color = (Card.CardColor)i;
                var sameColorDuos = duoSets.Where(duo => duo.All(c => c.Color == color));

                foreach (var duo in sameColorDuos)
                {
                    //Find all available jokers with the opposite color
                    var possibleJokers = jokerCards.Where(j => j.Color != color);
                    foreach (var joker in possibleJokers)
                    {
                        var newSet = new Set(duo[0], duo[1], joker);
                        possibleJokerSets.Add(newSet);
                    }
                }
            }

            // Now finish all red-black duos with all possible joker cards
            var mixedDuos = duoSets.Where(duo => duo[0].Color != duo[1].Color);
            foreach (var duo in mixedDuos)
            {
                foreach (var jokerCard in jokerCards)
                {
                    var newSet = new Set(duo[0], duo[1], jokerCard);
                    possibleJokerSets.Add(newSet);
                }
            }

            return possibleJokerSets;
        }

        /// <summary>
        /// Returns all possible duos (sets of two cards with different suits) from 'PlayerCards'
        /// </summary>
        public static List<List<Card>> GetAllDuoSets(List<Card> PlayerCards, List<Card> LaidDownCards, bool logMessage)
        {
            var allDuos = new List<List<Card>>();

            var cardsByRank = PlayerCards.GetCardsByRank()
                                            .Where(entry => entry.Key != Card.CardRank.JOKER && entry.Value.Count >= 2)
                                            .ToList();

            foreach (var entry in cardsByRank)
            {
                var newDuos = new List<List<Card>>();
                var rank = entry.Key;
                foreach (Card c1 in entry.Value)
                {
                    for (int j = entry.Value.IndexOf(c1); j < entry.Value.Count; j++)
                    {
                        Card c2 = entry.Value[j];
                        if (c1.Suit == c2.Suit)
                            continue;
                        var otherSuits = Card.GetOtherTwo(c1.Suit, c2.Suit);
                        int count1 = LaidDownCards.Count(c => c.Rank == rank && c.Suit == otherSuits[0]);
                        int count2 = LaidDownCards.Count(c => c.Rank == rank && c.Suit == otherSuits[1]);

                        if (count1 + count2 < 4)
                            newDuos.Add(new List<Card>() { c1, c2 });
                        else if (logMessage)
                            Tb.I.GameMaster.LogMsg("Not saving " + c1 + c2 + " because " +
                                Card.RankLetters[rank] + Card.SuitSymbols[otherSuits[0]] +
                                "/" + Card.SuitSymbols[otherSuits[1]] + " already laid twice each.", LogType.Log);
                    }
                }
                allDuos.AddRange(newDuos);
            }
            return allDuos;
        }

        /// <summary>
        /// Returns whether the given list of cards could form a valid run
        /// </summary>
        public static bool IsValidRun(List<Card> cards)
        {
            if (cards.Count < 3)
                return false;

            //A run can only consist of cards with the same suit (or joker with the matching color)
            Card representiveCard = cards.GetFirstCard();
            if (representiveCard == null)
                return false;

            foreach (var card in cards)
            {
                if (!card.IsJoker())
                {
                    if (card.Suit != representiveCard.Suit)
                        return false;
                }
                else
                {
                    if (card.Color != representiveCard.Color)
                        return false;
                }
            }

            for (int i = 0; i < cards.Count - 1; i++)
            {
                //Ace can be the start of a run
                if (i == 0 && cards[i].Rank == Card.CardRank.ACE)
                {
                    //Run is only valid if next card is a TWO or a JOKER
                    if (cards[i + 1].Rank != Card.CardRank.TWO && !cards[i + 1].IsJoker())
                        return false;
                }
                //otherwise, rank has to increase by one
                else if (cards[i + 1].Rank != cards[i].Rank + 1 && !cards[i].IsJoker() && !cards[i + 1].IsJoker())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns whether the given list of cards could form a valid set
        /// </summary>
        public static bool IsValidSet(List<Card> cards)
        {
            if (cards.Count < 3 || cards.Count > 4)
                return false;

            //A set can only consist of cards with the same rank and/or a joker
            if (cards.Any(c => !c.IsJoker() && c.Rank != cards.GetFirstCard().Rank))
                return false;

            var usedSuits = new List<Card.CardSuit>();
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].IsJoker())
                    continue; //Skip checking joker for now

                var suit = cards[i].Suit;
                if (usedSuits.Contains(suit))
                    return false;
                usedSuits.Add(suit);
            }

            //Check joker now if necessary
            Card joker = cards.FirstOrDefault(c => c.IsJoker());
            if (joker != null)
            {
                if (joker.IsBlack()
                    && usedSuits.Contains(Card.CardSuit.CLUBS)
                    && usedSuits.Contains(Card.CardSuit.SPADES))
                    return false;
                if (joker.IsRed()
                    && usedSuits.Contains(Card.CardSuit.HEARTS)
                    && usedSuits.Contains(Card.CardSuit.DIAMONDS))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the index of the first card which is not a joker, starting the search at startIndex and searching in the desired direction.
        /// Returns -1 if none was found.
        /// </summary>
        public static int GetFirstNonJokerCardIdx(List<Card> cards, int startIndex, bool searchForward)
        {
            int increment = searchForward ? +1 : -1;

            for (int i = startIndex; i < cards.Count && i >= 0; i += increment)
            {
                if (!cards[i].IsJoker())
                    return i;
            }
            return -1;
        }
    }

}