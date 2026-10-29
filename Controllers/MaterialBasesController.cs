
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.Data;

[Authorize(Roles = "Administrador")]
public class MaterialBasesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MaterialBasesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MATERIALBASES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.MaterialesBase.ToListAsync());
    }

    // GET: MATERIALBASES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var materialbase = await _context.MaterialesBase
            .FirstOrDefaultAsync(m => m.Id == id);
        if (materialbase == null)
        {
            return NotFound();
        }

        return View(materialbase);
    }

    // GET: MATERIALBASES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MATERIALBASES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,TipoMaterial,Descripcion")] MaterialBase materialbase)
    {
        if (ModelState.IsValid)
        {
            _context.Add(materialbase);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(materialbase);
    }

    // GET: MATERIALBASES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var materialbase = await _context.MaterialesBase.FindAsync(id);
        if (materialbase == null)
        {
            return NotFound();
        }
        return View(materialbase);
    }

    // POST: MATERIALBASES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,TipoMaterial,Descripcion")] MaterialBase materialbase)
    {
        if (id != materialbase.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(materialbase);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialBaseExists(materialbase.Id))
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
        return View(materialbase);
    }

    // GET: MATERIALBASES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var materialbase = await _context.MaterialesBase
            .FirstOrDefaultAsync(m => m.Id == id);
        if (materialbase == null)
        {
            return NotFound();
        }

        return View(materialbase);
    }

    // POST: MATERIALBASES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var materialbase = await _context.MaterialesBase.FindAsync(id);
        if (materialbase == null)
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.MaterialesBase.Remove(materialbase);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La base se eliminó correctamente.";
        }
        catch (DbUpdateException)
        {
            // La base rechaza el borrado si alguna lámpara de algún pedido usa esta base
            TempData["Error"] = $"No se puede eliminar la base \"{materialbase.TipoMaterial}\" porque está incluida en pedidos existentes.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool MaterialBaseExists(int? id)
    {
        return _context.MaterialesBase.Any(e => e.Id == id);
    }
}