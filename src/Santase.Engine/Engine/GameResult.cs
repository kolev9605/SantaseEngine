namespace Santase.Engine.Engine;

public class GameResult
{
    public GameResult(GameState gameState, bool trickCompleted, bool gameEnded, int? winner)
    {
        State = gameState;
        TrickCompleted = trickCompleted;
        GameEnded = gameEnded;
        Winner = winner;
    }
    public GameState State { get; }
    public bool TrickCompleted { get; }
    public bool GameEnded { get; }
    public int? Winner { get; }
}
