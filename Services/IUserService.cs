using System.Threading.Tasks;

namespace Leux.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(string username, string email, string password);
        Task<bool> LoginUserAsync(string email, string password);
    }
}

