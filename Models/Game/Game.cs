namespace Big2.Models.Game
{
    public class Game
    {
        public Deck Deck { get; private set; }
        public List<Player> Players { get; private set; }
        public Player? CurrentPlayer { get; private set; }
        public bool IsFirstPlay {  get; private set; }
        public Play? CurrentPlay { get; private set; }
        public Player? LastPlayerWhoPlayed { get; private set;  }
        public int ConsecutivePassCount { get; private set; }
        public bool IsGameOver { get; private set; } 
        public Player? Winner { get; private set; }
        public void RestoreIsFirstPlay(bool isFirstPlay)
        {
            IsFirstPlay = isFirstPlay;
        }
        public void RestoreCurrentPlay(List<Card>? cards)
        {
            if (cards == null)
            {
                CurrentPlay = null;
                return;
            }
            CurrentPlay = new Play(cards);
        }
        public void RestoreCurrentPlayer(int? playerId)
        {
            if(playerId == null)
            {
                CurrentPlayer = null;
                return;
            }
            CurrentPlayer = Players.SingleOrDefault(player => player.PlayerId == playerId.Value);
            if(CurrentPlayer == null)
            {
                throw new InvalidOperationException($"Player {playerId.Value} does not exist.");
            }
        }
        public void RestoreLastPlayerWhoPlayed(int? playerId)
        {
            if(playerId == null)
            {
                LastPlayerWhoPlayed = null;
                return;
            }
            LastPlayerWhoPlayed = Players.SingleOrDefault(player => player.PlayerId == playerId.Value);
            if(LastPlayerWhoPlayed == null)
            {
                throw new InvalidOperationException($"Player {playerId.Value} does not exist.");
            }
        }
        public void RestoreConsecutivePassCount(int consecutivePassCount)
        {
            ConsecutivePassCount = consecutivePassCount;
        }
        public void RestoreGameOverState(bool isGameOver, int? winnerId)
        {
            IsGameOver = isGameOver;
            if(winnerId == null)
            {
                Winner = null;
                return;
            }
            Winner = Players.SingleOrDefault(player => player.PlayerId == winnerId.Value);
            if(Winner == null)
            {
                throw new InvalidOperationException($"Player {winnerId.Value} does not exist.");
            }
        }
        public Game()
        {
            Deck = new Deck();
            Players = new List<Player>();
            CurrentPlay = null;
            Players.Add(new Player(0, false));
            Players.Add(new Player(1, true));
            Players.Add(new Player(2, true));
            Players.Add(new Player(3, true));
        }
        public void StartGame()
        {
            if (Players.Count == 0)
            {
                throw new InvalidOperationException("Cannot start a game with no players.");
            }
            Deck = new Deck();
            foreach(var player in Players)
            {
                player.ClearHand();
            }
            IsFirstPlay = true;
            CurrentPlay = null;
            CurrentPlayer = null;
            LastPlayerWhoPlayed = null;
            ConsecutivePassCount = 0;
            IsGameOver= false;
            Winner= null;
            Deck.Shuffle();
            int cardsPerPlayer = Deck.Cards.Count / Players.Count;
            for(int round =0; round < cardsPerPlayer; round++)
            {
                foreach (var player in Players)
                {
                    Card card = Deck.DrawCard();
                    player.ReceiveCard(card);
                }
            }
            foreach(Player player in Players)
            {
                player.SortHand();
            }
            CurrentPlayer = FindStartingPlayer();
            
        }
        private Player FindStartingPlayer()
        {
            foreach(Player player in Players)
            {
                foreach(Card card in player.Hand)
                {
                    if(card.Rank == Rank.Three && card.Suit == Suit.Diamond)
                    {
                        return player;
                    }
                }
            }
            throw new InvalidOperationException("Cannot locate the player holding ♦3.");
        }
        private void MoveToNextPlayer()
        {
            if(CurrentPlayer == null)
            {
                throw new InvalidOperationException("The game has not started yet.");
            }
            int currentIndex = Players.IndexOf(CurrentPlayer);
            if(currentIndex == -1)
            {
                throw new InvalidOperationException("Current player is not a part of this game.");
            }
            int nextIndex = (currentIndex + 1) % Players.Count;
            CurrentPlayer = Players[nextIndex];
        }
        public void PlayCard(Player player, Card card)
        {
            PlayCards(player, new List<Card> { card });
        }
        public void PlayCards(Player player, List<Card> cards)
        {
            if(cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }
            if (cards.Count == 0)
            {
                throw new ArgumentException("You must play at least one card.", nameof(cards));
            }
            ValidateGameIsActive();
            ValidatePlayerTurn(player);
            ValidateNoDuplicateCards(cards);
            ValidatePlayerOwnsCards(player, cards);
            ValidateFirstPlay(cards);
            ValidatePlayType(cards);
            ValidateBeatsCurrentPlay(cards);
            foreach(Card card in cards)
            {
                player.PlayCard(card);
            }
            CurrentPlay = new Play(cards);
            LastPlayerWhoPlayed = player;
            ConsecutivePassCount = 0;
            IsFirstPlay = false;
            if (HasPlayerWon(player))
            {
                EndGame(player);
                return;
            }
            MoveToNextPlayer();
        }
        private void ValidateNoDuplicateCards(List<Card> cards)
        {
            if (cards.Distinct().Count() != cards.Count)
            {
                throw new InvalidOperationException("The same card cannot be played more than once.");
            }
        }
        private void ValidatePlayerOwnsCards(Player player, List<Card> cards)
        {
            foreach (Card card in cards)
            {
                if (!player.Hand.Contains(card))
                {
                    throw new InvalidOperationException("You cannot play a card you do not own.");
                }
            }
        }
        private void ValidatePlayerTurn(Player player)
        {
            if(CurrentPlayer == null)
            {
                throw new InvalidOperationException("The game has not started yet.");
            }
            if (player != CurrentPlayer)
            {
                throw new InvalidOperationException("It's not this player's turn.");
            }
        }
        private void ValidateFirstPlay(List<Card> cards)
        {
            if(!IsFirstPlay)
            {
                return;
            }
            bool containsThreeOfDiamonds = cards.Any(card=>
               card.Rank == Rank.Three && card.Suit == Suit.Diamond);
            if (!containsThreeOfDiamonds)
            {
                throw new InvalidOperationException("The first play must contain ♦3.");
            }
        }
        private void ValidatePlayType(List<Card> cards)
        {
            if(!PlayTypeEvaluator.TryGetPlayType(cards, out _))
            {
                throw new ArgumentException("Invalid play type");
            }
        }
        private void ValidateBeatsCurrentPlay(List<Card> cards)
        {
            if (CurrentPlay == null)
            {
                return;
            }
            PlayType newPlayType = PlayTypeEvaluator.GetPlayType(cards);
            PlayType currentPlayType = PlayTypeEvaluator.GetPlayType(CurrentPlay.Cards);
            // Single, Pair, and Triple can only be beaten by the same play type.
            if (currentPlayType == PlayType.Single ||
                currentPlayType == PlayType.Pair ||
                currentPlayType == PlayType.Triple)
            {
                if (newPlayType != currentPlayType)
                {
                    throw new InvalidOperationException("You must play the same type of play.");
                }
            }
            // Five-card plays can beat each other according to play type hierarchy.
            else
            {
                if (newPlayType < currentPlayType)
                {
                    throw new InvalidOperationException("You have to play a stronger play.");
                }

                if (newPlayType > currentPlayType)
                {
                    return;
                }
            }
            // Validate that the new play is stronger than the current play
            switch (newPlayType)
            {
                case PlayType.Single:
                    CardComparer.ValidateSingleIsStronger(cards[0], CurrentPlay.Cards[0]);
                    break;
                case PlayType.Pair:
                    CardComparer.ValidatePairIsStronger(cards, CurrentPlay.Cards);
                    break;
                case PlayType.Triple:
                    CardComparer.ValidateTripleIsStronger(cards, CurrentPlay.Cards);
                    break;
                case PlayType.Straight:
                    CardComparer.ValidateStraightIsStronger(cards, CurrentPlay.Cards);
                    break;
                case PlayType.Flush:
                    CardComparer.ValidateFlushIsStronger(cards, CurrentPlay.Cards);
                    break;
                case PlayType.FullHouse:
                    CardComparer.ValidateFullHouseIsStronger(cards, CurrentPlay.Cards);
                    break;
                case PlayType.FourOfAKind:
                    CardComparer.ValidateFourOfAKindIsStronger(cards, CurrentPlay.Cards);
                    break;
                case PlayType.StraightFlush:
                    CardComparer.ValidateStraightFlushIsStronger(cards, CurrentPlay.Cards);
                    break;
            }
        }
        public void Pass(Player player)
        {
            ValidateGameIsActive();
            ValidatePlayerTurn(player);
            if (IsFirstPlay)
            {
                throw new InvalidOperationException("You cannot pass on the first play.");
            }
            if(CurrentPlay == null)
            {
                throw new InvalidOperationException("You cannot pass when you lead a new round.");
            }
            ConsecutivePassCount++;
            if (ConsecutivePassCount >= Players.Count - 1)
            {
                StartNewRound();
                return;
            }
            MoveToNextPlayer();
        }
        private void StartNewRound()
        {
            if(LastPlayerWhoPlayed == null)
            {
                throw new InvalidOperationException("No player has played any cards yet.");
            }
            CurrentPlay = null;
            ConsecutivePassCount = 0;
            CurrentPlayer = LastPlayerWhoPlayed;
        }
        private bool HasPlayerWon(Player player)
        {
            return player.Hand.Count == 0;
        }
        private void EndGame(Player winner)
        {
            IsGameOver = true;
            Winner = winner;
            CurrentPlayer = null;
        }
        private void ValidateGameIsActive()
        {
            if (IsGameOver)
            {
                throw new InvalidOperationException("The game is already over.");
            }
        }
    }
}
