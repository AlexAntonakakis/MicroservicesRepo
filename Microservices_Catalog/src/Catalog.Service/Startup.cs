
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Catalog.Service.Entities;
using Common.Settings;
using Common.MongoDB;
using Common.MassTransit;
using Common.Service.Settings;
using Common.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Catalog.Service
{
    public class Startup
    {
        private const string AllowedOriginSetting = "AllowedOrigin";
        private ServiceSettings serviceSettings;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            serviceSettings = Configuration.GetSection(nameof(serviceSettings)).Get<ServiceSettings>();

            services.AddMongo()
                    .AddMongoRepository<Item>("items")
                    .AddMassTransitWithRabbitMq()
                    .AddJwtBearerAuthentication();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Read, policy => 
                {
                    policy.RequireRole("Admin");
                    policy.RequireClaim("scope", "catalog.readaccess", "catalog.fullaccess");
                });

                options.AddPolicy(Policies.Write, policy => 
                {
                    policy.RequireRole("Admin");
                    policy.RequireClaim("scope", "catalog.writeaccess", "catalog.fullaccess");
                });
            });

            //services.AddMassTransitHostedService(); can be ignored for latest versions of masstransit

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.Service v1"));
                app.UseCors(builder => {
                    builder.WithOrigins(Configuration[AllowedOriginSetting])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

             app.UseAuthentication(); //between routing and authorization

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
