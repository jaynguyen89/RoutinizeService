using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class User
    {
        public User()
        {
            AppSettings = new HashSet<AppSetting>();
            Attachments = new HashSet<Attachment>();
            CollaborationCollaborators = new HashSet<Collaboration>();
            CollaborationUsers = new HashSet<Collaboration>();
            ContentGroups = new HashSet<ContentGroup>();
            DepartmentRoles = new HashSet<DepartmentRole>();
            GroupShares = new HashSet<GroupShare>();
            Notes = new HashSet<Note>();
            ParticipantReturnRequests = new HashSet<ParticipantReturnRequest>();
            Projects = new HashSet<Project>();
            RandomIdeas = new HashSet<RandomIdea>();
            TaskFolders = new HashSet<TaskFolder>();
            TaskLegends = new HashSet<TaskLegend>();
            TeamInvitationInvitationSentBies = new HashSet<TeamInvitation>();
            TeamInvitationInvitationSentTos = new HashSet<TeamInvitation>();
            TeamMembers = new HashSet<TeamMember>();
            TeamRequests = new HashSet<TeamRequest>();
            TeamRoles = new HashSet<TeamRole>();
            Teams = new HashSet<Team>();
            TodoDoneBies = new HashSet<Todo>();
            TodoUsers = new HashSet<Todo>();
            UserDepartments = new HashSet<UserDepartment>();
            UserOrganizations = new HashSet<UserOrganization>();
            UserPrivacies = new HashSet<UserPrivacy>();
            UserRsaKeys = new HashSet<UserRsaKey>();
        }

        public int Id { get; set; }
        public int AccountId { get; set; }
        public int? AddressId { get; set; }
        public string AvatarName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredName { get; set; }
        public bool Gender { get; set; }

        public virtual Account Account { get; set; }
        public virtual Address Address { get; set; }
        public virtual ICollection<AppSetting> AppSettings { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Collaboration> CollaborationCollaborators { get; set; }
        public virtual ICollection<Collaboration> CollaborationUsers { get; set; }
        public virtual ICollection<ContentGroup> ContentGroups { get; set; }
        public virtual ICollection<DepartmentRole> DepartmentRoles { get; set; }
        public virtual ICollection<GroupShare> GroupShares { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<ParticipantReturnRequest> ParticipantReturnRequests { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<RandomIdea> RandomIdeas { get; set; }
        public virtual ICollection<TaskFolder> TaskFolders { get; set; }
        public virtual ICollection<TaskLegend> TaskLegends { get; set; }
        public virtual ICollection<TeamInvitation> TeamInvitationInvitationSentBies { get; set; }
        public virtual ICollection<TeamInvitation> TeamInvitationInvitationSentTos { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<TeamRequest> TeamRequests { get; set; }
        public virtual ICollection<TeamRole> TeamRoles { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<Todo> TodoDoneBies { get; set; }
        public virtual ICollection<Todo> TodoUsers { get; set; }
        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
        public virtual ICollection<UserPrivacy> UserPrivacies { get; set; }
        public virtual ICollection<UserRsaKey> UserRsaKeys { get; set; }
    }
}
