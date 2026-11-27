using FindBack.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FindBack.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard Home
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var myItems = await _context.Items
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            // Dashboard Statistics
            ViewBag.TotalItems = myItems.Count;

            ViewBag.LostItems = myItems.Count(
                i => i.Type == "Lost");

            ViewBag.FoundItems = myItems.Count(
                i => i.Type == "Found");

            ViewBag.ReturnedItems = myItems.Count(
                i => i.IsReturned);

            return View(myItems);
        }
    }
}