using Santase.Engine.Engine;

var engine = new GameEngine();


System.Console.WriteLine("Initial Game State:");
System.Console.WriteLine(engine.CurrentState.GetStateString());
System.Console.WriteLine("----");
var gameEnded = false;
while (!gameEnded)
{
    var move = engine.GetLegalMoves(engine.CurrentState).FirstOrDefault()!;
    Console.WriteLine($"Applying move: {move}");
    var result = engine.ApplyMove(move, engine.CurrentState);
    System.Console.WriteLine("----");
    System.Console.WriteLine("Game State after move:");
    System.Console.WriteLine(result.State.GetStateString());
    System.Console.WriteLine("----");

    gameEnded = result.GameEnded;
}
