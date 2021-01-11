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
            CollaboratorTasks = new HashSet<CollaboratorTask>();
            Notes = new HashSet<Note>();
            Projects = new HashSet<Project>();
            RandomIdeas = new HashSet<RandomIdea>();
            TeamInvitationInvitationSentBies = new HashSet<TeamInvitation>();
            TeamInvitationInvitationSentTos = new HashSet<TeamInvitation>();
            TeamMembers = new HashSet<TeamMember>();
            TeamRequests = new HashSet<TeamRequest>();
            Teams = new HashSet<Team>();
            TodoDoneBies = new HashSet<Todo>();
            TodoUsers = new HashSet<Todo>();
            UserPrivacies = new HashSet<UserPrivacy>();
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
        public virtual ICollection<CollaboratorTask> CollaboratorTasks { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<RandomIdea> RandomIdeas { get; set; }
        public virtual ICollection<TeamInvitation> TeamInvitationInvitationSentBies { get; set; }
        public virtual ICollection<TeamInvitation> TeamInvitationInvitationSentTos { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<TeamRequest> TeamRequests { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<Todo> TodoDoneBies { get; set; }
        public virtual ICollection<Todo> TodoUsers { get; set; }
        public virtual ICollection<UserPrivacy> UserPrivacies { get; set; }
    }
}
