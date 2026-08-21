using System.Text;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// dotnet tool update --global dotnet-ef --version 9.0.19 --allow-downgrade  

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof (ApiExceptionFilter));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}).AddNewtonsoftJson();
    
builder.Services.AddOpenApi(options =>
{
    // No .NET 10 (OpenApi.NET v2) o AddOpenApi NÃO declara nenhum
    // security scheme sozinho. Precisamos adicionar o Bearer via
    // document transformer pro Scalar saber que existe autenticação JWT.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Informe apenas o token JWT (o prefixo 'Bearer ' é adicionado automaticamente)."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = bearerScheme;

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

string mySqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString)));

var secretKey = builder.Configuration["JWT:SecretKey"]
    ?? throw new ArgumentException("Invalid secret key!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    
    options.AddPolicy("SuperAdminOnly", policy => 
        policy.RequireRole("Admin", "SuperAdmin").RequireClaim("id", "kreft"));
    
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    
    options.AddPolicy("ExclusivePolicyOnly", policy => policy.RequireAssertion(context => context.User.HasClaim(Claim => 
        Claim.Type == "id" && Claim.Value == "kreft") || context.User.IsInRole("SuperAdmin")));
});

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnityOfWork>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<ProdutoDTOMappingProfile>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        // Deixa o Bearer/JWT já selecionado no painel de autenticação
        options.AddPreferredSecuritySchemes("Bearer");
        // Mantém o token salvo no navegador entre reloads (localStorage)
        options.EnablePersistentAuthentication();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();