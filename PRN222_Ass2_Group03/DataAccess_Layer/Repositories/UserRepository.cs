using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess_Layer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ILogger<UserRepository> logger)
        {
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Users
                    .Include(u => u.Dealer)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Users
                    .Include(u => u.Dealer)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Users
                    .Include(u => u.Dealer)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Users
                    .Include(u => u.Dealer)
                    .Where(u => u.IsActive.GetValueOrDefault())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user.Username);
                throw;
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.IsActive = false; // Soft delete
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Users
                    .AnyAsync(u => u.Username.ToLower() == username.ToLower() || 
                                  u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists: {Username}, {Email}", username, email);
                throw;
            }
        }
    }
}