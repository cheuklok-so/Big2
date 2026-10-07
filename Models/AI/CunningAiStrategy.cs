using GameModel = Big2.Models.Game.Game;
using Big2.Models.Game;
using System.Text.RegularExpressions;
namespace Big2.Models.AI
{
    public static class CunningAiStrategy
    {
        private static HashSet<Card> GetProtectedCards(AiPlayer ai)
        {
            HashSet<Card> protectedCards = new HashSet<Card>();
            // Protect all cards that belong to any valid 5-card play.
            List<List<Card>> fiveCardPlays = ai.GetFiveCardPlaysFromHand();
            foreach(List<Card> fiveCardPlay in fiveCardPlays)
            {
                foreach(Card card in fiveCardPlay)
                {
                    protectedCards.Add(card);
                }
            }
            // Protect all 2.
            foreach(Card card in ai.Player.Hand)
            {
                if(card.Rank == Rank.Two)
                {
                    protectedCards.Add(card);
                }
            }
            return protectedCards;
        }
        public static bool IsCritical(AiPlayer ai, GameModel game)
        {
            return game.Players
                .Where(player => player != ai.Player)
                .Any(player => player.Hand.Count == 1);
        }
        public static bool IsAlert(AiPlayer ai, GameModel game)
        {
            return game.Players
                .Where(player => player != ai.Player)
                .Any(player => player.Hand.Count >= 2 && player.Hand.Count <= 9);
        }
        public static List<Card>? ChooseLeadPlay(AiPlayer ai, GameModel game)
        {
            List<Card> hand = ai.Player.Hand;
            // Critical Mode
            if(IsCritical(ai, game))
            {
                // First priority: 5-card play
                List<List<Card>> fiveCardPlays = ai.GetFiveCardPlaysFromHand();
                if (fiveCardPlays.Count > 0)
                {
                    return fiveCardPlays.First();
                }
                // Secondary priority: exact triple
                List<List<Card>> triples = ai.GetTriplesFromHand(exactTripleOnly: true);
                if(triples.Count > 0)
                {
                    return triples.OrderBy(triple => triple[0].Rank).ThenBy(triple => triple.Max(card => card.Suit)).First();
                }
                // Third priority: exact pair
                List<List<Card>> pairs = ai.GetPairsFromHand(true);
                if(pairs.Count > 0)
                {
                    return pairs.OrderBy(pair => pair[0].Rank).ThenBy(pair => pair.Max(Card => Card.Suit)).First();
                }
            }
            // Alert Mode
            if(IsAlert(ai, game))
            {
                // First priority: 5-card play
                List<List<Card>> fiveCardPlays = ai.GetFiveCardPlaysFromHand();
                if(fiveCardPlays.Count > 0)
                {
                    fiveCardPlays.Sort(CardComparer.CompareFiveCardPlays);
                    return fiveCardPlays.First();
                }
                // Secondary priority: Single (Preserve Pair and Triple)
                Card? alertSingle = hand.GroupBy(card => card.Rank).Where(group => group.Count() == 1).Select(group => group.First()).OrderBy(card => card.Rank).ThenBy(card => card.Suit).FirstOrDefault();
                if(alertSingle != null)
                {
                    return new List<Card> { alertSingle };
                }
                // If there is no true single, break the weakest group instead of being unable to lead.
                Card? fallbackSingle = hand.OrderBy(card => card.Rank).ThenBy(card => card.Suit).FirstOrDefault();
                if(fallbackSingle != null)
                {
                    return new List<Card> { fallbackSingle };
                }
                return null;
            }
            // Normal Mode
            HashSet<Card> protectedCards = GetProtectedCards(ai);
            Card? unprotectedSingle = hand.Where(card => !protectedCards.Contains(card)).OrderBy(card => card.Rank).ThenBy(card => card.Suit).FirstOrDefault();
            // Prefer leading a card that does not break a good hand.
            if(unprotectedSingle != null)
            {
                return new List<Card> { unprotectedSingle };
            }
            // If every card is protected,
            Card? single = hand.OrderBy(card => card.Rank).ThenBy(card => card.Suit).FirstOrDefault();
            if(single != null)
            {
                return new List<Card> { single };
            }
            return null;
        }
        public static Card? ChooseSingle(AiPlayer ai, GameModel game, List<Card> playableCards)
        {
            if(playableCards.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if(IsCritical(ai, game))
            {
                return playableCards.OrderByDescending(card => card.Rank).ThenByDescending(card => card.Suit).First();
            }
            // Alert Mode
            if(IsAlert(ai, game))
            {
                return playableCards.OrderBy(card => card.Rank).ThenBy(card => card.Suit).First();
            }
            // Normal Mode
            HashSet<Card> protectedCards = GetProtectedCards(ai);
            List<Card> unprotectedPlayableCards = playableCards.Where(card => !protectedCards.Contains(card)).OrderBy(card => card.Rank).ThenBy(card => card.Suit).ToList();
            // Prefer a winning card that does not break a good hand.
            if(unprotectedPlayableCards.Count > 0)
            {
                return unprotectedPlayableCards.First();
            }
            // If every winning card is protected, still play the weakest winning card instead of passing.
            return playableCards.OrderBy(card => card.Rank).ThenBy(card => card.Suit).First();
        }
        public static List<Card>? ChoosePair(AiPlayer ai, GameModel game, List<List<Card>> playablePairs)
        {
            if (playablePairs.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if (IsCritical(ai, game))
            {
                return playablePairs.OrderByDescending(pair => pair[0].Rank).ThenByDescending(pair => pair.Max(card => card.Suit)).First();
            }
            // Alert Mode
            if (IsAlert(ai, game))
            {
                return playablePairs.OrderBy(pair => pair[0].Rank).ThenBy(pair => pair.Max(card => card.Suit)).First();
            }
            // Normal Mode
            HashSet<Card> protectedCards = GetProtectedCards(ai);
            List<List<Card>> unprotectedPlayablePairs = playablePairs.Where(pair => pair.All(card => !protectedCards.Contains(card))).OrderBy(pair => pair[0].Rank).ThenBy(pair => pair.Max(card => card.Suit)).ToList();
            // Prefer a winning card that does not break a good hand.
            if (unprotectedPlayablePairs.Count > 0)
            {
                return unprotectedPlayablePairs.First();
            }
            // If every winning card is protected, still play the weakest winning card instead of passing.
            return playablePairs.OrderBy(pair => pair[0].Rank).ThenBy(pair => pair.Max(card => card.Suit)).First();
        }
        public static List<Card>? ChooseTriple(AiPlayer ai, GameModel game, List<List<Card>> playableTriples)
        {
            if (playableTriples.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if (IsCritical(ai, game))
            {
                return playableTriples.OrderByDescending(triple => triple[0].Rank).ThenBy(triple => triple.Max(card => card.Suit)).First();
            }
            // Alert Mode
            if (IsAlert(ai, game))
            {
                return playableTriples.OrderBy(triple => triple[0].Rank).ThenByDescending(triple => triple.Max(card => card.Suit)).First();
            }
            // Normal Mode
            HashSet<Card> protectedCards = GetProtectedCards(ai);
            List<List<Card>> unprotectedPlayableTriples = playableTriples.Where(triple => triple.All(card => !protectedCards.Contains(card))).OrderBy(triple => triple[0].Rank).ThenBy(triple => triple.Max(card => card.Suit)).ToList();
            // Prefer a winning card that does not break a good hand.
            if (unprotectedPlayableTriples.Count > 0)
            {
                return unprotectedPlayableTriples.First();
            }
            // If every winning card is protected, still play the weakest winning card instead of passing.
            return playableTriples.OrderBy(triple => triple[0].Rank).ThenBy(triple => triple.Max(card => card.Suit)).First();
        }
        public static List<Card>? ChooseStraight(AiPlayer ai, GameModel game, List<List<Card>> playableStraights)
        {
            if(playableStraights.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if(IsCritical(ai, game))
            {
                return playableStraights.OrderByDescending(straight => straight.Max(card => card.Rank)).ThenByDescending(straight => straight.Max(Card => Card.Suit)).First();
            }
            // Normal Mode
            return playableStraights.OrderBy(straight => straight.Max(card => card.Rank)).ThenBy(straight => straight.Max(Card => Card.Suit)).First();
        }
        public static List<Card>? ChooseFlush(AiPlayer ai, GameModel game, List<List<Card>> playableFlushes)
        {
            if(playableFlushes.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if(IsCritical(ai, game))
            {
                List<Card> strongestFlush = playableFlushes[0];
                foreach(List<Card> flush in playableFlushes)
                {
                    if(ai.CompareFlushes(flush, strongestFlush) > 0)
                    {
                        strongestFlush = flush;
                    }
                }
                return strongestFlush;
            }
            // Normal Mode
            List<Card> weakestFlush = playableFlushes[0];
            foreach(List<Card> flush in playableFlushes)
            {
                if(ai.CompareFlushes(flush, weakestFlush) < 0)
                {
                    weakestFlush = flush;
                }
            }
            return weakestFlush;
        }
        public static List<Card>? ChooseFullHouse(AiPlayer ai, GameModel game, List<List<Card>> playableFullHouses)
        {
            if(playableFullHouses.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if(IsCritical(ai, game))
            {
                return playableFullHouses.OrderByDescending(fullHouse => fullHouse.GroupBy(card => card.Rank).First(group => group.Count() == 3).Key).First();
            }
            // Normal Mode
            return playableFullHouses.OrderBy(fullHouse => fullHouse.GroupBy(card => card.Rank).First(group => group.Count() == 3).Key).First();
        }
        public static List<Card>? ChooseFourOfAKind(AiPlayer ai, GameModel game, List<List<Card>> playableFourOfAKinds)
        {
            if(playableFourOfAKinds.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if(IsCritical(ai, game))
            {
                return playableFourOfAKinds.OrderByDescending(fourOfAKind => fourOfAKind.GroupBy(card => card.Rank).First(group => group.Count() == 4).Key).First();
            }
            // Normal Mode
            return playableFourOfAKinds.OrderBy(fourOfAKind => fourOfAKind.GroupBy(card => card.Rank).First(group => group.Count() == 4).Key).First();
        }
        public static List<Card>?  ChooseStraightFlush(AiPlayer ai, GameModel game, List<List<Card>> playableStraightFlushes)
        {
            if(playableStraightFlushes.Count == 0)
            {
                return null;
            }
            // Critical Mode
            if(IsCritical(ai, game))
            {
                return playableStraightFlushes.OrderByDescending(straightFlush => CardComparer.GetStraightRank(straightFlush)).First();
            }
            // Normal Mode
            return playableStraightFlushes.OrderBy(straightFlush => CardComparer.GetStraightRank(straightFlush)).First();
        }
    }
}
