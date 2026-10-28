
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AlmaDelMarPampeano.Models;
using AlmaDelMarPampeano.Data;

[Authorize(Roles = "Administrador")]
public class EstadoPedidosController : Controller
{
    private readonly ApplicationDbContext _context;

    public EstadoPedidosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ESTADOPEDIDOS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.EstadosPedido.ToListAsync());
    }

    // GET: ESTADOPEDIDOS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var estadopedido = await _context.EstadosPedido
            .FirstOrDefaultAsync(m => m.Id == id);
        if (estadopedido == null)
        {
            return NotFound();
        }

        return View(estadopedido);
    }

    // GET: ESTADOPEDIDOS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ESTADOPEDIDOS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NombreEstado")] EstadoPedido estadopedido)
    {
        if (ModelState.IsValid)
        {
            _context.Add(estadopedido);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(estadopedido);
    }

    // GET: ESTADOPEDIDOS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var estadopedido = await _context.EstadosPedido.FindAsync(id);
        if (estadopedido == null)
        {
            return NotFound();
        }
        return View(estadopedido);
    }

    // POST: ESTADOPEDIDOS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,NombreEstado")] EstadoPedido estadopedido)
    {
        if (id != estadopedido.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(estadopedido);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstadoPedidoExists(estadopedido.Id))
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
        return View(estadopedido);
    }

    // GET: ESTADOPEDIDOS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var estadopedido = await _context.EstadosPedido
            .FirstOrDefaultAsync(m => m.Id == id);
        if (estadopedido == null)
        {
            return NotFound();
        }

        return View(estadopedido);
    }

    // POST: ESTADOPEDIDOS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var estadopedido = await _context.EstadosPedido.FindAsync(id);
        if (estadopedido != null)
        {
            _context.EstadosPedido.Remove(estadopedido);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EstadoPedidoExists(int? id)
    {
        return _context.EstadosPedido.Any(e => e.Id == id);
    }
}
