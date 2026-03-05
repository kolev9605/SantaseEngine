namespace Santase.Engine;

public class Talon
{
    private List<Card> _cards;

    public Talon()
    {
        _cards = new List<Card>(GameConstants.TotalCardsCount);
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank type in Enum.GetValues(typeof(Rank)))
            {
                _cards.Add(new Card { Suit = suit, Type = type });
            }
        }

        _cards.Shuffle(new Random());
    }

    public IEnumerable<Card> Cards => _cards;

    public List<Card> Draw(int count)
    {
        var cardsToDraw = Math.Min(_cards.Count, count);
        var cardsDrawn = _cards.Take(cardsToDraw).ToList();
        _cards.RemoveRange(0, cardsToDraw);
        return cardsDrawn;
    }

    public Card? DrawOne()
    {
        return Draw(1).FirstOrDefault();
    }

    public override string ToString()
    {
        return string.Join(',', _cards.ToArray());
    }
}
