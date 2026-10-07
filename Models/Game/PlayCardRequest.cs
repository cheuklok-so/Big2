namespace Big2.Models.Game
{
    public class PlayCardRequest
    {
        public List<CardRequest> Cards { get; set; } = new();
    }
    public class CardRequest
    {
        public string Suit { get; set; } = "";
        public string Rank { get; set; } = "";
    }
}

