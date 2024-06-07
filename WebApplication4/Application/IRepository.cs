using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public interface IRepository
    {
        Task<int> AddAsync(UserDTO user, string createdBy);
        Task<UserDTO> GetByLoginAsync(string login);
        Task ChangeAsync(string login, string name, int gender, DateTime bDay, string nameAuth);
        Task<List<UserDTO>> GetAllActiveUsersAsync();
        Task<int> DeleteSoft(string login, string modifiedBy);
        Task<int> Delete(string login);
        Task<int> RecoverUserAsync(string login);
        Task ChangePasswordAsync(string login, string password, string name);
        Task ChangeLoginAsync(string login, string name);
        Task<List<UserDTO>> GetAllOlderThanAsync(int age);
    }
}
