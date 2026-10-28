using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AlmaDelMarPampeano.Data
{
    /// <summary>
    /// UserManager propio: cuando alguien se registra desde la pantalla de Identity
    /// (que llama a CreateAsync(user, password)), se le asigna automáticamente el rol "Cliente".
    /// El DbInitializer crea los usuarios de prueba con CreateAsync(user) (sin contraseña) y
    /// les asigna el rol a mano, por eso este override no se dispara al crear el Administrador.
    /// </summary>
    public class AppUserManager : UserManager<IdentityUser>
    {
        public const string RolCliente = "Cliente";

        public AppUserManager(
            IUserStore<IdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<IdentityUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
                   keyNormalizer, errors, services, logger)
        {
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        {
            var resultado = await base.CreateAsync(user, password);
            if (resultado.Succeeded)
            {
                await AddToRoleAsync(user, RolCliente);
            }
            return resultado;
        }
    }
}
