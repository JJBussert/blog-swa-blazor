using Microsoft.EntityFrameworkCore;

namespace Db
{
    public class SimpleDbContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=tcp:e13-bdo-test-db.database.windows.net,1433;Initial Catalog=dev;Persist Security Info=False;User ID=bdoadmin;Password=P@ssword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>()
                .HasKey(a => a.AuthorId);

            modelBuilder.Entity<Book>()
                .HasKey(b => b.BookId);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId);
        }
    }

    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Book> Books { get; set; } = null!;
    }

    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }

}