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
        // Validate move
        // Apply move
        // If trick complete → resolve trick
        // Draw cards (if open phase)
        // Update scores
        // Check win condition
        // Switch player
        ValidateMove(move);
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
        var trickCompleted = false;
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

            trickCompleted = true;

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
            trickCompleted: trickCompleted,
            gameEnded: gameEnded,
            winner: winner
        );
    }

    private void ValidateMove(Move move)
    {
        var legalMoves = GetLegalMoves(CurrentState);
        if (!legalMoves.Any(m => m.Card == move.Card))
        {
            throw new InvalidOperationException($"The move {move.Card} is not legal");
        }
    }

    private int EvaluateTrick(GameState gameState)
    {
        var leadCard = gameState.CurrentTrick[0];
        var replyCard = gameState.CurrentTrick[1];
        var leadCardValue = leadCard.Type.GetPower();
        var replyCardValue = replyCard.Type.GetPower();
        var leadPlayer = gameState.PlayerTurn == 0 ? 1 : 0;
        var replyPlayer = gameState.PlayerTurn;

        var leadWinning = false;
        // Same Suit evaluate by power,
        // Assume that firstCard is one played first
        if (leadCard.Suit == replyCard.Suit)
        {
            leadWinning = leadCardValue > replyCardValue;
        }
        else
        {
            // Different suit, check if second card is trump
            if (replyCard.Suit == gameState.TrumpSuit && leadCard.Suit != gameState.TrumpSuit)
            {
                leadWinning = false;
            }
            else if (leadCard.Suit == gameState.TrumpSuit && replyCard.Suit != gameState.TrumpSuit)
            {
                leadWinning = true;
            }
            // If none of the played cards are trump, the lead player wins the trick
            else if (leadCard.Suit != gameState.TrumpSuit && replyCard.Suit != gameState.TrumpSuit)
            {
                leadWinning = true;
            }
        }

        if (leadWinning)
        {
            if (leadPlayer == 0)
            {
                gameState.PlayerOnePoints += leadCard.Type.GetPointValue() + replyCard.Type.GetPointValue();
            }
            else
            {
                gameState.PlayerTwoPoints += leadCard.Type.GetPointValue() + replyCard.Type.GetPointValue();
            }
        }
        else
        {
            if (leadPlayer == 0)
            {
                gameState.PlayerTwoPoints += leadCard.Type.GetPointValue() + replyCard.Type.GetPointValue();
            }
            else
            {
                gameState.PlayerOnePoints += leadCard.Type.GetPointValue() + replyCard.Type.GetPointValue();
            }
        }

        return leadWinning ? leadPlayer : replyPlayer;
    }

    public IEnumerable<Move> GetLegalMoves(GameState gameState)
    {
        var legalMoves = gameState.PlayerTurn == 0 ? gameState.PlayerOneHand : gameState.PlayerTwoHand;

        // Card played
        if (gameState.CurrentTrick.Count == 1)
        {
            if (gameState.IsClosed || gameState.TalonExhausted)
            {
                legalMoves = legalMoves
                    .Where(c => c.Suit == gameState.CurrentTrick.FirstOrDefault()!.Suit || c.Suit == gameState.TrumpSuit)
                    .ToList();
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
