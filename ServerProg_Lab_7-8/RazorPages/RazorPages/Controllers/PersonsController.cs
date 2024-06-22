using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPages.Data;
using RazorPages.Models;

namespace RazorPages.Controllers
{
    public class PersonsIndexData
    {
        public IEnumerable<Person> Persons { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPersonsNumber { get; set; }
        public string? Search { get; set; }
    }

    public class PersonsController : Controller
    {
        private readonly MoviesContext _context;
        private int pageSize = 15;

        public PersonsController(MoviesContext context)
        {
            _context = context;
        }

        // GET: Persons
        public async Task<IActionResult> Index(int? page, string? search)
        {
            var pageValue = page ?? 1;
            if (_context.Persons != null)
            {
                IQueryable<Person> allQuery = _context.Persons;
                if (search != null)
                {
                    allQuery = allQuery.Where(x => x.Name.ToLower().Contains(search.ToLower()));
                }
                var portionQuery = allQuery.OrderBy(x => x.Id).Skip((pageValue - 1) * pageSize).Take(pageSize);
                var data = new PersonsIndexData()
                {
                    PageSize = pageSize,
                    CurrentPage = pageValue,
                    TotalPersonsNumber = allQuery.Count(),
                    Search = search,
                    Persons = await portionQuery.ToListAsync()
                };
                return View(data);
            }
            else
            {
                return Problem("Entity set 'MoviesContext.Persons'  is null."); ;
            }
        }

        // GET: Persons/Details/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Persons == null)
            {
                return NotFound();
            }

            var person = await _context.Persons.Include("Movies")
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: Persons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Persons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Person person)
        {
            if (ModelState.IsValid)
            {
                person.Id = _context.Persons.Select(m => m.Id).Max() + 1;
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        // GET: Persons/Edit/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Persons == null)
            {
                return NotFound();
            }

            var person = await _context.Persons.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: Persons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.Id))
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
            return View(person);
        }

        // GET: Persons/Delete/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Persons == null)
            {
                return NotFound();
            }

            var person = await _context.Persons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Persons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Persons == null)
            {
                return Problem("Entity set 'MoviesContext.Persons'  is null.");
            }
            var person = await _context.Persons.FindAsync(id);
            if (person != null)
            {
                var mps = _context.MoviePersons.Where(x => x.PersonId == person.Id);
                if (mps.Any())
                {
                    _context.MoviePersons.RemoveRange(mps);
                    await _context.SaveChangesAsync();
                }
                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
            }
           
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
          return (_context.Persons?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
