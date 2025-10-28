using Application.Interface.IGenericRepo;
using Application.Interface.IRepo;
using infrastucure.Data;
using infrastucure.GenericRepositary;
using infrastucure.Repositary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastucure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
           services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
           services.AddScoped<IAdminRepo, AdminRepositary>();
            //services.AddScoped<IVendorRepository, VendorRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            var connectionString = configuration.GetConnectionString("New");

            services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(connectionString));

            return services;
        }
    }
}
