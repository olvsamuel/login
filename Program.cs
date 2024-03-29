using login.Filters;
using login.APIBehavior;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(5000));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(MyExceptionFilter));
    options.Filters.Add(typeof(ParseBadRequest));
}).ConfigureApiBehaviorOptions(BadRequestsBehavior.Parse);

//builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        // ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Settings.Issuer,
        ValidAudience = Settings.JWTAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Settings.Secret.ToString())),
        ClockSkew = TimeSpan.Zero
    };
    x.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            DateTime expiration = context.SecurityToken.ValidTo;
            // Console.WriteLine(context.SecurityToken);
            if (expiration < DateTime.UtcNow)
            {
                context.Fail("Token has expired.");
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options => options.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders(["totalAmountOfRecords"]); //for pagination, if wanted
}));

builder.Services.AddResponseCaching();


var app = builder.Build();

app.Use(async (context, next) =>
{
    using (var swapStream = new MemoryStream())
    {
        var originalResponseBody = context.Response.Body;
        context.Response.Body = swapStream;

        await next.Invoke();

        swapStream.Seek(0, SeekOrigin.Begin);
        string responseBody = new StreamReader(swapStream).ReadToEnd();
        swapStream.Seek(0, SeekOrigin.Begin);

        await swapStream.CopyToAsync(originalResponseBody);
        context.Response.Body = originalResponseBody;

        ILogger logger = context.RequestServices.GetRequiredService<ILogger<StartupBase>>();
        
        logger.LogInformation(responseBody);
    }
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
