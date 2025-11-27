using Application;
using Application.Interface.IAuth;
using Application.Interface.IRepo;
using Application.Services;
using infrastructure.Hubs;
using infrastructure.Repositary;
using infrastucure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
            builder.Services.AddSignalR();

            #region Bridge between Application and Presentation
            builder.Services.AddService();
            #endregion

            #region Bridge between Infrastructure and Presentation
            builder.Services.AddInfrastructure(builder.Configuration);
            #endregion

            #region Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
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

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseAuthentication();


            app.UseCors(x =>
            x.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:4200", "http://localhost:5278"));


            app.MapControllers();

            app.MapHub<NotificationHub>("/notificationHub");
            app.Run();
        }
    }
}
