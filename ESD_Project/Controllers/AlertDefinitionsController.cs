using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ESD_Project.Data;
using ESD_Project.Models;

namespace ESD_Project.Controllers
{
    public class AlertDefinitionsController : Controller
    {
        private readonly AppDbContext _context;

        public AlertDefinitionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AlertDefinitions
        public async Task<IActionResult> Index(string? search)
        {
            // retrieve all definitions
            var query = _context.AlertDefinitions.AsQueryable();

            // if a search term was provided, filter by Name or TargetId
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.Name.Contains(search) ||
                    r.TargetId.Contains(search));
            }

            // pass the raw search term back to the view
            ViewData["Search"] = search;

            var list = await query.ToListAsync();
            return View(list);
        }


        // GET: AlertDefinitions/CreateBayRule
        public IActionResult CreateBayRule()
        {
            ViewData["SelectionList"] = new SelectList(new[] { "Bay-01", "Bay-02", "Bay-03" });
            ViewData["RoleList"] = new SelectList(
                                         new[] { "Engineer1", "Engineer2", "Engineer3" },
                                         selectedValue: null
                                       );
            var model = new AlertDefinition { Type = AlertType.BayPower };
            return View("Create", model);
        }

        // GET: AlertDefinitions/CreateCapacityRule
        public IActionResult CreateCapacityRule()
        {
            ViewData["SelectionList"] = new SelectList(new[] { "Train1", "Train2", "Train3", "Train4", "Train5" });
            ViewData["RoleList"] = new SelectList(
                                         new[] { "Staff1", "Staff2", "Staff3" },
                                         selectedValue: null
                                       );
            var model = new AlertDefinition { Type = AlertType.CapacityUtilization };
            return View("Create", model);
        }
        // POST: AlertDefinitions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlertDefinition alertDefinition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alertDefinition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // **very important**: put them back for the redisplayed form
            if (alertDefinition.Type == AlertType.BayPower)
            {
                ViewData["SelectionList"] = new SelectList(
                    new[] { "Bay-01", "Bay-02", "Bay-03" }, alertDefinition.TargetId);
                ViewData["RoleList"] = new SelectList(
                    new[] { "Engineer1", "Engineer2", "Engineer3" }, alertDefinition.Role);
            }
            else
            {
                ViewData["SelectionList"] = new SelectList(
                    new[] { "Train1", "Train2", "Train3", "Train4", "Train5" }, alertDefinition.TargetId);
                ViewData["RoleList"] = new SelectList(
                    new[] { "Staff1", "Staff2", "Staff3" }, alertDefinition.Role);
            }

            return View("Create", alertDefinition);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var m = await _context.AlertDefinitions.FindAsync(id);
            if (m == null) return NotFound();
            PopulateSelectionList(m.Type, m.TargetId);
            PopulateRoleList(m.Type);
            return View(m);
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Role,Threshold,MessageTemplate,TargetId")] AlertDefinition m)
        {
            if (id != m.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(m); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
                catch (DbUpdateConcurrencyException) { if (!_context.AlertDefinitions.Any(e => e.Id == id)) return NotFound(); throw; }
            }
            // invalid => repopulate
            PopulateSelectionList(m.Type, m.TargetId);
            PopulateRoleList(m.Type);
            return View(m);
        }

        // GET: AlertDefinitions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var alertDefinition = await _context.AlertDefinitions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alertDefinition == null) return NotFound();

            return View(alertDefinition);
        }

        // POST: AlertDefinitions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alertDefinition = await _context.AlertDefinitions.FindAsync(id);
            if (alertDefinition != null)
            {
                _context.AlertDefinitions.Remove(alertDefinition);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: AlertDefinitions
        // GET: AlertDefinitions
        // Now has a single Index that accepts an optional search

        private void PopulateSelectionList(AlertType t, string? selected)
        {
            if (t == AlertType.BayPower)
                ViewData["SelectionList"] = new SelectList(new[] { "Bay-01", "Bay-02", "Bay-03" }, selected);
            else
                ViewData["SelectionList"] = new SelectList(new[] { "Train1", "Train2", "Train3", "Train4", "Train5" }, selected);
        }
        private void PopulateRoleList(AlertType t)
        {
            if (t == AlertType.BayPower)
                ViewData["RoleList"] = new SelectList(new[] { "Engineer1", "Engineer2", "Engineer3" }, selectedValue: null);
            else
                ViewData["RoleList"] = new SelectList(new[] { "Staff1", "Staff2", "Staff3" }, selectedValue: null);
        }

    }
}


