using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OrderService.Infrastructure.Context;
using OrderService.MessagingBus;
using OrderService.MessagingBus.RecievedMessages;
using OrderService.MessagingBus.SendMessage;
using OrderService.Model.Services;
using OrderService.Model.Services.ProductServices;
using OrderService.Model.Services.RegisterOrderServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" });
            });
            services.AddDbContext<OrderDataBaseContext>(o => o.UseSqlServer
                (Configuration["OrderConnection"]) , ServiceLifetime.Singleton);

            services.AddTransient<IOrderService, OrderService.Model.Services.OrderService>();

            services.Configure<RabbitMqConfiguration>(Configuration.GetSection("RabbitMq"));

            services.AddHostedService<RecievedOrderCreatedMessage>();
            services.AddHostedService<RecievedPaymentOfOrderService>();
            services.AddHostedService<ReceivedUpdateProductNameMessage>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IRegisterOrderService, RegisterOrderService>();
            services.AddTransient<IMessageBus, RabbitMQMessageBus>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
