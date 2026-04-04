using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace _66022380.Models.Db;

public partial class BakerydbContext : DbContext
{
    public BakerydbContext()
    {
    }

    public BakerydbContext(DbContextOptions<BakerydbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Historyorder> Historyorders { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderdetail> Orderdetails { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionClaim> PromotionClaims { get; set; }

    public virtual DbSet<PromotionRewardItem> PromotionRewardItems { get; set; }

    public virtual DbSet<UserPromotion> UserPromotions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=bakerydb;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PRIMARY");

            entity.ToTable("address");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.AddressLine).HasMaxLength(255);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.Province).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("address_ibfk_1");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("category");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnType("text");
        });

        modelBuilder.Entity<Historyorder>(entity =>
        {
            entity.HasKey(e => e.HistoryOrderId).HasName("PRIMARY");

            entity.ToTable("historyorder");

            entity.HasIndex(e => e.OrderId, "OrderId");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.ItemSummary).HasColumnType("text");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);

            entity.HasOne(d => d.User).WithMany(p => p.Historyorders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("historyorder_ibfk_1");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PRIMARY");

            entity.ToTable("notification");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.LinkUrl).HasMaxLength(255);
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e => e.ReferenceType).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notification_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("order");

            entity.HasIndex(e => e.AddressId, "AddressId");

            entity.HasIndex(e => e.PromotionId, "PromotionId");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnType("enum('Pending','Paid','Preparing','Shipped','Completed','Cancelled')");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("order_ibfk_2");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("order_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("order_ibfk_1");
        });

        modelBuilder.Entity<Orderdetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PRIMARY");

            entity.ToTable("orderdetail");

            entity.HasIndex(e => e.OrderId, "OrderId");

            entity.HasIndex(e => e.ProductId, "ProductId");

            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.Order).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("orderdetail_ibfk_1");

            entity.HasOne(d => d.Product).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("orderdetail_ibfk_2");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PRIMARY");

            entity.ToTable("promotion");

            entity.HasIndex(e => e.RewardProductId, "RewardProductId");

            entity.Property(e => e.BuyQuantity).HasDefaultValueSql("'0'");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DiscountType).HasColumnType("enum('Percent','Fixed')");
            entity.Property(e => e.DiscountValue).HasPrecision(10, 2);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.IsCombinable).HasDefaultValueSql("'0'");
            entity.Property(e => e.MaxUsePerUser).HasDefaultValueSql("'1'");
            entity.Property(e => e.PromoType).HasDefaultValueSql("'1'");
            entity.Property(e => e.PromotionName).HasMaxLength(100);
            entity.Property(e => e.RequiresProof).HasDefaultValueSql("'0'");
            entity.Property(e => e.RewardQuantity).HasDefaultValueSql("'0'");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne<Stock>()
                .WithMany()
                .HasForeignKey(e => e.RewardProductId)
                .HasConstraintName("promotion_ibfk_1");
        });

        modelBuilder.Entity<PromotionClaim>(entity =>
        {
            entity.HasKey(e => e.ClaimId).HasName("PRIMARY");

            entity.ToTable("promotion_claim");

            entity.HasIndex(e => e.PromotionId, "PromotionId");

            entity.HasIndex(e => e.ReviewedByUserId, "ReviewedByUserId");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.ClaimId).ValueGeneratedOnAdd();
            entity.Property(e => e.Note).HasColumnType("text");
            entity.Property(e => e.ProofImagePath).HasMaxLength(255);
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.ReviewNote).HasColumnType("text");
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasColumnType("enum('Pending','Approved','Rejected')");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionClaims)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("promotion_claim_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.PromotionClaims)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("promotion_claim_ibfk_2");
        });

        modelBuilder.Entity<PromotionRewardItem>(entity =>
        {
            entity.HasKey(e => e.RewardItemId).HasName("PRIMARY");

            entity.ToTable("promotion_reward_item");

            entity.HasIndex(e => e.ProductId, "ProductId");

            entity.HasIndex(e => e.PromotionId, "PromotionId");

            entity.Property(e => e.Quantity).HasDefaultValueSql("'1'");
            entity.Property(e => e.SortOrder).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.Product).WithMany(p => p.PromotionRewardItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("promotion_reward_item_ibfk_2");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionRewardItems)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("promotion_reward_item_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("role");

            entity.Property(e => e.RoleName).HasColumnType("enum('Admin','Staff','User')");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("stock");

            entity.HasIndex(e => e.CategoryId, "CategoryId");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.Stock1)
                .HasDefaultValueSql("'0'")
                .HasColumnName("Stock");

            entity.HasOne(d => d.Category).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("stock_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.RoleId, "RoleId");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.OtpCode).HasMaxLength(10);
            entity.Property(e => e.OtpExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("user_ibfk_1");
        });

        modelBuilder.Entity<UserPromotion>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PromotionId }).HasName("PRIMARY");

            entity.ToTable("user_promotion");

            entity.HasIndex(e => e.PromotionId, "PromotionId");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.IsUsed).HasDefaultValueSql("'0'");
            entity.Property(e => e.UsedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Promotion).WithMany()
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("user_promotion_ibfk_2");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_promotion_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
