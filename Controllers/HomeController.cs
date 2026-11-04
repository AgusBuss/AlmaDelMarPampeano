using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlmaDelMarPampeano.Models;

namespace AlmaDelMarPampeano.Controllers;

// La portada del sitio es el catálogo (CatalogoController).
// Este controlador solo queda para la página de error (ver UseExceptionHandler en Program.cs).
public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}