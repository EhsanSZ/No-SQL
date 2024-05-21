using BasketService.Infrastructure.Contexts;
using BasketService.Infrastructure.MappingProfile;
using BasketService.MessageingBus;
using BasketService.MessageingBus.ReceivedMessages.ProductMessages;
using BasketService.Model.Services;
using BasketService.Model.Services.DiscountServices;
using BasketService.Model.Services.ProductServices;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasketService
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BasketService", Version = "v1" });
            });
            services.AddDbContext<BasketDataBaseContext>(o => o.UseSqlServer
            (Configuration["BasketConnection"]),ServiceLifetime.Singleton);

            services.AddAutoMapper(typeof(BasketMappingProfile));

            services.AddTransient<IBasketService, BasketService.Model.Services.BasketService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IDiscountService, BasketService.Model.Services.DiscountServices.DiscountService>();
            services.Configure<RabbitMqConfiguration>(Configuration.GetSection("RabbitMq"));


            services.AddTransient<IMessageBus, RabbitMQMessageBus>();

            services.AddHostedService<ReceivedUpdateProductNameMessage>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BasketService v1"));
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
