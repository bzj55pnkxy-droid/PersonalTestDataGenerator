using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

public class JsonPersonNameRepositoryTests
{
    [Fact]
    public void GetAllLoadsKnownPersonFromJsonFile()
    {
        // Arrange
        var repository = new JsonPersonNameRepository();

        // Act
        var names = repository.GetAll();

        // Assert
        Assert.Contains(new NameAndGender("Annemette P.", "Nilsson", Gender.Female), names);
    }
}