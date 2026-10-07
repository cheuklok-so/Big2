namespace Big2.Models.Game
{
    public static class CardComparer
    {
        public static void ValidateSingleIsStronger(Card card, Card currentCard)
        {
            int rankComparison = card.Rank.CompareTo(currentCard.Rank);
            if (rankComparison > 0)
            {
                return;
            }
            if (rankComparison == 0 && card.Suit.CompareTo(currentCard.Suit) > 0)
            {
                return;
            }
            throw new InvalidOperationException("You have to play a stronger single card.");
        }
        public static void ValidatePairIsStronger(List<Card> cards, List<Card> currentCards)
        {
            Rank newRank = cards[0].Rank;
            Rank currentRank = currentCards[0].Rank;
            if(newRank < currentRank)
            {
                throw new InvalidOperationException("You have to play a stronger pair.");
            }
            if(newRank > currentRank)
            {
                return;
            }
            Card strongestNewCards = cards.OrderByDescending(card => card.Suit).First();
            Card strongestCurrentCards = currentCards.OrderByDescending(card => card.Suit).First();
            if (strongestNewCards.Suit <= strongestCurrentCards.Suit)
            {
                throw new InvalidOperationException("You have to play a stronger pair.");
            }
        }
        public static void ValidateTripleIsStronger(List<Card> cards, List<Card> currentCards)
        {
            Rank newRank = cards[0].Rank;
            Rank currentRank = currentCards[0].Rank;
            if(newRank < currentRank)
            {
                throw new InvalidOperationException("You have to play a stronger triple.");
            }
            if(newRank > currentRank)
            {
                return;
            }
            Card strongestNewCards = cards.OrderByDescending(card => card.Suit).First();
            Card strongestCurrentCards = currentCards.OrderByDescending(card => card.Suit).First();
            if (strongestNewCards.Suit <= strongestCurrentCards.Suit)
            {
                throw new InvalidOperationException("You have to play a stronger triple.");
            }
        }
        public static int CompareFiveCardPlays(List<Card> play1, List<Card> play2)
        {
            PlayType type1 = PlayTypeEvaluator.GetPlayType(play1);
            PlayType type2 = PlayTypeEvaluator.GetPlayType(play2);

            // They have to be different types of plays, otherwise we can't compare them.
            // Straight < Flush < FullHouse < FourOfAKind < StraightFlush
            if (type1 != type2)
            {
                return type1.CompareTo(type2);
            }

            // If they are the same type of play, we compare their ranks.
            try
            {
                switch (type1)
                {
                    case PlayType.StraightFlush:
                        ValidateStraightFlushIsStronger(play1, play2);
                        break;
                    case PlayType.FourOfAKind:
                        ValidateFourOfAKindIsStronger(play1, play2);
                        break;
                    case PlayType.FullHouse:
                        ValidateFullHouseIsStronger(play1, play2);
                        break;
                    case PlayType.Flush:
                        ValidateFlushIsStronger(play1, play2);
                        break;
                    case PlayType.Straight:
                        ValidateStraightIsStronger(play1, play2);
                        break;
                    default:
                        throw new ArgumentException("Both plays must be five card plays");
                }
                // Play1 beat Play2, so return 1
                return 1;

            }
            catch (InvalidOperationException ex)
            {
                // Play1 did not beat Play2
                // Check the opposite irection to see if Play2 beat Play1
                try
                {
                    switch (type1)
                    {
                        case PlayType.StraightFlush:
                            ValidateStraightFlushIsStronger(play2, play1);
                            break;
                        case PlayType.FourOfAKind:
                            ValidateFourOfAKindIsStronger(play2, play1);
                            break;
                        case PlayType.FullHouse:
                            ValidateFullHouseIsStronger(play2, play1);
                            break;
                        case PlayType.Flush:
                            ValidateFlushIsStronger(play2, play1);
                            break;
                        case PlayType.Straight:
                            ValidateStraightIsStronger(play2, play1);
                            break;
                        default:
                            throw new ArgumentException("Both plays must be five card plays");
                    }
                    // Play2 beat Play1, so return -1
                    return -1;
                }
                catch (InvalidOperationException)
                {
                    // Neither play beat the other, so they are equal
                    return 0;
                }
            }
        }
        public static void ValidateFourOfAKindIsStronger(List<Card> cards, List<Card> currentCards)
        {
            Rank newRank = cards.GroupBy(card => card.Rank).First(group => group.Count() == 4).Key;
            Rank currentRank = currentCards.GroupBy(card => card.Rank).First(group => group.Count() == 4).Key;
            if (newRank <= currentRank)
            {
                throw new InvalidOperationException("You have to play a stronger four of a kind.");
            }
            if(newRank > currentRank)
            {
                return;
            }
        }
        public static void ValidateStraightIsStronger(List<Card> cards, List<Card> currentCards)
        {
            // A2345 is a special case, so we use int to compare Straight strength.
            int newStraightRank = GetStraightRank(cards); 
            int currentStraightRank = GetStraightRank(currentCards);
            if (newStraightRank < currentStraightRank)
            {
                throw new InvalidOperationException("You have to play a stronger straight.");
            }
            if(newStraightRank == currentStraightRank)
            {
                Card newHighCard = GetStraightHighCard(cards);
                Card currentHighCard = GetStraightHighCard(currentCards);
                if (newHighCard.Suit <= currentHighCard.Suit)
                {
                    throw new InvalidOperationException("You have to play a stronger straight.");
                }
            }
        }
        public static int GetStraightRank(List<Card> cards)
        {
            if (IsA2345(cards))
            {
                return 14;
            }
            return (int)cards.Max(card => card.Rank);
        }
        private static Card GetStraightHighCard(List<Card> cards)
        {
            return cards.MaxBy(card => card.Rank)!;
        }
        public static bool IsA2345(List<Card> cards)
        {
            List<Rank> ranks = cards.Select(card => card.Rank).OrderBy(rank => rank).ToList();
            return ranks.SequenceEqual(new List<Rank> { Rank.Three, Rank.Four, Rank.Five, Rank.Ace, Rank.Two });
        }
        public static void ValidateFlushIsStronger(List<Card> cards, List<Card> currentCards)
        {
            int suitComparison = cards[0].Suit.CompareTo(currentCards[0].Suit);
                if (suitComparison > 0)
                {
                    return;
                }
                if (suitComparison < 0)
                {
                    throw new InvalidOperationException("You have to play a stronger flush.");
                }
                // Same suit, so we compare ranks from highest to lowest.
            List<Card> newCards = cards.OrderByDescending(card => card.Rank).ToList();
            List<Card> oldCards = currentCards.OrderByDescending(card => card.Rank).ToList();
            for (int i =0; i< newCards.Count; i++)
            {
                int rankComparison = newCards[i].Rank.CompareTo(oldCards[i].Rank);
                if (rankComparison > 0)
                {
                    return;
                }
                if (rankComparison < 0)
                {
                    throw new InvalidOperationException("You have to play a stronger flush.");
                }
            }
                throw new InvalidOperationException("You have to play a stronger flush.");             
        }
        public static void ValidateFullHouseIsStronger(List<Card> cards, List<Card> currentCards)
        {
            Rank newTripleRank = cards.GroupBy(card => card.Rank).First(group => group.Count() == 3).Key;
            Rank currentTripleRank = currentCards.GroupBy(card => card.Rank).First(group => group.Count() == 3).Key;
            if (newTripleRank <= currentTripleRank)
            {
                throw new InvalidOperationException("You have to play a stronger full house.");
            }
        }
        public static void ValidateStraightFlushIsStronger(List<Card> cards, List<Card> currentCards)
        {
            // Same as Straight, we use int to compare Straight Flush strength.
            int newStraightRank = GetStraightRank(cards);
            int currentStraightRank = GetStraightRank(currentCards);
            if (newStraightRank < currentStraightRank)
            {
                throw new InvalidOperationException("You have to play a stronger straight flush.");
            }
            if(newStraightRank == currentStraightRank)
            {
                if (cards[0].Suit <= currentCards[0].Suit)
                {
                    throw new InvalidOperationException("You have to play a stronger straight flush.");
                }
            }
        }
    }
}
