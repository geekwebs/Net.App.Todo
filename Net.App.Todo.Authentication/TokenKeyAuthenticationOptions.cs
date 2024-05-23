using Microsoft.AspNetCore.Authentication;

namespace Net.App.Todo.Authentication;
public class TokenKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "TokenKey";
    public string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;

}
