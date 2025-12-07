using Application;
using Application.Common;
using Application.Interface.IService;
using infrastructure.ExternalServices;
using infrastructure.Hubs;
using infrastucure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.Middleware;
using Presentation.Providers;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // just check dist file adding or not 

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            // builder.Services.AddOpenApi();

            # region token and reguest checking
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserProvider>();
            #endregion

            builder.Services.AddSignalR();

            #region Bridge between Application and Presentation
            builder.Services.AddService();
            #endregion

            #region Bridge between Infrastructure and Presentation
            builder.Services.AddInfrastructure(builder.Configuration);
            #endregion

            #region Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Presentation", Version = "v1" });

                // 1. Definition: Tells Swagger "We use JWT Bearer tokens"
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter 'Bearer [space] and then your token'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // 2. Requirement: Tells Swagger "Apply this security to all endpoints"
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
            });
            #endregion

            #region Photo Upload
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            #endregion










            builder.Services.AddSingleton<SmtpClient>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();

                var smtpHost = config["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(config["EmailSettings:SmtpPort"]);
                var senderEmail = config["EmailSettings:SenderEmail"];
                var senderPassword = config["EmailSettings:SenderPassword"];

                var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                return smtpClient;
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var tokenKey = builder.Configuration["TokenKey"] ?? throw new InvalidOperationException("TokenKey not configured.");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            builder.Services.AddCors();

            var app = builder.Build();



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                    options.RoutePrefix = string.Empty;
                });


            }

            app.UseMiddleware<ExceptionMiddleware>();  // (MIDDLEWARE) Error Handling (First)

            app.UseHttpsRedirection();


            app.UseCors(x =>      // CORS (Before Auth)
            x.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:4200", "http://localhost:5278"));

            app.UseAuthentication();
            app.UseAuthorization();
         

            app.MapControllers();

            app.MapHub<NotificationHub>("/notificationHub");
            app.Run();
        }
    }
}
