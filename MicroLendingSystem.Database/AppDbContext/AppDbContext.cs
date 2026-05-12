using MicroLendingSystem.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroLendingSystem.Database.AppDbContext;

public partial class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Borrower> Borrowers { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<LoanSetting> LoanSettings { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=MicroLoanDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC0784AAD2F9");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.TableName).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AuditLogs__UserI__5FB337D6");
        });

        modelBuilder.Entity<Borrower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Borrower__3214EC07781F367E");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Nrcno)
                .HasMaxLength(50)
                .HasColumnName("NRCNo");
            entity.Property(e => e.PhoneNo).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(100);

            //entity.HasOne<User>()
            //    .WithMany()
            //    .HasForeignKey(e => e.CreatedById)
            //    .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC0797F94FE6");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasMany(d => d.Borrowers)
                .WithOne()
                .HasForeignKey(b => b.DocumentId)
                .HasConstraintName("FK__Borrowers__Docum__534D60F1");
        });

        modelBuilder.Entity<LoanSetting>(entity =>
        {
            entity.ToTable("LoanSettings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.PlanName).HasMaxLength(150);
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrincipalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InterestAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentStatus).HasDefaultValue(1); // 1 = Completed
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Loan).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");

            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(d => d.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Loans__3214EC0771AC5E38");

            entity.HasIndex(e => e.LoanCode, "UQ__Loans__9DCDCA66C41F29A2").IsUnique();

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LoanAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoanCode).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.TotalRepayableAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RemainingBalance).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Borrower).WithMany()
                .HasForeignKey(d => d.BorrowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Loans__BorrowerI__5812160E");

            entity.HasOne(d => d.LoanSetting).WithMany(p => p.Loans)
                .HasForeignKey(d => d.LoanSettingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0746C80BE4");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105348EE33B05").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasColumnName("Password")
                .HasMaxLength(512);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedRbac(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private static void SeedRbac(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Staff" },
            new Role { Id = 3, Name = "Borrower" });

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = "Loan Read" },
            new Permission { Id = 2, Name = "Loan Create" },
            new Permission { Id = 3, Name = "Loan Approve" },
            new Permission { Id = 4, Name = "Loan Repay" },
            new Permission { Id = 5, Name = "Loan Update" },
            new Permission { Id = 6, Name = "Loan Delete" },
            new Permission { Id = 7, Name = "User Read" },
            new Permission { Id = 8, Name = "User Create" },
            new Permission { Id = 9, Name = "User Update" },
            new Permission { Id = 10, Name = "User Delete" },
            new Permission { Id = 11, Name = "User Assign Role" },
            new Permission { Id = 12, Name = "Role Read" },
            new Permission { Id = 13, Name = "Role Create" },
            new Permission { Id = 14, Name = "Role Update" },
            new Permission { Id = 15, Name = "Role Delete" },
            new Permission { Id = 16, Name = "Role Assign Permissions" },
            new Permission { Id = 17, Name = "Permission Read" },
            new Permission { Id = 18, Name = "Permission Create" },
            new Permission { Id = 19, Name = "Permission Update" },
            new Permission { Id = 20, Name = "Permission Delete" },
            new Permission { Id = 21, Name = "Borrower Read" },
            new Permission { Id = 22, Name = "Borrower Create" },
            new Permission { Id = 23, Name = "Borrower Update" },
            new Permission { Id = 24, Name = "Borrower Delete" },
            new Permission { Id = 25, Name = "Loan Setting Read" },
            new Permission { Id = 26, Name = "Loan Setting Create" },
            new Permission { Id = 27, Name = "Loan Setting Update" },
            new Permission { Id = 28, Name = "Loan Setting Delete" },
            new Permission { Id = 29, Name = "Transaction List" },
            new Permission { Id = 30, Name = "Loan Request List" },
            new Permission { Id = 31, Name = "Transaction Export" });

        var adminRolePermissions = Enumerable.Range(1, 31)
            .Select(i => new RolePermission { RoleId = 1, PermissionId = i })
            .ToArray();

        // Staff: loans (list/read/create/update/repay), borrowers (read/create/update), loan settings read, transactions list, loan request list.
        var staffPermissionIds = new[] { 1, 2, 4, 5, 6, 21, 22, 23, 25, 29, 30 };
        var staffRolePermissions = staffPermissionIds
            .Select(pid => new RolePermission { RoleId = 2, PermissionId = pid })
            .ToArray();

        // Borrower: own loans (list/read/create/repay), profile read, transactions.
        var borrowerPermissionIds = new[] { 1, 2, 4, 21, 29, 30 };
        var borrowerRolePermissions = borrowerPermissionIds
            .Select(pid => new RolePermission { RoleId = 3, PermissionId = pid })
            .ToArray();

        modelBuilder.Entity<RolePermission>().HasData(
            adminRolePermissions.Concat(staffRolePermissions).Concat(borrowerRolePermissions).ToArray());
    }
}
