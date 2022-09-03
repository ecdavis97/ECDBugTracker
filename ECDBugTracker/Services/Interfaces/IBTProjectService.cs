using ECDBugTracker.Models;

namespace ECDBugTracker.Services.Interfaces


{
    public interface IBTProjectService
    {
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetArchiveProjectsByCompanyIdAsync(int companyId);
        public Task AddProjectAsync(Project project);
        public Task<Project> GetProjectByIdAsync(int projectId);
        public Task UpdateProjectAsync(Project project);
        public Task ArchiveProjectAsync(int projectId);
        public Task RestoreProjectAsync(int projectId);

    }
}
