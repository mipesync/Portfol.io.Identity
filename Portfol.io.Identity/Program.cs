using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Portfol.io.Identity.Common.Mappings;
using Portfol.io.Identity.Common.Middleware;
using Portfol.io.Identity.Common.Services;
using Portfol.io.Identity.Common.TokenIssue;
using Portfol.io.Identity.Data;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.Repositories;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<AppIdentityContext>(options =>
{
    options.UseNpgsql(connectionString, p =>
    {
        p.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    });
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppIdentityContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = JwtOptions.AUDIENCE,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtOptions.GetSymmetricSecurityKey()
        };
    })//TODO: Разобраться с реализацией и добавить остальные сервисы
    .AddOAuth("VK", "OAuth-VK", config =>
    {
        config.ClientId = builder.Configuration["OAuth-VK:ClientId"];
        config.ClientSecret = builder.Configuration["OAuth-VK:ClientSecret"];
        config.ClaimsIssuer = "OAuth-VK";
        config.CallbackPath = "/callback";
        config.AuthorizationEndpoint = "https://oauth.vk.com/authorize";
        config.TokenEndpoint = "https://oauth.vk.com/access_token";
        config.Scope.Add("email");
        config.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
        config.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        /*config.Events = new OAuthEvents
        {
            OnRemoteFailure = context =>
            {

                Console.WriteLine(context.Failure);
                return Task.CompletedTask;
            },
            OnCreatingTicket = context =>
            {
                context.RunClaimActions(context.TokenResponse.Response!.RootElement);
                return Task.CompletedTask;
            }
        };*/
    });

// NOTE: Корсы потом норм сделать
builder.Services.AddCors(options => options.AddPolicy("AllowAllOrigins", builder =>
{
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
    builder.AllowAnyOrigin();
}));

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(config =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    config.IncludeXmlComments(xmlPath);

    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
            },
            new string[] { }
        }
    });
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<ITokenManager, TokenManager>();
builder.Services.AddAutoMapper(u => u.AddProfile(new AssemblyMappingProfile(Assembly.GetExecutingAssembly())));
builder.Services.AddTransient<IFileUploader, FileUploader>();

builder.Services.AddTransient<IAuthRepository, AuthRepository>();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandlerMiddleware();
}

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<AppIdentityContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception e)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(e, $"An error occured while initializing the database: {e.Message}");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
