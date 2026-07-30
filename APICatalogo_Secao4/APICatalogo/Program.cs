using System.Text.Json.Serialization;
using APICatalogo.Context;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
// builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof (ApiExceptionFilter));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
    
builder.Services.AddOpenApi();


// var valor1 = builder.Configuration["chave1"];
// var valor2 = builder.Configuration["chave2"];

string mySqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString)));

builder.Services.AddScoped<ApiLoggingFilter>();
//
// builder.Services.AddTransient<IMeuServico, MeuServico>();
//
// builder.Services.Configure<ApiBehaviorOptions>(options =>
// {
//     options.DisableImplicitFromServicesParameters = true;
// });

// builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration()
// {
//     LogLevel = LogLevel.Information,
// }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => 
        options.SwaggerEndpoint("/openapi/v1.json", "Catalogo API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// app.Use(async (context, next) =>
// {
//     // adicionar o codigo antes do request
//     await next();
//     // adicionar o codigo depois do request
// });

app.MapControllers();

// app.Run(async (context) =>
// {
//     await context.Response.WriteAsync("Middleware final");
// });

app.Run();