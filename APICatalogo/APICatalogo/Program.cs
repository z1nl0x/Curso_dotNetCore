using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using APICatalogo.Context;
using APICatalogo.Domains;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
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

// DEFININDO UMA POLITICA CORS E NOMEANDO-A
builder.Services.AddCors(options =>
    options.AddPolicy("OrigensComAcessoPermitido",
        policy =>
        {
            policy.WithOrigins("https://apirequest.io", "http://apirequest.io")
                  .AllowAnyHeader()
                  .WithMethods("GET", "POST");
        })
);


// DEFININDO UMA POLITICA CORS DEFAULT
// builder.Services.AddCors(options =>
//     options.AddDefaultPolicy(
//         policy =>
//         {
//             policy.WithOrigins("https://apirequest.io", "http://apirequest.io")
//                   .AllowAnyHeader()
//                   .AllowAnyMethod();
//         })
// );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "APICatalogo",
        Version = "v1",
        Description = "Catálogo de Produtos e Categorias"
    });

    // Registra o esquema de segurança JWT Bearer (botão "Authorize" no Swagger UI).
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (o prefixo 'Bearer ' é adicionado automaticamente)."
    });

    // Exige o esquema Bearer nos endpoints protegidos.
    // No OpenApi.NET v2 a referência é feita via OpenApiSecuritySchemeReference.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
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

builder.Services.AddRateLimiter(rateLimitedOptions =>
{
    rateLimitedOptions.AddFixedWindowLimiter(policyName: "fixedwindow", options =>
    {
        options.PermitLimit = 1;
        options.Window = TimeSpan.FromSeconds(5);
        options.QueueLimit = 0;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    rateLimitedOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "APICatalogo v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.UseCors("OrigensComAcessoPermitido");
// app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();