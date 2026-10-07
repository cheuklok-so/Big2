namespace Big2.Models.Session
{
    public class PlayerSessionDto
    {
        public int PlayerId { get; set; }
        public bool IsAi { get; set; }
        public List<CardSessionDto> Hand { get; set; }
        = new List<CardSessionDto>();
    }
}
