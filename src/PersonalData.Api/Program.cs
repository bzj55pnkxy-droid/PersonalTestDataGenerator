using PersonalData.Api.Data;
using PersonalData.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Randomness and "today" are injected so tests can pass a seeded Random / fake TimeProvider.
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<IPersonNameRepository, JsonPersonNameRepository>();
builder.Services.AddScoped<IPostalCodeRepository, MySqlPostalCodeRepository>();

builder.Services.AddScoped<INameGenderGenerator, NameGenderGenerator>();
builder.Services.AddScoped<IBirthDateGenerator, BirthDateGenerator>();
builder.Services.AddScoped<ICprGenerator, CprGenerator>();
builder.Services.AddScoped<IAddressGenerator, AddressGenerator>();
builder.Services.AddScoped<IPhoneNumberGenerator, PhoneNumberGenerator>();
builder.Services.AddScoped<IFakePersonService, FakePersonService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Frontend: serves wwwroot/index.html at "/" and the files under wwwroot/css and wwwroot/js.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
