using System.ComponentModel.DataAnnotations;
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
    public ActionResult<Address> GetAddress() =>
        fakePersonService.GetAddress();

    [HttpGet("phone")]
    public ActionResult<PhoneNumberResponse> GetPhoneNumber() =>
        new PhoneNumberResponse(fakePersonService.GetPhoneNumber());

    [HttpGet("person")]
    public ActionResult<Person> GetPerson() =>
        fakePersonService.GetPerson();

    [HttpGet("persons")]
    public ActionResult<IReadOnlyList<Person>> GetPersons([FromQuery, Range(2, 100)] int count) =>
        Ok(fakePersonService.GetPersons(count));
}
