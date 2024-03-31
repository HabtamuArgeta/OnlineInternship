using InternshipDotCom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipDotCom.Controllers
{
    [Authorize(Roles = "admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Retrieve the existing user from the database
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update user properties
            existingUser.UserName = user.UserName;
            existingUser.FristName = user.FristName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Address = user.Address;
            // Update more user properties as needed

            // Save changes to the database
            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId, bool approved)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Approved = approved;

            var userRoles = await _userManager.GetRolesAsync(user);

            if (user.Approved)
            {
                foreach (var role in userRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role); // Remove from current roles
                }
                await _userManager.AddToRoleAsync(user, user.RegisteredAs); // Add to new role
            }
            else
            {
                foreach (var role in userRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role); // Remove from current roles
                }
                await _userManager.AddToRoleAsync(user, "Pending"); // Add to "Pending" role
            }


            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(); // Return a success response
            }
            else
            {
                return BadRequest("Failed to update user approval status."); // Return a failure response
            }
        }


        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index)); // Redirect to the user list page
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(user); // Return to the delete confirmation page with errors
            }
        }

    }

}
