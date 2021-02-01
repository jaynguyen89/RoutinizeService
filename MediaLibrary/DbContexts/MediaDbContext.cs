using MediaLibrary.Models;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace MediaLibrary.DbContexts
{
    public partial class MediaDbContext : DbContext
    {
        public MediaDbContext()
        {
        }

        public MediaDbContext(DbContextOptions<MediaDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<Userphoto> Userphotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Photo>(entity =>
            {
                entity.ToTable("photos");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Location).HasMaxLength(150);

                entity.Property(e => e.PhotoName).HasMaxLength(150);
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.ToTable("tokens");

                entity.Property(e => e.TokenId).HasColumnType("int(11)");

                entity.Property(e => e.Life)
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'5'");

                entity.Property(e => e.Target).HasMaxLength(70);

                entity.Property(e => e.TokenString)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Userphoto>(entity =>
            {
                entity.ToTable("userphotos");

                entity.HasIndex(e => e.PhotoId, "userphotos_ibfk_1");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.HidrogenianId).HasColumnType("int(11)");

                entity.Property(e => e.IsAvatar).HasDefaultValueSql("'0'");

                entity.Property(e => e.IsCover).HasDefaultValueSql("'0'");

                entity.Property(e => e.PhotoId).HasColumnType("int(11)");

                entity.HasOne(d => d.Photo)
                    .WithMany(p => p.Userphotos)
                    .HasForeignKey(d => d.PhotoId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("userphotos_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
