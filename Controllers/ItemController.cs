using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FindBack.Data;
using FindBack.Models;

namespace FindBack.Controllers
{
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.ToListAsync();

            return View(items);
        }
    }
}