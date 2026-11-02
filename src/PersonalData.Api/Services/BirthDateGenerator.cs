namespace PersonalData.Api.Services;

public sealed class BirthDateGenerator(Random random, TimeProvider timeProvider) : IBirthDateGenerator
{
    public DateOnly Generate() => throw new NotImplementedException();
}
