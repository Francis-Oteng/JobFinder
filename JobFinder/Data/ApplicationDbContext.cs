using JobFinder.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace JobFinder.Data
{
    public class ApplicationDbContext  : IdentityDbContext<ApplicationUser>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        // NOTE: there is deliberately NO OnConfiguring override here.
        // The connection string comes from appsettings.json via Program.cs.
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<Employer> Employers => Set<Employer>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<Interview> Interviews => Set<Interview>();
        public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
        public DbSet<Message> Messages => Set<Message>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================================================================
            // ApplicationUser  ->  table "Users"
            // =====================================================================
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FullName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.Email)
                      .HasMaxLength(256)
                      .IsUnicode(false);

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.IsActive)
                      .IsRequired();

                entity.Property(e => e.CreatedDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.LastLoginDate)
                      .HasColumnType("datetime2(0)");

                entity.HasIndex(e => e.Email)
                      .IsUnique()
                      .HasDatabaseName("UX_Users_Email");
            });

            // =====================================================================
            // Applicant     (ApplicationUser 1 : 0..1 Applicant)
            // =====================================================================
            modelBuilder.Entity<Applicant>(entity =>
            {
                entity.ToTable("Applicants", t =>
                    t.HasCheckConstraint(
                        "CK_Applicants_ExperienceYears",
                        "[ExperienceYears] IS NULL OR ([ExperienceYears] >= 0 AND [ExperienceYears] <= 60)"));

                entity.HasKey(e => e.ApplicantId);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.Location).HasMaxLength(150);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.Skills).HasMaxLength(1000);
                entity.Property(e => e.Education).HasMaxLength(500);
                entity.Property(e => e.Certifications).HasMaxLength(1000);

                entity.Property(e => e.ExperienceLevel)
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.PreferredJobType)
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.ResumeUrl).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.PortfolioUrl).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.PhotoUrl).HasMaxLength(500).IsUnicode(false);

                entity.Property(e => e.CreatedDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime2(0)");

                // Unique FK => one-to-one (zero-or-one) instead of a collection.
                entity.HasIndex(e => e.UserId)
                      .IsUnique()
                      .HasDatabaseName("UX_Applicants_UserId");

                entity.HasOne(a => a.User)
                      .WithOne(u => u.Applicant)
                      .HasForeignKey<Applicant>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Applicants_Users");
            });

            // =====================================================================
            // Employer      (ApplicationUser 1 : 0..1 Employer)
            // =====================================================================
            modelBuilder.Entity<Employer>(entity =>
            {
                entity.ToTable("Employers");

                entity.HasKey(e => e.EmployerId);

                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Industry).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Website).HasMaxLength(300).IsUnicode(false);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsUnicode(false);

                entity.Property(e => e.CompanySize)
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.LogoUrl).HasMaxLength(500).IsUnicode(false);

                entity.Property(e => e.IsVerified)
                      .IsRequired()
                      .HasDefaultValue(false);

                entity.Property(e => e.CreatedDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(e => e.UserId)
                      .IsUnique()
                      .HasDatabaseName("UX_Employers_UserId");

                entity.HasIndex(e => e.CompanyName)
                      .HasDatabaseName("IX_Employers_CompanyName");

                entity.HasOne(e => e.User)
                      .WithOne(u => u.Employer)
                      .HasForeignKey<Employer>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Employers_Users");
            });

            // =====================================================================
            // Job           (Employer 1 : many Job)
            // =====================================================================
            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("Jobs", t =>
                {
                    t.HasCheckConstraint(
                        "CK_Jobs_SalaryRange",
                        "[SalaryMin] IS NULL OR [SalaryMax] IS NULL OR [SalaryMax] >= [SalaryMin]");
                    t.HasCheckConstraint(
                        "CK_Jobs_ClosingDate",
                        "[ClosingDate] IS NULL OR [ClosingDate] >= [PostedDate]");
                });

                entity.HasKey(e => e.JobId);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);

                // Long-form free text -> nvarchar(max). Never SQL Server 'text'.
                entity.Property(e => e.Description).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Requirements).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Responsibilities).HasColumnType("nvarchar(max)");

                entity.Property(e => e.Benefits).HasMaxLength(2000);
                entity.Property(e => e.RequiredSkills).HasMaxLength(1000);
                entity.Property(e => e.Education).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(150);

                entity.Property(e => e.ExperienceLevel)
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.JobType)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.WorkArrangement)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.SalaryMin).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SalaryMax).HasColumnType("decimal(10,2)");

                entity.Property(e => e.PostedDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.ClosingDate).HasColumnType("datetime2(0)");

                entity.HasIndex(e => e.EmployerId).HasDatabaseName("IX_Jobs_EmployerId");

                // Drives the public job board: active jobs, newest first.
                entity.HasIndex(e => new { e.Status, e.PostedDate })
                      .HasDatabaseName("IX_Jobs_Status_PostedDate");

                entity.HasIndex(e => e.Category).HasDatabaseName("IX_Jobs_Category");

                entity.HasOne(j => j.Employer)
                      .WithMany(e => e.Jobs)
                      .HasForeignKey(j => j.EmployerId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Jobs_Employers");
            });

            // =====================================================================
            // Application   (Applicant 1 : many, Job 1 : many)
            // =====================================================================
            modelBuilder.Entity<Application>(entity =>
            {
                entity.ToTable("Applications", t =>
                    t.HasCheckConstraint(
                        "CK_Applications_AIMatchScore",
                        "[AIMatchScore] IS NULL OR ([AIMatchScore] >= 0 AND [AIMatchScore] <= 100)"));

                entity.HasKey(e => e.ApplicationId);

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.CoverLetter).HasMaxLength(4000);
                entity.Property(e => e.ResumeUrl).HasMaxLength(500).IsUnicode(false);
                entity.Property(e => e.EmployerNotes).HasMaxLength(2000);

                entity.Property(e => e.ApplicationDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.ReviewedDate).HasColumnType("datetime2(0)");

                // REQUIREMENT: an applicant cannot apply to the same job twice.
                entity.HasIndex(e => new { e.ApplicantId, e.JobId })
                      .IsUnique()
                      .HasDatabaseName("UX_Applications_Applicant_Job");

                // Employer's applicant pipeline screen.
                entity.HasIndex(e => new { e.JobId, e.Status })
                      .HasDatabaseName("IX_Applications_Job_Status");

                entity.HasOne(a => a.Applicant)
                      .WithMany(ap => ap.Applications)
                      .HasForeignKey(a => a.ApplicantId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Applications_Applicants");

                // Restrict, NOT Cascade: two cascade paths would reach Applications
                // (User -> Applicant -> Application and User -> Employer -> Job -> Application)
                // and SQL Server rejects that. Close a job instead of deleting it.
                entity.HasOne(a => a.Job)
                      .WithMany(j => j.Applications)
                      .HasForeignKey(a => a.JobId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Applications_Jobs");
            });

            // =====================================================================
            // Interview     (Application 1 : 0..many Interview)
            // =====================================================================
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.ToTable("Interviews");

                entity.HasKey(e => e.InterviewId);

                entity.Property(e => e.InterviewDate)
                      .IsRequired()
                      .HasColumnType("datetime2(0)");

                entity.Property(e => e.InterviewType)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasConversion<string>();

                entity.Property(e => e.MeetingLink).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.Property(e => e.CreatedDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(e => e.ApplicationId).HasDatabaseName("IX_Interviews_ApplicationId");
                entity.HasIndex(e => e.InterviewDate).HasDatabaseName("IX_Interviews_InterviewDate");

                entity.HasOne(i => i.Application)
                      .WithMany(a => a.Interviews)
                      .HasForeignKey(i => i.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Interviews_Applications");
            });

            // =====================================================================
            // SavedJob      (Applicant 1 : many, Job 1 : many)
            // =====================================================================
            modelBuilder.Entity<SavedJob>(entity =>
            {
                entity.ToTable("SavedJobs");

                entity.HasKey(e => e.SavedJobId);

                entity.Property(e => e.SavedDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                // REQUIREMENT: a job can only be saved once per applicant.
                entity.HasIndex(e => new { e.ApplicantId, e.JobId })
                      .IsUnique()
                      .HasDatabaseName("UX_SavedJobs_Applicant_Job");

                entity.HasOne(s => s.Applicant)
                      .WithMany(a => a.SavedJobs)
                      .HasForeignKey(s => s.ApplicantId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_SavedJobs_Applicants");

                // Restrict for the same multiple-cascade-path reason as Applications.
                entity.HasOne(s => s.Job)
                      .WithMany(j => j.SavedJobs)
                      .HasForeignKey(s => s.JobId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_SavedJobs_Jobs");
            });

            // =====================================================================
            // Message       (two separate FKs to Users + optional Job/Application)
            // =====================================================================
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages", t =>
                    t.HasCheckConstraint(
                        "CK_Messages_SenderNotRecipient",
                        "[SenderId] <> [RecipientId]"));

                entity.HasKey(e => e.MessageId);

                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(e => e.SentDate)
                      .HasColumnType("datetime2(0)")
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.ReadDate).HasColumnType("datetime2(0)");

                entity.Property(e => e.IsRead)
                      .IsRequired()
                      .HasDefaultValue(false);

                // Inbox: unread messages for a recipient, newest first.
                entity.HasIndex(e => new { e.RecipientId, e.IsRead, e.SentDate })
                      .HasDatabaseName("IX_Messages_Recipient_IsRead_SentDate");

                entity.HasIndex(e => e.SenderId).HasDatabaseName("IX_Messages_SenderId");
                entity.HasIndex(e => e.ApplicationId).HasDatabaseName("IX_Messages_ApplicationId");

                // Two relationships to the SAME principal table. Both must be declared
                // explicitly or EF Core creates shadow FKs / throws on ambiguity.
                // Both are Restrict: cascading here would create cycles on Users.
                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Messages_Users_Sender");

                entity.HasOne(m => m.Recipient)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.RecipientId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Messages_Users_Recipient");

                // Optional context — nullable FKs.
                entity.HasOne(m => m.Job)
                      .WithMany(j => j.Messages)
                      .HasForeignKey(m => m.JobId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Messages_Jobs");

                entity.HasOne(m => m.Application)
                      .WithMany(a => a.Messages)
                      .HasForeignKey(m => m.ApplicationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Messages_Applications");
            });
        }
    }
}
