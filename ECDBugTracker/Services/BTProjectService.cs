using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Models.Enums;
using ECDBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECDBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRoleService _roleService;

        public BTProjectService(ApplicationDbContext context,
                                IBTRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
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

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId)!;
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                //remove current PM
                if(currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                //Add new PM
                try
                {
                    await AddUserToProjectAsync(selectedPM!, projectId);
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddUserToProjectAsync(BTUser user, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                bool onProject = !project.Members!.Any(m => m.Id == user.Id);

                //check if user is on project
                if (onProject)
                {
                    project.Members?.Add(user);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
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
                Project project = await GetProjectByIdAsync(projectId);
;
                if (project != null)
                {
                    project.Archived = true;

                    foreach (Ticket ticket in project.Tickets!)
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
                List<Project> projects = await _context.Project
                                                       .Where(p => p.CompanyId == companyId && p.Archived)
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

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            Project? project = await _context.Project
                                             .Include(p => p.Company)
                                             .Include(p => p.Members)
                                             .Include(p => p.Tickets)
                                             .Include(p => p.ProjectPriority)
                                             .FirstOrDefaultAsync(m => m.Id == projectId);
            return project;
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            try
            {
                List<Project> projects = await GetAllProjectsByCompanyIdAsync(companyId);
                List<Project> unassignedProjects = new List<Project>();

                foreach (Project project in projects)
                {
                    if(await GetProjectManagerAsync(project.Id)! == null)
                    {
                        unassignedProjects.Add(project);
                    }                    
                }
                return unassignedProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser>? GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                foreach (BTUser member in project.Members)
                {
                    if(await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                if(project != null)
                {
                    //checks to see if the user(Id) is a project member
                    return project.Members!.Any(m => m.Id == userId);
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                foreach(BTUser member in project.Members!)
                {
                    if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        // Remove BTUser from project
                        await RemoveUserFromProjectAsync(member,projectId);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                bool onProject = project.Members!.Any(m=>m.Id == user.Id);

                //check if user is on project
                if (onProject)
                {
                    project.Members?.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);
;
                if (project != null)
                {
                    project.Archived = false;

                    foreach (Ticket ticket in project.Tickets!)
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
