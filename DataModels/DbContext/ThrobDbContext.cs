using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Data.DbContext
{
    public class ThrobDbContext : IdentityDbContext<ApplicationUser>
    {
        public ThrobDbContext(DbContextOptions<ThrobDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تكوين InstructorCourse
            modelBuilder.Entity<InstructorCourse>()
                .HasKey(ic => new { ic.InstructorId, ic.CourseId });

            modelBuilder.Entity<InstructorCourse>()
                .HasOne(ic => ic.Instructor)
                .WithMany(i => i.InstructorCourses)
                .HasForeignKey(ic => ic.InstructorId);

            modelBuilder.Entity<InstructorCourse>()
                .HasOne(ic => ic.Course)
                .WithMany(c => c.InstructorCourses)
                .HasForeignKey(ic => ic.CourseId);

            // تكوين StudentCourse
            modelBuilder.Entity<StudentCourse>()
                .HasKey(sc => new { sc.StudentId, sc.CourseId });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId);

            // تكوين LiveSession 
            modelBuilder.Entity<LiveSession>()
                .HasOne(ls => ls.Course)
                .WithOne(c => c.LiveSession)
                .HasForeignKey<LiveSession>(ls => ls.CourseId);

                
            // تكوين Question
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId);


            modelBuilder.Entity<DriveSession>()
                          .HasMany(ds => ds.Courses)
                          .WithMany(c => c.DriveSessions)
                          .UsingEntity(j => j.ToTable("DriveSessionCourses"));
            // 1. تكوين ExamRequestModel بشكل بسيط (بدون أي علاقات Many-to-Many مباشرة)
            modelBuilder.Entity<ExamRequestModel>(entity =>
            {
                entity.HasKey(e => e.ExamRequestId);
                entity.Property(e => e.ExamRequestId).ValueGeneratedOnAdd();
            });

            // 2. التكوين الصحيح والوحيد للجدول الوسيط اليدوي
            modelBuilder.Entity<ExamRequestQuestion>(entity =>
            {
                entity.ToTable("ExamRequestQuestions");

                entity.HasKey(eq => new { eq.ExamRequestId, eq.QuestionId });

                entity.HasOne(eq => eq.ExamRequest)
                      .WithMany(e => e.ExamRequestQuestions) // تأكد من وجود هذه الخاصية في ExamRequestModel
                      .HasForeignKey(eq => eq.ExamRequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(eq => eq.Question)
                      .WithMany(q => q.ExamRequestQuestions) // تأكد من وجود هذه الخاصية في Question
                      .HasForeignKey(eq => eq.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Course>()
    .Property(c => c.CoursePrice)
    .HasColumnType("decimal(18,2)");



            modelBuilder.Entity<InstructorCourse>().ToTable("InstructorCourse");
            modelBuilder.Entity<StudentCourse>().ToTable("StudentCourses");
            modelBuilder.Entity<InstructorCourse>().ToTable("InstructorCourse");
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<LiveSession> LiveSessions { get; set; }
        public DbSet<DriveSession> DriveSessions { get; set; }

        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<InstructorCourse> InstructorCourses { get; set; }
        public DbSet<Question> Questions { get; set; } // تصحيح الاسم من "questions" إلى "Questions" (اتباع التسمية المناسبة)
        public DbSet<QuestionOption> QuestionOptions { get; set; } // تصحيح الاسم من "questionOptions" إلى "QuestionOptions"
        public DbSet<ExamRequestModel> ExamRequestModels { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<LectureResource> LectureResources { get; set; }
        public DbSet<UserExamResult> UserExamResults { get; set; }
        public DbSet<ExamRequestQuestion> ExamRequestQuestions { get; set; }
    }
}