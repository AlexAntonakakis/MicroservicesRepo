using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Common.Service.Settings;
using Microsoft.IdentityModel.Tokens;

namespace Common.Identity
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IConfiguration configuration;

        public ConfigureJwtBearerOptions(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if  (name == JwtBearerDefaults.AuthenticationScheme)
            {
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings))
                                                    .Get<ServiceSettings>();

                    options.Authority = serviceSettings.Authority;
                    options.Audience = serviceSettings.ServiceName;

                    // comply with current identity standrards, because some claims renamed with legacy version
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
            }
        }
        public void Configure(JwtBearerOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }
}