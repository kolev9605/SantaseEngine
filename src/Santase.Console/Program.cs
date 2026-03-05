using Santase.Engine;

var state = new GameState();
System.Console.WriteLine("First player hand");
System.Console.WriteLine(string.Join(", ", state.PlayerOneHand.Select(c => c.CardId)));
System.Console.WriteLine("First player hand");
System.Console.WriteLine(string.Join(", ", state.PlayerTwoHand.Select(c => c.CardId)));
