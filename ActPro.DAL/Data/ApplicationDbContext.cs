using ActPro.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActPro.DAL.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public virtual DbSet<Activity> Activities { get; set; }

        public virtual DbSet<City> Cities { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<Favorite> Favorites { get; set; }

        public virtual DbSet<Place> Places { get; set; }

        public virtual DbSet<PlaceImage> PlaceImages { get; set; }

        public virtual DbSet<Reservation> Reservations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=ALEKSPC\\SQLEXPRESS;Database=ActProDB;Integrated Security=SSPI;TrustServerCertificate=True;MultipleActiveResultSets=true");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.AspNetUserId).HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Place).WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comments_Places");
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Favorites1");

                entity.Property(e => e.AspNetUserId).HasMaxLength(450);

                entity.HasOne(d => d.Place).WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("FK_Favorites1_Places");
            });

            modelBuilder.Entity<Place>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Activity).WithMany(p => p.Places)
                    .HasForeignKey(d => d.ActivityId)
                    .HasConstraintName("FK_Places_Activities");

                entity.HasOne(d => d.City).WithMany(p => p.Places)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_Places_Cities");
            });

            modelBuilder.Entity<PlaceImage>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .HasColumnName("ImageURL");

                entity.HasOne(d => d.Place).WithMany(p => p.PlaceImages)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("FK_PlaceImages_Places");
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.Property(e => e.AspNetUserId).HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.HasOne(d => d.Place).WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("FK_Reservations_Places");
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
