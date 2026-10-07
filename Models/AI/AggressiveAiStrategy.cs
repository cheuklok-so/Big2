using GameModel = Big2.Models.Game.Game;
using Big2.Models.Game;
using System.Text.RegularExpressions;
namespace Big2.Models.AI
{
    public static class AggressiveAiStrategy
    {
        public static List<Card>? ChooseLeadPlay(AiPlayer ai, GameModel game)
        {
            List<Card> hand = ai.Player.Hand;
            List<List<Card>> fiveCardPlays = GetFiveCardPlays(hand);
            if (fiveCardPlays.Count > 0)
            {
                fiveCardPlays.Sort(CardComparer.CompareFiveCardPlays);
                return fiveCardPlays.First();
            }

            // If no 5-card plays are available, choose the lowest triple.
            List<Card>? triple = hand
                .GroupBy(card => card.Rank)
                .Where(group => group.Count() >= 3)
                .OrderBy(group => group.Key)
                .Select(group => group
                .OrderBy(card => card.Suit)
                .Take(3)
                .ToList())
                .FirstOrDefault();
            if (triple != null)
            {
                return triple;
            }

            // If no triples are available, choose the lowest pair.
            List<Card>? pair = hand
                .GroupBy(card => card.Rank)
                .Where(group => group.Count() >= 2)
                .OrderBy(group => group.Key)
                .Select(group => group
                .OrderBy(card => card.Suit)
                .Take(2)
                .ToList())
                .FirstOrDefault();
            if (pair != null)
            {
                return pair;
            }

            // If no pairs are available, choose the lowest single card.
            Card? single = hand
                .OrderBy(card => card.Rank)
                .ThenBy(card => card.Suit)
                .FirstOrDefault();
            if (single != null)
            {
                return new List<Card> { single };
            }
            return null;
        }
        private static List<List<Card>> GetFiveCardPlays(List<Card> hand)
        {
            List<List<Card>> fiveCardPlays = new List<List<Card>>();

            // Generate every possible 5-card combination from the hand.
            for (int i = 0; i < hand.Count - 4; i++)
            {
                for (int j = i + 1; j < hand.Count - 3; j++)
                {
                    for (int k = j + 1; k < hand.Count - 2; k++)
                    {
                        for (int l = k + 1; l < hand.Count - 1; l++)
                        {
                            for (int m = l + 1; m < hand.Count; m++)
                            {
                                List<Card> combination = new List<Card> { hand[i], hand[j], hand[k], hand[l], hand[m] };
                                if (PlayTypeEvaluator.TryGetPlayType(combination, out PlayType playType))
                                {
                                    if (playType == PlayType.StraightFlush ||
                                        playType == PlayType.FourOfAKind ||
                                        playType == PlayType.FullHouse ||
                                        playType == PlayType.Flush ||
                                        playType == PlayType.Straight)
                                    {
                                        fiveCardPlays.Add(combination);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return fiveCardPlays;
        }
        public static Card? ChooseSingle(AiPlayer ai, List<Card> playableCards)
        {
            if (playableCards.Count == 0)
            {
                return null;
            }
            List<List<Card>> fiveCardPlays = GetFiveCardPlays(ai.Player.Hand);

            // Find every card that belongs to at least one comeplete 5-card play.
            HashSet<Card> protectedCards = fiveCardPlays.SelectMany(play => play).ToHashSet();

            // Prefer a playable card that does not break any complete five-card play.
            Card? freeCard = playableCards
                .Where(card => !protectedCards.Contains(card))
                .OrderBy(card => card.Rank)
                .ThenBy(card => card.Suit)
                .FirstOrDefault();
            if (freeCard != null)
            {
                return freeCard;
            }

            // If every winning card belongs to a five-card play, breaking a combination is allowed.
            return playableCards
                .OrderBy(card => card.Rank)
                .ThenBy(card => card.Suit)
                .FirstOrDefault();
        }
        public static List<Card>? ChoosePair(AiPlayer ai, List<List<Card>> playablePairs)
        {
            if (playablePairs.Count == 0)
            {
                return null;
            }
            List<List<Card>> fiveCardPlays = GetFiveCardPlays(ai.Player.Hand);

            // Find every card that belongs to at least one comeplete 5-card play.
            HashSet<Card> protectedCards = fiveCardPlays.SelectMany(play => play).ToHashSet();

            // Prefer a Pair where neither card belolngs to a complete five-card play.
            List<Card>? freePair = playablePairs
                .Where(pair => pair.All(card => !protectedCards.Contains(card)))
                .OrderBy(pair => pair[0].Rank)
                .ThenBy(pair => pair.Max(card => card.Suit))
                .FirstOrDefault();
            if (freePair != null)
            {
                return freePair;
            }

            // If every winning card belongs to a five-card play, breaking a combination is allowed.
            return playablePairs
                .OrderBy(pair => pair[0].Rank)
                .ThenBy(pair => pair.Max(card => card.Suit))
                .FirstOrDefault();
        }
        public static List<Card>? ChooseTriple(AiPlayer ai, List<List<Card>> playableTriples)
        {
            if (playableTriples.Count == 0)
            {
                return null;
            }
            List<List<Card>> fiveCardPlays = GetFiveCardPlays(ai.Player.Hand);

            // Find every card that belongs to at least one comeplete 5-card play.
            HashSet<Card> protectedCards = fiveCardPlays.SelectMany(play => play).ToHashSet();

            // Prefer a Triple where none of the cards belong to a complete five-card play.
            List<Card>? freeTriple = playableTriples
                .Where(triple => triple.All(card => !protectedCards.Contains(card)))
                .OrderBy(triple => triple[0].Rank)
                .ThenBy(triple => triple.Max(card => card.Suit))
                .FirstOrDefault();
            if (freeTriple != null)
            {
                return freeTriple;
            }

            // If every winning card belongs to a five-card play, breaking a combination is allowed.
            return playableTriples
                .OrderBy(triple => triple[0].Rank)
                .ThenBy(triple => triple.Max(card => card.Suit))
                .FirstOrDefault();
        }
    }
}
