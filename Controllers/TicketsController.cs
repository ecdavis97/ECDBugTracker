using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using ECDBugTracker.Services.Interfaces;
using ECDBugTracker.Extensions;
using ECDBugTracker.Models.ViewModels;
using System.Security.Cryptography.X509Certificates;
using ECDBugTracker.Services;

namespace ECDBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTTicketService _btTicketService;
        private readonly IBTTicketHistoryService _btTicketHistoryService;
        private readonly IBTNotificationService _btNotificationService;
        private readonly IBTRoleService _btRoleService;

        public TicketsController(ApplicationDbContext context,
                                 IImageService imageService,
                                 UserManager<BTUser> userManager,
                                 IBTProjectService btProjectService,
                                 IBTTicketService btTicketservice,
                                 IBTTicketHistoryService btTicketHistoryService,
                                 IBTNotificationService btNotificationService,
                                 IBTRoleService btRoleService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _btProjectService = btProjectService;
            _btTicketService = btTicketservice;
            _btTicketHistoryService = btTicketHistoryService;
            _btNotificationService = btNotificationService;
            _btRoleService = btRoleService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            var tickets = await _btTicketService.GetAllTicketByCompanyIdAsync(companyId);

            return View(tickets);
        }

        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            var tickets = await _btTicketService.GetUnassignedTicketsAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _context.Tickets
                .Include(t => t.Comments!).ThenInclude(c => c.User)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.TicketAttachments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {

                int statusId = (await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;

                ticket.TicketStatusId = statusId;
                ticket.Created = DataUtility.GetPostGresDate(DateTime.Now);
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                await _btTicketService.AddTicketAsync(ticket);

                int companyId = User.Identity!.GetCompanyId();
                string userId = _userManager.GetUserId(User);

                Ticket newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _btTicketHistoryService.AddHistoryAsync(null!, newTicket, userId);

                return RedirectToAction(nameof(Index));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                int companyId = User.Identity!.GetCompanyId();
                string userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                try
                {
                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.Now);

                    await _btTicketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


                //Add history
                Ticket newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _btTicketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);


                //Add notification
                BTUser btUser = await _userManager.GetUserAsync(User);
                BTUser? projectManager = await _btProjectService.GetProjectManagerAsync(ticket.ProjectId)!;
                Notification notification = new()
                {
                    ProjectId = ticket.ProjectId,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id,
                    TicketId = ticket.Id,
                    Title = "New Ticket Added.",
                    Message = $"New Ticket: {ticket.Title} was created by {btUser.FullName}",
                    Created = DataUtility.GetPostGresDate(DateTime.UtcNow),
                    SenderId = userId,
                    RecipientId = projectManager?.Id,

                };


                await _btNotificationService.AddNotificationAsync(notification);
                if (projectManager != null)
                {
                    await _btNotificationService.SendEmailNotificationAsync(notification, $"New Ticket Added for Project: {ticket.Project!.Name}");
                }
                else
                {
                    notification.RecipientId = userId;
                    await _btNotificationService.SendEmailNotificationAsync(notification, $"New Ticket Added for Project: {ticket.Project!.Name}");

                }

                return RedirectToAction(nameof(Index));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId); ;
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddTicketComment([Bind] ("Id,TicketId,Comment")] TicketComment ticketComment)
        //{

        //}

        //GET: MY Tickets
        public async Task<IActionResult> MyTickets()
        {
            string? userId = _userManager.GetUserId(User);

            List<Ticket> relatedTickets = await _btTicketService.GetAllTicketsRelatedToUser(userId);

            return View(relatedTickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _imageService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                //ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _btTicketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }


        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment? ticketComment)
            {
                ModelState.Remove("UserId");

                if (ModelState.IsValid)
                {
                    ticketComment!.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                    _context.Add(ticketComment);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Details", "Tickets", new {id = ticketComment.TicketId});

            }
        

        //Assign Developer GET
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AssignDeveloperViewModel model = new();

            model.Ticket = await _btTicketService.GetTicketByIdAsync(id.Value);

            model.DeveloperList = new SelectList(await _btProjectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(BTRoles.Developer)), "Id", "FullName", model.Ticket.DeveloperUserId);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DeveloperID))
            {
                int companyId = User.Identity!.GetCompanyId();
                string userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _btTicketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id, companyId);

                try
                {
                    await _btTicketService.AddDeveloperToTicketAsync(model.DeveloperID, model.Ticket!.Id);
                }
                catch (Exception)
                {

                    throw;
                }

                //add history
                Ticket? newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(model.Ticket.Id, companyId);
                await _btTicketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);


                BTUser btUser = await _userManager.GetUserAsync(User);

                //add ticket notification
                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id,
                    TicketId = model.Ticket.Id,
                    Title = "Ticket Assignment",
                    Message = $"Ticket: {model.Ticket.Title} was assigned by {btUser.FullName}",
                    Created = DataUtility.GetPostGresDate(DateTime.UtcNow),
                    SenderId = userId,
                    RecipientId = model.DeveloperID,

                };

                await _btNotificationService.AddNotificationAsync(notification);
                await _btNotificationService.SendEmailNotificationAsync(notification, "Ticket Assigned");

                return RedirectToAction(nameof(Details), new { id = model.Ticket.Id });


            }

            //To do: custom error message
            ModelState.AddModelError("DeveloperID", "No Developer chosen! Please select a Project Developer.");

            model.Ticket = await _btTicketService.GetTicketByIdAsync(model.Ticket!.Id);

            model.DeveloperList = new SelectList(await _btProjectService.GetProjectMembersByRoleAsync(model.Ticket!.ProjectId, nameof(BTRoles.Developer)), "Id", "FullName", model.DeveloperID);
            return View(model);

        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _btTicketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileType;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }


        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
