namespace Santase.Engine;

public class Talon
{
    public Talon()
    {
        Cards = new List<Card>(GameConstants.TotalCardsCount);
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank type in Enum.GetValues(typeof(Rank)))
            {
                Cards.Add(new Card { Suit = suit, Type = type });
            }
        }

        Cards.Shuffle(new Random());
    }

    public List<Card> Cards { get; set; }

    public List<Card> Draw(int count)
    {
        var cardsToDraw = Math.Min(Cards.Count, count);
        var cardsDrawn = Cards.Take(cardsToDraw).ToList();
        Cards.RemoveRange(0, cardsToDraw);
        return cardsDrawn;
    }

    public Card? DrawOne()
    {
        return Draw(1).FirstOrDefault();
    }

    public override string ToString()
    {
        return string.Join(',', Cards.ToArray());
    }
}
