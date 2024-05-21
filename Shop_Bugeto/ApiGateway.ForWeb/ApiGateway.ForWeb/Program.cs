using ApiGateway.ForWeb.Models.DiscountServices;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IDiscountService, ApiGateway.ForWeb.Models.DiscountServices.DiscountService>();

IConfiguration configuration = builder.Configuration;
IWebHostEnvironment webHostEnvironment = builder.Environment;

builder.Configuration.SetBasePath(webHostEnvironment.ContentRootPath)
    .AddJsonFile("ocelot.json")
    .AddOcelot(webHostEnvironment)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(configuration);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); ;
});

app.UseOcelot().Wait();
app.Run();
