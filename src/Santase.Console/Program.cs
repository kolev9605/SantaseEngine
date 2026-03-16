using Santase.Engine;
using Santase.Engine.Engine;

var engine = new GameEngine();
var random = new Random();

PrintWelcome();

var gameEnded = false;
while (!gameEnded)
{
    PrintRoundHeader(engine.CurrentState);

    Move move;
    if (engine.CurrentState.PlayerTurn == 0)
    {
        move = ReadPlayerMove(engine);
    }
    else
    {
        move = SelectRandomMove(engine, random);
        PrintComputerMove(move);
    }

    var result = engine.ApplyMove(move, engine.CurrentState);
    gameEnded = result.GameEnded;

    PrintPostMoveSummary(result.State);
    Pause();
}

PrintGameOver(engine.CurrentState);

static void PrintWelcome()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("========================================");
    Console.WriteLine("            SANTASE CONSOLE             ");
    Console.WriteLine("========================================");
    Console.ResetColor();
    Console.WriteLine("You are Player 1. Choose a card index to play each turn.");
    Console.WriteLine("Player 2 is controlled by the computer.");
    Console.WriteLine();
}

static void PrintRoundHeader(GameState state)
{
    Console.WriteLine("----------------------------------------");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Turn: {(state.PlayerTurn == 0 ? "Player 1 (You)" : "Player 2 (Computer)")}");
    Console.ResetColor();
    Console.WriteLine($"Score -> You: {state.PlayerOnePoints} | Computer: {state.PlayerTwoPoints}");
    Console.WriteLine($"Trump: {state.TrumpSuit} {(state.TrumpCard is null ? "(drawn)" : $"({state.TrumpCard})")}");
    Console.WriteLine($"Talon cards left: {state.Talon.Cards.Count}");

    if (state.CurrentTrick.Count > 0)
    {
        Console.WriteLine($"Current trick: {string.Join(", ", state.CurrentTrick)}");
    }
    else
    {
        Console.WriteLine("Current trick: (empty)");
    }

    Console.WriteLine();
}

static Move ReadPlayerMove(GameEngine engine)
{
    var legalMoves = engine.GetLegalMoves(engine.CurrentState).ToList();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Your hand:");
    Console.ResetColor();
    var hand = engine.CurrentState.PlayerTurn == 0 ? engine.CurrentState.PlayerOneHand : engine.CurrentState.PlayerTwoHand;
    System.Console.WriteLine(string.Join(", ", hand));

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Legal moves:");
    Console.ResetColor();
    for (var i = 0; i < legalMoves.Count; i++)
    {
        Console.WriteLine($"  [{i + 1}] {legalMoves[i].Card}");
    }

    Console.WriteLine();
    Console.Write("Choose card number to play: ");

    while (true)
    {
        var input = Console.ReadLine();
        if (int.TryParse(input, out var selectedIndex)
            && selectedIndex >= 1
            && selectedIndex <= legalMoves.Count)
        {
            var selectedMove = legalMoves[selectedIndex - 1];
            Console.WriteLine($"You played: {selectedMove.Card}");
            return selectedMove;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Invalid choice. Enter a number from the list: ");
        Console.ResetColor();
    }
}

static Move SelectRandomMove(GameEngine engine, Random random)
{
    var legalMoves = engine.GetLegalMoves(engine.CurrentState).ToList();
    var randomIndex = random.Next(legalMoves.Count);
    return legalMoves[randomIndex];
}

static void PrintComputerMove(Move move)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"Computer played: {move.Card}");
    Console.ResetColor();
}

static void PrintPostMoveSummary(GameState state)
{
    Console.WriteLine();
    Console.WriteLine("Updated score:");
    Console.WriteLine($"  You: {state.PlayerOnePoints}");
    Console.WriteLine($"  Computer: {state.PlayerTwoPoints}");
    Console.WriteLine();
}

static void PrintGameOver(GameState state)
{
    Console.WriteLine("========================================");
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("               GAME OVER                ");
    Console.ResetColor();
    Console.WriteLine("========================================");

    Console.WriteLine($"Final score -> You: {state.PlayerOnePoints} | Computer: {state.PlayerTwoPoints}");

    if (state.PlayerOnePoints >= 66)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Winner: You");
    }
    else if (state.PlayerTwoPoints >= 66)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Winner: Computer");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Winner: Not determined");
    }

    Console.ResetColor();
}

static void Pause()
{
    Console.WriteLine();
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
    Console.WriteLine();
}
