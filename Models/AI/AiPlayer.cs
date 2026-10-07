using GameModel = Big2.Models.Game.Game;
using Big2.Models.Game;
namespace Big2.Models.AI
{
    public class AiPlayer
    {
        public Player Player { get; }
        public AiStrategy Strategy { get; }
        public AiPlayer(Player player)
        {
            Player = player;
            Strategy = player.PlayerId switch
            {
                1 => AiStrategy.Aggressive,
                2 => AiStrategy.Cunning,
                3 => AiStrategy.Conservative,
                _ => throw new ArgumentOutOfRangeException(nameof(player.PlayerId), "Invalid Aiplayer.")
            };
        }
        public AiTurnResult TakeTurn(GameModel game)
        {
            if (game.CurrentPlayer != Player)
            {
                throw new InvalidOperationException("It is not this AI player's turn.");
            }
            // New round or first play of the game
            // Currently, the AI will always play the lowest single card available when starting a new round or when it is the first play of the game.
            // 2026/9/19 updated. AiStrategy.Conservative is avaliable.
            // 2026/9/20 updated. AiStrategy.Aggressive is avaliable.
            // 2026/9/20 updated. AiStrategy.Cunning (Critical Mode) is avaliable.
            // 2026/9/21 updated. AiStrategy.Cunning (Alert Mode) is avaliable.
            if (game.CurrentPlay == null)
            {
                if(Strategy == AiStrategy.Conservative && !game.IsFirstPlay)
                {
                    List<Card>? selectedCards = ConservativeAiStrategy.ChooseLeadPlay(this, game);
                    if(selectedCards != null)
                    {
                        game.PlayCards(Player, selectedCards);
                        return AiTurnResult.Played;
                    }
                }
                if(Strategy == AiStrategy.Aggressive && !game.IsFirstPlay)
                {
                    List<Card>? selectedCards = AggressiveAiStrategy.ChooseLeadPlay(this, game);
                    if (selectedCards != null)
                    {
                        game.PlayCards(Player, selectedCards);
                        return AiTurnResult.Played;
                    }
                }
                if(Strategy == AiStrategy.Cunning && !game.IsFirstPlay)
                {
                    List<Card>? selectedCards = CunningAiStrategy.ChooseLeadPlay(this, game);
                    if(selectedCards != null)
                    {
                        game.PlayCards(Player, selectedCards);
                        return AiTurnResult.Played;
                    }
                }
                Card? selectedSingle = ChooseSingle(game);
                if(selectedSingle != null)
                {
                    game.PlayCard(Player, selectedSingle);
                    return AiTurnResult.Played;
                }
                game.Pass(Player);
                return AiTurnResult.Passed;
            }
            // AI logic for responding to the current play
            PlayType currentPlayType = PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards);
            switch(currentPlayType)
            {
                case PlayType.Single:
                    Card? selectedSingle = ChooseSingle(game);
                    if (selectedSingle != null)
                    {
                        game.PlayCard(Player, selectedSingle);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.Pair:
                    List<Card>? selectedPair = ChoosePair(game);
                    if (selectedPair != null)
                    {
                        game.PlayCards(Player, selectedPair);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.Triple:
                    List<Card>? selectedTriple = ChooseTriple(game);
                    if (selectedTriple != null)
                    {
                        game.PlayCards(Player, selectedTriple);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.Straight:
                    List<Card>? selectedStraight = ChooseStraight(game);
                    if (selectedStraight != null)
                    {
                        game.PlayCards(Player, selectedStraight);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.Flush:
                    List<Card>? selectedFlush = ChooseFlush(game);
                    if (selectedFlush != null)
                    {
                        game.PlayCards(Player, selectedFlush);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.FullHouse:
                    List<Card>? selectedFullHouse = ChooseFullHouse(game);
                    if (selectedFullHouse != null)
                    {
                        game.PlayCards(Player, selectedFullHouse);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.FourOfAKind:
                    List<Card>? selectedFourOfAKind = ChooseFourOfAKind(game);
                    if (selectedFourOfAKind != null)
                    {
                        game.PlayCards(Player, selectedFourOfAKind);
                        return AiTurnResult.Played;
                    }
                    break;
                case PlayType.StraightFlush:
                    List<Card>? selectedStraightFlush = ChooseStraightFlush(game);
                    if (selectedStraightFlush != null)
                    {
                        game.PlayCards(Player, selectedStraightFlush);
                        return AiTurnResult.Played;
                    }
                    break;
                default:
                    throw new NotImplementedException($"AI logic for play type {currentPlayType} is not implemented.");
            }
            game.Pass(Player);
            return AiTurnResult.Passed;
        }
        public Card? ChooseSingle(GameModel game)
        {
            List<Card> playableCards = GetPlayableSingles(game);
            if (playableCards.Count == 0)
            {
                return null;
            }
            if (Strategy == AiStrategy.Conservative && game.CurrentPlay != null)
            {
                return ConservativeAiStrategy.ChooseSingle(playableCards);
            }
            if (Strategy == AiStrategy.Aggressive && game.CurrentPlay != null)
            {
                return AggressiveAiStrategy.ChooseSingle(this, playableCards);
            }
            if (Strategy == AiStrategy.Cunning && game.CurrentPlay != null)
            {
                return CunningAiStrategy.ChooseSingle(this, game, playableCards);
            }
            return playableCards.OrderBy(card => card.Rank).ThenBy(card => card.Suit).First();
        }
        public List<Card> GetPlayableSingles(GameModel game)
        {
            List<Card> playableCards = new List<Card>();

            foreach (Card card in Player.Hand)
            {
                if (game.CurrentPlay == null)
                {
                    playableCards.Add(card);
                    continue;
                }

                if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.Single)
                {
                    continue;
                }

                try
                {
                    CardComparer.ValidateSingleIsStronger(card, game.CurrentPlay.Cards[0]);
                    playableCards.Add(card);
                }
                catch (InvalidOperationException)
                {
                    continue;
                } 
            }
            return playableCards;
        }
        public List<Card>? ChoosePair(GameModel game)
        {
            List<List<Card>> playablePairs = GetPlayablePairs(game);

            if (playablePairs.Count == 0)
            {
                return null;
            }
            if (Strategy == AiStrategy.Conservative)
            {
                return ConservativeAiStrategy.ChoosePair(playablePairs);
            }
            if (Strategy == AiStrategy.Aggressive)
            {
                return AggressiveAiStrategy.ChoosePair(this, playablePairs);
            }
            if (Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChoosePair(this, game, playablePairs);
            }
            return playablePairs
                .OrderBy(pair => pair[0].Rank)
                .ThenBy(pair => pair.Max(card => card.Suit))
                .First();
        }
        public List<List<Card>> GetPairsFromHand(bool exactPairOnly = false)
        {
            return Player.Hand
                .GroupBy(card => card.Rank)
                .Where(group => exactPairOnly ? group.Count() == 2 : group.Count() >= 2)
                .Select(group => group
                .OrderBy(card => card.Suit)
                .Take(2)
                .ToList())
                .ToList();
        }
        public List<List<Card>> GetPlayablePairs(GameModel game)
        {
            List<List<Card>> playablePairs = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playablePairs;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.Pair)
            {
                return playablePairs;
            }
            List<List<Card>> pairs = GetPairsFromHand();
            foreach (List<Card> pair in pairs)
            {
                try
                {
                    CardComparer.ValidatePairIsStronger(pair, game.CurrentPlay.Cards);
                    playablePairs.Add(pair);
                }
                catch (InvalidOperationException)
                {
                    // This pair is not strong enough.
                    continue;
                }
            }
            return playablePairs;
        }
        public List<Card>? ChooseTriple(GameModel game)
        {
            List<List<Card>> playableTriples = GetPlayableTriples(game);

            if (playableTriples.Count == 0)
            {
                return null;
            }
            if (Strategy == AiStrategy.Conservative)
            {
                return ConservativeAiStrategy.ChooseTriple(playableTriples);
            }
            if (Strategy == AiStrategy.Aggressive)
            {
                return AggressiveAiStrategy.ChooseTriple(this, playableTriples);
            }
            if (Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChooseTriple(this, game, playableTriples);
            }
            return playableTriples
                .OrderBy(triple => triple[0].Rank)
                .ThenBy(triple => triple.Max(card => card.Suit))
                .First();
        }
        public List<List<Card>> GetTriplesFromHand(bool exactTripleOnly = false)
        {
            return Player.Hand
                .GroupBy(card => card.Rank)
                .Where(group => exactTripleOnly ? group.Count() == 3 : group.Count() >= 3)
                .Select(group => group
                .OrderBy(card => card.Suit)
                .Take(3)
                .ToList())
                .ToList();
        }
        public List<List<Card>> GetPlayableTriples(GameModel game)
        {
            List<List<Card>> playableTriples = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playableTriples;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.Triple)
            {
                return playableTriples;
            }
            List<List<Card>> triples = GetTriplesFromHand();
            foreach (List<Card> triple in triples)
            {
                try
                {
                    CardComparer.ValidateTripleIsStronger(triple, game.CurrentPlay.Cards);
                    playableTriples.Add(triple);
                }
                catch (InvalidOperationException)
                {
                    // This triple is not strong enough.
                    continue;
                }
            }
            return playableTriples;
        }
        public List<List<Card>> GetFiveCardPlaysFromHand()
        {
            List<List<Card>> fiveCardPlays = new List<List<Card>>();
            List<List<Card>> combinations = GetCombinations(Player.Hand, 5);
            foreach(List<Card> combination in combinations)
            {
                if (PlayTypeEvaluator.TryGetPlayType(combination, out PlayType playType))
                {
                    if (playType == PlayType.Straight ||
                    playType == PlayType.Flush ||
                    playType == PlayType.FullHouse ||
                    playType == PlayType.FourOfAKind ||
                    playType == PlayType.StraightFlush)
                    {
                        fiveCardPlays.Add(combination);
                    }
                }
            }
            return fiveCardPlays;
        }
        public List<Card>? ChooseStraight(GameModel game)
        {
            List<List<Card>> playableStraights = GetPlayableStraights(game);
            if (playableStraights.Count == 0)
            {
                return null;
            }
            if(Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChooseStraight(this, game, playableStraights);
            }
            return playableStraights
                .OrderBy(straight => straight.Max(card => card.Rank))
                .ThenBy(straight => straight.Max(card => card.Suit))
                .First();
        }
        public List<List<Card>> GetPlayableStraights(GameModel game)
        {
            List<List<Card>> playableStraights = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playableStraights;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.Straight)
            {
                return playableStraights;
            }
            for(int startValue = (int)Rank.Three; startValue <= (int)Rank.Ten; startValue++)
            {
                List<Card> straight = new List<Card>();
                bool isValidStraight = true;
                for (int i = 0; i < 5; i++)
                {
                    Rank requiredRank = (Rank)(startValue + i);
                    Card? card = Player.Hand.FirstOrDefault(c => c.Rank == requiredRank);
                    if (card == null)
                    {
                        isValidStraight = false;
                        break;
                    }
                    straight.Add(card);
                }
                if(!isValidStraight)
                {
                    continue;
                }
                if (straight.Count == 5)
                {
                    try
                    {
                        CardComparer.ValidateStraightIsStronger(straight, game.CurrentPlay.Cards);
                        playableStraights.Add(straight);
                    }
                    catch (InvalidOperationException)
                    {
                        // This straight is not strong enough.
                        continue;
                    }
                }
            }
            return playableStraights;
        }
        public List<Card>? ChooseFlush(GameModel game)
        {
            List<List<Card>> playableFlushes = GetPlayableFlushes(game);

            if (playableFlushes.Count == 0)
            {
                return null;
            }
            if(Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChooseFlush(this, game, playableFlushes);
            }
            List<Card> weakestFlush = playableFlushes[0];
            foreach(List<Card> flush in playableFlushes)
            {
                if (CompareFlushes(flush, weakestFlush) < 0)
                {
                    weakestFlush = flush;
                }
            }
            return weakestFlush;
        }  
        public int CompareFlushes(List<Card> flush1, List<Card> flush2)
        {
            int suitComparison = flush1[0].Suit.CompareTo(flush2[0].Suit);
            if(suitComparison != 0)
            {
                return suitComparison;
            }
            List<Card> sortedFlush1 = flush1.OrderByDescending(card => card.Rank).ToList();
            List<Card> sortedFlush2 = flush2.OrderByDescending(card => card.Rank).ToList();
            for (int i = 0; i < sortedFlush1.Count; i++)
            {
                int rankComparison = sortedFlush1[i].Rank.CompareTo(sortedFlush2[i].Rank);
                if (rankComparison != 0)
                {
                    return rankComparison;
                }
            }
            return sortedFlush1[0].Suit.CompareTo(sortedFlush2[0].Suit);
        }
        public List<List<Card>> GetPlayableFlushes(GameModel game)
        {
            List<List<Card>> playableFlushes = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playableFlushes;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.Flush)
            {
                return playableFlushes;
            }
            var flushes = Player.Hand.GroupBy(card => card.Suit)
                                   .Where(group => group.Count() >= 5);
            foreach (var flush in flushes)
            {
                List<Card> flushCards = flush.ToList();
                foreach(List<Card> combination in GetCombinations(flushCards, 5))
                try
                {
                    CardComparer.ValidateFlushIsStronger(combination, game.CurrentPlay.Cards);
                        List<Card> sortedCombination = combination.OrderBy(card => card.Rank).ThenBy(card => card.Suit).ToList();
                    playableFlushes.Add(sortedCombination);
                }
                catch (InvalidOperationException)
                {
                    // This flush is not strong enough.
                    continue;
                }
            }
            return playableFlushes;
        }
        private List<List<Card>> GetCombinations(List<Card> cards, int size)
        {
            List<List<Card>> combinations = new List<List<Card>>();
            void Generate(List<Card> current, int startIndex)
            {
                if(current.Count == size)
                {
                    combinations.Add(new List<Card>(current));
                    return;
                }
                for (int i = startIndex; i < cards.Count; i++)
                {
                    current.Add(cards[i]);
                    Generate(current, i + 1);
                    current.RemoveAt(current.Count - 1);
                }
            }
            Generate(new List<Card>(), 0);
            return combinations;
        }
        public List<Card>? ChooseFullHouse(GameModel game)
        {
            List<List<Card>> playableFullHouses = GetPlayableFullHouses(game);
            if (playableFullHouses.Count == 0)
            {
                return null;
            }
            if(Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChooseFullHouse(this, game, playableFullHouses);
            }
            List<Card> weakestFullHouse = playableFullHouses[0];
            foreach (List<Card> fullHouse in playableFullHouses)
            {
                Rank fullHouseTripleTrank = fullHouse.GroupBy(card => card.Rank).First(group => group.Count() == 3).Key;
                Rank weakestTripleRank = weakestFullHouse.GroupBy(card => card.Rank).First(group => group.Count() == 3).Key;
                if (fullHouseTripleTrank < weakestTripleRank)
                {
                    weakestFullHouse = fullHouse;
                }
            }
            return weakestFullHouse;
        }
        public List<List<Card>> GetPlayableFullHouses(GameModel game)
        {
            List<List<Card>> playableFullHouses = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playableFullHouses;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.FullHouse)
            {
                return playableFullHouses;
            }
            var triples = Player.Hand.GroupBy(card => card.Rank)
                .Where(group => group.Count() >= 3);
            var pairs = Player.Hand.GroupBy(card => card.Rank)
                .Where(group => group.Count() >= 2);
            foreach (var triple in triples)
            {
                foreach (var pair in pairs)
                {
                    // Triple and pair must be of different ranks to form a valid full house.
                    if (triple.Key == pair.Key)
                    {
                        continue; 
                    }
                    List<Card> fullHouse = triple.Take(3).ToList();
                    fullHouse.AddRange(pair.Take(2));
                    try
                    {
                        CardComparer.ValidateFullHouseIsStronger(fullHouse, game.CurrentPlay.Cards);
                        playableFullHouses.Add(fullHouse);
                    }
                    catch (InvalidOperationException)
                    {
                        // This full house is not strong enough.
                        continue;
                    }
                }
            }
            return playableFullHouses;
        }
        public List<Card>? ChooseFourOfAKind(GameModel game)
        {
            List<List<Card>> playableFourOfAKinds = GetPlayableFourOfAKinds(game);
            if (playableFourOfAKinds.Count == 0)
            {
                return null;
            }
            if(Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChooseFourOfAKind(this, game, playableFourOfAKinds);
            }
            List<Card> weakestFourOfAKind = playableFourOfAKinds[0];
            Rank weakestRank = weakestFourOfAKind.GroupBy(card => card.Rank).First(group => group.Count() == 4).Key;
            foreach (List<Card> fourOfAKind in playableFourOfAKinds)
            {
                Rank fourOfAKindRank = fourOfAKind.GroupBy(card => card.Rank).First(group => group.Count() == 4).Key;                
                if (fourOfAKindRank < weakestRank)
                {
                    weakestFourOfAKind = fourOfAKind;
                    weakestRank = fourOfAKindRank;
                }
            }
            return weakestFourOfAKind;
        }
        public List<List<Card>> GetPlayableFourOfAKinds(GameModel game)
        {
            List<List<Card>> playableFourOfAKinds = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playableFourOfAKinds;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.FourOfAKind)
            {
                return playableFourOfAKinds;
            }
            var fourOfAKinds = Player.Hand.GroupBy(card => card.Rank)
                .Where(group => group.Count() == 4);
            foreach (var fourOfAKind in fourOfAKinds)
            {
                List<Card> combination = fourOfAKind.ToList();
                //Kicker is the highest card that is not part of the four of a kind.
                var kicker = Player.Hand.Where(card => card.Rank != fourOfAKind.Key).OrderBy(card => card.Rank).FirstOrDefault();
                if (kicker == null)
                {
                    continue;
                }
                combination.Add(kicker);
                try
                {
                    CardComparer.ValidateFourOfAKindIsStronger(combination, game.CurrentPlay.Cards);
                    playableFourOfAKinds.Add(combination);
                }
                catch (InvalidOperationException)
                {
                    // This four of a kind is not strong enough.
                    continue;
                }
            }
            return playableFourOfAKinds;
        }
        public List<Card>? ChooseStraightFlush(GameModel game)
        {
            List<List<Card>> playableStraightFlushes = GetPlayableStraightFlushes(game);
            if (playableStraightFlushes.Count == 0)
            {
                return null;
            }
            if(Strategy == AiStrategy.Cunning)
            {
                return CunningAiStrategy.ChooseStraightFlush(this, game, playableStraightFlushes);
            }
            List<Card> weakestStraightFlush = playableStraightFlushes[0];
            int weakestStraightRank = CardComparer.GetStraightRank(weakestStraightFlush);
            foreach (List<Card> straightFlush in playableStraightFlushes)
            {
                int straightFlushRank = CardComparer.GetStraightRank(straightFlush);
                if (straightFlushRank < weakestStraightRank)
                {
                    weakestStraightFlush = straightFlush;
                    weakestStraightRank = straightFlushRank;
                }
            }
            return weakestStraightFlush;
        }
        public List<List<Card>> GetPlayableStraightFlushes(GameModel game)
        {
            List<List<Card>> playableStraightFlushes = new List<List<Card>>();
            if (game.CurrentPlay == null)
            {
                return playableStraightFlushes;
            }
            if (PlayTypeEvaluator.GetPlayType(game.CurrentPlay.Cards) != PlayType.StraightFlush)
            {
                return playableStraightFlushes;
            }
            var flushes = Player.Hand.GroupBy(card => card.Suit)
                                   .Where(group => group.Count() >= 5);
            foreach (var flush in flushes)
            {
                List<Card> flushCards = flush.ToList();
                foreach (List<Card> combination in GetCombinations(flushCards, 5))
                {
                    if(!HandEvaluator.IsStraight(combination))
                    {
                        continue;
                    }
                    try
                    {
                        CardComparer.ValidateStraightFlushIsStronger(combination, game.CurrentPlay.Cards);
                        List<Card> sortedCombination = combination.OrderBy(card => card.Rank).ThenBy(card => card.Suit).ToList();
                        playableStraightFlushes.Add(sortedCombination);
                    }
                    catch (InvalidOperationException)
                    {
                        // This straight flush is not strong enough.
                        continue;
                    }
                }
            }
            return playableStraightFlushes;
        }
    }
}
