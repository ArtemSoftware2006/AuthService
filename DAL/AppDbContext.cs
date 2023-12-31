using Domain.entity;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public virtual  DbSet<User> Users { get; set; }
        public virtual  DbSet<RefreshToken> RefreshTokens { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {   }
        /// <summary>
        /// Только для тестов
        /// </summary>
        public AppDbContext()
        {   }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   }
    }
}