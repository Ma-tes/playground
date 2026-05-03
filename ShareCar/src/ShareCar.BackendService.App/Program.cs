using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShareCar.BackendService.App.Configuration;
using ShareCar.BackendService.Domain.Configuration;
using ShareCar.BackendService.Domain.Repositories;
using ShareCar.BackendService.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
  ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var pricingConfig = builder.Configuration.GetSection("Pricing").Get<PricingConfiguration>()
  ?? throw new InvalidOperationException("Pricing configuration not found.");

var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>()
  ?? throw new InvalidOperationException("JWT configuration not found.");

// value

builder.Services.AddControllers();
builder.Services.AddSingleton<IPricingConfiguration>(pricingConfig);
builder.Services.AddSingleton<IJwtConfiguration>(jwtConfig);
builder.Services.AddDomainRepositories(connectionString);
builder.Services.AddDomainServices();

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtConfig.Issuer,
    ValidAudience = jwtConfig.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
  };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "ShareCar API",
    Version = "v1",
    Description = "ShareCar Backend Service API"
  });

  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      []
    }
  });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<ShareCarDbContext>();
  dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ShareCar API v1");
  });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

