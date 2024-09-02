using System;
using System.Collections.Generic;
using DCI_TASK_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DCI_TASK_API.Contexts;

public partial class DBSCM : DbContext
{
    public DBSCM()
    {
    }

    public DBSCM(DbContextOptions<DBSCM> options)
        : base(options)
    {
    }

    public virtual DbSet<DciTask> DciTasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.86;Database=dbSCM;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<DciTask>(entity =>
        {
            entity.HasKey(e => e.TaskId);

            entity.ToTable("DCI_TASK");

            entity.Property(e => e.TaskId).HasColumnName("TASK_ID");
            entity.Property(e => e.TaskCreateBy)
                .HasMaxLength(5)
                .HasColumnName("TASK_CREATE_BY");
            entity.Property(e => e.TaskCreateDt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("TASK_CREATE_DT");
            entity.Property(e => e.TaskDesc)
                .HasMaxLength(250)
                .HasColumnName("TASK_DESC");
            entity.Property(e => e.TaskDuedate)
                .HasColumnType("datetime")
                .HasColumnName("TASK_DUEDATE");
            entity.Property(e => e.TaskPriority)
                .HasMaxLength(50)
                .HasDefaultValueSql("(N'normal')")
                .HasColumnName("TASK_PRIORITY");
            entity.Property(e => e.TaskStatus)
                .HasMaxLength(25)
                .HasColumnName("TASK_STATUS");
            entity.Property(e => e.TaskTitle)
                .HasMaxLength(50)
                .HasColumnName("TASK_TITLE");
            entity.Property(e => e.TaskUpdateBy)
                .HasMaxLength(5)
                .HasColumnName("TASK_UPDATE_BY");
            entity.Property(e => e.TaskUpdateDt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("TASK_UPDATE_DT");
            entity.Property(e => e.TaskWarning)
                .HasDefaultValueSql("((7))")
                .HasColumnName("TASK_WARNING");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
