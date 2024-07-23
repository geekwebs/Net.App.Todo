using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Net.App.Todo.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => // Begin Keycloak Integration
{
    options.Authority = builder.Configuration.GetValue<string>("IdentityBroker:JwtBearer:Authority");
    options.Audience = builder.Configuration.GetValue<string>("IdentityBroker:JwtBearer:Audience");
    options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("IdentityBroker:JwtBearer:RequireHttpsMetadata");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("IdentityBroker:JwtBearer:Authority"),
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetValue<string>("IdentityBroker:JwtBearer:Audience"),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.HandleResponse(); // Suppress the default logic

                // Check if the error is due to an expired token
                var exception = context.AuthenticateFailure;
                var message = exception != null && exception.GetType() == typeof(SecurityTokenExpiredException)
                    ? "Your token has been expired"
                    : "You are not authorized to access this resource";

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { message });
                return context.Response.WriteAsync(result);
            }
            return Task.CompletedTask;
        }
    };
}) // End Keycloak Integration
.AddScheme<TokenKeyAuthenticationOptions, TokenKeyAuthenticationHandler>(TokenKeyAuthenticationOptions.DefaultScheme, options => { });

// Configure Authorization 
builder.Services.AddAuthorization(options => // Support Multi Scheme Authentication with Keycloak
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, TokenKeyAuthenticationOptions.DefaultScheme)
        .RequireAuthenticatedUser()
        .Build();
}); // End Support Multi Scheme Authentication with Keycloak

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
