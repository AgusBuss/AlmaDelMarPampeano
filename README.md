# Alma del Mar Pampeano

Aplicación web en **ASP.NET Core MVC (.NET 10)** para una marca de lámparas artesanales de roca, fósil y madera pampeana (La Pampa, Argentina). El visitante recorre un catálogo con las piezas terminadas, el cliente puede pedir una pieza o diseñar una lámpara a medida y seguir el estado de su pedido, y el administrador gestiona el catálogo, los pedidos y la auditoría del sistema.

> Trabajo Final Integrador — ASP.NET Core MVC.

**Sitio publicado:** https://almadelmarpampeano.runasp.net

> El sitio está en un plan de alojamiento gratuito: la primera visita después de un rato sin uso puede tardar unos segundos en cargar.

## Tecnologías
- ASP.NET Core MVC + Razor (.NET 10), Bootstrap 5, jQuery Validation, SweetAlert2
- Entity Framework Core (Code First, migraciones) sobre **SQL Server**
- ASP.NET Core Identity con roles (`Administrador`, `Cliente`)
- Chatbot con la API de Gemini (Google)

## Funcionalidades

### Para cualquier visitante
- **Inicio** con portada, colecciones, un adelanto de lámparas y los cuadernos de colección (PDF).
- **Página de lámparas** con todas las piezas, filtro por colección y fotos en carrusel.
- **Lumi**, el asistente virtual: responde dudas sobre la marca, las colecciones y cómo pedir.

### Para el cliente
- Registro e ingreso en español (quien se registra recibe el rol Cliente).
- **Pedir una pieza del catálogo** (precio y materiales salen de la pieza) o **diseñar una lámpara a medida** eligiendo campana, base y tipo de luz. Un pedido corresponde a una lámpara.
- Adjuntar imágenes de referencia (JPG, PNG o PDF, hasta 5 MB).
- Ver "Mis pedidos", con una barra de avance y un mensaje que explica qué pasa en cada estado.
- Editar las notas y **cancelar** el pedido mientras esté en estado *Pendiente*.
- Cambiar su contraseña.

### Para el administrador
- ABM de **lámparas** (con varias fotos), campanas, bases y estados de pedido.
- Ver todos los pedidos, filtrarlos por estado y **cambiar su estado**.
- Subir archivos al pedido (comprobantes, presupuestos).
- **Gestión de usuarios:** asignar una contraseña temporal a un cliente que la olvidó.
- **Auditoría:** consulta de todas las altas, modificaciones y bajas, con usuario y fecha.

## Roles
| Rol | Puede |
|---|---|
| Visitante | Ver el catálogo, las colecciones y los cuadernos; usar el chatbot |
| Cliente | Todo lo anterior + pedir, ver y cancelar **sus** pedidos, subir archivos, cambiar su contraseña |
| Administrador | Gestionar el catálogo, los pedidos de todos los clientes, los usuarios y consultar la auditoría |

## Credenciales de prueba
| Rol | Usuario | Contraseña |
|---|---|---|
| Administrador | admin@almapampeana.com | Admin123! |
| Cliente | cliente@almapampeana.com | Cliente123! |

## Seguridad y buenas prácticas
- **ViewModels** en todos los formularios: se reciben solo los campos permitidos (no hay *overposting*).
- **El precio lo calcula siempre el servidor**; nunca se toma del formulario.
- Cada pedido se obtiene con un único punto de acceso que verifica el dueño: un cliente no puede ver ni modificar pedidos ajenos, ni siquiera cambiando la URL.
- Controladores de administración protegidos con `[Authorize(Roles = "Administrador")]`.
- Token antifalsificación (`[ValidateAntiForgeryToken]`) en todos los POST.
- Archivos subidos: extensión validada, tamaño máximo y nombre generado por el servidor (GUID).
- Claves foráneas `Restrict` en los datos del catálogo: no se puede borrar algo que está en uso (por ejemplo, una lámpara con pedidos), y la app lo explica con un mensaje claro.
- **Auditoría automática** en `SaveChangesAsync` de cada entidad relevante.
- Límite de pedidos por minuto al chatbot (*rate limiting*).
- La clave de Gemini **no está en el repositorio** (ver más abajo).

## Cómo ejecutarlo en local
1. Requisitos: **.NET 10 SDK** y SQL Server (LocalDB alcanza).
2. Revisar la cadena `DefaultConnection` en `appsettings.json`.
3. Configurar la clave de Gemini (ver la sección siguiente).
4. Ejecutar `dotnet run`. Al iniciar, la app aplica las migraciones y crea los roles, los usuarios de prueba y los datos base.

### Configurar el chatbot (clave de Gemini)
La clave nunca se sube a GitHub. En desarrollo se guarda con *user-secrets*:

```bash
dotnet user-secrets set "Gemini:ApiKey" "TU_CLAVE"
```

En el servidor, la clave va en un archivo `appsettings.Production.json` que **no está versionado**. Los nombres de los modelos se configuran en `appsettings.json` (`Gemini:Model` y `Gemini:ModelosAlternativos`); si el modelo principal falla, el servicio reintenta con los alternativos.

## Modelo de datos
Entidades principales: `Coleccion`, `Lampara`, `FotoLampara`, `MaterialCampana`, `MaterialBase`, `TipoLuz`, `Pedido`, `DetallePedido`, `ArchivoAdjunto`, `EstadoPedido` y `Auditoria`, además de las tablas de Identity.

Un `Pedido` pertenece a un usuario y tiene un estado. Su `DetallePedido` indica la campana, la base y el tipo de luz, y apunta a la `Lampara` del catálogo cuando se pidió una pieza terminada (queda vacío en las lámparas a medida).

![Diagrama entidad-relación](docs/DER.png)

## Estructura
- `Models/` entidades de la base de datos
- `ViewModels/` modelos de entrada y de vista (evitan recibir campos que el usuario no debe tocar)
- `Controllers/` lógica MVC
- `Services/` integración con Gemini
- `Data/` `ApplicationDbContext` (con la auditoría automática) y `DbInitializer`
- `Areas/Identity/` pantallas de ingreso y registro
- `Migrations/` migraciones de EF Core
- `wwwroot/` estilos, scripts, imágenes y archivos subidos

## Próximos pasos
- Registro de pagos (seña) y cobro con Mercado Pago.
- Enviar avisos por correo cuando cambia el estado del pedido.