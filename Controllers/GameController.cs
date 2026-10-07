using Microsoft.AspNetCore.Mvc;
using Big2.Models.Session;
using GameModel = Big2.Models.Game.Game;
using Big2.Models.Game;
using Big2.Models.AI;
using System.Diagnostics;
namespace Big2.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            GameModel? game = GameSessionStorage.Load(HttpContext.Session);
            if(game == null)
            {
                game = new GameModel();
                game.StartGame();
                if(game.CurrentPlayer != null && game.CurrentPlayer.IsAi)
                {
                    RunAiTurn(game);
                }
                GameSessionStorage.Save(HttpContext.Session, game);
            }
            return View(game);
        }
        private AiTurnResult RunAiTurn(GameModel game)
        {
            AiPlayer ai = new AiPlayer(game.CurrentPlayer);
            return ai.TakeTurn(game);
        }
        [HttpPost]
        public IActionResult AiTurn()
        {
            GameModel? game = GameSessionStorage.Load(HttpContext.Session);
            if (game == null)
            {
                return BadRequest("Game session not found.");
            }
            if(game.CurrentPlayer == null || !game.CurrentPlayer.IsAi || game.IsGameOver)
            {
                return BadRequest("It's not the AI's turn or the game is over.");
            }
            Player aiPlayer = game.CurrentPlayer;
            AiTurnResult result = RunAiTurn(game);
            bool isLastCard = result == AiTurnResult.Played && aiPlayer.Hand.Count == 1;
            GameSessionStorage.Save(HttpContext.Session, game);
            return Ok(new
            {
                passed = result == AiTurnResult.Passed,
                isLastCard = isLastCard
            });
        }
        [HttpPost]
        public IActionResult PlayCards([FromBody] PlayCardRequest request)
        {
            GameModel? game = GameSessionStorage.Load(HttpContext.Session);
            if (game == null)
            {
                return BadRequest("Game session not found.");
            }
            Player? player = game.Players.FirstOrDefault(p => p.PlayerId == 0);
            if(player == null)
            {
                return BadRequest("Player not found.");
            }
            List<Card> cardsToPlay = new List<Card>();
            foreach (CardRequest cardRequest in request.Cards)
            {
                if (!Enum.TryParse<Suit>(cardRequest.Suit, out Suit selectedSuit))
                {
                    return BadRequest("Invalid card suit");
                }
                if (!Enum.TryParse<Rank>(cardRequest.Rank, out Rank selectedRank))
                {
                    return BadRequest("Invalid card rank");
                }
                Card? cardToPlay = player.Hand.FirstOrDefault(c => c.Suit == selectedSuit && c.Rank == selectedRank);
                if (cardToPlay == null)
                {
                    return BadRequest("Card not found in player's hand");
                }
                cardsToPlay.Add(cardToPlay);
            }
            try
            {
                game.PlayCards(player, cardsToPlay);
                bool isLastCard = player.Hand.Count == 1;
                GameSessionStorage.Save(HttpContext.Session, game);
                return Ok(new
                {
                    isLastCard = isLastCard
                });
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult Pass()
        {
            GameModel? game = GameSessionStorage.Load(HttpContext.Session);
            if (game == null)
            {
                return BadRequest("Game session not found.");
            }
            Player? player = game.Players.FirstOrDefault(p => p.PlayerId == 0);
            if (player == null)
            {
                return BadRequest("Player not found.");
            }
            try
            {
                game.Pass(player);
                GameSessionStorage.Save(HttpContext.Session, game);
                return RedirectToAction("Index");
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult NewGame()
        {
            GameModel game = new GameModel();
            game.StartGame();
            if(game.CurrentPlayer != null && game.CurrentPlayer.IsAi)
            {
                RunAiTurn(game);
            }
            GameSessionStorage.Save(HttpContext.Session, game);
            return RedirectToAction("Index");
        }
    }
}
