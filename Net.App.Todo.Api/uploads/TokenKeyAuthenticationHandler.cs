using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Net.App.Todo.Authentication
{
    public class TokenKeyAuthenticationHandler : AuthenticationHandler<TokenKeyAuthenticationOptions>
    {
        public TokenKeyAuthenticationHandler(
            IOptionsMonitor<TokenKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Adinskey"))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Missing Adinskey Header"));
            }

            var token = Request.Headers["Adinskey"].ToString();

            // Here, validate the token (this example assumes a dummy validation)
            if (token != "eUpDcWVQVGk5Yk4yTUc4Y3cvQ0pLejFsQXc5UHJkTldtUEoraXB3QTVnOEhNTj0=")
            {
                return await Task.FromResult(AuthenticateResult.Fail("Invalid Token"));
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
                new Claim(ClaimTypes.Name, "username")
            };
            var identity = new ClaimsIdentity(claims, nameof(TokenKeyAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (!Response.HasStarted) // Untuk handling challange salah satu scheme
            {
                Response.StatusCode = 401;
                Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = "Unauthorized", message = "You are not authorized to access this resource" });
                await Response.WriteAsync(result);
            }
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            if (!Response.HasStarted) // Untuk handling challange salah satu scheme
            {
                Response.StatusCode = 403;
                Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { message = "Forbidden" });
                await Response.WriteAsync(result);
            }
        }
    }
}
