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

        public async Task<IActionResult> RegisterForInternship(int internshipId)
        {
            var internship = await _context.Internship.FindAsync(internshipId);

            if (internship == null)
            {
                return NotFound();
            }

            var applicantId  = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            var applicant = await _context.Applicant.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound();
            }

            //var application = new InternshipApplication
            //{
            //    //ApplicantId = applicantId,
            //    InternshipId = internshipId,
            //    IsSaved = false ,
            //    IsRegisterd = true
            //};

            //_context.InternshipApplication.Add(application);
            //await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SaveForFuture(int internshipId)
        {
            var internship = await _context.Internship.FindAsync(internshipId);
            if (internship == null)
            {
                return NotFound();
            }

            var applicantId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var applicant = await _context.Applicant.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound();
            }

            //var application = new InternshipApplication
            //{
            //    //ApplicantId = applicantId,
            //    InternshipId = internshipId,
            //    IsSaved = true ,
            //    IsRegisterd = false
            //};

            //_context.InternshipApplication.Add(application);
            //await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
            if (ModelState.IsValid)
            {
                _context.Add(applicant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(applicant);
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
