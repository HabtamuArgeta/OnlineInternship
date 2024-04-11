using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternshipDotCom.Models;
using InternshipDotCom.Servieces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Data;


namespace InternshipDotCom.Controllers
{
    [Authorize]
    public class InternshipsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InternshipsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;

            _userManager = userManager;

            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Internships
        [Authorize(Roles = "admin,applicant,InternshipCordinator,organization")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve organization IDs owned by the current user
            var userOrganizationIds = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            // Filter internships based on the organization IDs
            IQueryable<Internship> internships = _context.Internship
                .Include(i => i.Organization)
                .Where(i => userOrganizationIds.Contains(i.OrganizationId));

            // Pass the list of organization IDs to the view
            ViewBag.OrganizationIds = userOrganizationIds;

            // Retrieve the organizations owned by the user
            var userOrganizations = await _context.Organization
                .Where(o => userOrganizationIds.Contains(o.Id))
                .ToListAsync();

            // Pass the list of organizations to the view
            ViewBag.Organizations = userOrganizations;

            return View(await internships.ToListAsync());
        }

        [Authorize(Roles = "admin,applicant,InternshipCordinator,organization")]
        public async Task<IActionResult>  Filter(int? organizationId)
        {
            // Retrieve organization IDs owned by the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userOrganizationIds = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            // Filter internships based on the organization ID if provided
            IQueryable<Internship> internships = _context.Internship
                .Include(i => i.Organization)
                .Where(i => userOrganizationIds.Contains(i.OrganizationId));

            if (organizationId.HasValue)
            {
                internships = internships.Where(i => i.OrganizationId == organizationId);
            }

            // Pass the list of organization IDs to the view
            ViewBag.OrganizationIds = userOrganizationIds;

            // Retrieve the organizations owned by the user
            var userOrganizations = await _context.Organization
                .Where(o => userOrganizationIds.Contains(o.Id))
                .ToListAsync();

            // Pass the list of organizations to the view
            ViewBag.Organizations = userOrganizations;

            // Return a partial view with the filtered internships
            return PartialView("_InternshipTable", await internships.ToListAsync());
        }



        // GET: Internships/Details/5
        [Authorize(Roles = "admin,applicant,InternshipCordinator,organization")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internship = await _context.Internship
                .Include(i => i.Organization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (internship == null)
            {
                return NotFound();
            }

            return View(internship);
        }

        // GET: Internships/Create
        // GET: Internships/Create
        [Authorize(Roles = "organization")]
        public async Task<IActionResult> Create()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve organization IDs owned by the current user
            var userOrganizations = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .ToListAsync();

            if (userOrganizations == null)
            {
                // Handle case where user doesn't have an organization
                return RedirectToAction("Create", "Organizations");
            }

            // Populate the dropdown list with user's organizations
            ViewData["OrganizationId"] = new SelectList(userOrganizations, "Id", "OrganizationName");

            return View();
        }




        // POST: Internships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "organization")]

        public async Task<IActionResult> Create([Bind("Id,InternshipName,InternshipType,NumberOfApplicant,Address,Description,OrganizationId,Image")] Internship internship)
        {
            if (internship.Image != null)

            {
                string AbsolutePath = "C:\\Program Files\\installed apps\\Linux comanned\\crzylearning\\DotNet Apps\\InternshipDotCom\\wwwroot";
                string RelativePath = "/Images/Internships/";
                RelativePath += Guid.NewGuid().ToString() + "_" + internship.Image.FileName;
                AbsolutePath += RelativePath;
                internship.ImagePath = RelativePath;

                string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, AbsolutePath);

                await internship.Image.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));

            }
            else
            {
                internship.ImagePath = "/Images/ImageNotUploaded.jpeg";
            }

             internship.PostedAt = DateTime.Now;
                _context.Add(internship);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           
        }

        // GET: Internships/Edit/5
        [Authorize(Roles = "organization")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internship = await _context.Internship.FindAsync(id);
            if (internship == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve organization IDs owned by the current user
            var userOrganizations = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .ToListAsync();

            if (userOrganizations == null)
            {
                // Handle case where user doesn't have an organization
                return RedirectToAction("Create", "Organizations");
            }

            // Populate the dropdown list with user's organizations
            ViewData["OrganizationId"] = new SelectList(userOrganizations, "Id", "OrganizationName", internship.OrganizationId);

            return View(internship);
        }



        // POST: Internships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "organization")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,InternshipName,InternshipType,NumberOfApplicant,Address,Description,OrganizationId,Image")] Internship internship)
        {
            if (id != internship.Id)
            {
                return NotFound();
            }
            if (internship.Image != null)

            {
                string AbsolutePath = "C:\\Program Files\\installed apps\\Linux comanned\\crzylearning\\DotNet Apps\\InternshipDotCom\\wwwroot";
                string RelativePath = "/Images/Internships/";
                RelativePath += Guid.NewGuid().ToString() + "_" + internship.Image.FileName;
                AbsolutePath += RelativePath;
                internship.ImagePath = RelativePath;

                string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, AbsolutePath);

                await internship.Image.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));

            }
            else
            {
                var existingInternship = _context.Internship.AsNoTracking().FirstOrDefault(b => b.Id == internship.Id);

                if (existingInternship != null)
                {
                    internship.ImagePath = existingInternship.ImagePath;
                }
                else
                {
                    internship.ImagePath = "/default-image-path.jpg";
                }
            }
            try
                {
                    _context.Update(internship);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InternshipExists(internship.Id))
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

        // GET: Internships/Delete/5
        [Authorize(Roles = "organization")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var internship = await _context.Internship
                .Include(i => i.Organization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (internship == null)
            {
                return NotFound();
            }

            return View(internship);
        }

        // POST: Internships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "organization")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var internship = await _context.Internship.FindAsync(id);
            if (internship != null)
            {
                _context.Internship.Remove(internship);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InternshipExists(int id)
        {
            return _context.Internship.Any(e => e.Id == id);
        }
    }
}
