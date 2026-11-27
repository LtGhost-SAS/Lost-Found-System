
using FindBack.Data;
using FindBack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FindBack.Controllers
{
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ItemController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // 1. INDEX - View All Items + Search & Filter
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            string? type,
            string? category,
            string? location)
        {
            var items = _context.Items.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                items = items.Where(i =>
                    i.Title.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                items = items.Where(i =>
                    i.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                items = items.Where(i =>
                    i.Category.Contains(category));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                items = items.Where(i =>
                    i.Location.Contains(location));
            }

            var result = await items
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            return View(result);
        }


        // ==========================================
        // 2. DETAILS - View Single Item
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }


        // ==========================================
        // 3. CREATE - GET
        // ==========================================

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            var item = new Item
            {
                Date = DateTime.Today
            };

            return View(item);
        }


        // ==========================================
        // 4. CREATE - POST with Image Upload
        // ==========================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Item item,
            IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Get logged-in user's ID
                item.UserId = _userManager.GetUserId(User);

                // Image Upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var allowedExtensions = new[]
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".webp"
                    };

                    var extension = Path
                        .GetExtension(imageFile.FileName)
                        .ToLowerInvariant();

                    // Validate extension
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(
                            "imageFile",
                            "Only JPG, JPEG, PNG and WEBP images are allowed."
                        );

                        return View(item);
                    }

                    // Maximum image size: 5 MB
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "imageFile",
                            "Image size cannot exceed 5 MB."
                        );

                        return View(item);
                    }

                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "images"
                    );

                    Directory.CreateDirectory(uploadsFolder);

                    // Generate unique file name
                    var fileName = Guid.NewGuid().ToString()
                                   + extension;

                    var filePath = Path.Combine(
                        uploadsFolder,
                        fileName
                    );

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    item.ImageUrl = "/images/" + fileName;
                }

                _context.Items.Add(item);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }


        // ==========================================
        // 5. EDIT - GET
        // Only Owner Can Edit
        // ==========================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (item.UserId != userId)
            {
                return Forbid();
            }

            return View(item);
        }


        // ==========================================
        // 6. EDIT - POST
        // Only Owner Can Update
        // ==========================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            var existingItem = await _context.Items.FindAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (existingItem.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Update allowed properties only
                existingItem.Title = item.Title;
                existingItem.Description = item.Description;
                existingItem.Category = item.Category;
                existingItem.Location = item.Location;
                existingItem.Date = item.Date;
                existingItem.Type = item.Type;
                existingItem.ContactName = item.ContactName;
                existingItem.ContactEmail = item.ContactEmail;
                existingItem.IsReturned = item.IsReturned;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }


        // ==========================================
        // 7. DELETE - GET
        // Only Owner Can Access
        // ==========================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (item.UserId != userId)
            {
                return Forbid();
            }

            return View(item);
        }


        // ==========================================
        // 8. DELETE - POST
        // Only Owner Can Delete
        // ==========================================

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (item.UserId != userId)
            {
                return Forbid();
            }

            _context.Items.Remove(item);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReturned(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (item.UserId != userId)
                return Forbid();

            item.IsReturned = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
