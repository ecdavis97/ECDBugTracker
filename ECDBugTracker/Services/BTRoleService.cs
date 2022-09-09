using ECDBugTracker.Models;
using ECDBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ECDBugTracker.Services
{
    public class BTRoleService : IBTRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        public BTRoleService(RoleManager<IdentityRole> roleManager,
                             UserManager<BTUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<BTUser> btUsers = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                List<BTUser> results = btUsers.Where(b =>b.CompanyId == companyId).ToList();

                return results;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserInRoleAsync(BTUser member, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(member, roleName);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
