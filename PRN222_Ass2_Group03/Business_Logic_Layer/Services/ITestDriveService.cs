using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public interface ITestDriveService 
    {
        public Task<TestDriveAppointment?> GetTestDriveByIdAsync(Guid id);
        public Task<List<TestDriveAppointment>> GetPendingTestDrivesAsync();
        public Task<List<TestDriveAppointment>> GetAllTestDrivesAsync();
        public Task<bool> ConfirmTestDriveAsync(Guid id);
        public Task<bool> CancelTestDriveAsync(Guid id, string notes);
    }
}
