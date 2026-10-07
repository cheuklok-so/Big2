namespace Big2.Models.Game
{
    public static class HandEvaluator
    {
        public static bool IsSingle(List<Card> cards)
        {
            return cards.Count == 1;
        }
        private static bool IsSameRank(List<Card> cards, int exceptedCount)
        {
            return cards.Count == exceptedCount && cards.All(c => c.Rank == cards[0].Rank);
        }
        public static bool IsPair(List<Card> cards)
        {
            return IsSameRank(cards, 2);
        }
        public static bool IsTriple(List<Card> cards)
        {
            return IsSameRank(cards, 3);
        }
        public static bool IsFourOfAKind(List<Card> cards)
        {
            if(cards.Count != 5)
            {
                return false;
            }
            return cards.GroupBy(c => c.Rank).Any(g => g.Count() == 4);
        }
        public static bool IsStraight(List<Card> cards)
        {
            if(cards.Count != 5)
            {
                return false;
            }
            List<Rank> ranks = cards.Select(card => card.Rank).OrderBy(rank => rank).ToList();
            // Special Straight: A2345
            if(CardComparer.IsA2345(cards))
            {
                return true;
            }
            if (ranks.Contains(Rank.Two))
            {
                return false;
            }
            for (int i =1; i<ranks.Count; i++)
            {
                if (ranks[i] != ranks[i - 1] + 1)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsFlush(List<Card> cards)
        {
            return cards.Count == 5 && cards.All(c => c.Suit == cards[0].Suit);
        }
        public static bool IsFullHouse(List<Card> cards)
        {
            if(cards.Count != 5)
            {
                return false;
            }
            var groupCounts = cards.GroupBy(card => card.Rank).Select(group => group.Count()).OrderByDescending(count => count).ToList();
            return groupCounts.SequenceEqual(new[] { 3, 2 });
        }
        public static bool IsStraightFlush(List<Card> cards)
        {
            return IsStraight(cards) && IsFlush(cards);
        }
    }
}
