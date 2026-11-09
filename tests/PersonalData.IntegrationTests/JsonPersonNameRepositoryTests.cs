using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

public class JsonPersonNameRepositoryTests
{
    [Fact]
public void GetAll_LoadsKnownPersonFromJsonFile()
{
    // Arrange
    var repository = new JsonPersonNameRepository();

    // Act
    var names = repository.GetAll();

    // Assert
    Assert.Contains(names, n =>
        n.FirstName == "Annemette P." &&
        n.LastName == "Nilsson" &&
        n.Gender == Gender.Female);
}
}