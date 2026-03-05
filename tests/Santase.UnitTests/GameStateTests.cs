using Santase.Engine;

namespace Santase.UnitTests;

public class GameStateTests
{
    [Fact]
    public void GameState_Initialize_ShouldCreateAValidSantaseStartingState()
    {
        var state = new GameState();
        var talonSizeAfterInit = GameConstants.TotalCardsCount - (GameConstants.PlayerInitialHandSize + GameConstants.PlayerInitialHandSize + 1);

        Assert.Equal(GameConstants.PlayerInitialHandSize, state.PlayerOneHand.Count);
        Assert.Equal(GameConstants.PlayerInitialHandSize, state.PlayerTwoHand.Count);
        Assert.NotNull(state.TrumpCard);
        Assert.Equal(state.TrumpCard!.Suit, state.TrumpSuit);
        Assert.Equal(talonSizeAfterInit, state.Talon.Cards.Count());

        Assert.Empty(state.CurrentTrick);
        Assert.Equal(0, state.PlayerOnePoints);
        Assert.Equal(0, state.PlayerTwoPoints);
        Assert.False(state.IsClosed);
    }

    [Fact]
    public void GameState_Initialize_ShouldContainAll24UniqueCards()
    {
        var state = new GameState();

        var allCards = state.PlayerOneHand
            .Concat(state.PlayerTwoHand)
            .Append(state.TrumpCard!)
            .Concat(state.Talon.Cards)
            .ToList();

        Assert.Equal(GameConstants.TotalCardsCount, allCards.Count);
        Assert.Equal(GameConstants.TotalCardsCount, allCards.Select(c => c.CardId).Distinct().Count());

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            Assert.Equal(6, allCards.Count(card => card.Suit == suit));
        }

        foreach (Rank rank in Enum.GetValues(typeof(Rank)))
        {
            Assert.Equal(4, allCards.Count(card => card.Type == rank));
        }
    }

    [Fact]
    public void GameState_Initialize_ShouldProduceDifferentCardOrdersAcrossGames()
    {
        const int samples = 12;
        var signatures = new HashSet<string>();

        for (int i = 0; i < samples; i++)
        {
            var state = new GameState();

            var cardOrderSignature = string.Join(",", state.PlayerOneHand
                .Concat(state.PlayerTwoHand)
                .Append(state.TrumpCard!)
                .Concat(state.Talon.Cards)
                .Select(c => c.CardId));

            signatures.Add(cardOrderSignature);
        }

        Assert.True(
            signatures.Count > 1,
            "Card order should vary between game initializations after shuffling.");
    }

    [Fact]
    public void GameState_DrawMoreCardsThanTalon_ShouldDrawOnlyTheRemainingCards()
    {
        var state = new GameState();

        var talonSizeAfterInit = GameConstants.TotalCardsCount - (GameConstants.PlayerInitialHandSize + GameConstants.PlayerInitialHandSize + 1);

        var cardsDrawn = state.Talon.Draw(600);

        Assert.Equal(talonSizeAfterInit, cardsDrawn.Count);

        var drawOneCard = state.Talon.DrawOne();
        Assert.Null(drawOneCard);
    }

    [Fact]
    public void GameState_DrawOnEmptyTalon_ShouldReturnEmptyCollection()
    {
        var state = new GameState();

        var talonSizeAfterInit = GameConstants.TotalCardsCount - (GameConstants.PlayerInitialHandSize + GameConstants.PlayerInitialHandSize + 1);

        var cardsDrawn = state.Talon.Draw(talonSizeAfterInit);

        Assert.Equal(talonSizeAfterInit, cardsDrawn.Count);

        var moreDrawnCards = state.Talon.Draw(50);

        Assert.Empty(moreDrawnCards);

        var drawOneCard = state.Talon.DrawOne();
        Assert.Null(drawOneCard);
    }
}
