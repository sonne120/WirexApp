using System.Threading.Tasks;

namespace WirexApp.Domain.User
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(UserId userId);
        void Save(User user);
    }
}
