using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InternshipDotCom.Models;
using InternshipDotCom.Servieces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InternshipDotCom.Controllers
{
    [Authorize(Roles = "organization")]

    public class OrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrganizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Organizations
        // GET: Organizations
        public async Task<IActionResult> Index()
        {
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve organizations owned by the current user
            var organizations = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .ToListAsync();

            return View(organizations);
        }


        // GET: Organizations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organization
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // GET: Organizations/Create
        public IActionResult Create()
        {
           
            return View();
        }

        // POST: Organizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrganizationName,Address,Description")] Organization organization)
        {
            
                // Set the ApplicationUserId property to the ID of the current user
                organization.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _context.Add(organization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            
        }


        // GET: Organizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organization.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", organization.ApplicationUserId);
            return View(organization);
        }

        // POST: Organizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrganizationName,Address,Description")] Organization organization)
        {
            if (id != organization.Id)
            {
                return NotFound();
            }

            organization.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
                {
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.Id))
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



        public async Task<IActionResult> ViewRegisteredStudents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve organization IDs owned by the current user
            var userOrganizationIds = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            // Filter internships based on the organization IDs
            var internships = await _context.Internship
                .Include(i => i.Organization)
                .Where(i => userOrganizationIds.Contains(i.OrganizationId))
                .ToListAsync();

            // Retrieve the IDs of internships associated with the organizations
            var internshipIds = internships.Select(i => i.Id).ToList();

            // Retrieve registered students for these internships
            var registeredStudents = await _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.Internship)
                .Where(ai => internshipIds.Contains(ai.InternshipId) && ai.IsApplied)
                .ToListAsync();

            // Get unique organizations for which at least one student has applied
            var appliedOrganizationIds = registeredStudents.Select(rs => rs.Internship.OrganizationId).Distinct().ToList();

            // Retrieve organizations corresponding to these IDs
            var appliedOrganizations = await _context.Organization
                .Where(org => appliedOrganizationIds.Contains(org.Id))
                .ToListAsync();

            ViewBag.Organizations = appliedOrganizations;

            return View(registeredStudents);
        }


        public async Task<IActionResult> ViewApplicantDetails(string applicationUserId, int internshipId)
        {
            var applicant = await _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.Internship)
                .FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == internshipId);

            if (applicant == null)
            {
                return NotFound();
            }

            return PartialView("_ApplicantDetails", applicant);
        }




        public async Task<IActionResult> FilterApplicants(int? organizationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve organization IDs owned by the current user
            var userOrganizationIds = await _context.Organization
                .Where(o => o.ApplicationUserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            // Filter internships based on the organization IDs owned by the user
            IQueryable<Internship> internshipsQuery = _context.Internship
                .Include(i => i.Organization)
                .Where(i => userOrganizationIds.Contains(i.OrganizationId));

            // Filter internships based on the provided organization ID
            if (organizationId.HasValue)
            {
                internshipsQuery = internshipsQuery.Where(i => i.OrganizationId == organizationId);
            }

            // Retrieve the IDs of internships associated with the organizations
            var internshipIds = await internshipsQuery.Select(i => i.Id).ToListAsync();

            // Retrieve applicants who have applied for these internships
            var filteredApplicants = await _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.Internship) // Ensure Internship property is loaded
                .Where(ai => internshipIds.Contains(ai.InternshipId) && ai.IsApplied)
                .ToListAsync();

            // Get unique organizations for which at least one student has applied
            var appliedOrganizationIds = filteredApplicants.Select(ai => ai.Internship.OrganizationId).Distinct().ToList();

            // Retrieve organizations corresponding to these IDs
            var appliedOrganizations = await _context.Organization
                .Where(org => appliedOrganizationIds.Contains(org.Id))
                .ToListAsync();

            ViewBag.Organizations = appliedOrganizations;

            return PartialView("_FilteredApplicant", filteredApplicants);
        }


        public async Task<IActionResult> LoadApplicants(int page = 1)
        {
            var pageSize = 5;
            var skip = (page - 1) * pageSize;
            int totalItemCount = await _context.ApplicantInternship
                .Where(ai => ai.IsApplied)
                .CountAsync();
            ViewBag.TotalItemCount = totalItemCount;

            var applicants = await _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.Internship)
                .Where(ai => ai.IsApplied)
                .OrderBy(ai => ai.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return PartialView("_FilteredApplicant", applicants);
        }


        [HttpPost]
        public async Task<IActionResult> CallForInterview(string applicationUserId, int internshipId, string interviewLink, DateOnly interviewDate, TimeSpan interviewTime)
        {
            var applicant = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == internshipId);

            if (applicant == null)
            {
                return NotFound();
            }

            // Update IsCalledForInterview and InterviewLink properties
            applicant.IsCalledForInterview = true;
            applicant.interviewTime = interviewTime;
            applicant.interviewDate = interviewDate;
            applicant.InterviewLink = interviewLink;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        [HttpPost]
        public async Task<IActionResult> startInternship(string applicationUserId, int internshipId)
        {
            var applicant = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == internshipId);

            if (applicant == null)
            {
                return NotFound();
            }

            applicant.StartedInternship = true;
            

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> finishInternship(string applicationUserId, int internshipId)
        {
            var applicant = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == internshipId);

            if (applicant == null)
            {
                return NotFound();
            }

            applicant.FinishedInternship = true;


            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        // GET: Organizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organization
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // POST: Organizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organization = await _context.Organization.FindAsync(id);
            if (organization != null)
            {
                _context.Organization.Remove(organization);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organization.Any(e => e.Id == id);
        }
    }
}
