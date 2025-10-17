using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess_Layer.Repositories
{
    public class CustomerTestDriveAppointment : ICustomerTestDriveAppointment
    {
        private readonly EVDealerSystemContext _context;

        public CustomerTestDriveAppointment(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<TestDriveAppointment?> GetByIdAsync(Guid id)
        {
            return await _context.TestDriveAppointments
                .Include(t => t.Customer)
                .Include(t => t.Dealer)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.TestDriveAppointments
                .Include(t => t.Dealer)
                .Include(t => t.Vehicle)
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetAvailableAppointmentsAsync(Guid dealerId, DateTime date)
        {
            var timeSlots = new List<TimeSpan>
            {
                new TimeSpan(9, 0, 0),  // 9:00 AM
                new TimeSpan(10, 0, 0), // 10:00 AM
                new TimeSpan(11, 0, 0), // 11:00 AM
                new TimeSpan(14, 0, 0), // 2:00 PM
                new TimeSpan(15, 0, 0), // 3:00 PM
                new TimeSpan(16, 0, 0)  // 4:00 PM
            };

            var appointments = await _context.TestDriveAppointments
                .Where(t => t.DealerId == dealerId && t.AppointmentDate.Date == date.Date)
                .Select(t => new { t.AppointmentDate, t.Status })
                .ToListAsync();

            var availableSlots = timeSlots
                .Where(slot => !appointments.Any(a =>
                    a.AppointmentDate.TimeOfDay == slot &&
                    (a.Status == "Confirmed")))
                .Select(slot => new TestDriveAppointment
                {
                    AppointmentDate = date.Date.Add(slot),
                    Status = "Available"
                })
                .ToList();

            return availableSlots;
        }

        public async Task<TestDriveAppointment> CreateAsync(TestDriveAppointment appointment)
        {
            _context.TestDriveAppointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<TestDriveAppointment> UpdateAsync(TestDriveAppointment appointment)
        {
            _context.Entry(appointment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var appointment = await _context.TestDriveAppointments.FindAsync(id);
            if (appointment == null) return false;

            _context.TestDriveAppointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(Guid dealerId, DateTime appointmentDate, TimeSpan startTime)
        {
            var conflict = await _context.TestDriveAppointments
                .AnyAsync(t => t.DealerId == dealerId &&
                              t.AppointmentDate.Date == appointmentDate.Date &&
                              t.AppointmentDate.TimeOfDay == startTime &&
                              (t.Status == "Confirmed"));
            return !conflict;
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(Guid dealerId)
        {
            return await _context.Vehicles
                .Where(v => v.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _context.Dealers
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetAllAppointmentsAsync(Guid userId)
        {
            return await _context.TestDriveAppointments
                .Include(t => t.Customer)
                .Include(t => t.Dealer)
                .Include(t => t.Vehicle)
                .Where(t => t.CustomerId == userId)
                .OrderByDescending(t => t.AppointmentDate)
                .AsNoTracking()
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<TestDriveAppointment>)t.Result);
        }
    }
}
