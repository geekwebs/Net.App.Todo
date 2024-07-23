using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using WebAuthn;
using WebAuthn.AspNetCore;
using Net.App.Todo.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

public partial class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                 webBuilder.ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                });

                webBuilder.ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.Configure<FormOptions>(options =>
                    {
                        options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB per file
                    });

                    // services.AddDbContext<ApplicationDbContext>(options =>
                    //     options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

                    services.AddWebAuthn(options =>
                    {
                        options.RelyingPartyName = Configuration["WebAuthn:RelyingPartyName"];
                        options.RelyingPartyOrigin = new Uri(Configuration["WebAuthn:RelyingPartyOrigin"]);
                    });

                    services.AddControllers();
                    // Add CORS services
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAll", builder =>
                        {
                            builder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                        });
                    });

                    services.AddEndpointsApiExplorer();
                    services.AddSwaggerGen();
                    services.AddLogging();

                    // Configure Authentication
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => // Begin Keycloak Integration
                    {
                        options.Authority = configuration.GetValue<string>("IdentityBroker:JwtBearer:Authority");
                        options.Audience = configuration.GetValue<string>("IdentityBroker:JwtBearer:Audience");
                        options.RequireHttpsMetadata = configuration.GetValue<bool>("IdentityBroker:JwtBearer:RequireHttpsMetadata");
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = configuration.GetValue<string>("IdentityBroker:JwtBearer:Authority"),
                            ValidateAudience = true,
                            ValidAudience = configuration.GetValue<string>("IdentityBroker:JwtBearer:Audience"),
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
                                        ? "Your token has expired"
                                        : "You are not authorized to access this resource";

                                    context.Response.StatusCode = 401;
                                    context.Response.ContentType = "application/json";
                                    var result = JsonSerializer.Serialize(new { message });
                                    return context.Response.WriteAsync(result);
                                }
                                return Task.CompletedTask;
                            }
                        };
                    })
                    .AddScheme<TokenKeyAuthenticationOptions, TokenKeyAuthenticationHandler>(TokenKeyAuthenticationOptions.DefaultScheme, options => { });

                    // Configure Authorization 
                    services.AddAuthorization(options => // Support Multi Scheme Authentication with Keycloak
                    {
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, TokenKeyAuthenticationOptions.DefaultScheme)
                            .RequireAuthenticatedUser()
                            .Build();
                    }); 
                });

                webBuilder.Configure((context, app) =>
                {
                    var env = context.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }

                    // app.UseHttpsRedirection();
                    // Use CORS middleware
                    app.UseCors("AllowAll");

                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });
}
