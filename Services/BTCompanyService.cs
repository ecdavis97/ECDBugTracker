using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECDBugTracker.Services
{

    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new();

                if(companyId != null)
                {
                    company = await _context.Companies
                                            .Include(c => c.Projects)
                                            .Include(c => c.Invites)
                                            .Include(c => c.Members)
                                            .FirstOrDefaultAsync(c => c.Id == companyId);
                }
                return company!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                Company? company = await GetCompanyInfoAsync(companyId);
                if (company != null)
                {
                    List<BTUser> members = company.Members.ToList();
                    return members;
                }
                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
