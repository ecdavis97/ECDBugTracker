using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace ECDBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRoleService _roleService;

        public BTTicketService(ApplicationDbContext context,
                               IBTRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task<bool> AddDeveloperToTicketAsync(string userId, int ticketId)
        {
            try
            {
                BTUser? currentDev = await GetDeveloperAsync(ticketId);
                BTUser? selectedDev = await _context.Users.FindAsync(userId);

                if(currentDev != null)
                {
                    await RemoveDeveloperAsync(ticketId);
                }

                try
                {
                    await AddDeveloperToTicketAsync(selectedDev.Id, ticketId);
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

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddUserToTicketAsync(BTUser user, int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                bool onTicket = ticket.Project.Members.Any(m => m.Id == user.Id);

                if (onTicket)
                {
                    ticket.Project.Members.Add(user);
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

        public async Task ArchiveTicketAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);
    
                if(ticket != null)
                {
                    ticket.Archived = true;
                }
                
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketByProjectIdAsync(int projectId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                             .Where(t => t.ProjectId == projectId)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.ProjectId)
                                                             .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchiveTicketsByProjectIdAsync(int projectId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                             .Where(t => t.ProjectId == projectId)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.TicketPriority)
                                                             .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser>? GetDeveloperAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                if(ticket.DeveloperUser != null)
                {
                    return ticket.DeveloperUser!;
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            Ticket ticket = await _context.Tickets
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.Project)                                              
                                            .Include(t => t.TicketType)
                                            .Include(t => t.TicketAttachments)
                                            .Include(t => t.TicketPriority)                                              
                                            .Include(t => t.TicketStatus)
                                            .FirstOrDefaultAsync(t => t.Id == ticketId);
            return ticket!;
                                              
            

        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int projectId)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketByProjectIdAsync(projectId);
                List<Ticket> unassignedTickets = new List<Ticket>();

                foreach (Ticket ticket in tickets)
                {
                    if(await GetDeveloperAsync(ticket.Id)! == null)
                    {
                        unassignedTickets.Add(ticket);
                    }
                }
                return unassignedTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserOnTicketAsync(string userId, int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                if(ticket != null)
                {
                    return ticket.Project!.Members!.Any(t => t.Id == userId);
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveDeveloperAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);
                BTUser user = await GetDeveloperAsync(ticketId);

                if(ticket.DeveloperUser != null)
                {
                    await RemoveUserFromTicketAsync(user, ticketId);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromTicketAsync(BTUser user, int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);
                bool onTicket = ticket.Project.Members!.Any(t => t.Id == user.Id);

                if (onTicket)
                {
                    ticket.Project.Members!.Remove(user);
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

        public async Task RestoreTicketAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);

                if(ticket != null)
                {
                    ticket.Archived = false;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            };
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
