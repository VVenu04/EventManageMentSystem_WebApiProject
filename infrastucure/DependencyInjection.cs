using Application.Interface.IGenericRepo;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Services;
using Domain.Entities;
using infrastructure.Repositary;
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
           services.AddScoped<IAuthRepository, AuthRepository>();
           services.AddScoped<ICustomerRepo, CustomerRepository>();
           services.AddScoped<IVendorRepo, VendorRepository>();
           services.AddScoped<IEventRepo, EventRepository>();
           services.AddScoped<IFunctionRepo, FunctionRepository>();


            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository > ();
            //services.AddScoped<IVendorRepository, VendorRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            var connectionString = configuration.GetConnectionString("Smart");

            services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(connectionString));

            return services;
        }
    }
}
