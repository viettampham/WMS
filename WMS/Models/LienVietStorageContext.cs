using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WMS.Models;

public partial class LienVietStorageContext : DbContext
{
    public LienVietStorageContext()
    {
    }

    public LienVietStorageContext(DbContextOptions<LienVietStorageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audit> Audits { get; set; }

    public virtual DbSet<AuditDetail> AuditDetails { get; set; }

    public virtual DbSet<Cont> Conts { get; set; }

    public virtual DbSet<ContImage> ContImages { get; set; }

    public virtual DbSet<CtpatCriterion> CtpatCriteria { get; set; }

    public virtual DbSet<CtpatTopic> CtpatTopics { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DetechIssue> DetechIssues { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<OperationStandard> OperationStandards { get; set; }

    public virtual DbSet<PathConfig> PathConfigs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost;Database=LienVietStorage;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.ToTable("Audit");

            entity.Property(e => e.Auditor).HasMaxLength(50);
            entity.Property(e => e.DateCreate).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<AuditDetail>(entity =>
        {
            entity.ToTable("AuditDetail");

            entity.Property(e => e.Assignee).HasMaxLength(50);
            entity.Property(e => e.DateCreate).HasColumnType("datetime");
            entity.Property(e => e.DetechIssue).HasMaxLength(300);
            entity.Property(e => e.DoneTime).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Evalue).HasMaxLength(10);
            entity.Property(e => e.ImageAfter).HasMaxLength(300);
            entity.Property(e => e.ImageBefore).HasMaxLength(300);
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(300);
            entity.Property(e => e.OperationStandard).HasMaxLength(300);
            entity.Property(e => e.Owner).HasMaxLength(50);
            entity.Property(e => e.PlanAction).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Cont>(entity =>
        {
            entity.ToTable("Cont");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NguoiKhaiBao).HasMaxLength(50);
            entity.Property(e => e.SoBooking).HasMaxLength(50);
            entity.Property(e => e.SoCont).HasMaxLength(50);
        });

        modelBuilder.Entity<ContImage>(entity =>
        {
            entity.ToTable("Cont_Image");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Idcont).HasColumnName("IDCont");
            entity.Property(e => e.NguoiChup).HasMaxLength(50);
            entity.Property(e => e.Path).HasMaxLength(200);
            entity.Property(e => e.ThoiGianChup).HasColumnType("datetime");
        });

        modelBuilder.Entity<CtpatCriterion>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Criteria).HasMaxLength(300);
            entity.Property(e => e.Idtopic).HasColumnName("IDTopic");
            entity.Property(e => e.Note).HasMaxLength(300);
        });

        modelBuilder.Entity<CtpatTopic>(entity =>
        {
            entity.ToTable("CtpatTopic");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.TakeOn).HasMaxLength(20);
            entity.Property(e => e.Topic).HasMaxLength(300);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.Property(e => e.DepartmentName).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(100);
        });

        modelBuilder.Entity<DetechIssue>(entity =>
        {
            entity.ToTable("DetechIssue");

            entity.Property(e => e.CodeIssue).HasMaxLength(10);
            entity.Property(e => e.DetechIssue1)
                .HasMaxLength(300)
                .HasColumnName("DetechIssue");
            entity.Property(e => e.Type).HasMaxLength(10);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");

            entity.Property(e => e.Location1)
                .HasMaxLength(50)
                .HasColumnName("Location");
            entity.Property(e => e.Note).HasMaxLength(300);
        });

        modelBuilder.Entity<OperationStandard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangMuc");

            entity.ToTable("OperationStandard");

            entity.Property(e => e.Note).HasMaxLength(300);
            entity.Property(e => e.OperationStandard1)
                .HasMaxLength(300)
                .HasColumnName("OperationStandard");
        });

        modelBuilder.Entity<PathConfig>(entity =>
        {
            entity.ToTable("Path_Config");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PathLocation).HasMaxLength(300);
            entity.Property(e => e.PathName).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleLevel).HasName("PK_Role_1");

            entity.ToTable("Role");

            entity.Property(e => e.RoleLevel).ValueGeneratedNever();
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.Cccd)
                .HasMaxLength(50)
                .HasColumnName("CCCD");
            entity.Property(e => e.CreateAt).HasColumnType("datetime");
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.DeleteAt).HasColumnType("datetime");
            entity.Property(e => e.DeleteBy).HasMaxLength(50);
            entity.Property(e => e.EditAt).HasColumnType("datetime");
            entity.Property(e => e.EditBy).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.LockedUntil).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PhongBan).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.RoleLevelNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
