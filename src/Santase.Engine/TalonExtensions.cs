namespace Santase.Engine;

static class TalonExtensions
{
    public static void Shuffle(this List<Card> array, Random rng)
    {
        int n = array.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            Card temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
