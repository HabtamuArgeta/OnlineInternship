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
using Microsoft.AspNetCore.Hosting;

namespace InternshipDotCom.Controllers
{
    [Authorize (Roles = "applicant")]
    public class ApplicantsController : Controller
    {
        private readonly ApplicationDbContext _context;


        private readonly IWebHostEnvironment _webHostEnvironment;
        public ApplicantsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;

            _webHostEnvironment = webHostEnvironment;

        
        }



        public async Task<IActionResult> PostedInternship()
        {

            var allOrganizations = await _context.Organization.ToListAsync();


            ViewBag.Organizations = allOrganizations;


            var allInternships = await _context.Internship
                .Include(i => i.Organization)
                .ToListAsync();
            

            ViewBag.AllInternships = allInternships;
            if (allInternships.Count == 0)
            {
                return View("SaveOrApplyInternships");
            }

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


        public async Task<IActionResult> FilterApplied(int? organizationId)
        {
            var allOrganizations = await _context.Organization.ToListAsync();
            ViewBag.Organizations = allOrganizations;

            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appliedInternshipsQuery = _context.ApplicantInternship
                .Where(ai => ai.ApplicationUserId == applicationUserId && ai.IsApplied)
                .Select(ai => ai.InternshipId);
 
            var internshipsQuery = _context.Internship
                .Where(i => appliedInternshipsQuery.Contains(i.Id))
                .Include(i => i.Organization)
                .AsQueryable();

            if (organizationId.HasValue)
            {
                internshipsQuery = internshipsQuery.Where(i => i.OrganizationId == organizationId);
            }

            var filteredInternships = await internshipsQuery.ToListAsync();

            return PartialView("_AppliedInternshipTable", filteredInternships);
        }

        public async Task<IActionResult> FilterSaved(int? organizationId)
        {
            var allOrganizations = await _context.Organization.ToListAsync();
            ViewBag.Organizations = allOrganizations;

            var applicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get saved internships for the current user
            var savedInternshipsQuery = _context.ApplicantInternship
                .Where(ai => ai.ApplicationUserId == applicationUserId && ai.IsSaved)
                .Select(ai => ai.InternshipId);

            // Query internships based on the saved internships for the current user
            var internshipsQuery = _context.Internship
                .Where(i => savedInternshipsQuery.Contains(i.Id))
                .Include(i => i.Organization)
                .AsQueryable();

            // Apply organization filter if provided
            if (organizationId.HasValue)
            {
                internshipsQuery = internshipsQuery.Where(i => i.OrganizationId == organizationId);
            }

            // Get the filtered internships
            var filteredInternships = await internshipsQuery.ToListAsync();

            // Return the filtered internships as a partial view
            return PartialView("_SavedInternshipTable", filteredInternships);
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

            var universities = _context.University.ToList();
            var universityListItems = universities.Select(u => new SelectListItem
            {
                Value = u.id.ToString(), 
                Text = u.name 
            }).ToList();     
            ViewBag.UniversityId = universityListItems;


            var departments  = _context.Department.ToList();
            var departmentsListItems = departments.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(), 
                Text = u.Name 
            }).ToList();
            ViewBag.DepartmentId = departmentsListItems;

            var yearOfstudy = _context.YearOfStudy.ToList();
            var yearOfstudyListItems = yearOfstudy.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Year
            }).ToList();
            ViewBag.YearOfStudyId = yearOfstudyListItems;

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
                existingApplication.DepartmentId = applicantInternship.DepartmentId;
                existingApplication.UniversityId = applicantInternship.UniversityId;
                existingApplication.YearOfStudyId = applicantInternship.YearOfStudyId;
                existingApplication.CoverLetter = applicantInternship.CoverLetter;
                existingApplication.IsApplied = true;

                if (applicantInternship.Resume != null && applicantInternship.Resume.ContentType == "application/pdf")

                {
                    string AbsolutePath = "C:\\Program Files\\installed apps\\Linux comanned\\crzylearning\\DotNet Apps\\InternshipDotCom\\wwwroot";
                    string RelativePath = "/Files/Resume/";
                    RelativePath += Guid.NewGuid().ToString() + "_" + applicantInternship.Resume.FileName;
                    AbsolutePath += RelativePath;
                    existingApplication.ResumePath = RelativePath;

                    string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, AbsolutePath);

                    await applicantInternship.Resume.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));

                }
            }
            else
            {

                var newApplicantInternship = new ApplicantInternship();

                newApplicantInternship.ApplicationUserId = applicantInternship.ApplicationUserId;
                newApplicantInternship.InternshipId = applicantInternship.InternshipId;
                newApplicantInternship.FirstName = applicantInternship.FirstName;
                newApplicantInternship.LastName = applicantInternship.LastName;
                newApplicantInternship.DepartmentId = applicantInternship.DepartmentId;
                newApplicantInternship.UniversityId = applicantInternship.UniversityId;
                newApplicantInternship.YearOfStudyId = applicantInternship.YearOfStudyId;
                newApplicantInternship.CoverLetter = applicantInternship.CoverLetter;
                newApplicantInternship.IsApplied = true;

                if (applicantInternship.Resume != null && applicantInternship.Resume.ContentType == "application/pdf")

                {
                    string AbsolutePath = "C:\\Program Files\\installed apps\\Linux comanned\\crzylearning\\DotNet Apps\\InternshipDotCom\\wwwroot";
                    string RelativePath = "/Files/Resume/";
                    RelativePath += Guid.NewGuid().ToString() + "_" + applicantInternship.Resume.FileName;
                    AbsolutePath += RelativePath;
                    newApplicantInternship.ResumePath = RelativePath;

                    string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, AbsolutePath);

                    await applicantInternship.Resume.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));

                }

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


        public async Task<IActionResult> InternshipDetailsCalledForInterview(int? id)
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
                existingInternship.IsCalledForInterview = false;
                existingInternship.IsInterviewAccepted = false;
                existingInternship.FinishedInternship = false;
                existingInternship.interviewDate = null;
                existingInternship.interviewTime = null;

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



        public async Task<IActionResult> ViewApplicationResponses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var calledInternships = await _context.ApplicantInternship
                .Include(ai => ai.Internship)
                    .ThenInclude(i => i.Organization)
                .Where(ai => ai.ApplicationUserId == userId && ai.IsCalledForInterview && ai.IsApplied)
                .ToListAsync();

            return View(calledInternships);
        }




        public async Task<IActionResult> AcceptInterview(int internshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var applicant = await _context.ApplicantInternship.FirstOrDefaultAsync(ai => ai.ApplicationUserId == userId && ai.InternshipId == internshipId);

            if (applicant == null)
            {
                return NotFound();
            }

            
            applicant.IsInterviewAccepted = true;
            await _context.SaveChangesAsync();

           
            string interviewLink = applicant.InterviewLink;

            return Json(new { success = true, interviewLink = interviewLink });
        }



        public async Task<int> GetPendingInterviewsCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pendingInterviewsCount = await _context.ApplicantInternship
                .Where(ai => ai.ApplicationUserId == userId && ai.IsCalledForInterview && !ai.IsInterviewAccepted)
                .CountAsync();

            return pendingInterviewsCount;
        }



    }
}
    






