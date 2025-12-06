using Application.Interface.IGenericRepo;
using Application.Interface.IRepo;
using Application.Interface.IService;
using infrastructure;
using infrastructure.GenericRepositary;
using infrastructure.Repositary;
using infrastucure.Data;
using infrastucure.Repositary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
           


   
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IServiceItemRepository, ServiceItemRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();


            services.AddScoped<IPackageRequestRepository, PackageRequestRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IRealTimeNotifier, SignalRNotifier>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            var connectionString = configuration.GetConnectionString("Smart");

            services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(connectionString));

            return services;
        }
    }
}
