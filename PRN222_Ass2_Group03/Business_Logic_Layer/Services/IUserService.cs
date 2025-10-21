using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public interface IUserService
    {
        Task<UserDTO?> LoginAsync(string username, string password);
        Task<UserDTO?> GetUserByIdAsync(Guid id);
        Task<UserDTO?> GetUserByUsernameAsync(string username);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO?> CreateUserAsync(UserDTO user, string password);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<IEnumerable<UserDTO>> GetCustomersAsync();
    }
}