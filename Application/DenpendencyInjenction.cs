using Application.Interface.IService;
using Application.Service;
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
            //services.AddScoped<IVendorService, VendorService>();
            //services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
