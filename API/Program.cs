using DAL;
using Domain.VM;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.Implemt;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var conf_builder = new ConfigurationBuilder();

conf_builder.SetBasePath(Directory.GetCurrentDirectory());
conf_builder.AddJsonFile("security.json");
var config = conf_builder.Build();

var connection = config.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config.GetSection("Issuer").Value,
                ValidAudience = config.GetSection("Audience").Value,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(config.GetSection("Secret").Value))
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = async context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        var refreshToken = context.HttpContext.Request.Cookies["refreshToken"];
                        var email = context.HttpContext.Request.Cookies["email"];

                        var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();

                        var response = tokenService.ValidateRefreshTokenAsync(new RefreshTokenVM() 
                        {
                            RefreshToken = refreshToken, 
                            Email = email
                        }).Result;

                        if (response.StatusCode == Domain.enums.StatusCode.Ok)
                        {
                            var refreshedAccessToken = await tokenService.RefreshAccessToken(new RefreshTokenVM() 
                            {
                                RefreshToken = refreshToken, 
                                Email = email
                            });

                            if (!string.IsNullOrEmpty(refreshedAccessToken))
                            {
                                context.Response.Headers.Add("Authorization", $"Bearer {refreshedAccessToken}");
                            }   
                        }
                    }
                },
                OnTokenValidated = async context  => {
                    Console.WriteLine("sfsdfsdfsdfsd");
                },
                OnForbidden = async constext => {
                    Console.WriteLine("Forbidden");
                }
            };
        });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
