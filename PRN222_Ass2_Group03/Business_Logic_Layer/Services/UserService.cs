using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Business_Logic_Layer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                
                if (user == null || !user.IsActive.GetValueOrDefault())
                {
                    _logger.LogWarning("Login attempt failed: User not found or inactive - {Username}", username);
                    return null;
                }

                if (VerifyPassword(password, user.Password))
                {
                    _logger.LogInformation("User logged in successfully - {Username}", username);
                    return user;
                }

                _logger.LogWarning("Login attempt failed: Invalid password - {Username}", username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for user {Username}", username);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetUserByUsernameAsync(username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            try
            {
                // Hash the password before storing
                user.Password = HashPassword(user.Password);
                
                // Set default values - using CreatedAt instead of CreatedDate
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                var createdUser = await _userRepository.CreateUserAsync(user);
                
                if (createdUser != null)
                {
                    _logger.LogInformation("User created successfully - {Username}", user.Username);
                }
                
                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", user.Username);
                throw;
            }
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
            return existingUser == null;
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(email);
            return existingUser == null;
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await LoginAsync(username, password);
            return user != null;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // For demo purposes, check if it's plain text first (for existing users)
            if (password == storedHash)
            {
                return true;
            }
            
            // Otherwise, hash the input and compare
            var hashedInput = HashPassword(password);
            return hashedInput == storedHash;
        }

        private string HashPassword(string password)
        {
            // For demo purposes, using SHA256. In production, use BCrypt or Argon2
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}