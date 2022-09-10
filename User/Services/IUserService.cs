namespace User.Services
{
    public interface IUserService
    {
        Task<Models.User> AddUserAsync(Models.User user);
        Task<List<Models.User>> GetUsersAsync();
        Task<Models.User> GetByMobileNumber(string mobileNumber);
    }
}
