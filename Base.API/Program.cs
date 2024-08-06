using AutoMapper;
using Base.API.Mapper;
using Base.API.Permission;
using Base.API.Service;
using Base.Repository.Common;
using Base.Repository.Identity;
using Base.Service.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using Hangfire;
using Hangfire.SqlServer;
using static Base.API.Middleware.GlobalExceptionMiddleware;
using Base.API.Middleware;
using Base.Repository;
using Base.Service;
using Base.API.Common;
using CloudinaryDotNet;
using HttpMethod = System.Net.Http.HttpMethod;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using IMailService = Base.Service.Common.IMailService;
using MailService = Base.Service.Common.MailService;
using Base.Repository.Entity;

var builder = WebApplication.CreateBuilder(args);

/*builder.Host.ConfigureServices((context, services) =>
{
    HostConfig.CertPath = context.Configuration["CertPath"]!;
});*/

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80);
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps("/etc/ssl/certs/sams.pfx", "pa55w0rd!");
    });
});

var Configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddRepository(Configuration);
builder.Services.AddService(Configuration);

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});

#region Cloudinary
var cloudinaryConfig = Configuration.GetSection("CloudinaryConfig").Get<CloudinaryConfig>();
Account cloudinaryAccount = new Account
{
    Cloud = cloudinaryConfig.CloudName,
    ApiKey = cloudinaryConfig.ApiKey,
    ApiSecret = cloudinaryConfig.ApiSecret,
};
Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);
#endregion

#region Email Service
var emailConfig = Configuration.GetSection("EmailConfig").Get<Base.Service.Common.EmailConfig>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IMailService, MailService>();
#endregion

builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
builder.Services.AddSingleton<IKeyManager, KeyManager>();

//Add GoogleSheet Service
builder.Services.AddSingleton(typeof(Base.API.Common.GoogleSheetsHelper));

builder.Services.AddIdentityCore<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    // Passord settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 5;
    options.Password.RequiredUniqueChars = 0;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = false;

    //UserName settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";


    options.Tokens.EmailConfirmationTokenProvider = "UserTokenProvider";
    options.Tokens.ChangeEmailTokenProvider = "UserTokenProvider";
    options.Tokens.PasswordResetTokenProvider = "UserTokenProvider";
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IJWTTokenService<IdentityUser<Guid>>, JWTTokenService<IdentityUser<Guid>>>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, HasScopeHandler>();

builder.Services.AddSingleton(provider => new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new RequestToModel(provider.GetService<ICurrentUserService>()!));
    cfg.AddProfile(new ModelToResponse(provider.GetService<WebSocketConnectionManager1>()!));
}).CreateMapper());

builder.Services.AddControllers(options =>
{
    // Add Global Exception Filter here
    //options.Filters.Add<HttpResponseExceptionFilter>();
})
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var errorStrings = new List<string>();
            var modelState = actionContext.ModelState;

            // values is IEnumerable of ModelStateEntry, we need to take error messages from each ModleStateEntry
            // error messages of each ModelStateEntry is stored in Errors property, which will return Collection of ModelError
            var values = modelState.Values;

            foreach (var modelStateEntry in values)
            {
                foreach (var modelError in modelStateEntry.Errors)
                {
                    errorStrings.Add(modelError.ErrorMessage);
                }
            }
            return new BadRequestObjectResult(new
            {
                Status = 400,
                Title = "One or more validation errors occurred",
                Errors = errorStrings
            });
        };
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
        o.JsonSerializerOptions.DictionaryKeyPolicy = null;
    })
    .AddNewtonsoftJson(option =>
    {
        option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie("cookie")
.AddJwtBearer(x =>
{
    var keyManager = new KeyManager();
    var key = keyManager.RsaKey;

    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = Configuration["Jwt:Issuer"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new RsaSecurityKey(key ?? throw new ArgumentException("Key not found for authentication scheme"))
    };

    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var secProtocalHeaders = context.Request.Headers["Sec-WebSocket-Protocol"].ToString();
            var values = secProtocalHeaders.Split(",");
            if (values.First().Trim() == "access_token")
            {
                var accessToken = values.Last().Trim();
                // If the request is for ws client...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/ws/client")))
                {
                    // Read the token out of the request header
                    context.Token = accessToken;
                }
            }
            return Task.CompletedTask;
        }
    };
})
.AddOAuth("google", o =>
{
    o.SignInScheme = "cookie";

    o.ClientId = "118248411285-pj96okif8j20j05g78kbhu5lghd5450l.apps.googleusercontent.com";
    o.ClientSecret = "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";

    o.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
    o.TokenEndpoint = "https://oauth2.googleapis.com/token";
    o.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo";

    o.CallbackPath = "/oauth/google-cb";
    o.SaveTokens = true;

    o.Scope.Add("profile");
    o.Scope.Add("email");

    o.ClaimActions.MapJsonKey("sub", "id");
    o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

    o.Events.OnCreatingTicket = async ctx =>
    {
        // Can get service here
        // ctx.HttpContext.RequestServices<>();
        using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
        using var result = ctx.Backchannel.Send(request);
        var user = await result.Content.ReadFromJsonAsync<JsonElement>();
        ctx.RunClaimActions(user);
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.Requirements.Add(new HasScopeRequirement("Admin", Configuration["Jwt:Issuer"]!));
    });

    options.AddPolicy("Lecturer", policy =>
    {
        policy.Requirements.Add(new HasScopeRequirement("Lecturer", Configuration["Jwt:Issuer"]!));
    }); 

    options.AddPolicy("Student", policy =>
    {
        policy.Requirements.Add(new HasScopeRequirement("Student", Configuration["Jwt:Issuer"]!));
    });

    options.AddPolicy("Admin Lecturer", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return context.User.HasClaim(claim =>

                (claim.Type == "scope") &&
                (
                    claim.Value.Equals("Admin") ||
                    claim.Value.Equals("Lecturer")
                )
            );
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("All", policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

builder.Services.AddHangfire(configuration =>
{
    configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(Configuration.GetConnectionString("MsSQLConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
    });
});

builder.Services.AddHangfireServer();

builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = Configuration["Redis:RedisURL"];
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MapPlatform.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
});

builder.Services.AddSingleton<WebsocketEventManager>();
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<WebSocketConnectionManager1>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<HangfireService>();
builder.Services.AddSingleton<HangfireServiceSingleton>();

var app = builder.Build();

// <snippet_UseWebSocket>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Base.API v1"));
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Base.API v1"));
}

app.UseWebSockets(webSocketOptions);

app.UseMiddleware<WebsocketMiddleware>();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 401)
    {
        // Customize the response for 401 Unauthorized status code
        context.HttpContext.Response.ContentType = "application/json";
        var error = new ErrorDetails()
        {
            StatusCode = context.HttpContext.Response.StatusCode,
            Title = "Unauthorize: You do not have permission to access this resource."
        };
        await context.HttpContext.Response.WriteAsync(error.ToString());
    }

    if (context.HttpContext.Response.StatusCode == 403)
    {
        // Customize the response for 403 Forbidden status code
        context.HttpContext.Response.ContentType = "application/json";
        var error = new ErrorDetails()
        {
            StatusCode = context.HttpContext.Response.StatusCode,
            Title = "Forbidden: You do not have sufficient privileges to access this resource."
        };
        await context.HttpContext.Response.WriteAsync(error.ToString());
    }
});

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseHangfireDashboard("/hangfire");

app.UseStaticFiles();
// app.UseCookiePolicy();

app.UseRouting();
// app.UseRateLimiter();
// app.UseRequestLocalization();

app.UseCors("All");

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();

    endpoints.MapGet("/login-google", context =>
    {
        var linkGenerator = context.RequestServices.GetRequiredService<LinkGenerator>();
        var redirectUri = linkGenerator.GetUriByAction(context, "UserInformation", "TestAuth");

        return AuthenticationHttpContextExtensions
            .ChallengeAsync(context, "google",
            new AuthenticationProperties
            {
                RedirectUri = redirectUri,
            });
    });
});



app.Run();


public static class HostConfig
{
    public static string CertPath { get; set; }
    public static string CertPassword { get; set; }
}