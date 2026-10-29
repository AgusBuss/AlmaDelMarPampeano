# Alma del Mar Pampeano

Aplicación web en **ASP.NET Core MVC (.NET 10)** para gestionar pedidos de lámparas artesanales de la marca *Alma del Mar Pampeano* (La Pampa, Argentina). El cliente arma su pedido eligiendo campana y base, adjunta imágenes de referencia o comprobantes y sigue el estado de su pedido. El administrador gestiona el catálogo de materiales, el estado de cada pedido y consulta la auditoría del sistema.

> Trabajo Final Integrador — ASP.NET Core MVC.

## Tecnologías
- ASP.NET Core MVC + Razor, Bootstrap 5, jQuery Validation Unobtrusive, SweetAlert2
- Entity Framework Core (Code First, migraciones) sobre **SQL Server**
- ASP.NET Core Identity con roles (`Administrador`, `Cliente`)

## Funcionalidades
- **Autenticación y autorización:** ingreso y registro en español; quien se registra recibe automáticamente el rol Cliente.
- **Catálogo (solo Administrador):** ABM de campanas (con precio), bases (con precio fijo por tipo) y estados de pedido.
- **Pedidos:** el cliente crea pedidos, agrega y quita lámparas (el total lo calcula el servidor: campana + base, por cantidad) y sube archivos (JPG, PNG o PDF, hasta 5 MB).
- **Seguridad:** un cliente solo ve y modifica sus propios pedidos; solo puede borrarlos o quitarles lámparas mientras estén en estado "Pendiente".
- **Integridad de datos:** no se puede borrar una campana, base o estado que esté en uso, ni borrar o renombrar el estado "Pendiente".
- **Auditoría:** cada alta, modificación o baja queda registrada con el usuario, la operación y la fecha. El administrador la consulta (con filtro por usuario) en el menú "Auditoría".
- **Interfaz:** mensajes en español, validaciones de cliente y servidor, modales de confirmación con SweetAlert2 y menú según el rol.

## Roles
| Rol | Puede |
|---|---|
| Administrador | ABM de campanas, bases y estados; ver y gestionar todos los pedidos; ver la auditoría |
| Cliente | Crear sus pedidos, agregar o quitar lámparas, subir archivos, editar observaciones y borrar pedidos pendientes. No ve pedidos ajenos |

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
- `Data/` `ApplicationDbContext` (con la auditoría automática), `DbInitializer` y `AppUserManager`
- `Areas/Identity/` pantallas de ingreso y registro
- `Migrations/` migraciones de EF Core

## Próximos pasos
- Tipo de luz, lámparas personalizadas, colecciones y catálogo de lámparas terminadas.
- Registro de pagos (seña) con Mercado Pago.
- Módulo de IA (chatbot con Gemini) y despliegue público.
- Identidad visual de la marca.

## Enlaces
- Sistema desplegado: _(completar al desplegar)_