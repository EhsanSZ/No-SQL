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
using PaymentService.Application.Contexts;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.MessagingBus.Configs;
using PaymentService.Infrastructure.MessagingBus.ReceivedMessage.GetPaymetMessages;
using PaymentService.Infrastructure.MessagingBus.SendMessages;
using PaymentService.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.EndPoint
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService.EndPoint", Version = "v1" });
            });

            services.AddTransient<IPaymentDataBaseContext, PaymentDataBaseContext>();
            services.AddDbContext<PaymentDataBaseContext>(p =>
            p.UseSqlServer(Configuration["PaymentConnection"]),ServiceLifetime.Singleton);

            services.AddTransient<IPaymentService, PaymentServiceConcrete>();
            services.AddHostedService<RecievedMessagePaymentForOrder>();
            services.Configure<RabbitMqConfiguration>
                (Configuration.GetSection("RabbitMq"));
            services.AddTransient<IMessageBus, RabbitMQMessageBus>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService.EndPoint v1"));
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
