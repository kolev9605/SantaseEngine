namespace Santase.Engine;

public class Card
{
    public Suit Suit { get; set; }
    public Rank Type { get; set; }

    public string CardId => $"{Type.ToString()[0]}{Suit.ToString()[0]}";

    public override string ToString()
    {
        return $"{Type} of {Suit}";
    }
}
