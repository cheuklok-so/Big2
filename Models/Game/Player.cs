namespace Big2.Models.Game
{
    public class Player
    {
        public int PlayerId { get; private set; }
        public bool IsAi { get; private set; }
        public List<Card> Hand { get; private set; }
        public Player(int playerId, bool isAi)
        {
            PlayerId = playerId;
            IsAi = isAi;
            Hand = new List<Card>();
        }
        public void ReceiveCard(Card card)
        {
            Hand.Add(card);
        }
        public void ClearHand()
        {
            Hand.Clear();
        }
        public void SortHand()
        {
            Hand.Sort((card1, card2) =>
            {
                int rankComparison = card1.Rank.CompareTo(card2.Rank);
                if (rankComparison != 0)
                {
                    return rankComparison;
                }
                return card1.Suit.CompareTo(card2.Suit);
            });
        }
        public void PlayCard(Card card)
        {
            if(!Hand.Contains(card))
            {
                throw new InvalidOperationException("Player does not have the specified card in hand.");
            }
            Hand.Remove(card);
        }
    }
}
