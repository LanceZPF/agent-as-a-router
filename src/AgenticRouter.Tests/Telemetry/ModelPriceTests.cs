using AgenticRouter.Telemetry;

namespace AgenticRouter.Tests.Telemetry;

/// <summary>Covers <see cref="ModelPrice.EstimateCost"/>.</summary>
public class ModelPriceTests
{
    [Fact]
    public void EstimateCost_ComputesPerMillionTokenRatesCorrectly()
    {
        var price = new ModelPrice(InputPerMillionTokens: 3.00m, OutputPerMillionTokens: 15.00m);

        var cost = price.EstimateCost(promptTokens: 1_000_000, completionTokens: 1_000_000);

        Assert.Equal(18.00m, cost);
    }

    [Fact]
    public void EstimateCost_FractionOfAMillionTokens_ScalesLinearly()
    {
        var price = new ModelPrice(InputPerMillionTokens: 2.00m, OutputPerMillionTokens: 10.00m);

        // 500,000 prompt tokens = half of $2.00 = $1.00; 100,000 completion tokens = 1/10 of $10.00 = $1.00.
        var cost = price.EstimateCost(promptTokens: 500_000, completionTokens: 100_000);

        Assert.Equal(2.00m, cost);
    }

    [Fact]
    public void EstimateCost_ZeroTokens_IsZero()
    {
        var price = new ModelPrice(InputPerMillionTokens: 5.00m, OutputPerMillionTokens: 20.00m);

        var cost = price.EstimateCost(0, 0);

        Assert.Equal(0m, cost);
    }
}
