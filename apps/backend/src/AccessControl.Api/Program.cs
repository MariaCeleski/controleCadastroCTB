using AccessControl.Application.Companies;
using AccessControl.Application.Security;
using AccessControl.Application.Users;
using AccessControl.Infrastructure.Persistence;
using AccessControl.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AccessControl") ?? throw new InvalidOperationException("Connection string 'AccessControl' is required.");
// Runtime secrets come from User Secrets locally and the secret manager in deployed environments, never source control.
var encryptionKey = builder.Configuration["Security:CredentialEncryptionKey"] ?? throw new InvalidOperationException("Credential encryption key is required.");
var jwtIssuer = builder.Configuration["Security:Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is required.");
var jwtAudience = builder.Configuration["Security:Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is required.");
var jwtKey = builder.Configuration["Security:Jwt:SigningKey"] ?? throw new InvalidOperationException("JWT signing key is required.");

builder.Services.AddControllers();
var allowedOrigins = builder.Configuration.GetSection("Client:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(options => options.AddPolicy("DesktopClient", policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new()
{
    ValidateIssuer = true, ValidIssuer = jwtIssuer, ValidateAudience = true, ValidAudience = jwtAudience,
    ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1)
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AccessControlDbContext>();
builder.Services.AddDbContext<AccessControlDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IValidator<CreateCompanyRequest>, CreateCompanyRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterOrganizationRequest>, RegisterOrganizationRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService>(_ => new JwtTokenService(jwtIssuer, jwtAudience, jwtKey));
builder.Services.AddSingleton<ICredentialCipher>(_ => new AesCredentialCipher(encryptionKey));
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("DesktopClient");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public partial class Program;
