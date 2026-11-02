namespace PersonalData.Api.Models;

/// <param name="Street">Random alphabetic characters.</param>
/// <param name="Number">1-999, optionally followed by an uppercase letter, e.g. <c>43B</c>.</param>
/// <param name="Floor"><c>st</c> or 1-99.</param>
/// <param name="Door"><c>th</c>, <c>mf</c>, <c>tv</c>, 1-50, or a lowercase letter optionally followed by a dash, then 1-3 digits, e.g. <c>c3</c>, <c>d-14</c>.</param>
/// <param name="PostalCode">From the <c>addresses</c> database.</param>
/// <param name="TownName">From the <c>addresses</c> database.</param>
public sealed record Address(string Street, string Number, string Floor, string Door, string PostalCode, string TownName);
