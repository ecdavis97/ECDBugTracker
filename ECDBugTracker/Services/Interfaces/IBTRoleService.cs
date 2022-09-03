using ECDBugTracker.Models;

namespace ECDBugTracker.Services.Interfaces
{
    public interface IBTRoleService
    {
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
    }
}
