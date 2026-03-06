namespace Santase.Engine.Engine;

public class Move
{
    public Card Card { get; }
    public bool DeclareMarriage { get; }
    public bool CloseGame { get; }

    public Move(Card card, bool declareMarriage = false, bool closeGame = false)
    {
        Card = card;
        DeclareMarriage = declareMarriage;
        CloseGame = closeGame;
    }

    override public string ToString()
    {
        return $"Move: {Card}, DeclareMarriage: {DeclareMarriage}, CloseGame: {CloseGame}";
    }
}
