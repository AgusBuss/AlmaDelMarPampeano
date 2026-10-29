
using AlmaDelMarPampeano.Data;
using AlmaDelMarPampeano.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Administrador")]
public class MaterialCampanasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MaterialCampanasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MATERIALCAMPANAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.MaterialesCampana.ToListAsync());
    }

    // GET: MATERIALCAMPANAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var materialcampana = await _context.MaterialesCampana
            .FirstOrDefaultAsync(m => m.Id == id);
        if (materialcampana == null)
        {
            return NotFound();
        }

        return View(materialcampana);
    }

    // GET: MATERIALCAMPANAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MATERIALCAMPANAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nombre,HistoriaOrigen,PrecioBase")] MaterialCampana materialcampana)
    {
        if (ModelState.IsValid)
        {
            _context.Add(materialcampana);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(materialcampana);
    }

    // GET: MATERIALCAMPANAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var materialcampana = await _context.MaterialesCampana.FindAsync(id);
        if (materialcampana == null)
        {
            return NotFound();
        }
        return View(materialcampana);
    }

    // POST: MATERIALCAMPANAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,HistoriaOrigen,PrecioBase")] MaterialCampana materialcampana)
    {
        if (id != materialcampana.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(materialcampana);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialCampanaExists(materialcampana.Id))
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
        return View(materialcampana);
    }

    // GET: MATERIALCAMPANAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var materialcampana = await _context.MaterialesCampana
            .FirstOrDefaultAsync(m => m.Id == id);
        if (materialcampana == null)
        {
            return NotFound();
        }

        return View(materialcampana);
    }

    // POST: MATERIALCAMPANAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var materialcampana = await _context.MaterialesCampana.FindAsync(id);
        if (materialcampana == null)
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.MaterialesCampana.Remove(materialcampana);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La campana se eliminó correctamente.";
        }
        catch (DbUpdateException)
        {
            // La base rechaza el borrado si alguna lámpara de algún pedido usa esta campana
            TempData["Error"] = $"No se puede eliminar la campana \"{materialcampana.Nombre}\" porque está incluida en pedidos existentes.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool MaterialCampanaExists(int? id)
    {
        return _context.MaterialesCampana.Any(e => e.Id == id);
    }
}
