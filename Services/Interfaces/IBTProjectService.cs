using ECDBugTracker.Models;

namespace ECDBugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task AddProjectAsync(Project project);
        public Task<bool> AddUserToProjectAsync(BTUser user, int projectId);
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);
        public Task ArchiveProjectAsync(int projectId);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority);
        public Task<List<Project>> GetArchiveProjectsByCompanyIdAsync(int companyId);
        public Task<Project> GetProjectByIdAsync(int projectId);
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName);
        public Task<BTUser>? GetProjectManagerAsync(int projectId);
        public Task<List<Project>> GetUnassignedProjectsAsync(int companyId);
        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId);
        public Task RestoreProjectAsync(int projectId);
        public Task UpdateProjectAsync(Project project);

    }
}
