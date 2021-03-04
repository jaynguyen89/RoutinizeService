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
        public virtual DbSet<ColorPallete> ColorPalletes { get; set; }
        public virtual DbSet<ContentGroup> ContentGroups { get; set; }
        public virtual DbSet<Cooperation> Cooperations { get; set; }
        public virtual DbSet<CooperationParticipant> CooperationParticipants { get; set; }
        public virtual DbSet<CooperationRequest> CooperationRequests { get; set; }
        public virtual DbSet<CooperationTaskVault> CooperationTaskVaults { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<DepartmentAccess> DepartmentAccesses { get; set; }
        public virtual DbSet<DepartmentRole> DepartmentRoles { get; set; }
        public virtual DbSet<FolderItem> FolderItems { get; set; }
        public virtual DbSet<GroupShare> GroupShares { get; set; }
        public virtual DbSet<Industry> Industries { get; set; }
        public virtual DbSet<IterationTask> IterationTasks { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        public virtual DbSet<NoteSegment> NoteSegments { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<ParticipantReturnRequest> ParticipantReturnRequests { get; set; }
        public virtual DbSet<PositionTitle> PositionTitles { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectIteration> ProjectIterations { get; set; }
        public virtual DbSet<ProjectRelation> ProjectRelations { get; set; }
        public virtual DbSet<RandomIdea> RandomIdeas { get; set; }
        public virtual DbSet<Relationship> Relationships { get; set; }
        public virtual DbSet<SigningChecker> SigningCheckers { get; set; }
        public virtual DbSet<TaskComment> TaskComments { get; set; }
        public virtual DbSet<TaskFolder> TaskFolders { get; set; }
        public virtual DbSet<TaskLegend> TaskLegends { get; set; }
        public virtual DbSet<TaskPermission> TaskPermissions { get; set; }
        public virtual DbSet<TaskRelation> TaskRelations { get; set; }
        public virtual DbSet<TaskVaultItem> TaskVaultItems { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TeamDepartment> TeamDepartments { get; set; }
        public virtual DbSet<TeamInvitation> TeamInvitations { get; set; }
        public virtual DbSet<TeamMember> TeamMembers { get; set; }
        public virtual DbSet<TeamProject> TeamProjects { get; set; }
        public virtual DbSet<TeamRequest> TeamRequests { get; set; }
        public virtual DbSet<TeamRole> TeamRoles { get; set; }
        public virtual DbSet<TeamRoleClaim> TeamRoleClaims { get; set; }
        public virtual DbSet<TeamTask> TeamTasks { get; set; }
        public virtual DbSet<Todo> Todos { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserDepartment> UserDepartments { get; set; }
        public virtual DbSet<UserOrganization> UserOrganizations { get; set; }
        public virtual DbSet<UserPrivacy> UserPrivacies { get; set; }
        public virtual DbSet<UserRsaKey> UserRsaKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FcmToken).HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(100);

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

                entity.Property(e => e.Building).HasMaxLength(100);

                entity.Property(e => e.Country).HasMaxLength(100);

                entity.Property(e => e.Latitude).HasMaxLength(30);

                entity.Property(e => e.Longitude).HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Postcode).HasMaxLength(20);

                entity.Property(e => e.State).HasMaxLength(100);

                entity.Property(e => e.Street).HasMaxLength(300);

                entity.Property(e => e.Suburb).HasMaxLength(100);
            });

            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.ToTable("AppSetting");

                entity.Property(e => e.DateTimeFormat)
                    .IsRequired()
                    .HasMaxLength(30)
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

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.AddressId);

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

                entity.Property(e => e.SessionId).HasMaxLength(250);

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
                    .HasForeignKey(d => d.ChallengeQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChallengeRecord_ChallengeQuestion_QuestionId");
            });

            modelBuilder.Entity<Collaboration>(entity =>
            {
                entity.ToTable("Collaboration");

                entity.Property(e => e.InvitedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message).HasMaxLength(300);

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

                entity.Property(e => e.AssignedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message).HasMaxLength(300);

                entity.Property(e => e.TaskType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Collaboration)
                    .WithMany(p => p.CollaboratorTasks)
                    .HasForeignKey(d => d.CollaborationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<ColorPallete>(entity =>
            {
                entity.ToTable("ColorPallete");

                entity.Property(e => e.ColorCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.ColorName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SupplementColorIds).HasMaxLength(200);
            });

            modelBuilder.Entity<ContentGroup>(entity =>
            {
                entity.ToTable("ContentGroup");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.GroupName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.GroupOfType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.ContentGroups)
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Cooperation>(entity =>
            {
                entity.ToTable("Cooperation");

                entity.Property(e => e.ConfidedRequestResponderIds).HasMaxLength(1000);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.RequestAcceptancePolicy).HasMaxLength(1000);
            });

            modelBuilder.Entity<CooperationParticipant>(entity =>
            {
                entity.ToTable("CooperationParticipant");

                entity.Property(e => e.ParticipantType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.ParticipatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Cooperation)
                    .WithMany(p => p.CooperationParticipants)
                    .HasForeignKey(d => d.CooperationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<CooperationRequest>(entity =>
            {
                entity.ToTable("CooperationRequest");

                entity.Property(e => e.AcceptanceNote).HasMaxLength(100);

                entity.Property(e => e.Message).HasMaxLength(2000);

                entity.Property(e => e.RequestedByType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.RequestedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RequestedToType)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<CooperationTaskVault>(entity =>
            {
                entity.ToTable("CooperationTaskVault");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.TaskVaultName).HasMaxLength(100);

                entity.HasOne(d => d.AssignedToCooperator)
                    .WithMany(p => p.CooperationTaskVaultAssignedToCooperators)
                    .HasForeignKey(d => d.AssignedToCooperatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.AssociatedWithDepartment)
                    .WithMany(p => p.CooperationTaskVaults)
                    .HasForeignKey(d => d.AssociatedWithDepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Cooperation)
                    .WithMany(p => p.CooperationTaskVaults)
                    .HasForeignKey(d => d.CooperationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.PossessedByCooperator)
                    .WithMany(p => p.CooperationTaskVaultPossessedByCooperators)
                    .HasForeignKey(d => d.PossessedByCooperatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");

                entity.Property(e => e.Avatar).HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(4000);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Departments)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId);
            });

            modelBuilder.Entity<DepartmentAccess>(entity =>
            {
                entity.ToTable("DepartmentAccess");

                entity.Property(e => e.AccessibleDepartmentIds)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.Cooperation)
                    .WithMany(p => p.DepartmentAccesses)
                    .HasForeignKey(d => d.CooperationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DepartmentAccess_Cooperation_CooerationId");
            });

            modelBuilder.Entity<DepartmentRole>(entity =>
            {
                entity.ToTable("DepartmentRole");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.ForDepartmentIds).HasMaxLength(4000);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.AddedBy)
                    .WithMany(p => p.DepartmentRoles)
                    .HasForeignKey(d => d.AddedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.DepartmentRoles)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<FolderItem>(entity =>
            {
                entity.ToTable("FolderItem");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ItemType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.FolderItems)
                    .HasForeignKey(d => d.FolderId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<GroupShare>(entity =>
            {
                entity.ToTable("GroupShare");

                entity.Property(e => e.SharedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SharedToType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.SideNotes).HasMaxLength(150);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupShares)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.SharedBy)
                    .WithMany(p => p.GroupShares)
                    .HasForeignKey(d => d.SharedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Industry>(entity =>
            {
                entity.ToTable("Industry");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId);
            });

            modelBuilder.Entity<IterationTask>(entity =>
            {
                entity.ToTable("IterationTask");

                entity.Property(e => e.AssignedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message).HasMaxLength(300);

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

                entity.Property(e => e.Title).HasMaxLength(150);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Notes)
                    .HasForeignKey(d => d.GroupId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<NoteSegment>(entity =>
            {
                entity.ToTable("NoteSegment");

                entity.HasOne(d => d.Note)
                    .WithMany(p => p.NoteSegments)
                    .HasForeignKey(d => d.NoteId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");

                entity.Property(e => e.BriefDescription).HasMaxLength(1000);

                entity.Property(e => e.FullName).HasMaxLength(100);

                entity.Property(e => e.LogoName).HasMaxLength(100);

                entity.Property(e => e.RegistrationCode).HasMaxLength(30);

                entity.Property(e => e.RegistrationNumber).HasMaxLength(30);

                entity.Property(e => e.ShortName).HasMaxLength(60);

                entity.Property(e => e.UniqueId)
                    .IsRequired()
                    .HasMaxLength(35);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.AddressId);

                entity.HasOne(d => d.Industry)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.IndustryId);

                entity.HasOne(d => d.Mother)
                    .WithMany(p => p.InverseMother)
                    .HasForeignKey(d => d.MotherId);
            });

            modelBuilder.Entity<ParticipantReturnRequest>(entity =>
            {
                entity.ToTable("ParticipantReturnRequest");

                entity.Property(e => e.Message).HasMaxLength(300);

                entity.Property(e => e.RequestedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RespondNote).HasMaxLength(300);

                entity.HasOne(d => d.CooperationParticipant)
                    .WithMany(p => p.ParticipantReturnRequests)
                    .HasForeignKey(d => d.CooperationParticipantId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.RespondedBy)
                    .WithMany(p => p.ParticipantReturnRequests)
                    .HasForeignKey(d => d.RespondedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PositionTitle>(entity =>
            {
                entity.ToTable("PositionTitle");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.PositionTitles)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Project");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(2000);

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

                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.IterationName).HasMaxLength(100);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectIterations)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<ProjectRelation>(entity =>
            {
                entity.ToTable("ProjectRelation");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.FromProject)
                    .WithMany(p => p.ProjectRelationFromProjects)
                    .HasForeignKey(d => d.FromProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Relationship)
                    .WithMany(p => p.ProjectRelations)
                    .HasForeignKey(d => d.RelationshipId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ToProject)
                    .WithMany(p => p.ProjectRelationToProjects)
                    .HasForeignKey(d => d.ToProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<RandomIdea>(entity =>
            {
                entity.ToTable("RandomIdea");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.RandomIdeas)
                    .HasForeignKey(d => d.GroupId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RandomIdeas)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Relationship>(entity =>
            {
                entity.ToTable("Relationship");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.OppositeName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<SigningChecker>(entity =>
            {
                entity.ToTable("SigningChecker");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ForActivity).HasMaxLength(50);

                entity.HasOne(d => d.CooperationParticipant)
                    .WithMany(p => p.SigningCheckers)
                    .HasForeignKey(d => d.CooperationParticipantId)
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

            modelBuilder.Entity<TaskFolder>(entity =>
            {
                entity.ToTable("TaskFolder");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.FillColor).HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Color)
                    .WithMany(p => p.TaskFolders)
                    .HasForeignKey(d => d.ColorId);

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.TaskFolders)
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TaskLegend>(entity =>
            {
                entity.ToTable("TaskLegend");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.FillColor).HasMaxLength(10);

                entity.Property(e => e.ForDepartmentIds).HasMaxLength(4000);

                entity.Property(e => e.ForProjectIds).HasMaxLength(4000);

                entity.Property(e => e.ForTeamIds).HasMaxLength(4000);

                entity.Property(e => e.LegendName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Color)
                    .WithMany(p => p.TaskLegends)
                    .HasForeignKey(d => d.ColorId);

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.TaskLegends)
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TaskPermission>(entity =>
            {
                entity.ToTable("TaskPermission");

                entity.Property(e => e.Role).HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<TaskRelation>(entity =>
            {
                entity.ToTable("TaskRelation");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RelatedToType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.TaskType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Relationship)
                    .WithMany(p => p.TaskRelations)
                    .HasForeignKey(d => d.RelationshipId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TaskVaultItem>(entity =>
            {
                entity.ToTable("TaskVaultItem");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ItemType)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.AddedByUser)
                    .WithMany(p => p.TaskVaultItems)
                    .HasForeignKey(d => d.AddedByUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.TaskVault)
                    .WithMany(p => p.TaskVaultItems)
                    .HasForeignKey(d => d.TaskVaultId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TeamName).HasMaxLength(100);

                entity.Property(e => e.UniqueCode).HasMaxLength(50);

                entity.HasOne(d => d.CoverImage)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.CoverImageId);

                entity.HasOne(d => d.CreatedBy)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.CreatedById);
            });

            modelBuilder.Entity<TeamDepartment>(entity =>
            {
                entity.ToTable("TeamDepartment");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.TeamDepartments)
                    .HasForeignKey(d => d.DepartmentId);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.TeamDepartments)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamDepartments)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
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

            modelBuilder.Entity<TeamProject>(entity =>
            {
                entity.ToTable("TeamProject");

                entity.Property(e => e.AssignedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.TeamProjects)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamProjects)
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

            modelBuilder.Entity<TeamRole>(entity =>
            {
                entity.ToTable("TeamRole");

                entity.Property(e => e.AddedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.ForTeamIds).HasMaxLength(4000);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TeamRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TeamRoleClaim>(entity =>
            {
                entity.ToTable("TeamRoleClaim");

                entity.HasOne(d => d.Claimer)
                    .WithMany(p => p.TeamRoleClaims)
                    .HasForeignKey(d => d.ClaimerId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.TeamRole)
                    .WithMany(p => p.TeamRoleClaims)
                    .HasForeignKey(d => d.TeamRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TeamTask>(entity =>
            {
                entity.ToTable("TeamTask");

                entity.Property(e => e.AssignedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Message).HasMaxLength(300);

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

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.DoneBy)
                    .WithMany(p => p.TodoDoneBies)
                    .HasForeignKey(d => d.DoneById);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Todos)
                    .HasForeignKey(d => d.GroupId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TodoUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.AvatarName).HasMaxLength(100);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.PreferredName).HasMaxLength(50);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.AddressId);
            });

            modelBuilder.Entity<UserDepartment>(entity =>
            {
                entity.ToTable("UserDepartment");

                entity.Property(e => e.EmployeeCode).HasMaxLength(50);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.UserDepartments)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.DepartmentRole)
                    .WithMany(p => p.UserDepartments)
                    .HasForeignKey(d => d.DepartmentRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.UserDepartments)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserDepartments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UserOrganization>(entity =>
            {
                entity.ToTable("UserOrganization");

                entity.Property(e => e.EmployeeCode).HasMaxLength(50);

                entity.HasOne(d => d.DepartmentRole)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.DepartmentRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserOrganizations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UserPrivacy>(entity =>
            {
                entity.ToTable("UserPrivacy");

                entity.Property(e => e.NamePolicy).HasDefaultValueSql("((2))");

                entity.Property(e => e.UsernamePolicy).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPrivacies)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UserRsaKey>(entity =>
            {
                entity.ToTable("UserRsaKey");

                entity.Property(e => e.GeneratedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrivateKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.PublicKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRsaKeys)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
