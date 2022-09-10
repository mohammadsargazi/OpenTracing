using MongoDB.Entities;

namespace User.Services
{
    public class UserService : IUserService
    {
        public async Task<List<Models.User>> GetUsersAsync() => await DB.Find<Models.User>().ExecuteAsync();
        public async Task<Models.User> GetByMobileNumber(string mobileNumber)
        {
            return await DB.Find<Models.User>()
                         .Match(u => u.MobileNumber == mobileNumber).ExecuteFirstAsync();
        }

        public async Task<Models.User> AddUserAsync(Models.User user)
        {
            await user.SaveAsync();
            return user;
        }
    }
}
