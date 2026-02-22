using JobMarketPlaceApi;
using JobMarketPlaceApi.Auth;
using JobMarketPlaceApi.Data;
using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authorization;

/* JWT: 
 * Extended later if we want to support JWT-based authentication
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
*/


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<JobMarketPlaceApiContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("JobMarketPlaceApiContext") ?? throw new InvalidOperationException("Connection string 'JobMarketPlaceApiContext' not found.")));

// Register the simple development authentication scheme.
// NOTE: This authenticates every request as a "dev-user". Replace in production with JWT/Cookie/etc.
builder.Services.AddAuthentication("Dev")
    .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>("Dev", options => { });

/* JWT:
// Read JWT settings from configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT configuration missing: set Jwt:Key in configuration.");
}

// Configure JwtBearer authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });
*/
builder.Services.AddAuthorization();


// For Repository pattern and Service layers
// Register repositories and services (DI) for customer search
builder.Services.AddScoped<JobMarketPlaceApi.Data.Repositories.ICustomerRepository, JobMarketPlaceApi.Data.Repositories.CustomerRepository>();
builder.Services.AddScoped<JobMarketPlaceApi.Services.ICustomerSearchService, JobMarketPlaceApi.Services.CustomerSearchService>();
// Register repositories for create jobs
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ICustomerJobService, CustomerJobService>();
// Register repositories and services for create job oofers
builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();
builder.Services.AddScoped<IContractorJobOfferService, ContractorJobOfferService>();


/* Cache:
// Add in-memory cache configured with a size limit so entries are evicted when capacity is reached.
// SizeLimit units are abstract; we set one unit per cached entry below.
// note: For a multi-instance deployment use a distributed cache (Redis)
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 10000; // adjust based on available memory and expected cached items
});
*/


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ensure authentication middleware runs before authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// For controller-based endpoints only; minimal API endpoints are mapped separately below
//app.MapControllers();

// Minimal API endpoints mapped from extension methods
// in separate files for better organization
// can be converted to Controller-based endpoints if preferred; the data access and auth patterns remain the same.
app.MapJobEndpoints();
app.MapJobOfferEndpoints();
app.MapCustomerEndpoints();
app.MapContractorEndpoints(); 

app.Run();
