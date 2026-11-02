using Microsoft.AspNetCore.Identity;

namespace AlmaDelMarPampeano.Data
{
    // Traduce al español los mensajes de error que genera ASP.NET Core Identity
    // (contraseñas débiles, usuarios repetidos, etc.).
    public class DescriptorErroresIdentity : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new() { Code = nameof(DefaultError), Description = "Ocurrió un error inesperado. Intentá de nuevo." };

        public override IdentityError ConcurrencyFailure() =>
            new() { Code = nameof(ConcurrencyFailure), Description = "Otro usuario modificó este dato al mismo tiempo. Volvé a intentarlo." };

        public override IdentityError PasswordMismatch() =>
            new() { Code = nameof(PasswordMismatch), Description = "La contraseña actual es incorrecta." };

        public override IdentityError InvalidToken() =>
            new() { Code = nameof(InvalidToken), Description = "El código de seguridad no es válido." };

        public override IdentityError LoginAlreadyAssociated() =>
            new() { Code = nameof(LoginAlreadyAssociated), Description = "Ya existe un usuario con ese inicio de sesión." };

        public override IdentityError InvalidUserName(string? userName) =>
            new() { Code = nameof(InvalidUserName), Description = $"El nombre de usuario '{userName}' no es válido. Solo puede tener letras y números." };

        public override IdentityError InvalidEmail(string? email) =>
            new() { Code = nameof(InvalidEmail), Description = $"El correo '{email}' no es válido." };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"El usuario '{userName}' ya está registrado." };

        public override IdentityError DuplicateEmail(string email) =>
            new() { Code = nameof(DuplicateEmail), Description = $"El correo '{email}' ya está registrado." };

        public override IdentityError InvalidRoleName(string? role) =>
            new() { Code = nameof(InvalidRoleName), Description = $"El nombre de rol '{role}' no es válido." };

        public override IdentityError DuplicateRoleName(string role) =>
            new() { Code = nameof(DuplicateRoleName), Description = $"El rol '{role}' ya existe." };

        public override IdentityError UserAlreadyHasPassword() =>
            new() { Code = nameof(UserAlreadyHasPassword), Description = "Este usuario ya tiene una contraseña." };

        public override IdentityError UserLockoutNotEnabled() =>
            new() { Code = nameof(UserLockoutNotEnabled), Description = "El bloqueo de cuenta no está habilitado para este usuario." };

        public override IdentityError UserAlreadyInRole(string role) =>
            new() { Code = nameof(UserAlreadyInRole), Description = $"El usuario ya tiene el rol '{role}'." };

        public override IdentityError UserNotInRole(string role) =>
            new() { Code = nameof(UserNotInRole), Description = $"El usuario no tiene el rol '{role}'." };

        public override IdentityError PasswordTooShort(int length) =>
            new() { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe tener al menos un símbolo (por ejemplo ! o @)." };

        public override IdentityError PasswordRequiresDigit() =>
            new() { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe tener al menos un número (0 a 9)." };

        public override IdentityError PasswordRequiresLower() =>
            new() { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe tener al menos una letra minúscula (a a z)." };

        public override IdentityError PasswordRequiresUpper() =>
            new() { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe tener al menos una letra mayúscula (A a Z)." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"La contraseña debe tener al menos {uniqueChars} caracteres distintos." };

        public override IdentityError RecoveryCodeRedemptionFailed() =>
            new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "El código de recuperación no es válido." };
    }
}