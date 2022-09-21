using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ECDBugTracker.Models.ViewModels;
using ECDBugTracker.Models.Enums;
using System.ComponentModel.Design;
using ECDBugTracker.Extensions;

namespace ECDBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRoleService _roleService;

        public ProjectsController(ApplicationDbContext context,
                                  IImageService imageService,
                                  UserManager<BTUser> userManager,
                                  IBTProjectService projectService,
                                  IBTRoleService roleService)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _projectService = projectService;
            _roleService = roleService;
        }

        // GET: Projects
        public async Task<IActionResult> MyProjects()
        {

            //int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _context.Projects
                                                   .Include(p => p.Company)
                                                   .Include(p => p.ProjectPriority)
                                                   .Where(p => p.CompanyId == companyId && !p.Archived)
                                                   .ToListAsync();

            return View(projects);

        }

        public async Task<IActionResult> ArchivedProjects()
        {

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _projectService.GetArchiveProjectsByCompanyIdAsync(companyId);

            return View(projects);

        }


        #region GET: ADD MEMBERS TO PROJECT
        //GET Add member to project
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AddMemberViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            Project project = await _projectService.GetProjectByIdAsync(id.Value);
            model.Project = project;
            model.MemberIds = project.Members!.Select(m => m.Id).ToList();

            model.MemberList = new MultiSelectList(await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync(), "Id", "FullName", model.MemberIds);

            return View(model);
        }

        #endregion
        #region POST: ADD MEMBER TO PROJECT
        //POST Add member to project
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(AddMemberViewModel model)
        {
            if (model.MemberIds != null && model.MemberIds.Count > 0)
            {

                Project selectedProject = await _projectService.GetProjectByIdAsync(model.Project!.Id);

                foreach(BTUser member in selectedProject.Members!)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, selectedProject.Id);
                }

                foreach (string memberId in model.MemberIds)
                {

                    BTUser user = await _userManager.FindByIdAsync(memberId);

                    await _projectService.AddUserToProjectAsync(user, model.Project!.Id);

                }

                return RedirectToAction(nameof(MyProjects));
            }

            ModelState.AddModelError("MemberIds", "No Members chosen! Please select Project Members.");


            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            Project project = await _projectService.GetProjectByIdAsync(model.Project!.Id);
            model.Project = project;
            model.MemberIds = project.Members!.Select(m => m.Id).ToList();

            model.MemberList = new MultiSelectList(await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync(), "Id", "FullName", model.MemberIds);

            return View(model);

        }

        #endregion

        //Get AssignProjectManager
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> AssignProjectManager(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            AssignPMViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = await _projectService.GetProjectByIdAsync(id.Value);

            //get current PM(if they exist) 
            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

            //service call to RoleService to get all PMs for the company 
            model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);

            return View(model);
        }

        //POST AssignProjectManager:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project!.Id);

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("PMID", "No Project Manager chosen! Please select a PM.");

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            //get company id
            model.Project = await _projectService.GetProjectByIdAsync(model.Project!.Id);

            //get current PM(if they exist) 
            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

            //service call to RoleService to get all PMs for the company 
            model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);

            return View(model);

        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        // GET: Projects/Create
        [Authorize(Roles="Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            AssignPMViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = new();
            
            model.Project.StartDate = DataUtility.GetPostGresDate(DateTime.UtcNow);
            model.Project.EndDate = DataUtility.GetPostGresDate(DateTime.UtcNow);

            //service call to RoleService to get all PMs for the company 
            model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            //ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Name");

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignPMViewModel model)
        {
            if (ModelState.IsValid)
            { 
                //TODO: make companyId retreival more efficient

                //get company ID
                model.Project!.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;

                model.Project.Created = DataUtility.GetPostGresDate(DateTime.Now);
                model.Project.StartDate = DataUtility.GetPostGresDate(model.Project.StartDate);
                model.Project.EndDate = DataUtility.GetPostGresDate(model.Project.EndDate);

                if (model.Project.ImageFormFile != null)
                {
                    model.Project.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                }

                //service call
                if (!User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                    await _projectService.AddProjectAsync(model.Project);
                }


                //save selected PM to project
                await _projectService.AddProjectManagerAsync(model.PMID!, model.Project.Id);


                return RedirectToAction(nameof(MyProjects));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.Project!.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", model.Project.ProjectPriorityId);
            return View(model.Project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AssignPMViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            //service call to RoleService to get all PMs for the company 
            model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");

            model.Project = await _context.Projects.FindAsync(id);
            if (model.Project == null)
            {
                return NotFound();
            }

            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project!.Id)!)?.Id;
            //service call to RoleService to get all PMs for the company 
            model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), model.Project.CompanyId), "Id", "FullName", currentPMId);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.Project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", model.Project.ProjectPriorityId);

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignPMViewModel model)
        {

            if (model.Project!.Id != model.Project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    model.Project.Created = DataUtility.GetPostGresDate(DateTime.Now);
                    model.Project.StartDate = DataUtility.GetPostGresDate(model.Project.StartDate);
                    model.Project.EndDate = DataUtility.GetPostGresDate(model.Project.EndDate);

                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }
                                       
                    //service call
                    await _projectService.UpdateProjectAsync(model.Project);

                    await _projectService.AddProjectManagerAsync(model.PMID!, model.Project.Id);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyProjects));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.Project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", model.Project.ProjectPriorityId);
            return View(model.Project);
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int id)
        {

            var project = await _projectService.GetProjectByIdAsync(id);
            
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            //service call 
            await _projectService.ArchiveProjectAsync(id);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ArchivedProjects));
        }


        // GET: Projects/Restore/5
        public async Task<IActionResult> Restore(int id)
        {

            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            //service call 
            await _projectService.RestoreProjectAsync(id);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get: Projects/GetUnassignedProjects
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            if (projects == null)
            {
                return NotFound();
            }

            return View(projects);
        }

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
