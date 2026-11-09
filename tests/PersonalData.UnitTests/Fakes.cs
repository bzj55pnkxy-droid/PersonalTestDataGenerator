using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.UnitTests;

/// <summary>Dice that always roll the lowest possible number.</summary>
public sealed class LowestRandom : Random
{
    public override int Next(int maxValue) => 0;

    public override int Next(int minValue, int maxValue) => minValue;
}

/// <summary>
/// Dice that roll the lowest possible number, except for the rolls you load:
/// <c>new LoadedRandom { [(1, 21)] = 17 }</c> answers 17 whenever the code calls <c>Next(1, 21)</c>.
/// <c>Next(max)</c> counts as <c>Next(0, max)</c>.
/// </summary>
public sealed class LoadedRandom : Random
{
    private readonly Dictionary<(int Min, int Max), int> _rolls = [];

    public int this[(int Min, int Max) range]
    {
        get => _rolls[range];
        set => _rolls[range] = value;
    }

    public override int Next(int maxValue) => Next(0, maxValue);

    public override int Next(int minValue, int maxValue) =>
        _rolls.GetValueOrDefault((minValue, maxValue), minValue);
}

/// <summary>A clock that always says <paramref name="now"/>, in UTC so the date never shifts with the machine's time zone.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

public sealed class StubPostalCodeRepository(params PostalTown[] towns) : IPostalCodeRepository
{
    public IReadOnlyList<PostalTown> GetAll() => towns;
}

public sealed class StubPersonNameRepository(params NameAndGender[] names) : IPersonNameRepository
{
    public IReadOnlyList<NameAndGender> GetAll() => names;
}
