using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class FakePersonController(IFakePersonService fakePersonService) : ControllerBase
{
    [HttpGet("cpr")]
    [ProducesResponseType<CprResponse>(StatusCodes.Status200OK)]
    public ActionResult<CprResponse> GetCpr() =>
        Ok(new CprResponse(fakePersonService.GetCpr()));

    [HttpGet("name-gender")]
    [ProducesResponseType<NameAndGender>(StatusCodes.Status200OK)]
    public ActionResult<NameAndGender> GetNameAndGender() =>
        Ok(fakePersonService.GetNameAndGender());

    [HttpGet("name-gender-dob")]
    [ProducesResponseType<NameGenderAndBirthDate>(StatusCodes.Status200OK)]
    public ActionResult<NameGenderAndBirthDate> GetNameGenderAndBirthDate() =>
        Ok(fakePersonService.GetNameGenderAndBirthDate());

    [HttpGet("cpr-name-gender")]
    [ProducesResponseType<CprNameAndGender>(StatusCodes.Status200OK)]
    public ActionResult<CprNameAndGender> GetCprNameAndGender() =>
        Ok(fakePersonService.GetCprNameAndGender());

    [HttpGet("cpr-name-gender-dob")]
    [ProducesResponseType<CprNameGenderAndBirthDate>(StatusCodes.Status200OK)]
    public ActionResult<CprNameGenderAndBirthDate> GetCprNameGenderAndBirthDate() =>
        Ok(fakePersonService.GetCprNameGenderAndBirthDate());

    [HttpGet("address")]
    [ProducesResponseType<Address>(StatusCodes.Status200OK)]
    public ActionResult<Address> GetAddress() =>
        Ok(fakePersonService.GetAddress());

    [HttpGet("phone")]
    [ProducesResponseType<PhoneNumberResponse>(StatusCodes.Status200OK)]
    public ActionResult<PhoneNumberResponse> GetPhoneNumber() =>
        Ok(new PhoneNumberResponse(fakePersonService.GetPhoneNumber()));

    [HttpGet("person")]
    [ProducesResponseType<Person>(StatusCodes.Status200OK)]
    public ActionResult<Person> GetPerson() =>
        Ok(fakePersonService.GetPerson());

    [HttpGet("persons")]
    [ProducesResponseType<IReadOnlyList<Person>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<Person>> GetPersons([FromQuery, BindRequired, Range(2, 100)] int count) =>
        Ok(fakePersonService.GetPersons(count));
}
