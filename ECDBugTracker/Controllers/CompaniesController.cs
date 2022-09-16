﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECDBugTracker.Data;
using ECDBugTracker.Models;
using ECDBugTracker.Models.ViewModels;
using ECDBugTracker.Extensions;
using ECDBugTracker.Services.Interfaces;

namespace ECDBugTracker.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRoleService _roleService;

        public CompaniesController(ApplicationDbContext context, 
                                   IBTCompanyService bTCompanyService,
                                   IBTRoleService roleService)
        {
            _context = context;
            _companyService = bTCompanyService;
            _roleService = roleService;
        }

        //// GET: Companies
        //public async Task<IActionResult> Index()
        //{
        //      return _context.Companies != null ? 
        //                  View(await _context.Companies.ToListAsync()) :
        //                  Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        //}

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }


        public async Task<IActionResult> ManageUserRoles()
        {
            //instance of new viewmodel
            List<ManageUserRolesViewModel> model= new();

            //get companyId
            int companyId = User.Identity.GetCompanyId();

            //get all company users
            List<BTUser> members = await _companyService.GetMembersAsync(companyId);

            //loop over the users to populate the viewmodel
            //      -instantiate single viewmodel
            //      -use _roleService
            //      -Create multiselects
            foreach(BTUser member in members)
            {
                ManageUserRolesViewModel viewModel = new();
                IEnumerable<string> currentRoles = await _roleService.GetUserRolesAsync(member);

                viewModel.BTUser = member;
                viewModel.Roles = new MultiSelectList(await _roleService.GetRolesAsync(),"Name","Name",currentRoles);

                model.Add(viewModel);
            }

            //return the model to the view

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            //get company id
            int companyId = User.Identity!.GetCompanyId();

            //instantiate btuser
            BTUser? btUser = (await _companyService.GetMembersAsync(companyId)).FirstOrDefault(m=>m.Id == member.BTUser!.Id);

            //get the roles for the user
            IEnumerable<string> currentRoles = await _roleService.GetUserRolesAsync(btUser);

            //get the selected role(s) for the user
            string? selectedRole = member.SelectedRoles!.FirstOrDefault();

            //remove current role(s) and add the new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _roleService.RemoveUserFromRolesAsync(btUser, currentRoles))
                {
                    await _roleService.AddUserToRoleAsync(btUser, selectedRole);
                } 
            }

            //navigate
            return RedirectToAction(nameof(ManageUserRoles));

        }




        //// GET: Companies/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Companies/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileName,ImageFileType")] Company company)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(company);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(company);
        //}

        //// GET: Companies/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Companies == null)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Companies.FindAsync(id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(company);
        //}

        //// POST: Companies/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileName,ImageFileType")] Company company)
        //{
        //    if (id != company.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(company);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CompanyExists(company.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(company);
        //}

        //// GET: Companies/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Companies == null)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Companies
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(company);
        //}

        //// POST: Companies/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Companies == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        //    }
        //    var company = await _context.Companies.FindAsync(id);
        //    if (company != null)
        //    {
        //        _context.Companies.Remove(company);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool CompanyExists(int id)
        //{
        //  return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
