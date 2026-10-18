using GYM.Application.Abstractions;

namespace GYM.Infrastructure.Services;

internal sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        // Requires BCrypt.Net-Next package (BCrypt.Net.BCrypt)
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        // Requires BCrypt.Net-Next package (BCrypt.Net.BCrypt)
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
