namespace Santase.Engine.Engine;

public class GameEngine
{
    private const int TrumpMarriagePoints = 40;
    private const int NonTrumpMarriagePoints = 20;

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
        if (move is null)
        {
            var nullMoveState = gameState.Clone();
            return new GameResult(
                nullMoveState,
                trickCompleted: false,
                gameEnded: true,
                winner: 0
            );
        }

        ValidateMove(move, gameState);
        var newState = gameState.Clone();

        // Assign the cloned state to the engine
        CurrentState = newState;
        newState.IsClosed = move.CloseGame || newState.IsClosed;

        ApplyMarriageDeclaration(move, newState);

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

    private void ValidateMove(Move move, GameState gameState)
    {
        var legalMoves = GetLegalMoves(gameState);
        if (!legalMoves.Any(m => m.Card == move.Card && m.DeclareMarriage == move.DeclareMarriage && m.CloseGame == move.CloseGame))
        {
            throw new InvalidOperationException($"The move {move} is not legal");
        }
    }

    private static bool IsMarriageCard(Card card)
    {
        return card.Type is Rank.Queen or Rank.King;
    }

    private static Rank GetMarriagePairRank(Rank rank)
    {
        return rank switch
        {
            Rank.Queen => Rank.King,
            Rank.King => Rank.Queen,
            _ => throw new InvalidOperationException("Only Queen and King can form a marriage.")
        };
    }

    private static bool HasMarriagePair(Card playedCard, List<Card> hand)
    {
        if (!IsMarriageCard(playedCard))
        {
            return false;
        }

        var pairRank = GetMarriagePairRank(playedCard.Type);
        return hand.Any(c => c.Suit == playedCard.Suit && c.Type == pairRank);
    }

    private void ApplyMarriageDeclaration(Move move, GameState gameState)
    {
        if (!move.DeclareMarriage)
        {
            return;
        }

        if (gameState.CurrentTrick.Count != 0)
        {
            throw new InvalidOperationException("Marriage can only be declared when leading the trick.");
        }

        if (!IsMarriageCard(move.Card))
        {
            throw new InvalidOperationException("Marriage can only be declared with a Queen or King.");
        }

        var isPlayerOne = gameState.PlayerTurn == 0;
        var hand = isPlayerOne ? gameState.PlayerOneHand : gameState.PlayerTwoHand;
        var declaredMarriages = isPlayerOne ? gameState.PlayerOneDeclaredMarriages : gameState.PlayerTwoDeclaredMarriages;

        if (!HasMarriagePair(move.Card, hand))
        {
            throw new InvalidOperationException("The corresponding marriage card is not in hand.");
        }

        if (!declaredMarriages.Add(move.Card.Suit))
        {
            throw new InvalidOperationException("This marriage suit has already been declared.");
        }

        var marriagePoints = move.Card.Suit == gameState.TrumpSuit ? TrumpMarriagePoints : NonTrumpMarriagePoints;
        if (isPlayerOne)
        {
            gameState.PlayerOnePoints += marriagePoints;
        }
        else
        {
            gameState.PlayerTwoPoints += marriagePoints;
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

        var winner = leadWinning ? leadPlayer : replyPlayer;
        var trickPoints = leadCard.Type.GetPointValue() + replyCard.Type.GetPointValue();

        if (winner == 0)
        {
            gameState.PlayerOnePoints += trickPoints;
        }
        else
        {
            gameState.PlayerTwoPoints += trickPoints;
        }

        return winner;
    }

    public IEnumerable<Move> GetLegalMoves(GameState gameState)
    {
        var legalCards = gameState.PlayerTurn == 0 ? gameState.PlayerOneHand : gameState.PlayerTwoHand;

        // Card played
        if (gameState.CurrentTrick.Count == 1)
        {
            if (gameState.IsClosed || gameState.TalonExhausted)
            {
                legalCards = legalCards
                    .Where(c => c.Suit == gameState.CurrentTrick.FirstOrDefault()!.Suit || c.Suit == gameState.TrumpSuit)
                    .ToList();
            }
        }

        var isLeading = gameState.CurrentTrick.Count == 0;
        var cardsPlayedCount = gameState.PlayerOnePlayedCards.Count + gameState.PlayerTwoPlayedCards.Count;
        var canClose = isLeading && !gameState.IsClosed && cardsPlayedCount > 0;
        var canDeclareMarriage = isLeading;
        var declaredMarriages = gameState.PlayerTurn == 0 ? gameState.PlayerOneDeclaredMarriages : gameState.PlayerTwoDeclaredMarriages;
        var sortedCards = legalCards
            .OrderBy(c => c.Suit)
            .ThenBy(c => c.Type.GetPower())
            .ToList();

        var legalMoves = new List<Move>();
        foreach (var card in sortedCards)
        {
            legalMoves.Add(new Move(card, declareMarriage: false, closeGame: false));

            var canDeclareForCard = canDeclareMarriage
                && !declaredMarriages.Contains(card.Suit)
                && HasMarriagePair(card, sortedCards);
            if (canDeclareForCard)
            {
                legalMoves.Add(new Move(card, declareMarriage: true, closeGame: false));
            }

            if (canClose)
            {
                legalMoves.Add(new Move(card, declareMarriage: false, closeGame: true));

                if (canDeclareForCard)
                {
                    legalMoves.Add(new Move(card, declareMarriage: true, closeGame: true));
                }
            }
        }

        return legalMoves;
    }
}
