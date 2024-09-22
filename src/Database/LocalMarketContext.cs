using Microsoft.EntityFrameworkCore;

namespace portal_web.Database
{
    public class LocalMarketContext : DbContext
    {
        public LocalMarketContext(DbContextOptions<LocalMarketContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
