using System.Text.Json;
using Microsoft.AspNetCore.Http;
using GameModel = Big2.Models.Game.Game;
namespace Big2.Models.Session
{
    public class GameSessionStorage
    {
        private const string SessionKey = "Big2Game";
        public static void Save(ISession session, GameModel game)
        {
            GameSessionDto dto = GameSessionDto.FromGame(game);
            string json = JsonSerializer.Serialize(dto);
            session.SetString(SessionKey, json);
        }
        public static GameModel? Load(ISession session)
        {
            string? json = session.GetString(SessionKey);
            if(json == null)
            {
                return null;
            }
            GameSessionDto? dto = JsonSerializer.Deserialize<GameSessionDto>(json);
            if(dto == null)
            {
                return null;
            }
            return dto.ToGame();
        }
    }
}
