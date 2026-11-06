using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class AddressGenerator(IPostalCodeRepository postalCodes, Random random) : IAddressGenerator
{
    public async Task<Address> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var towns = await postalCodes.GetAllAsync(cancellationToken);
        if (towns.Count == 0)
        {
            throw new InvalidOperationException("No postal codes are available.");
        }

        var town = towns[random.Next(towns.Count)];
        var street = GetRandomText(40);

        var number = random.Next(1, 1000).ToString();
        if (random.Next(100) < 20)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            number += letters[random.Next(letters.Length)];
        }

        var floor = random.Next(100) < 30 ? "st" : random.Next(1, 100).ToString();
        var door = GetRandomDoor();

        return new Address(street, number, floor, door, town.PostalCode, town.TownName);
    }

    private string GetRandomText(int length)
    {
        const string characters = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZæøåÆØÅ";
        var text = new char[length];

        // The street must start with a letter.
        text[0] = characters[random.Next(1, characters.Length)];
        for (var i = 1; i < length; i++)
        {
            text[i] = characters[random.Next(characters.Length)];
        }

        return new string(text);
    }

    private string GetRandomDoor()
    {
        var doorType = random.Next(1, 21);

        if (doorType <= 7) return "th";
        if (doorType <= 14) return "tv";
        if (doorType <= 16) return "mf";
        if (doorType <= 18) return random.Next(1, 51).ToString();

        const string letters = "abcdefghijklmnopqrstuvwxyzøæå";
        var letter = letters[random.Next(letters.Length)];
        var separator = doorType == 20 ? "-" : "";

        return $"{letter}{separator}{random.Next(1, 1000)}";
    }
}
