namespace PersonalData.Api.Services;

public sealed class PhoneNumberGenerator(Random random) : IPhoneNumberGenerator
{
    public string Generate() => throw new NotImplementedException();
}
