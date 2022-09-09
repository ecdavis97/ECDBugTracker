using ECDBugTracker.Models;

namespace ECDBugTracker.Services.Interfaces
{
    public interface IBTRoleService
    {
        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);

        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        

    }
}
