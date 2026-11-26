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
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images"
                    );

                    Directory.CreateDirectory(uploadsFolder);

                    var extension = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
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
        
        // GET: Item/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

// POST: Item/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Items.Update(item);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }
        
        // GET: Item/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

// POST: Item/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        
        public async Task<IActionResult> Index(
            string searchString,
            string type,
            string category,
            string location)
        {
            var items = _context.Items.AsQueryable();

            // Search by item name
            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(i =>
                    i.Title.Contains(searchString));
            }

            // Filter by Lost or Found
            if (!string.IsNullOrEmpty(type))
            {
                items = items.Where(i => i.Type == type);
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                items = items.Where(i =>
                    i.Category.Contains(category));
            }

            // Search by location
            if (!string.IsNullOrEmpty(location))
            {
                items = items.Where(i =>
                    i.Location.Contains(location));
            }

            var result = await items
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            return View(result);
        }

    }
}