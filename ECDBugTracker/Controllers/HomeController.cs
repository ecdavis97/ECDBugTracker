using ECDBugTracker.Extensions;
using ECDBugTracker.Models;
using ECDBugTracker.Models.ChartModels;
using ECDBugTracker.Models.Enums;
using ECDBugTracker.Models.ViewModels;
using ECDBugTracker.Services;
using ECDBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
namespace ECDBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTCompanyService _btCompanyService;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTTicketService _btTicketService;
        private readonly IBTRoleService _btRoleService;
        private readonly SignInManager<BTUser> _signInManager;
        public HomeController(ILogger<HomeController> logger, 
                              IBTCompanyService btCompanyService,
                              IBTProjectService btProjectService,
                              IBTTicketService btTicketService,
                              IBTRoleService btRoleService,
                              SignInManager<BTUser> signInManager)
        {
            _logger = logger;
            _btCompanyService = btCompanyService;
            _btProjectService = btProjectService;
            _btTicketService = btTicketService;
            _btRoleService = btRoleService;
            _signInManager = signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            Company? company = await _btCompanyService.GetCompanyInfoAsync(companyId);
            List<Project> projects = await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId);
            List<Ticket> tickets = await _btTicketService.GetAllTicketByCompanyIdAsync(companyId);
            List<BTUser> members = await _btCompanyService.GetMembersAsync(companyId);
            
            model.Company = company;
            model.Projects = projects;
            model.Tickets = tickets;
            model.Members = members;

            return View(model);
        }

        public IActionResult Landing()
        {
            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> PlotlyBarChart()
        {
            PlotlyBarData plotlyData = new();
            List<PlotlyBar> barData = new();
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId);

            //Bar One
            PlotlyBar barOne = new()
            {
                X = projects.Where(p => p.Tickets!.Any()).Select(p => p.Name).ToArray()!,
                Y = projects.Where(p => p.Tickets!.Any()).SelectMany(p => p.Tickets!).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
                Name = "Tickets",
                Type = "bar"
            };

            //Bar Two
            PlotlyBar barTwo = new()
            {
                X = projects.Where(p => p.Tickets!.Any()).Select(p => p.Name).ToArray()!,
                Y = projects.Where(p => p.Tickets!.Any()).Select(async p => (await _btProjectService.GetProjectMembersByRoleAsync(p.Id, BTRoles.Developer.ToString())).Count).Select(c => c.Result).ToArray(),
                Name = "Developers",
                Type = "bar"
            };

            barData.Add(barOne);
            barData.Add(barTwo);

            plotlyData.Data = barData;

            return Json(plotlyData);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity.GetCompanyId();

            List<Project> projects = await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
            {
                int priorityCount = (await _btProjectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {

            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == false && p.Tickets!.Any()).ToList();

            foreach (Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _btProjectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Developer))).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();


            return Json(amChartData.Data);
        }


        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}