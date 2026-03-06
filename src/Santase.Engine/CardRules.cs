namespace Santase.Engine;

public static class CardRules
{
    public static int GetPointValue(this Rank rank) => rank switch
    {
        Rank.Ace => 11,
        Rank.Ten => 10,
        Rank.King => 4,
        Rank.Queen => 3,
        Rank.Jack => 2,
        _ => 0
    };

    public static int GetPower(this Rank rank) => rank switch
    {
        Rank.Ace => 6,
        Rank.Ten => 5,
        Rank.King => 4,
        Rank.Queen => 3,
        Rank.Jack => 2,
        Rank.Nine => 1,
        _ => 0
    };
}
