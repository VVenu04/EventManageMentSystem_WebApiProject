using Application.Interface.IAuth;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class DenpendencyInjenction
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IVendorService, VendorService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IFunctionService, FunctionService>();
            services.AddScoped<IBookingService, BookingService>();
            //services.AddScoped<IVendorService, VendorService>();
            //services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
