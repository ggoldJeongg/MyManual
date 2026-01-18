using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MyManual.Models;

namespace MyManual.Data
{
    public class AppDbContext : DbContext
    {
        // ==================== DbSet (테이블) ====================

        public DbSet<User> Users { get; set; }
        public DbSet<Manual> Manuals { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<HistoryItem> HistoryItems { get; set; }
        public DbSet<OnboardingTask> OnboardingTasks { get; set; }
        public DbSet<UserChecklistStatus> UserChecklistStatuses { get; set; }
        public DbSet<UserTaskStatus> UserTaskStatuses { get; set; }

        // ==================== DB 연결 설정 ====================

        public static string DbPath { get; }

        static AppDbContext()
        {
            // DB 파일 경로: %LocalAppData%\MyManual\mymanual.db
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(folder, "MyManual");

            // 폴더가 없으면 생성
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            DbPath = Path.Combine(appFolder, "mymanual.db");
        }

        // DI를 통해 주입받는 생성자
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // 이미 설정된 경우 (DI에서 주입된 경우) 스킵
            if (options.IsConfigured) return;

            // 직접 생성 시 기본 설정 (DatabaseInitializer 등)
            var connectionString = $"Data Source={DbPath};Mode=ReadWriteCreate;Cache=Shared";
            options.UseSqlite(connectionString);
        }

        // ==================== 테이블 관계 설정 ====================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Manual 1:N ChecklistItem
            modelBuilder.Entity<ChecklistItem>()
                .HasOne(c => c.Manual)
                .WithMany(m => m.Checklist)
                .HasForeignKey(c => c.ManualId)
                .OnDelete(DeleteBehavior.Cascade);  // 매뉴얼 삭제 시 체크리스트도 삭제

            // Manual 1:N HistoryItem
            modelBuilder.Entity<HistoryItem>()
                .HasOne(h => h.Manual)
                .WithMany(m => m.History)
                .HasForeignKey(h => h.ManualId)
                .OnDelete(DeleteBehavior.Cascade);

            // Manual 1:N OnboardingTask
            modelBuilder.Entity<OnboardingTask>()
                .HasOne(t => t.Manual)
                .WithMany(m => m.OnboardingTasks)
                .HasForeignKey(t => t.ManualId)
                .OnDelete(DeleteBehavior.Cascade);

            // User 1:N OnboardingTask (사용자별 할당된 태스크)
            modelBuilder.Entity<OnboardingTask>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User 1:N UserChecklistStatus
            modelBuilder.Entity<UserChecklistStatus>()
                .HasOne(s => s.User)
                .WithMany(u => u.ChecklistStatuses)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChecklistItem 1:N UserChecklistStatus
            modelBuilder.Entity<UserChecklistStatus>()
                .HasOne(s => s.ChecklistItem)
                .WithMany(c => c.UserStatuses)
                .HasForeignKey(s => s.ChecklistItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // User 1:N UserTaskStatus
            modelBuilder.Entity<UserTaskStatus>()
                .HasOne(s => s.User)
                .WithMany(u => u.TaskStatuses)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // OnboardingTask 1:N UserTaskStatus
            modelBuilder.Entity<UserTaskStatus>()
                .HasOne(s => s.OnboardingTask)
                .WithMany(t => t.UserStatuses)
                .HasForeignKey(s => s.OnboardingTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserChecklistStatus: 복합 유니크 인덱스 (같은 사용자가 같은 항목을 중복 체크 방지)
            modelBuilder.Entity<UserChecklistStatus>()
                .HasIndex(s => new { s.UserId, s.ChecklistItemId })
                .IsUnique();

            // UserTaskStatus: 복합 유니크 인덱스
            modelBuilder.Entity<UserTaskStatus>()
                .HasIndex(s => new { s.UserId, s.OnboardingTaskId })
                .IsUnique();
        }
    }
}
