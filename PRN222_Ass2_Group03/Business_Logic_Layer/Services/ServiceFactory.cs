using DataAccess_Layer;
using DataAccess_Layer.Repositories;
using DataAccess_Layer.Repositories.Implement;
using DataAccess_Layer.Repositories.Interface;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Business_Logic_Layer.Services
{
    public class ServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public EVDealerSystemContext CreateDbContext()
        {
            var options = _serviceProvider.GetRequiredService<DbContextOptions<EVDealerSystemContext>>();
            return new EVDealerSystemContext(options);
        }

        public IUserRepository CreateUserRepository()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<UserRepository>>();
            return new UserRepository(logger);
        }

        public IOrderRepository CreateOrderRepository()
        {
            var context = CreateDbContext();
            return new OrderRepository(context);
        }

        public ICustomerTestDriveAppointment CreateCustomerTestDriveAppointmentRepository()
        {
            var context = CreateDbContext();
            return new CustomerTestDriveAppointment(context);
        }

        public IUserService CreateUserService()
        {
            var userRepository = CreateUserRepository();
            var logger = _serviceProvider.GetRequiredService<ILogger<UserService>>();
            return new UserService(userRepository, logger);
        }

        public IOrderService CreateOrderService()
        {
            var orderRepository = CreateOrderRepository();
            return new OrderService(orderRepository);
        }

        public ICustomerTestDriveAppointmentService CreateCustomerTestDriveAppointmentService()
        {
            var appointmentRepository = CreateCustomerTestDriveAppointmentRepository();
            return new CustomerTestDriveAppointmentService(appointmentRepository);
        }

        public IVehicleService CreateVehicleService()
        {
            var context = CreateDbContext();
            return new VehicleService(context);
        }
    }
}
