using AbTests.Api.Helpers.Interfaces;

namespace AbTests.Api.Helpers;

public class RandomHelper : IRandomHelper
{
    private static readonly Random _rnd = new();

    public int Next(int maxValue)
    {
        return _rnd.Next(maxValue);
    }
}