using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;
using RentEasy.Models;
using System.Security.Claims;

namespace RentEasy.Controllers
{

    public class AccountController : Controller
    {
        private readonly RentEasyContext _reDbContext;

        public AccountController(RentEasyContext reDbContext)
        {
            _reDbContext = reDbContext;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If there are validation errors, redisplay the form (password fields will be cleared as expected)
                return View(model);
            }

            // Check if username already exists
            bool userExists = await _reDbContext.Users.AnyAsync(u => u.Username == model.Username);
            if (userExists)
            {
                ModelState.AddModelError("", "Username or Email already in use.");
                return View(model);
            }

            // Create new user with plain text password
            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                Role = model.Role
            };

            _reDbContext.Users.Add(user);
            await _reDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        // Login 
        [HttpPost]
        public async Task<IActionResult> Login(string identifier, string password)
        {
            var user = await _reDbContext.Users.FirstOrDefaultAsync(u => (u.Username == identifier ) && u.Password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid credentials.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
