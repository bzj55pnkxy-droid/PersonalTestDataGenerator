using Microsoft.AspNetCore.Mvc;
using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class FakePersonController(IFakePersonService fakePersonService) : ControllerBase
{
    [HttpGet("cpr")]
    public ActionResult<CprResponse> GetCpr() =>
        new CprResponse(fakePersonService.GetCpr());

    [HttpGet("name-gender")]
    public ActionResult<NameAndGender> GetNameAndGender() =>
        fakePersonService.GetNameAndGender();

    [HttpGet("name-gender-dob")]
    public ActionResult<NameGenderAndBirthDate> GetNameGenderAndBirthDate() =>
        fakePersonService.GetNameGenderAndBirthDate();

    [HttpGet("cpr-name-gender")]
    public ActionResult<CprNameAndGender> GetCprNameAndGender() =>
        fakePersonService.GetCprNameAndGender();

    [HttpGet("cpr-name-gender-dob")]
    public ActionResult<CprNameGenderAndBirthDate> GetCprNameGenderAndBirthDate() =>
        fakePersonService.GetCprNameGenderAndBirthDate();

    [HttpGet("address")]
    public async Task<ActionResult<Address>> GetAddress(CancellationToken cancellationToken) =>
        await fakePersonService.GetAddressAsync(cancellationToken);

    [HttpGet("phone")]
    public ActionResult<PhoneNumberResponse> GetPhoneNumber() =>
        new PhoneNumberResponse(fakePersonService.GetPhoneNumber());

    [HttpGet("person")]
    public async Task<ActionResult<Person>> GetPerson(CancellationToken cancellationToken) =>
        await fakePersonService.GetPersonAsync(cancellationToken);

    [HttpGet("persons")]
    public async Task<ActionResult<IReadOnlyList<Person>>> GetPersons([FromQuery] int count, CancellationToken cancellationToken) =>
        Ok(await fakePersonService.GetPersonsAsync(count, cancellationToken));
}
