namespace Santase.Engine.Engine;

public class GameEngine
{
    public GameEngine()
    {
        CurrentState = new GameState();
    }

    public GameState CurrentState { get; private set; }

    public GameResult ApplyMove(Move move, GameState gameState)
    {
        //Validate move
        // Apply move
        // If trick complete → resolve trick
        // Draw cards (if open phase)
        // Update scores
        // Check win condition
        // Switch player
        // ValidateMove(move);
        var newState = gameState.Clone();
        if (move is null)
        {
            return new GameResult(
                newState,
                trickCompleted: false,
                gameEnded: true,
                winner: 0
            );
        }

        // Assign the cloned state to the engine
        CurrentState = newState;
        newState.IsClosed = move.CloseGame || newState.IsClosed;

        newState.CurrentTrick.Add(move.Card);

        if (newState.PlayerTurn == 0)
        {
            newState.PlayerOneHand.Remove(move.Card);
            newState.PlayerOnePlayedCards.Add(move.Card);
        }
        else
        {
            newState.PlayerTwoHand.Remove(move.Card);
            newState.PlayerTwoPlayedCards.Add(move.Card);
        }

        // If trick completed:
        // 1. Evaluate the trick
        // 2. Check win conditions
        // 3. Update points
        // 4. Draw Cards
        // 5. Switch Player
        if (newState.CurrentTrick.Count == 2)
        {
            if (newState.Talon.Cards.Any() && newState.TrumpCard is not null)
            {
                var cardDrawn = newState.Talon.DrawOne();
                if (cardDrawn is null)
                {
                    cardDrawn = newState.TrumpCard;
                    newState.TrumpCard = null;
                }

                if (cardDrawn is not null)
                {
                    newState.PlayerOneHand.Add(cardDrawn);
                }

                var secondCardDrawn = newState.Talon.DrawOne();
                if (secondCardDrawn is null)
                {
                    secondCardDrawn = newState.TrumpCard;
                    newState.TrumpCard = null;
                }

                if (secondCardDrawn is not null)
                {
                    newState.PlayerTwoHand.Add(secondCardDrawn);
                }
            }

            // Evaluate the trick
            var winnerOfTheTrick = EvaluateTrick(newState);
            newState.SetPlayerTurn(winnerOfTheTrick);

            newState.CurrentTrick.Clear();
        }
        else
        {
            newState.SwitchPlayer();
        }

        var gameEnded = newState.PlayerOnePoints >= 66 || newState.PlayerTwoPoints >= 66;
        var winner = gameEnded ? (newState.PlayerOnePoints >= 66 ? 0 : 1) : (int?)null;
        return new GameResult(
            newState,
            trickCompleted: newState.CurrentTrick.Count == 2,
            gameEnded: gameEnded,
            winner: winner
        );
    }

    private int EvaluateTrick(GameState gameState)
    {
        var firstCard = gameState.CurrentTrick[0];
        var secondCard = gameState.CurrentTrick[1];
        var firstCardValue = firstCard.Type.GetPower();
        var secondCardValue = secondCard.Type.GetPower();

        int winner = -1;
        // Same Suit evaluate by power,
        // Assume that firstCard is one played first
        if (firstCard.Suit == secondCard.Suit)
        {
            if (firstCardValue > secondCardValue)
            {
                gameState.PlayerOnePoints += firstCard.Type.GetPointValue() + secondCard.Type.GetPointValue();
                winner = 0;
            }
            else
            {
                gameState.PlayerTwoPoints += firstCard.Type.GetPointValue() + secondCard.Type.GetPointValue();
                winner = 1;
            }
        }
        else
        {
            // Different suit, check if second card is trump
            if (secondCard.Suit == gameState.TrumpSuit && firstCard.Suit != gameState.TrumpSuit)
            {
                gameState.PlayerTwoPoints += firstCard.Type.GetPointValue() + secondCard.Type.GetPointValue();
                winner = 1;
            }
            else if (firstCard.Suit == gameState.TrumpSuit && secondCard.Suit != gameState.TrumpSuit)
            {
                gameState.PlayerOnePoints += firstCard.Type.GetPointValue() + secondCard.Type.GetPointValue();
                winner = 0;
            }
            // If none of the played cards are trump, the first player wins the trick
            else if (firstCard.Suit != gameState.TrumpSuit && secondCard.Suit != gameState.TrumpSuit)
            {
                gameState.PlayerOnePoints += firstCard.Type.GetPointValue() + secondCard.Type.GetPointValue();
                winner = 0;
            }
        }

        return winner;
    }

    public IEnumerable<Move> GetLegalMoves(GameState gameState)
    {
        var legalMoves = gameState.PlayerTurn == 0 ? gameState.PlayerOneHand : gameState.PlayerTwoHand;

        // Card played
        if (gameState.CurrentTrick.Count == 1)
        {
            if (gameState.IsClosed || gameState.TalonExhausted)
            {
                legalMoves = legalMoves.Where(c => c.Suit == gameState.CurrentTrick.FirstOrDefault()!.Suit).ToList();
            }
        }

        bool canClose = !gameState.IsClosed && (gameState.Talon.Cards.Count < GameConstants.TotalCardsCount || gameState.Talon.Cards.Count > 2);
        bool canDeclareMarriage = gameState.Talon.Cards.Count < GameConstants.TotalCardsCount;
        var x = legalMoves
            .Where(m => m.Type == Rank.Queen || m.Type == Rank.King)
            .GroupBy(c => c.Suit)
            .ToDictionary(c => c.Key, c => c.ToList())
            .Where(g => g.Value.Count == 2);

        return legalMoves
            .OrderBy(c => c.Suit)
            .ThenBy(c => c.Type.GetPower())
            .Select(c => new Move(c, false, canClose));
    }
}
