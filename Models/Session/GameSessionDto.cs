using Big2.Models.Game;
using GameModel = Big2.Models.Game.Game;
namespace Big2.Models.Session
{
    public class GameSessionDto
    {
        public List<PlayerSessionDto> Players { get; set; }
        = new List<PlayerSessionDto>();
        public bool IsFirstPlay { get; set; }
        public int? CurrentPlayerId { get; set; }
        public int? LastPlayerWhoPlayedId { get; set; }
        public int ConsecutivePassCount { get; set; }
        public List<CardSessionDto>? CurrentPlayCards { get; set; }
        public bool IsGameOver { get; set; }
        public int? WinnerId { get; set; }
        public static GameSessionDto FromGame(GameModel game)
        {
            GameSessionDto dto = new GameSessionDto();
            dto.IsFirstPlay = game.IsFirstPlay;
            dto.LastPlayerWhoPlayedId = game.LastPlayerWhoPlayed?.PlayerId;
            dto.ConsecutivePassCount = game.ConsecutivePassCount;
            dto.IsGameOver = game.IsGameOver;
            dto.WinnerId = game.Winner?.PlayerId;
            foreach (Player player in game.Players)
            {
                PlayerSessionDto playerDto = new PlayerSessionDto
                {
                    PlayerId = player.PlayerId,
                    IsAi = player.IsAi
                };
                foreach(Card card in player.Hand)
                {
                    playerDto.Hand.Add(new CardSessionDto
                    {
                        Suit = card.Suit,
                        Rank = card.Rank
                    });
                }
                dto.Players.Add(playerDto);
            }
            dto.CurrentPlayerId = game.CurrentPlayer?.PlayerId;
            if(game.CurrentPlay != null)
            {
                dto.CurrentPlayCards = game.CurrentPlay.Cards.Select(card => new CardSessionDto{
                    Suit = card.Suit,
                        Rank = card.Rank
                })
                .ToList();
            }
            return dto;
        }
        public GameModel ToGame()
        {
            GameModel game = new GameModel();
            for(int i =0; i<Players.Count; i++)
            {
                PlayerSessionDto playerDto = Players[i];
                Player player = game.Players.Single(p => p.PlayerId == playerDto.PlayerId);

                player.Hand.Clear();

                foreach(CardSessionDto cardDto in playerDto.Hand)
                {
                    player.Hand.Add(new Card(cardDto.Suit, cardDto.Rank));
                }
            }
            List<Card>? currentPlayCards = null;
            if(CurrentPlayCards != null)
            {
                currentPlayCards = CurrentPlayCards.Select(cardDto => new Card(cardDto.Suit, cardDto.Rank)).ToList();
            }
            game.RestoreIsFirstPlay(IsFirstPlay);
            game.RestoreCurrentPlayer(CurrentPlayerId);
            game.RestoreLastPlayerWhoPlayed(LastPlayerWhoPlayedId);
            game.RestoreCurrentPlay(currentPlayCards);
            game.RestoreConsecutivePassCount(ConsecutivePassCount);
            game.RestoreGameOverState(IsGameOver, WinnerId);
            return game;
        }
    }
}
