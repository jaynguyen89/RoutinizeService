namespace RoutinizeCore.ViewModels.User {

    public sealed class UserVM {
        
        public int Id { get; set; }
        
        public string Avatar { get; set; }
        
        public string Name { get; set; }
        
        public bool Gender { get; set; }

        public static implicit operator UserVM(Models.User user) {
            return new() {
                Id = user.Id,
                Avatar = user.AvatarName,
                Name = user.PreferredName ?? $"{ user.FirstName } { user.LastName }",
                Gender = user.Gender
            };
        }
    }
}