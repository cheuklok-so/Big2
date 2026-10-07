using GameModel = Big2.Models.Game.Game;
using Big2.Models.Game;
using System.Text.RegularExpressions;
namespace Big2.Models.AI
{
    public static class ConservativeAiStrategy
    {
        public static List<Card>? ChooseLeadPlay(AiPlayer ai, GameModel game)
        {
            List<Card> hand = ai.Player.Hand;
            // If the entire hand can be played, play all cards and win the game.
            if (CanPlayEntireHand(hand))
            {
                return hand.ToList();
            }
            var rankGroups = ai.Player.Hand
                .GroupBy(card => card.Rank)
                .Where(group => group.Count() <= 3)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .ToList();

            // Prefer normal-value cards and preserve K, A and 2 for later plays.
            var normalRankGroups = rankGroups
                .Where(group => !IsHighValueRank(group.Key))
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .ToList();
            if (normalRankGroups.Count > 0)
            {
                return normalRankGroups.First().ToList();
            }

            // If only high-value cards remain, prefer the largest group,
            // then choose the lowest rank when group sizes are equal.
            var highValueRankGroups = rankGroups
                .Where(group => IsHighValueRank(group.Key))
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .ToList();
            if (highValueRankGroups.Count > 0)
            {
                return highValueRankGroups.First().ToList();
            }
            return null;
        }
        private static bool IsHighValueRank(Rank rank)
        {
            return rank == Rank.King || rank == Rank.Ace || rank == Rank.Two;
        }
        private static bool CanPlayEntireHand(List<Card> hand)
        {
            if (hand.Count != 1 && hand.Count != 2 && hand.Count != 3 && hand.Count != 5)
            {
                return false;
            }
            try
            {
                PlayTypeEvaluator.GetPlayType(hand);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
        public static Card? ChooseSingle(List<Card> playableCards)
        {
            List<Card> normalValueCards = playableCards
                .Where(card => !IsHighValueRank(card.Rank))
                .OrderBy(card => card.Rank)
                .ThenBy(card => card.Suit)
                .ToList();
            if (normalValueCards.Count == 0)
            {
                return null;
            }
            return normalValueCards.First();
        }
        public static List<Card>? ChoosePair(List<List<Card>> playablePairs)
        {
            var normalValuePairs = playablePairs
                .Where(pair => !IsHighValueRank(pair[0].Rank))
                .OrderBy(pair => pair[0].Rank)
                .ThenBy(pair => pair.Max(card => card.Suit))
                .ToList();
            if (normalValuePairs.Count == 0)
            {
                return null;
            }
            return normalValuePairs.First();
        }
        public static List<Card>? ChooseTriple(List<List<Card>> playableTriples)
        {
            var normalValueTriples = playableTriples
                .Where(triple => !IsHighValueRank(triple[0].Rank))
                .OrderBy(triple => triple[0].Rank)
                .ThenBy(triple => triple.Max(card => card.Suit))
                .ToList();
            if (normalValueTriples.Count == 0)
            {
                return null;
            }
            return normalValueTriples.First();
        }
    }
}
