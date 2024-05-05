using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternshipDotCom.Models;
using InternshipDotCom.Servieces;
using Microsoft.AspNetCore.Authorization;

namespace InternshipDotCom.Controllers
{
    [Authorize(Roles="admin")]
    public class AssignedCoordinatorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignedCoordinatorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AssignedCoordinators
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AssignedCoordinator.Include(a => a.ApplicationUser).Include(a => a.department).Include(a => a.university);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AssignedCoordinators/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignedCoordinator = await _context.AssignedCoordinator
                .Include(a => a.ApplicationUser)
                .Include(a => a.department)
                .Include(a => a.university)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignedCoordinator == null)
            {
                return NotFound();
            }

            return View(assignedCoordinator);
        }

        // GET: AssignedCoordinators/Create

        public IActionResult Create()
        {
            // Step 1: Fetch the role ID for "InternshipCordinator"
            var coordinatorRoleId = _context.Roles
                .Where(r => r.NormalizedName == "InternshipCordinator")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (coordinatorRoleId == null)
            {
                // Handle if role doesn't exist
                return NotFound();
            }

            // Step 2: Find all user IDs with the role ID from UserRoles table
            var coordinatorUserIds = _context.UserRoles
                .Where(ur => ur.RoleId == coordinatorRoleId)
                .Select(ur => ur.UserId)
                .ToList();

            // Step 3: Return user information from Users table based on the IDs from step 2
            var coordinators = _context.Users
                .Where(u => coordinatorUserIds.Contains(u.Id))
                .ToList();

            ViewData["ApplicationUserId"] = new SelectList(coordinators, "Id", "UserName");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["UniversityId"] = new SelectList(_context.University, "id", "name");

            return View();
        }




        // POST: AssignedCoordinators/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ApplicationUserId,UniversityId,DepartmentId")] AssignedCoordinator assignedCoordinator)
        {
            // Check if the user already exists in the AssignedCoordinator table
            var existingCoordinator = await _context.AssignedCoordinator
                                                .FirstOrDefaultAsync(ac => ac.ApplicationUserId == assignedCoordinator.ApplicationUserId);

            // If the user doesn't exist, add them to the database
            if (existingCoordinator == null)
            {
                _context.Add(assignedCoordinator);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: AssignedCoordinators/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignedCoordinator = await _context.AssignedCoordinator.FindAsync(id);
            if (assignedCoordinator == null)
            {
                return NotFound();
            }
            // Step 1: Fetch the role ID for "InternshipCordinator"
            var coordinatorRoleId = _context.Roles
                .Where(r => r.NormalizedName == "InternshipCordinator")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (coordinatorRoleId == null)
            {
                // Handle if role doesn't exist
                return NotFound();
            }

            // Step 2: Find all user IDs with the role ID from UserRoles table
            var coordinatorUserIds = _context.UserRoles
                .Where(ur => ur.RoleId == coordinatorRoleId)
                .Select(ur => ur.UserId)
                .ToList();

            // Step 3: Return user information from Users table based on the IDs from step 2
            var coordinators = _context.Users
                .Where(u => coordinatorUserIds.Contains(u.Id))
                .ToList();
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "UserName", assignedCoordinator.ApplicationUserId);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", assignedCoordinator.DepartmentId);
            ViewData["UniversityId"] = new SelectList(_context.University, "id", "name", assignedCoordinator.UniversityId);
            return View(assignedCoordinator);
        }

        // POST: AssignedCoordinators/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ApplicationUserId,UniversityId,DepartmentId")] AssignedCoordinator assignedCoordinator)
        {
            if (id != assignedCoordinator.Id)
            {
                return NotFound();
            }

           
                try
                {
                    _context.Update(assignedCoordinator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignedCoordinatorExists(assignedCoordinator.Id))
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

        // GET: AssignedCoordinators/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignedCoordinator = await _context.AssignedCoordinator
                .Include(a => a.ApplicationUser)
                .Include(a => a.department)
                .Include(a => a.university)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignedCoordinator == null)
            {
                return NotFound();
            }

            return View(assignedCoordinator);
        }

        // POST: AssignedCoordinators/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignedCoordinator = await _context.AssignedCoordinator.FindAsync(id);
            if (assignedCoordinator != null)
            {
                _context.AssignedCoordinator.Remove(assignedCoordinator);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssignedCoordinatorExists(int id)
        {
            return _context.AssignedCoordinator.Any(e => e.Id == id);
        }
    }
}
