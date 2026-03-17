using Santase.Engine;
using Santase.Engine.Engine;

namespace Santase.UnitTests;

public class GameEngineRulesTests
{
    [Fact]
    public void GetLegalMoves_ClosedPhase_ReturnsTheCardsFromWeakerToStronger()
    {
        var engine = new GameEngine();
        var state = engine.CurrentState;

        state.IsClosed = true;
        state.PlayerOneHand.Clear();
        state.PlayerTwoHand.Clear();
        state.CurrentTrick.Clear();

        var leadCard = new Card { Suit = Suit.Clubs, Type = Rank.Ten };
        state.CurrentTrick.Add(leadCard);
        state.SetPlayerTurn(1);

        var weakerSameSuit = new Card { Suit = Suit.Clubs, Type = Rank.Nine };
        var strongerSameSuit = new Card { Suit = Suit.Clubs, Type = Rank.Ace };
        var offSuit = new Card { Suit = Suit.Hearts, Type = Rank.Ace };

        state.PlayerTwoHand.Add(weakerSameSuit);
        state.PlayerTwoHand.Add(strongerSameSuit);
        state.PlayerTwoHand.Add(offSuit);

        var legalMoves = engine.GetLegalMoves(state).ToList();

        Assert.Same(weakerSameSuit, legalMoves[0].Card);
        Assert.Same(strongerSameSuit, legalMoves[1].Card);
    }

    [Fact]
    public void GetLegalMoves_ClosedPhase_WhenCannotFollowButHasTrump_ShouldReturnOnlyTrumpCards()
    {
        var engine = new GameEngine();
        var state = engine.CurrentState;

        state.IsClosed = true;
        state.PlayerOneHand.Clear();
        state.PlayerTwoHand.Clear();
        state.CurrentTrick.Clear();

        var leadSuit = Enum.GetValues<Suit>().First(s => s != state.TrumpSuit);
        state.CurrentTrick.Add(new Card { Suit = leadSuit, Type = Rank.Ace });
        state.SetPlayerTurn(1);

        var trumpCard = new Card { Suit = state.TrumpSuit, Type = Rank.Nine };
        var offSuit = new Card { Suit = Enum.GetValues<Suit>().First(s => s != state.TrumpSuit && s != leadSuit), Type = Rank.King };

        state.PlayerTwoHand.Add(trumpCard);
        state.PlayerTwoHand.Add(offSuit);

        var legalMoves = engine.GetLegalMoves(state).ToList();

        Assert.Single(legalMoves);
        Assert.Same(trumpCard, legalMoves[0].Card);
    }

    [Fact]
    public void ApplyMove_ClosedPhase_WhenMoveViolatesStrictRules_ShouldThrow()
    {
        var engine = new GameEngine();
        var state = engine.CurrentState;

        state.IsClosed = true;
        state.PlayerOneHand.Clear();
        state.PlayerTwoHand.Clear();
        state.CurrentTrick.Clear();

        var leadCard = new Card { Suit = Suit.Spades, Type = Rank.Ace };
        state.CurrentTrick.Add(leadCard);
        state.SetPlayerTurn(1);

        var followSuitCard = new Card { Suit = Suit.Spades, Type = Rank.Nine };
        var illegalOffSuitCard = new Card { Suit = Suit.Hearts, Type = Rank.King };
        state.PlayerTwoHand.Add(followSuitCard);
        state.PlayerTwoHand.Add(illegalOffSuitCard);

        var illegalMove = new Move(illegalOffSuitCard);

        Assert.Throws<InvalidOperationException>(() => engine.ApplyMove(illegalMove, state));
    }

    [Fact]
    public void ApplyMove_WhenSecondPlayerLeadsAndWins_ShouldKeepWinnerOnTurnAndAwardPointsToWinner()
    {
        var engine = new GameEngine();
        var state = engine.CurrentState;

        state.IsClosed = true;
        state.PlayerOneHand.Clear();
        state.PlayerTwoHand.Clear();
        state.CurrentTrick.Clear();
        state.PlayerOnePoints = 0;
        state.PlayerTwoPoints = 0;
        state.SetPlayerTurn(1);

        var nonTrumpSuit = Enum.GetValues<Suit>().First(s => s != state.TrumpSuit);
        var leadCard = new Card { Suit = nonTrumpSuit, Type = Rank.Ace };
        var replyCard = new Card { Suit = nonTrumpSuit, Type = Rank.Nine };

        state.PlayerTwoHand.Add(leadCard);
        state.PlayerOneHand.Add(replyCard);

        var firstMove = new Move(leadCard);
        var firstResult = engine.ApplyMove(firstMove, state);

        Assert.False(firstResult.GameEnded);
        Assert.Equal(0, firstResult.State.PlayerTurn);

        var secondMove = new Move(replyCard);
        var secondResult = engine.ApplyMove(secondMove, firstResult.State);

        Assert.True(secondResult.TrickCompleted);
        Assert.Equal(1, secondResult.State.PlayerTurn);
        Assert.Equal(11, secondResult.State.PlayerTwoPoints);
        Assert.Equal(0, secondResult.State.PlayerOnePoints);
    }
    [Fact]
    public void ApplyMarriageMove_ShouldPlayAutomaticallyTheMarriage()
    {
        var engine = new GameEngine();
        var state = engine.CurrentState;

        state.PlayerOneHand.Clear();
        state.PlayerTwoHand.Clear();
        state.CurrentTrick.Clear();
        state.SetPlayerTurn(0);

        var nonTrumpSuit = Enum.GetValues<Suit>().First(s => s != state.TrumpSuit);
        var queen = new Card { Suit = nonTrumpSuit, Type = Rank.Queen };
        var king = new Card { Suit = nonTrumpSuit, Type = Rank.King };
        state.PlayerOneHand.Add(queen);
        state.PlayerOneHand.Add(king);

        var randomCard = new Card { Suit = nonTrumpSuit, Type = Rank.Nine };
        state.PlayerTwoHand.Add(randomCard);

        var legalMoves = engine.GetLegalMoves(state).ToList();
        var move = legalMoves.First();
        engine.ApplyMove(move, state);

        // Player 1 turn
        var legalMovesReply = engine.GetLegalMoves(state).ToList();
        var replyMove = legalMoves.First();
        engine.ApplyMove(move, state);


    }
}

