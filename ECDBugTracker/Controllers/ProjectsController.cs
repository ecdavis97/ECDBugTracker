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
        public async Task<IActionResult> Index()
        {

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _context.Project
                                                   .Include(p => p.Company)
                                                   .Include(p => p.ProjectPriority)
                                                   .Where(p => p.CompanyId == companyId && !p.Archived)
                                                   .ToListAsync();

            return View(projects);

        }
        public async Task<IActionResult> ArchivedProjects()
        {

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);

        }

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
            //service call to RoleService
            model.PMList = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");

            return View(model);
        }

        //POST:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                //Add PM to project and TODO: Enhance this process
                Project project = await _projectService.GetProjectByIdAsync(model.Project!.Id);
                BTUser? projectManager = await _context.Users.FindAsync(model.PMID);

                project.Members!.Add(projectManager!);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(AssignProjectManager), new { id = model.Project!.Id});
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles="Admin, ProjectManager")]
        public IActionResult Create()
        {
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            //ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Name");

            return View(new Project());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageFormFile,ImageFileData")] Project project)
        {
            if (ModelState.IsValid)
            { 
                //TODO: make companyId retreival more efficient

                //get company ID
                project.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;

                project.Created = DataUtility.GetPostGresDate(DateTime.Now);
                project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageContentType = project.ImageFormFile.ContentType;
                }

                //service call
                await _projectService.AddProjectAsync(project);

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile")] Project project)
        {

            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageContentType = project.ImageFormFile.ContentType;
                    }

                    //service call
                    await _projectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Archive(companyId)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            await _projectService.GetArchiveProjectsByCompanyIdAsync(companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Project'  is null.");
            }
            //service call 
            await _projectService.ArchiveProjectAsync(id);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //replicate the archive to make a restore function

        private bool ProjectExists(int id)
        {
          return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
