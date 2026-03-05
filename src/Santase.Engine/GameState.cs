namespace Santase.Engine;

public class GameState
{
    public GameState()
    {
        Talon = new Deck();
        PlayerOneHand = Talon.Draw(GameConstants.PlayerInitialHandSize);
        PlayerTwoHand = Talon.Draw(GameConstants.PlayerInitialHandSize);

        TrumpCard = Talon.DrawOne() ?? throw new ArgumentNullException("First card can't be null");
        TrumpSuit = TrumpCard.Suit;
    }

    public List<Card> PlayerOneHand { get; init; } = new();
    public List<Card> PlayerTwoHand { get; init; } = new();

    public Deck Talon { get; init; }

    public Suit TrumpSuit { get; init; }
    public Card? TrumpCard { get; init; }

    public List<Card> CurrentTrick { get; init; } = new();

    public int PlayerOnePoints { get; set; }
    public int PlayerTwoPoints { get; set; }

    public bool IsClosed { get; set; }
}
