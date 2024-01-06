using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RabbitMQ_Messenger_Lib.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore;
using Microsoft.OpenApi.Models;
using Tenant_service.AuthConfig;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<TenantContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TenantDatabase"), b =>
    {
        b.MigrationsAssembly("Tenant-service");
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "You api title", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
});

// CORS Configuration
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Auth0 Services
var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = domain;
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("read:current_user", policy => policy.Requirements.Add(new Tenant_service.AuthConfig.HasScopeRequirement("read:current_user", domain)));
});

builder.Services.AddSingleton<IAuthorizationHandler, Tenant_service.AuthConfig.HasScopeHandler>();
builder.Services.AddSingleton(new ManagementAPIConfig() { Audiance = builder.Configuration["Auth0:Audiance"], Domain = builder.Configuration["Auth0:Domain"], ClientID = builder.Configuration["Auth0:ManagementAPI:ClientID"], ClientSecret = builder.Configuration["Auth0:ManagementAPI:ClientSecret"] });


// Messenger configuration
MessengerConfig messengerConfig = new MessengerConfig { HostName = builder.Configuration["MessageBus:Host"], Exchange = builder.Configuration["MessageBus:Exchange"] };
builder.Services.AddSingleton(messengerConfig);

var app = builder.Build();

// EF migration
try
{
    using (IServiceScope serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
    {
        DbContext context = serviceScope.ServiceProvider.GetRequiredService<TenantContext>();
        context.Database.Migrate();
    }
}
catch (Exception e)
{
    Console.WriteLine("An error occured during EF Migration, migration aborted");
    Console.WriteLine(e.Message);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
