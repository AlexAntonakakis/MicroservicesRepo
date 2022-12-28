using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;

namespace Play.Trading.Service.SignalR
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}