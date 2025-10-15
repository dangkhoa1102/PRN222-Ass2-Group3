using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess_Layer.Repositories
{
    public class TestDriveRepository : ITestDriveRepository
    {
        private readonly EVDealerSystemContext _context;
        public TestDriveRepository(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<bool> CancelTestDriveAsync(Guid id, string notes)
        {
            var testDrive = await _context.TestDriveAppointments.FirstOrDefaultAsync(td => td.Id == id);
            if (testDrive == null)
            {
                return false;
            }
            testDrive.Status = "Cancelled";
            if(!string.IsNullOrEmpty(notes))
            {
                testDrive.Notes = notes;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmTestDriveAsync(Guid id)
        {
            var testDrive = await _context.TestDriveAppointments.FirstOrDefaultAsync(td => td.Id == id);
            if(testDrive == null)
            {
                return false;
            }
            testDrive.Status = "Confirmed";
          
            await _context.SaveChangesAsync();
            return true;
        }

       

        public async Task<List<TestDriveAppointment>> GetAllTestDrivesAsync()
        {
            return await _context.TestDriveAppointments
                .Include(td => td.Customer)
                .Include(td => td.Dealer)
                .Include(td => td.Vehicle)
                .ToListAsync();
        }

        public async Task<List<TestDriveAppointment>> GetPendingTestDrivesAsync()
        {
            return await _context.TestDriveAppointments
                .Include(td => td.Customer)
                .Include(td => td.Dealer)
                .Include(td => td.Vehicle)
                .Where(td => td.Status == "Pending")
                .ToListAsync();
        }

        public async Task<TestDriveAppointment?> GetTestDriveByIdAsync(Guid id)
        {
            return await _context.TestDriveAppointments
                .Include(td => td.Customer)
                .Include(td => td.Dealer)
                .Include(td => td.Vehicle)
                .FirstOrDefaultAsync(td => td.Id == id);
        }

        public async Task<List<TestDriveAppointment>> GetTestDrivesByCustomerIdAsync(Guid customerId)
        {
            return await _context.TestDriveAppointments
                 .Include(td => td.Customer)
                 .Include(td => td.Dealer)
                 .Include(td => td.Vehicle)
                 .Where(td => td.CustomerId == customerId)
                 .ToListAsync();
        }

        public async Task<List<TestDriveAppointment>>GetTestDrivesByDealerIdAsync(Guid dealerId)
        {
            return await _context.TestDriveAppointments
                 .Include(td => td.Customer)
                 .Include(td => td.Dealer)
                 .Include(td => td.Vehicle)
                 .Where(td => td.DealerId == dealerId)
                 .ToListAsync();
        }
    }
}
