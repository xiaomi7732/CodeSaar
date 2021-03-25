using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to help registering JWT auth related services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static JWTAuthOptions _jwtAuthOptionsCache = null;

        /// <summary>
        /// Add JWT authentication related services.
        /// </summary>
        public static IServiceCollection AddJWTAuth(this IServiceCollection services, Action<JWTAuthOptions> configure = null)
        {
            services.AddOptions<JWTAuthOptions>().BindConfiguration(JWTAuthOptions.SectionName).Configure<ILogger<JWTAuthOptions>>((options, logger) =>
            {
                configure?.Invoke(options);

                // Quick check for IssuerSigningSecret
                if (string.IsNullOrEmpty(options.IssuerSigningSecret))
                {
                    logger?.LogWarning("Issuer signing secret is not specified. Using random string as secrets temperory. Please specify issuer signing secret.");
                    options.IssuerSigningSecret = Guid.NewGuid().ToString();
                }

                // TODO: Is it a good idea to keep it in the static field like this?
                _jwtAuthOptionsCache = options;
            });

            services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptionsCache.IssuerSigningSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtAuthOptionsCache.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAuthOptionsCache.Audience,
                    NameClaimType = _jwtAuthOptionsCache.NameClaimType,
                };
                if (!string.Equals(_jwtAuthOptionsCache.RoleClaimType, "role", StringComparison.OrdinalIgnoreCase))
                {
                    opt.TokenValidationParameters.RoleClaimType = _jwtAuthOptionsCache.RoleClaimType;
                }

                if (_jwtAuthOptionsCache.OnJWTAuthenticationMessageReceived != null)
                {
                    opt.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = _jwtAuthOptionsCache.OnJWTAuthenticationMessageReceived,
                    };
                }
            });

            return services;
        }
    }
}