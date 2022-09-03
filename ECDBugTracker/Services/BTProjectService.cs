using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECDBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                await _context.AddAsync(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId)
;
                if (project != null)
                {
                    project.Archived = true;

                    foreach (Ticket ticket in project.Tickets)
                    {
                        ticket.ArchivedByProject = true;
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Project
                                                       .Where(p => p.CompanyId == companyId && !p.Archived)
                                                       .Include(p => p.Company)
                                                       .Include(p => p.ProjectPriority)
                                                       .ToListAsync();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetArchiveProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                 Project projects = await _context.Project
                                                 .Include(p => p.Company)
                                                 .Include(p => p.ProjectPriority)
                                                 .FirstOrDefaultAsync(m => m.Id == id);
                 return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<Project> GetProjectByIdAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task RestoreProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId)
;
                if (project != null)
                {
                    project.Archived = false;

                    foreach (Ticket ticket in project.Tickets)
                    {
                        ticket.ArchivedByProject = false;
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
