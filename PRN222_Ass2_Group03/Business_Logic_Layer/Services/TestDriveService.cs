using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly ITestDriveRepository _testDriveRepository;
        private readonly ILogger<TestDriveService> _logger;

        public TestDriveService(ITestDriveRepository testDriveRepository, ILogger<TestDriveService> logger)
        {
            _testDriveRepository = testDriveRepository;
            _logger = logger;
        }

        public async Task<bool> CancelTestDriveAsync(Guid id, string notes)
        {
           var result = await _testDriveRepository.CancelTestDriveAsync(id, notes);
            if (result)
            {
                _logger.LogInformation("Test drive cancelled successfully - ID: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Failed to cancel test drive - ID: {Id}", id);
            }
            return result;
        }

        public async Task<bool> ConfirmTestDriveAsync(Guid id, string notes)
        {
            try
            {
                var result = await _testDriveRepository.ConfirmTestDriveAsync(id);

                if (result)
                {
                    _logger.LogInformation("Test drive confirmed successfully - ID: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to confirm test drive - ID: {Id}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming test drive: {Id}", id);
                throw;
            }
        }

        public Task<bool> ConfirmTestDriveAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async  Task<List<TestDriveAppointment>> GetAllTestDrivesAsync()
        {
           return await _testDriveRepository.GetAllTestDrivesAsync();
        }

        public Task<List<TestDriveAppointment>> GetPendingTestDrivesAsync()
        {
           return _testDriveRepository.GetPendingTestDrivesAsync();
        }

        public async Task<TestDriveAppointment?> GetTestDriveByIdAsync(Guid id)
        {
           return await _testDriveRepository.GetTestDriveByIdAsync(id);
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetTestDrivesByCustomerIdAsync(Guid customerId)
        {
            return await _testDriveRepository.GetTestDrivesByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetTestDrivesByDealerIdAsync(Guid dealerId)
        {
            return await _testDriveRepository.GetTestDrivesByDealerIdAsync(dealerId);
        }

      
    }
}
