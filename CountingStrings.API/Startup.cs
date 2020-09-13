using System;
using System.Linq;
using System.Security.Claims;
using CountingStrings.API.Auth;
using CountingStrings.API.Contract;
using CountingStrings.API.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NServiceBus;
using NSwag;


namespace CountingStrings.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Auth

            string domain = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = Configuration["Auth0:ApiIdentifier"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("write:sessions", policy =>
                policy.Requirements.Add(new HasScopeRequirement("write:sessions", domain)));
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();


            #endregion

            #region Bus

            var endpointConfiguration = new EndpointConfiguration("CountingStrings.API");
            endpointConfiguration.SendOnly();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq-countingstringsbroker;username=guest;password=MTk5MTgzNjc4OWNQLjE=");
            transport.UseConventionalRoutingTopology();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(OpenSession).Assembly, "CountingStrings.Service");

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpoint);

            #endregion

            #region Swagger

            services.AddOpenApiDocument(document =>
            {
                document.AddSecurity("oauth2", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Name = "Authorization",
                    Description = "Integration to Auth0",
                    Flow = OpenApiOAuth2Flow.Implicit,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl =
                            $"https://{Configuration["Auth0:Domain"]}/authorize?audience={Configuration["Auth0:ApiIdentifier"]}",
                            TokenUrl = Configuration["Auth0:ApiIdentifier"]
                        }
                    },
                    In = OpenApiSecurityApiKeyLocation.Header,

                });
            });


            #endregion

            services.AddTransient<ICountingStringsRepository, CountingStringsRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi(settings =>
            {
                settings.PostProcess = (document, request) =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "CountingStrings";
                    document.Info.Description = "A stock-take application";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Cristian Perez Matturro",
                        Email = "crperez.informatica@gmail.com",
                        Url = "https://crperez.dev"
                    };
                };
            });

            app.UseSwaggerUi3();

            app.Use(async (ctx, next) =>
            {
                Console.WriteLine("A new request has been receivied.");

                var bus = ctx.RequestServices.GetService<IMessageSession>();
                await bus.Send(new LogRequest
                {
                    Id = Guid.NewGuid(),
                    RequestDate = DateTime.UtcNow
                });

                await next();
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
