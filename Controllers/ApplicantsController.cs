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
           
            var appliedOrganizations = await _context.ApplicantInternship
                .Where(ai => ai.ApplicationUserId == userId && ai.IsApplied)
                .Select(ai => ai.Internship.Organization)
                .Distinct()
                .ToListAsync();

            ViewBag.Organizations = appliedOrganizations;

            var appliedInternships = await _context.ApplicantInternship
                .Include(ai => ai.Internship)
                    .ThenInclude(i => i.Organization)
                .Where(ai => ai.ApplicationUserId == userId && ai.IsApplied)
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
          
            var savedOrganizations = await _context.ApplicantInternship
                .Where(ai => ai.ApplicationUserId == userId && ai.IsSaved)
                .Select(ai => ai.Internship.Organization)
                .Distinct()
                .ToListAsync();

            ViewBag.Organizations = savedOrganizations;

            var savedInternships = await _context.ApplicantInternship
                .Include(ai => ai.Internship)
                .ThenInclude(i => i.Organization)
                .Where(ai => ai.ApplicationUserId == userId && ai.IsSaved)
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


        public async Task<IActionResult> Apply(int id)
        {
            var internship = _context.Internship.FirstOrDefault(i => i.Id == id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isApplied = await _context.ApplicantInternship.AnyAsync(ai => ai.ApplicationUserId == userId && ai.InternshipId == id && ai.IsApplied);

            if (internship == null)
            {
                return NotFound();
            }

            if (isApplied)
            {
                TempData["successMessage"] = "You have already applied for this internship.";
                return RedirectToAction("AppliedInternships");
            }
           
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId);
 
            ViewBag.FirstName = currentUser.FristName;
            ViewBag.LastName = currentUser.LastName;
            ViewBag.ApplicationUserId = userId;
            ViewBag.InternshipId = id;

            var applicantInternship = new ApplicantInternship();

            return View(applicantInternship);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForInternship(ApplicantInternship applicantInternship)
        {
            
            bool internshipExist = await _context.ApplicantInternship.AnyAsync(ai => ai.ApplicationUserId == applicantInternship.ApplicationUserId && ai.InternshipId == applicantInternship.InternshipId);

            if (internshipExist)
            {
                  var existingApplication = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicantInternship.ApplicationUserId && ai.InternshipId == applicantInternship.InternshipId);

                    existingApplication.FirstName = applicantInternship.FirstName;
                    existingApplication.LastName = applicantInternship.LastName;
                    existingApplication.Department = applicantInternship.Department;
                    existingApplication.University = applicantInternship.University;
                    existingApplication.Year = applicantInternship.Year;
                    existingApplication.IsApplied = true;     
            }
            else
            {
                
                var newApplicantInternship = new ApplicantInternship
                {
                    ApplicationUserId = applicantInternship.ApplicationUserId,
                    InternshipId = applicantInternship.InternshipId,
                    FirstName = applicantInternship.FirstName,
                    LastName = applicantInternship.LastName,
                    Department = applicantInternship.Department,
                    University = applicantInternship.University,
                    Year = applicantInternship.Year,
                    IsApplied = true
                };

                _context.ApplicantInternship.Add(newApplicantInternship);
            }

            
            await _context.SaveChangesAsync();

            TempData["successMessage"] = "You have Applied this internship succesfully";
            return RedirectToAction("PostedInternship", "Applicants");
        }



        public async Task<IActionResult> SaveInternshipForFuture(int id)
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool internshipExist = await _context.ApplicantInternship.AnyAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == id);

            if (internshipExist)
            {
                bool isSaved = await _context.ApplicantInternship.AnyAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == id && ai.IsSaved);
                if (isSaved)
                {
                    TempData["successMessage"] = "You have already saved  this internship.";
                    return RedirectToAction("PostedInternship");
                }
                else
                {
                    
                    var existingInternship = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == id);
                    if (existingInternship != null)
                    {
                        existingInternship.IsSaved = true;
                    }
                }
            }
            else
            {
                // If the internship does not exist, create a new entry and mark it as saved
                var applicantInternship = new ApplicantInternship
                {
                    ApplicationUserId = applicationUserId,
                    InternshipId = id,
                    IsSaved = true
                };
                _context.ApplicantInternship.Add(applicantInternship);
                
            }
            await _context.SaveChangesAsync();
            TempData["successMessage"] = "You have saved this internship succesfully";
            return RedirectToAction("PostedInternship");
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


        public async Task<IActionResult> cancelApplication(int id)
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingInternship = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == id);
            if (existingInternship != null)
            {
                existingInternship.IsApplied = false;

            }

            await _context.SaveChangesAsync();
            return RedirectToAction("PostedInternship");
        }

        public async Task<IActionResult> cancelSaving(int id)
        {
            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingInternship = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == applicationUserId && ai.InternshipId == id);
            if (existingInternship != null)
            {
                existingInternship.IsSaved = false;

            }

            await _context.SaveChangesAsync();
            return RedirectToAction("PostedInternship");
        }

    }
}


