using AccessControl.Application.Companies;
using AccessControl.Application.Security;
using AccessControl.Infrastructure.Persistence;
using AccessControl.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AccessControl") ?? throw new InvalidOperationException("Connection string 'AccessControl' is required.");
var encryptionKey = builder.Configuration["Security:CredentialEncryptionKey"] ?? throw new InvalidOperationException("Credential encryption key is required.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddNpgSql(connectionString);
builder.Services.AddDbContext<AccessControlDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddValidatorsFromAssemblyContaining<CreateCompanyRequestValidator>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddSingleton<ICredentialCipher>(_ => new AesCredentialCipher(encryptionKey));
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public partial class Program;
