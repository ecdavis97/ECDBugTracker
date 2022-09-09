﻿using ECDBugTracker.Models;

namespace ECDBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddTicketAsync(Ticket ticket);
        public Task<bool> AddUserToTicketAsync(BTUser user, int ticketId);
        public Task<bool> AddDeveloperToTicketAsync(string userId, int ticketId);
        public Task ArchiveTicketAsync(int ticketId);
        public Task<List<Ticket>> GetAllTicketByProjectIdAsync(int ticketId);
        public Task<List<Ticket>> GetArchiveTicketsByProjectIdAsync(int projectId);
        public Task<Ticket> GetTicketByIdAsync(int ticketId);
        public Task<BTUser>? GetDeveloperAsync(int ticketId);
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int projectId);
        public Task<bool> IsUserOnTicketAsync(string userId, int ticketId);
        public Task RemoveDeveloperAsync(int ticketId);
        public Task<bool> RemoveUserFromTicketAsync(BTUser user, int ticketId);
        public Task RestoreTicketAsync(int ticketId);
        public Task UpdateTicketAsync(Ticket ticket);
    }
}
