using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MediaLibrary.DbContexts {

    public partial class MediaDbContext {

        private IConfiguration Configuration;

        public MediaDbContext(IConfiguration configuration) {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySQL(Configuration.GetConnectionString("MediaDbServer"));
        }
    }
}