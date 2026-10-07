namespace Big2.Models.Game
{
    public enum Suit
    {
        Diamond=1,
        Club=2,
        Heart=3,
        Spade=4
    }
    public enum Rank
    {
        Three=1,
        Four=2,
        Five=3,
        Six=4,
        Seven=5,
        Eight=6,
        Nine=7,
        Ten=8,
        Jack=9,
        Queen=10,
        King=11,
        Ace=12,
        Two=13
    }
    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}
