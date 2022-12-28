using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Identity;
using Common.MongoDB;
using Common.MassTransit;
using Common.Service.Settings;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Trading.Service.StateMachines;
using Play.Trading.Service.Exceptions;
using System.Text.Json.Serialization;
using Play.Trading.Service.Entities;
using System.Reflection;
using GreenPipes;
using Play.Trading.Service.Settings;
using Inventory.Contracts;
using MyIdentity.Contracts;
using Play.Trading.Service.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Play.Trading.Service
{
    public class Startup
    {
        private const string AllowedOriginSetting = "AllowedOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo()
                .AddMongoRepository<CatalogItem>("catalogitems")
                .AddMongoRepository<InventoryItem>("Inventoryitems")
                .AddMongoRepository<ApplicationUser>("users")
                .AddJwtBearerAuthentication();

            AddMassTransit(services);

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;  // to keep "Async" suffix in controller's actions 
            })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            );



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Trading.Service", Version = "v1" });
            });

            services.AddSingleton<IUserIdProvider, UserIdProvider>()
                    .AddSingleton<MessageHub>()
                    .AddSignalR();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Trading.Service v1"));

                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedOriginSetting])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/messagehub");
            });
        }

        private void AddMassTransit(IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                configure.UsingPlayEconomyRabbitMq(retryConfigurator =>
                {
                    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                    retryConfigurator.Ignore(typeof(UnknownItemException));
                });

                configure.AddConsumers(Assembly.GetEntryAssembly()); // Register MassTransit Consumers
                configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
                {
                    sagaConfigurator.UseInMemoryOutbox();  // no message will be sent from saga pipeline until we have transition into the state in the database
                })
                    .MongoDbRepository(r => 
                    {
                        var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).
                                                            Get<ServiceSettings>();
                        var mongoSettings = Configuration.GetSection(nameof(MongoDbSettings)).
                                                            Get<MongoDbSettings>();

                        r.Connection = mongoSettings.ConnectionString;            
                        r.DatabaseName = serviceSettings.ServiceName;                            
                    });
            });
            
            var queueSettings = Configuration.GetSection(nameof(QueueSettings)).
                                                            Get<QueueSettings>();
            EndpointConvention.Map<GrantItems>(new System.Uri(queueSettings.GrantItemsQueueAddress));
            EndpointConvention.Map<DebitGil>(new System.Uri(queueSettings.DebitGilQueueAddress));
            EndpointConvention.Map<SubstractItems>(new System.Uri(queueSettings.SubstractQueueAddress));
            
            services.AddMassTransitHostedService();
            services.AddGenericRequestClient();
        }
    }
}
