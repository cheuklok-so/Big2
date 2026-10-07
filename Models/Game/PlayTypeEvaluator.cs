namespace Big2.Models.Game
{
    public class PlayTypeEvaluator
    {
        public static PlayType GetPlayType(List<Card> cards)
        {
            if (HandEvaluator.IsStraightFlush(cards))
            {
                return PlayType.StraightFlush;
            }
            else if (HandEvaluator.IsFourOfAKind(cards))
            {
                return PlayType.FourOfAKind;
            }
            else if (HandEvaluator.IsFullHouse(cards))
            {
                return PlayType.FullHouse;
            }
            else if (HandEvaluator.IsFlush(cards))
            {
                return PlayType.Flush;
            }
            else if (HandEvaluator.IsStraight(cards))
            {
                return PlayType.Straight;
            }
            else if (HandEvaluator.IsTriple(cards))
            {
                return PlayType.Triple;
            }
            else if (HandEvaluator.IsPair(cards))
            {
                return PlayType.Pair;
            }
            else if (HandEvaluator.IsSingle(cards))
            {
                return PlayType.Single;
            }
            else
            {
                throw new ArgumentException("Invalid play type");
            }
        }
        public static bool TryGetPlayType(List<Card> cards, out PlayType playType)
        {
            if (HandEvaluator.IsStraightFlush(cards))
            {
                playType = PlayType.StraightFlush;
                return true;
            }
            if (HandEvaluator.IsFourOfAKind(cards))
            {
                playType = PlayType.FourOfAKind;
                return true;
            }
            if (HandEvaluator.IsFullHouse(cards))
            {
                playType = PlayType.FullHouse;
                return true;
            }
            if (HandEvaluator.IsFlush(cards))
            {
                playType = PlayType.Flush;
                return true;
            }
            if (HandEvaluator.IsStraight(cards))
            {
                playType = PlayType.Straight;
                return true;
            }
            if (HandEvaluator.IsTriple(cards))
            {
                playType = PlayType.Triple;
                return true;
            }
            if (HandEvaluator.IsPair(cards))
            {
                playType = PlayType.Pair;
                return true;
            }
            if (HandEvaluator.IsSingle(cards))
            {
                playType = PlayType.Single;
                return true;
            }
            playType = default;
            return false;
        }
    }
}
