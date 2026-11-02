using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

/// <summary>Random first name, last name and gender from <c>person-names.json</c>.</summary>
public interface INameGenderGenerator
{
    NameAndGender Generate();
}
