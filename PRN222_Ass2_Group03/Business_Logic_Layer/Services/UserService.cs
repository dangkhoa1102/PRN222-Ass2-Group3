using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Business_Logic_Layer.DTOs;

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

        private UserDTO ConvertToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDTO?> LoginAsync(string username, string password)
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
                    return ConvertToDTO(user);
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

        public async Task<UserDTO?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return user != null ? ConvertToDTO(user) : null;
        }

        public async Task<UserDTO?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            return user != null ? ConvertToDTO(user) : null;
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null ? ConvertToDTO(user) : null;
        }

        public async Task<UserDTO?> CreateUserAsync(UserDTO userDto, string password)
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = userDto.Username,
                    Email = userDto.Email,
                    FullName = userDto.FullName,
                    Phone = userDto.Phone,
                    Role = userDto.Role,
                    Password = HashPassword(password), // Hash the password
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                
                var createdUser = await _userRepository.CreateUserAsync(user);
                
                if (createdUser != null)
                {
                    _logger.LogInformation("User created successfully - {Username}", user.Username);
                    return ConvertToDTO(createdUser);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", userDto.Username);
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

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(ConvertToDTO);
        }

        public async Task<IEnumerable<UserDTO>> GetCustomersAsync()
        {
            var users = await _userRepository.GetCustomersAsync();
            return users.Select(ConvertToDTO);
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