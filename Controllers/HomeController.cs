using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlmaDelMarPampeano.Models;

namespace AlmaDelMarPampeano.Controllers;

// La portada del sitio es el catálogo (CatalogoController).
// Este controlador solo maneja las páginas de error.
public class HomeController : Controller
{
    // Errores inesperados del servidor (ver UseExceptionHandler en Program.cs)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Códigos de estado como "no encontrado" (404) o "sin permiso" (403).
    // El código HTTP original se conserva; acá solo se elige el mensaje.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Estado(int codigo)
    {
        (string titulo, string mensaje) = codigo switch
        {
            404 => ("No encontramos esa página", "Puede que el enlace esté mal escrito o que ya no exista."),
            403 => ("No tenés permiso para entrar acá", "Esta sección no está disponible para tu usuario."),
            _ => ("Algo no salió como esperábamos", "Probá de nuevo en unos instantes.")
        };

        ViewData["Codigo"] = codigo;
        ViewData["Titulo"] = titulo;
        ViewData["Mensaje"] = mensaje;
        return View();
    }
}