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


        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> AddDeveloperToTicketAsync(string userId, int ticketId)
        {

            Ticket? ticket = await _context.Tickets!.FindAsync(ticketId);
            ticket!.DeveloperUserId = userId;

            await _context.SaveChangesAsync();

            return true;
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

                bool onTicket = ticket.Project!.Members!.Any(m => m.Id == user.Id);

                if (onTicket)
                {
                    ticket.Project.Members!.Add(user);
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

        public async Task<List<Ticket>> GetAllTicketByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                     .SelectMany(p => p.Tickets!)
                                                       .Include(t => t.TicketAttachments)
                                                       .Include(t => t.DeveloperUser)
                                                       .Include(t => t.Comments)
                                                       .Include(t => t.History)
                                                       .Include(t => t.Project)
                                                       .Include(t => t.TicketType)
                                                       .Include(t => t.TicketAttachments)
                                                       .Include(t => t.TicketPriority)
                                                       .Include(t => t.TicketStatus)
                                                       .Where(t => !t.Archived && !t.ArchivedByProject)
                                                     .ToListAsync(); 
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsRelatedToUser(string userId)
        {
            try
            {
                List<Ticket> relatedTickets = await _context.Tickets!.Where(t => t.DeveloperUserId == userId ||
                                                                           t.SubmitterUserId == userId ||
                                                                           t.Project!.Members!.Any(m => m.Id == userId))
                                                                    .Include(t => t.DeveloperUser)
                                                                    .Include(t => t.Project)
                                                                    .Include(t => t.SubmitterUser)
                                                                    .Include(t => t.TicketPriority)
                                                                    .Include(t => t.TicketStatus)
                                                                    .Include(t => t.TicketType)
                                                                    .ToListAsync();
                return relatedTickets;
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
                List<Ticket> tickets = await _context.Tickets!
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

        #region Get Ticket As No Tracking Async
        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Projects
                                .Where(p => p.CompanyId == companyId && p.Archived == false)
                                .SelectMany(p => p.Tickets!)
                                  .Include(t => t.TicketAttachments)
                                  .Include(t => t.DeveloperUser)
                                  .Include(t => t.Comments)
                                  .Include(t => t.History)
                                  .Include(t => t.Project)
                                  .Include(t => t.TicketType)
                                  .Include(t => t.TicketAttachments)
                                  .Include(t => t.TicketPriority)
                                  .Include(t => t.TicketStatus)
                                  .Where(t => !t.Archived && !t.ArchivedByProject)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(t => t.Id == ticketId);
                return ticket!;


            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            Ticket? ticket = await _context.Tickets!
                                            .Include(t => t.SubmitterUser)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.Comments)
                                            .Include(t => t.History)
                                            .Include(t => t.Project)                                              
                                            .Include(t => t.TicketType)
                                            .Include(t => t.TicketAttachments)
                                            .Include(t => t.TicketPriority)                                              
                                            .Include(t => t.TicketStatus)
                                            .FirstOrDefaultAsync(t => t.Id == ticketId);
            return ticket!;
                                              
            

        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketByCompanyIdAsync(companyId);

                List<Ticket> unassignedTickets = tickets.Where(t => t.DeveloperUserId == null).ToList();

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
                BTUser user = await GetDeveloperAsync(ticketId)!;

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
                bool onTicket = ticket.Project!.Members!.Any(t => t.Id == user.Id);

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
