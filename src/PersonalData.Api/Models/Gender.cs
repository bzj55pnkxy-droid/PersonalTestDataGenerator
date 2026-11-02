using System.Text.Json.Serialization;

namespace PersonalData.Api.Models;

/// <summary>Serialized as <c>"female"</c> / <c>"male"</c>, matching <c>person-names.json</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<Gender>))]
public enum Gender
{
    [JsonStringEnumMemberName("female")]
    Female,

    [JsonStringEnumMemberName("male")]
    Male,
}
