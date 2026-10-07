namespace Big2.Models.Game
{
    public class Play
    {
        public List<Card> Cards { get; private set; }

        public Play(List<Card> cards)
        {
            Cards = new List<Card>(cards);
        }
    }
}
