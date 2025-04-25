using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RestaurantDomain.Models;

namespace RestaurantInfrastructure
{
    public partial class RestaurantDbContext : IdentityDbContext<ApplicationUser>
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Cook> Cooks { get; set; }
        public virtual DbSet<Dish> Dishes { get; set; }
        public virtual DbSet<Ingredient> Ingredients { get; set; }
        public virtual DbSet<Restaurant> Restaurants { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07DBFD7161");
                entity.Property(e => e.Description).HasMaxLength(100);
            });

            modelBuilder.Entity<Cook>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Cooks__3214EC075F8F54F4");
                entity.Property(e => e.Surname).HasMaxLength(50);

                entity.HasOne(d => d.Restaurant)
                    .WithMany(p => p.Cooks)
                    .HasForeignKey(d => d.RestaurantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Cooks__Restauran__3B75D760");

                entity.HasMany(d => d.Dishes)
                    .WithMany(p => p.Cooks)
                    .UsingEntity<Dictionary<string, object>>(
                        "CookDishes",
                        r => r.HasOne<Dish>()
                            .WithMany()
                            .HasForeignKey("DishId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__CookDishe__DishI__440B1D61"),
                        l => l.HasOne<Cook>()
                            .WithMany()
                            .HasForeignKey("CookId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__CookDishe__CookI__4316F928"),
                        j =>
                        {
                            j.HasKey("CookId", "DishId").HasName("PK__CookDish__F4903394A5BC686B");
                        });
            });

            modelBuilder.Entity<Dish>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Dishes__3214EC07E6E03191");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Receipt).HasMaxLength(500);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Dishes)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Dishes__Category__3E52440B");

                entity.HasMany(d => d.Ingredients)
                    .WithMany(p => p.Dishes)
                    .UsingEntity<Dictionary<string, object>>(
                        "DishIngredients",
                        r => r.HasOne<Ingredient>()
                            .WithMany()
                            .HasForeignKey("IngredientId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__DishIngre__Ingre__47DBAE45"),
                        l => l.HasOne<Dish>()
                            .WithMany()
                            .HasForeignKey("DishId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("FK__DishIngre__DishI__46E78A0C"),
                        j =>
                        {
                            j.HasKey("DishId", "IngredientId").HasName("PK__DishIngr__A369A4752A665EE3");
                        });
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Ingredie__3214EC07DD41F209");
                entity.Property(e => e.Name).HasMaxLength(100);

                // Налаштування конвертації для WeightMeasure
                var weightMeasureConverter = new ValueConverter<string?, decimal?>(
                    v => v == null ? null : decimal.Parse(v), // Конвертація string -> decimal
                    v => v.HasValue ? v.Value.ToString("F2") : null // Конвертація decimal -> string
                );

                entity.Property(e => e.WeightMeasure)
                      .HasColumnType("decimal(10, 2)") // Вказуємо тип у базі даних
                      .HasConversion(weightMeasureConverter); // Додаємо конвертер
            });

            modelBuilder.Entity<Restaurant>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Restaura__3214EC0721DA1F08");
                entity.Property(e => e.Contacts).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Reviews).HasMaxLength(500);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}