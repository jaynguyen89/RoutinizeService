using Microsoft.EntityFrameworkCore;
using RoutinizeCore.Models;

#nullable disable

namespace RoutinizeCore.DbContexts
{
    public partial class RoutinizeDbContext : DbContext
    {
        public RoutinizeDbContext()
        {
        }

        public RoutinizeDbContext(DbContextOptions<RoutinizeDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<AttachmentPermission> AttachmentPermissions { get; set; }
        public virtual DbSet<AuthRecord> AuthRecords { get; set; }
        public virtual DbSet<ChallengeQuestion> ChallengeQuestions { get; set; }
        public virtual DbSet<ChallengeRecord> ChallengeRecords { get; set; }
        public virtual DbSet<Collaboration> Collaborations { get; set; }
        public virtual DbSet<CollaboratorTask> CollaboratorTasks { get; set; }
        public virtual DbSet<IterationTask> IterationTasks { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        public virtual DbSet<NoteSegment> NoteSegments { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectIteration> ProjectIterations { get; set; }
        public virtual DbSet<RandomIdea> RandomIdeas { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleClaim> RoleClaims { get; set; }
        public virtual DbSet<TaskComment> TaskComments { get; set; }
        public virtual DbSet<TaskPermission> TaskPermissions { get; set; }
        public virtual DbSet<TaskRelation> TaskRelations { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TeamInvitation> TeamInvitations { get; set; }
        public virtual DbSet<TeamMember> TeamMembers { get; set; }
        public virtual DbSet<TeamRequest> TeamRequests { get; set; }
        public virtual DbSet<TeamTask> TeamTasks { get; set; }
        public virtual DbSet<Todo> Todos { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserPrivacy> UserPrivacies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.RecoveryToken).HasMaxLength(60);

                entity.Property(e => e.TwoFaSecretKey).HasMaxLength(12);

                entity.Property(e => e.UniqueId).HasMaxLength(35);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.Building).HasMaxLength(30);

                entity.Property(e => e.Country).HasMaxLength(30);

                entity.Property(e => e.Latitude).HasMaxLength(30);

                entity.Property(e => e.Longitude).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.State).HasMaxLength(30);

                entity.Property(e => e.Street).HasMaxLength(50);

                entity.Property(e => e.Suburb).HasMaxLength(30);
            });

            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.ToTable("AppSetting");

                entity.Property(e => e.DateTimeFormat)
                    .IsRequired()
                    .HasMaxLength(1)
                    .HasDefaultValueSql("('FRIENDLY_DMY')");

                entity.Property(e => e.Theme).HasDefaultValueSql("((3))");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AppSettings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("Attachment");

                entity.Property(e => e.AttachedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.AttachmentName).HasMaxLength(100);

                entity.Property(e => e.AttachmentUrl).HasMaxLength(250);

                entity.Property(e => e.ItemType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.PermissionId);

                entity.HasOne(d => d.UploadedBy)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.UploadedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<AttachmentPermission>(entity =>
            {
                entity.ToTable("AttachmentPermission");

                entity.Property(e => e.MembersToAllowDelete).HasMaxLength(250);

                entity.Property(e => e.MembersToAllowDownload).HasMaxLength(250);

                entity.Property(e => e.MembersToAllowEdit).HasMaxLength(250);

                entity.Property(e => e.MembersToAllowView).HasMaxLength(250);
            });

            modelBuilder.Entity<AuthRecord>(entity =>
            {
                entity.ToTable("AuthRecord");

                entity.Property(e => e.AuthTokenSalt)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DeviceInformation).HasMaxLength(250);

                entity.Property(e => e.SessionId)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AuthRecords)
                    .HasForeignKey(d => d.AccountId);
            });

            modelBuilder.Entity<ChallengeQuestion>(entity =>
            {
                entity.ToTable("ChallengeQuestion");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Question)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ChallengeRecord>(entity =>
            {
                entity.ToTable("ChallengeRecord");

                entity.Property(e => e.RecordedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Response)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ChallengeRecords)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ChallengeQuestion)
                    .WithMany(p => p.ChallengeRecords)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Collaboration>(entity =>
            {
                entity.ToTable("Collaboration");

                entity.Property(e => e.InvitedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Collaborator)
                    .WithMany(p => p.CollaborationCollaborators)
                    .HasForeignKey(d => d.CollaboratorId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.CollaborationUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<CollaboratorTask>(entity =>
            {
                entity.ToTable("CollaboratorTask");

                entity.Property(e => e.TaskType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Collaboration)
                    .WithMany(p => p.CollaboratorTasks)
                    .HasForeignKey(d => d.CollaborationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.CollaboratorTasks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<IterationTask>(entity =>
            {
                entity.ToTable("IterationTask");

                entity.Property(e => e.AssignedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TaskType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Iteration)
                    .WithMany(p => p.IterationTasks)
                    .HasForeignKey(d => d.IterationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.ToTable("Note");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeletedOn).HasMaxLength(7);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<NoteSegment>(entity =>
            {
                entity.ToTable("NoteSegment");

                entity.Property(e => e.Body).HasMaxLength(4000);

                entity.HasOne(d => d.Note)
                    .WithMany(p => p.NoteSegments)
                    .HasForeignKey(d => d.NoteId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Project");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.ProjectCode).HasMaxLength(20);

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.CoverImage)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(d => d.CoverImageId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(d => d.CreatedById);
            });

            modelBuilder.Entity<ProjectIteration>(entity =>
            {
                entity.ToTable("ProjectIteration");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.IterationName).HasMaxLength(50);

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.ProjectIterations)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<RandomIdea>(entity =>
            {
                entity.ToTable("RandomIdea");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RandomIdeas)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<RoleClaim>(entity =>
            {
                entity.ToTable("RoleClaim");

                entity.HasOne(d => d.Claimer)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.ClaimerId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TaskComment>(entity =>
            {
                entity.ToTable("TaskComment");

                entity.Property(e => e.CommentOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CommentedOnType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.CommenterReferenceType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.HasOne(d => d.RepliedTo)
                    .WithMany(p => p.InverseRepliedTo)
                    .HasForeignKey(d => d.RepliedToId)
                    .HasConstraintName("FK_TaskComment_TaskComment_QuotedCommentId");
            });

            modelBuilder.Entity<TaskPermission>(entity =>
            {
                entity.ToTable("TaskPermission");

                entity.Property(e => e.Role).HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<TaskRelation>(entity =>
            {
                entity.ToTable("TaskRelation");

                entity.Property(e => e.RelatedToType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.RelationshipName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasDefaultValueSql("('RELATES TO')");

                entity.Property(e => e.TaskType)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TeamName).HasMaxLength(50);

                entity.Property(e => e.UniqueCode).HasMaxLength(150);

                entity.HasOne(d => d.CoverImage)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.CoverImageId);

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.CreatedById);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.ProjectId);
            });

            modelBuilder.Entity<TeamInvitation>(entity =>
            {
                entity.ToTable("TeamInvitation");

                entity.Property(e => e.IssuedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message).HasMaxLength(100);

                entity.HasOne(d => d.InvitationSentBy)
                    .WithMany(p => p.TeamInvitationInvitationSentBies)
                    .HasForeignKey(d => d.InvitationSentById)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.InvitationSentTo)
                    .WithMany(p => p.TeamInvitationInvitationSentTos)
                    .HasForeignKey(d => d.InvitationSentToId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.ToTable("TeamMember");

                entity.Property(e => e.MemberRole).HasDefaultValueSql("((2))");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TeamRequest>(entity =>
            {
                entity.ToTable("TeamRequest");

                entity.Property(e => e.Message).HasMaxLength(100);

                entity.Property(e => e.RequestedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.RequestedBy)
                    .WithMany(p => p.TeamRequests)
                    .HasForeignKey(d => d.RequestedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamRequests)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeamRequest_TeamId");
            });

            modelBuilder.Entity<TeamTask>(entity =>
            {
                entity.ToTable("TeamTask");

                entity.Property(e => e.AssignedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TaskType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamTasks)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Todo>(entity =>
            {
                entity.ToTable("Todo");

                entity.Property(e => e.CoverImage).HasMaxLength(100);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeletedOn).HasMaxLength(7);

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.Details).HasMaxLength(4000);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.DoneBy)
                    .WithMany(p => p.TodoDoneBies)
                    .HasForeignKey(d => d.DoneById);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TodoUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.AvatarName).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.PreferredName).HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.AddressId);
            });

            modelBuilder.Entity<UserPrivacy>(entity =>
            {
                entity.ToTable("UserPrivacy");

                entity.Property(e => e.NamePolicy).HasDefaultValueSql("((2))");

                entity.Property(e => e.UernamePolicy).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPrivacies)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
