using Microservices.Web.Frontend.Services.BasketServices;
using Microservices.Web.Frontend.Services.DiscountServices;
using Microservices.Web.Frontend.Services.OrderServices;
using Microservices.Web.Frontend.Services.PaymentServices;
using Microservices.Web.Frontend.Services.ProductServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservices.Web.Frontend
{
    public class Startup
    {

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            this.environment = environment;

        }

        public IConfiguration Configuration { get; }
        private readonly IHostEnvironment environment;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcService = services.AddControllersWithViews();
            if (environment.IsDevelopment())
                mvcService.AddRazorRuntimeCompilation();

            //services.AddScoped<IDiscountService, Services.DiscountServices.DiscountService>();

            services.AddScoped<IDiscountService>(p =>
            {
                return new DiscountServiceRestful(new RestClient(
                    Configuration["MicroservicAddress:ApiGatewayForWeb:Uri"]));
            });

            services.AddScoped<IProductService>(p =>
            {
                return new ProductService(
                    new RestClient(Configuration["MicroservicAddress:ApiGatewayForWeb:Uri"]));
            });

            services.AddScoped<IBasketService>(p =>
            {
                return new BasketService(
                    new RestClient(Configuration["MicroservicAddress:ApiGatewayForWeb:Uri"]));
            }); 
            
            services.AddScoped<IOrderService>(p =>
            {
                return new OrderService(
                    new RestClient(Configuration["MicroservicAddress:ApiGatewayForWeb:Uri"]));
            });   
            
            services.AddScoped<IPaymentService>(p =>
            {
                return new  PaymentService(
                    new RestClient(Configuration["MicroservicAddress:ApiGatewayForWeb:Uri"]));
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
