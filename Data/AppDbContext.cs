using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MyManual.Models;

namespace MyManual.Data
{
    public class AppDbContext : DbContext
    {
        // ==================== 싱글톤 패턴 ====================

        private static AppDbContext? _instance;
        private static readonly object _lock = new();

        public static AppDbContext Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new AppDbContext();
                    return _instance;
                }
            }
        }

        // ==================== DbSet (테이블) ====================

        public DbSet<User> Users { get; set; }
        public DbSet<Manual> Manuals { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<HistoryItem> HistoryItems { get; set; }
        public DbSet<OnboardingTask> OnboardingTasks { get; set; }
        public DbSet<UserChecklistStatus> UserChecklistStatuses { get; set; }
        public DbSet<UserTaskStatus> UserTaskStatuses { get; set; }

        // ==================== DB 연결 설정 ====================

        private static readonly string _dbPath;

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

            _dbPath = Path.Combine(appFolder, "mymanual.db");
        }

        // private 생성자 (외부에서 new 방지)
        private AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // WAL 모드: 동시 읽기/쓰기 허용
            // Busy Timeout: 잠금 대기 시간 (5초)
            // Pooling=false: 연결 풀링 비활성화 (WPF 단일 앱에서 권장)
            var connectionString = $"Data Source={_dbPath};Mode=ReadWriteCreate;Cache=Shared";
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
