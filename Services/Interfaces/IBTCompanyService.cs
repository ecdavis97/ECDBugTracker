﻿using ECDBugTracker.Models;

namespace ECDBugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<List<BTUser>> GetMembersAsync(int? companyId);
        public Task<Company> GetCompanyInfoAsync(int? companyId);
    }
}
