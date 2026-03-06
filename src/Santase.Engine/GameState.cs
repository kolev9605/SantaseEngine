namespace Santase.Engine;

public class GameState
{
    public GameState()
    {
        Talon = new Talon();
        PlayerOneHand = Talon.Draw(GameConstants.PlayerInitialHandSize);
        PlayerTwoHand = Talon.Draw(GameConstants.PlayerInitialHandSize);

        TrumpCard = Talon.DrawOne() ?? throw new ArgumentNullException("First card can't be null");
        TrumpSuit = TrumpCard.Suit;

        PlayerTurn = 0;
    }

    public List<Card> PlayerOneHand { get; init; } = new();
    public List<Card> PlayerTwoHand { get; init; } = new();
    public List<Card> PlayerOnePlayedCards { get; init; } = new();
    public List<Card> PlayerTwoPlayedCards { get; init; } = new();

    public Talon Talon { get; init; }

    public Suit TrumpSuit { get; init; }
    public Card? TrumpCard { get; init; }

    public List<Card> CurrentTrick { get; init; } = new();

    public int PlayerOnePoints { get; set; }
    public int PlayerTwoPoints { get; set; }

    /// <summary>
    /// Value 0 if player 1,
    /// Value 1 if player 2
    /// </summary>
    public int PlayerTurn { get; private set; }

    public bool IsClosed { get; set; }
    public bool TalonExhausted => Talon.Cards.Count == 0;

    public void SwitchPlayer()
    {
        PlayerTurn = PlayerTurn == 1 ? 0 : 1;
    }

    public string GetStateString()
    {
        return $"Player 1 hand: {string.Join(", ", PlayerOneHand.Select(c => c.CardId))}\n" +
               $"Player 2 hand: {string.Join(", ", PlayerTwoHand.Select(c => c.CardId))}\n" +
               $"Trump card: {TrumpCard}\n" +
               $"Current trick: {string.Join(", ", CurrentTrick.Select(c => c.CardId))}\n" +
               $"Player 1 points: {PlayerOnePoints}\n" +
               $"Player 2 points: {PlayerTwoPoints}\n" +
               $"Player turn: {(PlayerTurn == 0 ? "Player 1" : "Player 2")}\n" +
               $"Is closed: {IsClosed}";
    }

    public GameState Clone()
    {
        return new GameState
        {
            PlayerOneHand = [.. PlayerOneHand],
            PlayerTwoHand = [.. PlayerTwoHand],
            Talon = new Talon { Cards = [.. Talon.Cards] },
            TrumpSuit = TrumpSuit,
            TrumpCard = TrumpCard,
            CurrentTrick = [.. CurrentTrick],
            PlayerOnePoints = PlayerOnePoints,
            PlayerTwoPoints = PlayerTwoPoints,
            PlayerTurn = PlayerTurn,
            IsClosed = IsClosed
        };
    }
}
