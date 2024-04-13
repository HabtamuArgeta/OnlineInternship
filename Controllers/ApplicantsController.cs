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
    [Authorize (Roles = "applicant")]
    public class ApplicantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Applicants
        public async Task<IActionResult> Index()
        {
            return View(await _context.Applicant.ToListAsync());
        }

        public async Task<IActionResult> PostedInternship()
        {
            
            var allOrganizations = await _context.Organization.ToListAsync();

            
            ViewBag.Organizations = allOrganizations;

           
            var allInternships = await _context.Internship
                .Include(i => i.Organization)
                .ToListAsync();

            
            ViewBag.AllInternships = allInternships;

            return View(allInternships);

        }


        public async Task<IActionResult> AppliedInternships()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var applicant = await _context.Applicant.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (applicant == null)
            {
                return View("SaveOrApplyInternships");
            }
            var appliedOrganizations = await _context.ApplicantInternship
                .Where(ai => ai.ApplicantId == applicant.Id && ai.IsApplied)
                .Select(ai => ai.Internship.Organization)
                .Distinct()
                .ToListAsync();

            ViewBag.Organizations = appliedOrganizations;

            var appliedInternships = await _context.ApplicantInternship
                .Include(ai => ai.Internship)
                    .ThenInclude(i => i.Organization)
                .Where(ai => ai.Applicant.ApplicationUserId == userId && ai.IsApplied)
                .Select(ai => ai.Internship)
                .ToListAsync();

            if (appliedInternships.Count == 0)
            {
                return View("SaveOrApplyInternships");
            }
            ViewBag.AllInternships = appliedInternships;
            return View(appliedInternships);
        }

        public async Task<IActionResult> SavedInternships()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var applicant = await _context.Applicant.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (applicant == null)
            {
                return View("SaveOrApplyInternships");
            }

            var savedOrganizations = await _context.ApplicantInternship
                .Where(ai => ai.ApplicantId == applicant.Id && ai.IsSaved)
                .Select(ai => ai.Internship.Organization)
                .Distinct()
                .ToListAsync();

            ViewBag.Organizations = savedOrganizations;

            var savedInternships = await _context.ApplicantInternship
                .Include(ai => ai.Internship)
                .ThenInclude(i => i.Organization)
                .Where(ai => ai.Applicant.ApplicationUserId == userId && ai.IsSaved)
                .Select(ai => ai.Internship)
                .ToListAsync();
            if (savedInternships.Count == 0)
            {
                return View("SaveOrApplyInternships");
            }
            ViewBag.AllInternships = savedInternships;
            return View(savedInternships);
        }


        public async Task<IActionResult> Filter(int? organizationId)
        {
            
            var allOrganizations = await _context.Organization.ToListAsync();

            
            ViewBag.Organizations = allOrganizations;

           
            var internshipsQuery = _context.Internship
                .Include(i => i.Organization)
                .AsQueryable();

            
            if (organizationId.HasValue)
            {
                internshipsQuery = internshipsQuery.Where(i => i.OrganizationId == organizationId);
            }

            
            var filteredInternships = await internshipsQuery.ToListAsync();

            
            return PartialView("_InternshipTable", filteredInternships);

        }

        public async Task<IActionResult> RegisterForInternship(int id)
        {

            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var applicant = await _context.Applicant.FirstOrDefaultAsync(a => a.ApplicationUserId == applicationUserId);

            if (applicant == null)
            {

                return RedirectToAction("Create");
            }


            bool isApplied = await _context.ApplicantInternship.AnyAsync(ai => ai.ApplicantId == applicant.Id && ai.InternshipId == id && ai.IsApplied);

            if (isApplied)
            {

                return RedirectToAction("PostedInternship");
            }


            var applicantInternship = new ApplicantInternship
            {
                ApplicantId = applicant.Id,
                InternshipId = id,
                IsApplied = true
            };


            _context.ApplicantInternship.Add(applicantInternship);
            await _context.SaveChangesAsync();


            return RedirectToAction("PostedInternship");
        }


        public async Task<IActionResult> SaveInternshipForFuture(int id)
        {

            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var applicant = await _context.Applicant.FirstOrDefaultAsync(a => a.ApplicationUserId == applicationUserId);

            if (applicant == null)
            {

                return RedirectToAction("Create");
            }


            bool isSaved = await _context.ApplicantInternship.AnyAsync(ai => ai.ApplicantId == applicant.Id && ai.InternshipId == id && ai.IsSaved);

            if (isSaved)
            {

                return RedirectToAction("PostedInternship");
            }


            var applicantInternship = new ApplicantInternship
            {
                ApplicantId = applicant.Id,
                InternshipId = id,
                IsSaved = true
            };


            _context.ApplicantInternship.Add(applicantInternship);
            await _context.SaveChangesAsync();


            return RedirectToAction("PostedInternship");
        }


        // GET: Applicants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }


        public async Task<IActionResult> InternshipDetails(int? id)
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

        // GET: Applicants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Applicants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Department,University,year")] Applicant applicant)
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            applicant.ApplicationUserId = applicationUserId;

            _context.Add(applicant);
             await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index));
           
            
        }

        // GET: Applicants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicant.FindAsync(id);
            if (applicant == null)
            {
                return NotFound();
            }
            return View(applicant);
        }

        // POST: Applicants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Department,University,year")] Applicant applicant)
        {
            if (id != applicant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicantExists(applicant.Id))
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
            return View(applicant);
        }

        // GET: Applicants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicant = await _context.Applicant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicant == null)
            {
                return NotFound();
            }

            return View(applicant);
        }

        // POST: Applicants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicant = await _context.Applicant.FindAsync(id);
            if (applicant != null)
            {
                _context.Applicant.Remove(applicant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicantExists(int id)
        {
            return _context.Applicant.Any(e => e.Id == id);
        }
    }
}
