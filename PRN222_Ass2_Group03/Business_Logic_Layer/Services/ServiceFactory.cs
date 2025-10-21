using DataAccess_Layer.Repositories;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Business_Logic_Layer.Services
{
    public class ServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ServiceFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        private EVDealerSystemContext CreateDbContext()
        {
            // Create a new context instance with connection string
            var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EVDealerSystemContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("MyDbConnection"));
            return new EVDealerSystemContext(optionsBuilder.Options);
        }

        public IUserService CreateUserService()
        {
            var context = CreateDbContext();
            var userRepository = new UserRepository(context, _loggerFactory.CreateLogger<UserRepository>());
            var logger = _loggerFactory.CreateLogger<UserService>();
            return new UserService(userRepository, logger);
        }

        public IOrderService CreateOrderService()
        {
            var context = CreateDbContext();
            var orderRepository = new OrderRepository(context);
            return new OrderService(orderRepository);
        }

        public ICustomerTestDriveAppointmentService CreateCustomerTestDriveAppointmentService()
        {
            var context = CreateDbContext();
            var appointmentRepository = new CustomerTestDriveAppointment(context);
            return new CustomerTestDriveAppointmentService(appointmentRepository);
        }

        public IVehicleService CreateVehicleService()
        {
            var context = CreateDbContext();
            return new VehicleService(context);
        }

        public IDealerService CreateDealerService()
        {
            var context = CreateDbContext();
            return new DealerService(context);
        }
    }
}
