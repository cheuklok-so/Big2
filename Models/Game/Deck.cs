namespace Big2.Models.Game
{
    public class Deck
    {
        public List<Card> Cards { get; private set; }
        public Deck()
        {
            Cards = new List<Card>();
            CreateDeck();
        }
        private void CreateDeck()
        {
            foreach(Suit suit in Enum.GetValues<Suit>())
            {
                foreach (Rank rank in Enum.GetValues<Rank>())
                {
                    Card card = new Card(suit, rank);
                    Cards.Add(card);
                }
            }
        }
        //By using Fisher-Yates Shuffle, we can randomize the order of cards.
        public void Shuffle()
        {
            //Start from the last card and move backwards.
            //Within this Forloop, we set a limit for how many times the randomization will goes for.
            for (int i = Cards.Count -1; i>0; i--)
            {
                //let int randomIndex is an randomized number from 0 to i
                int randomIndex = Random.Shared.Next(i + 1);
                //Temporary SavePoint = i
                Card temp = Cards[i];
                //Move the randomly selected card to i
                Cards[i] = Cards[randomIndex];
                //Move the previously saved card to the next random index
                Cards[randomIndex] = temp;
            }
        }
        public Card DrawCard()
        {
            if (Cards.Count == 0)
            {
                throw new InvalidOperationException("Cannot draw a card because the deck is empty.");
            }
            Card card = Cards[Cards.Count - 1];
            Cards.RemoveAt(Cards.Count - 1);
            return card;
        }
    }
}
