using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

/// <summary>Names and genders from <c>person-names.json</c>.</summary>
public interface IPersonNameRepository
{
    IReadOnlyList<NameAndGender> GetAll();
}
