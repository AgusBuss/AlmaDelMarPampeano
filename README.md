# Alma del Mar Pampeano

Aplicación web en **ASP.NET Core MVC (.NET 10)** para gestionar pedidos de lámparas artesanales: el cliente arma su pedido eligiendo campana y base, adjunta imágenes de referencia o comprobantes, y el administrador gestiona el catálogo de materiales y el estado de cada pedido.

> Trabajo Final Integrador — ASP.NET Core MVC.

## Tecnologías
- ASP.NET Core MVC + Razor, Bootstrap 5, jQuery Validation Unobtrusive
- Entity Framework Core (Code First, migraciones) sobre **SQL Server**
- ASP.NET Core Identity con roles (`Administrador`, `Cliente`)

## Roles
| Rol | Puede |
|---|---|
| Administrador | ABM de materiales de campana, materiales de base y estados; ver y gestionar todos los pedidos |
| Cliente | Crear sus pedidos, agregar lámparas, subir archivos, editar observaciones y borrar pedidos pendientes. No ve pedidos ajenos |

Cualquier persona que se registra desde la web recibe automáticamente el rol **Cliente**.

## Credenciales de prueba
| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | admin@almapampeana.com | Admin123! |
| Cliente | cliente@almapampeana.com | Cliente123! |

## Cómo ejecutarlo en local
1. Requisitos: .NET 10 SDK y SQL Server (LocalDB alcanza).
2. Revisar la cadena `DefaultConnection` en `appsettings.json`.
3. Ejecutar `dotnet run`. Al iniciar, la app aplica las migraciones y crea los roles, los usuarios de prueba y los estados de pedido.

## Estructura
- `Models/` entidades de la base de datos
- `ViewModels/` modelos de entrada para formularios (evitan recibir campos que el usuario no debe tocar)
- `Controllers/` lógica MVC
- `Data/` `ApplicationDbContext`, `DbInitializer` y `AppUserManager`
- `Migrations/` migraciones de EF Core

## Pendiente
- Auditoría de operaciones, módulo de IA (Gemini) y despliegue público.

## Enlaces
- Sistema desplegado: _(completar al desplegar)_
