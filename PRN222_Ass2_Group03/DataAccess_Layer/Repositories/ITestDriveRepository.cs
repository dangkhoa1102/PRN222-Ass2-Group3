using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess_Layer.Repositories
{
    public interface ITestDriveRepository
    {
        public Task<TestDriveAppointment> GetTestDriveByIdAsync(Guid id);
        public Task<List<TestDriveAppointment>> GetTestDrivesByDealerIdAsync(Guid dealerId);
        public Task<List<TestDriveAppointment>> GetTestDrivesByCustomerIdAsync(Guid customerId);
        public Task<List<TestDriveAppointment>> GetPendingTestDrivesAsync();
        public Task<List<TestDriveAppointment>> GetAllTestDrivesAsync();
        public Task<bool> ConfirmTestDriveAsync(Guid id);
        public Task<bool> CancelTestDriveAsync(Guid id,string notes);

    }
}
